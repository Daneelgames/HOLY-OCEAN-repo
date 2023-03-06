using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using MrPink.Tools;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MrPink
{
    public class ScoringSystem : MonoBehaviour
    {
        public static ScoringSystem Instance;
        [SerializeField] [ReadOnly] int currentGold = 3000;
        
        public int CurrentGold
        {
            get => currentGold;
            set => currentGold = value;
        }
        
        [BoxGroup("MOJO")][SerializeField] private List<MojoLevel> _mojoLevels = new List<MojoLevel>();
        public List<MojoLevel> MojoLevels => _mojoLevels;
        public MojoLevel GetCurrentMojoLevel => _mojoLevels[currentMojoLevelIndex];
        
        [BoxGroup("MOJO")][SerializeField] [ReadOnly] private int currentMojoLevelIndex = 0;
        public int GetCurrentMojoLevelIndex => currentMojoLevelIndex;
        [BoxGroup("MOJO")][SerializeField] [ReadOnly] private float currentDamageInCombo;
        [BoxGroup("MOJO")][SerializeField] private float comboReduceCooldown = 1;
        [BoxGroup("MOJO")][SerializeField] [ReadOnly] private float currentComboReduceCooldown;
        
        [BoxGroup("MOJO")] [SerializeField] [ReadOnly] private float currentMojoDamageScaler = 1;
        public float GetCurrentMojoDamageScaler => currentMojoDamageScaler;
        [Serializable]
        public class MojoLevel
        {
            [Header("for level up")]
            public int minDamage;
            [Header("Doesnt count cooldown")] [SerializeField] public float timeToDrainFull;
            [ReadOnly] public float comboReduceSpeed;

            [Range(0.01f, 10)] public float damageScaler = 1; 
            
            [Header("equipment set for the level")]
            public Tool HeadTool;
            public Tool HandLTool;
            public Tool HandRTool;
            public Tool BodyTool;
            public Tool LegsTool;
        }

        [Header("UI")] 
        public Text currentGoldText;
        public Text comboLevelText;
        public Text dmgFeedbackText;
        public Image comboBar;
        
        public AudioSource scoreAddedAu;

        public Transform addedScoreFeedbackTransform;
        public Text addedScoreFeedbackText;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        
            if (PlayerPrefs.HasKey("currentScore"))
            {
                CurrentGold = PlayerPrefs.GetInt("currentScore");
                currentGoldText.text = "DOLAS: " + CurrentGold;
            }

            addedScoreFeedbackTransform.transform.localScale = new Vector3(1, 0, 1);
            UpdateMojoLevelUi();
        }


        private void OnValidate()
        {
            for (var index = 0; index < _mojoLevels.Count; index++)
            {
                float localZeroMojo = 0;
                if (index > 0)
                    localZeroMojo = _mojoLevels[index - 1].minDamage;
                
                var mojoLevel = _mojoLevels[index];
                float amountToDrain = mojoLevel.minDamage - localZeroMojo;

                if (mojoLevel.timeToDrainFull > 0)
                {
                    mojoLevel.comboReduceSpeed = amountToDrain / mojoLevel.timeToDrainFull; 
                }
                else
                {
                    mojoLevel.comboReduceSpeed = 0;
                }
            }
        }

        void Update()
        {
            if (currentComboReduceCooldown > 0)
            {
                currentComboReduceCooldown -= Time.deltaTime;
            }
            else
            {
                if (currentDamageInCombo > 0)
                {
                    currentDamageInCombo -= Time.deltaTime * _mojoLevels[currentMojoLevelIndex].comboReduceSpeed;
                }

                if (currentMojoLevelIndex > 0)
                {
                    if (currentDamageInCombo <= _mojoLevels[currentMojoLevelIndex - 1].minDamage)
                    {
                        // dont drop levels
                        currentDamageInCombo = _mojoLevels[currentMojoLevelIndex - 1].minDamage + 1;
                        //DecreaseMojoLevel();
                    }
                }
                if (currentDamageInCombo < 0)
                    currentDamageInCombo = 0;
            }
            
            comboBar.fillAmount = GetComboFillAmount();
        }

        public void RegisterDamage(int damage)
        {
            currentDamageInCombo += damage;
            currentComboReduceCooldown = comboReduceCooldown; 
            if (damageFeedbackAnimateCoroutine != null)
                StopCoroutine(damageFeedbackAnimateCoroutine);
            
            damageFeedbackAnimateCoroutine = StartCoroutine(DamageFeedbackAnimate(damage));


            if (currentDamageInCombo >= _mojoLevels[currentMojoLevelIndex].minDamage)
                IncreaseMojoLevel();
        }

        public void IncreaseMojoLevel()
        {
            var newMojo = currentMojoLevelIndex + 1;
            
            if (newMojo >= _mojoLevels.Count)
            {
                currentMojoDamageScaler += 0.5f;
                newMojo = 0;
            }
            
            //newMojo = Mathf.Clamp(newMojo, 0, _mojoLevels.Count-1);
            if (newMojo == currentMojoLevelIndex)
                return;

            currentMojoLevelIndex = newMojo;
            var prevIndex = currentMojoLevelIndex - 1;
            if (prevIndex < 0) prevIndex = 0;
            currentDamageInCombo = _mojoLevels[prevIndex].minDamage + 1;
            ItemFoundSound();
            UpdateMojoLevelUi();
            Game.LocalPlayer.Health.RestoreHealth(0.3f);
            UpdateMojoInventory();
        }

        public void ResetMojo()
        {
            currentMojoLevelIndex = 0;
            currentMojoDamageScaler = 1;
            currentDamageInCombo = 0;
            
            ItemFoundSoundLowPitch();
            UpdateMojoLevelUi();
            UpdateMojoInventory();
        }
        public void UpdateMojoInventory()
        {
            Game.LocalPlayer.Inventory.DropAll(false, false);
            
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevelIndex].HeadTool);
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevelIndex].HandLTool);
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevelIndex].HandRTool);
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevelIndex].BodyTool);
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevelIndex].LegsTool);
        }

        void UpdateMojoLevelUi()
        {
            string scalerString = String.Empty;
            if (currentMojoDamageScaler > 1)
                scalerString = "X" + currentMojoDamageScaler + " DMG";
            comboLevelText.text = "MOJO " + currentMojoLevelIndex + scalerString;
        }

        [Button]
        public void GetCurrentComboFillAmount()
        {
            Debug.LogError("GetCurrentComboFillAmount " + GetComboFillAmount());
        }

        float GetComboFillAmount()
        {
            float max = _mojoLevels[currentMojoLevelIndex].minDamage;
            float min = 0;
            if (currentMojoLevelIndex > 0)
                min = _mojoLevels[currentMojoLevelIndex - 1].minDamage;
            var targetMax = max - min;
            var targetCurrent = currentDamageInCombo - min;
            
            return targetCurrent/targetMax;
        }
        
        public void RegisterAction(ScoringActionType scoringAction, float addToCooldown = 5)
        {
            return;
        }

        private Coroutine damageFeedbackAnimateCoroutine;
        IEnumerator DamageFeedbackAnimate(int dmg)
        {
            dmgFeedbackText.text = "+" + dmg + "DMG";
            float t = 0;
            while (t < 0.1f)
            {
                dmgFeedbackText.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.5f, t/0.1f);
                t += Time.deltaTime;
                yield return null;
            }

            t = 0;
            while (t < 0.2f)
            {
                dmgFeedbackText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t/0.2f);
                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(3);
            t = 0;
            while (t < 1f)
            {
                dmgFeedbackText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                t += Time.deltaTime;
                yield return null;
            }
        }

        void ItemFoundSound()
        {
            scoreAddedAu.pitch = Random.Range(0.9f, 1.1f);
            scoreAddedAu.Play();
        }
        void ItemFoundSoundLowPitch()
        {
            scoreAddedAu.pitch = Random.Range(0.2f, 0.4f);
            scoreAddedAu.Play();
        }
        
        public void AddGold(int amount)
        {
            if (amount == 0)
                return;
            
            CurrentGold += amount;
            string text;
            if (amount > 0)
                text = "+" + amount;
            else
                text = amount.ToString();
            
            CustomTextMessage(text);
        
            PlayerPrefs.SetInt("currentScore", CurrentGold);
            currentGoldText.text = "DOLAS: " + CurrentGold;
            PlayerPrefs.Save();
        }

        public void CustomTextMessage(string text)
        {
            addedScoreFeedbackText.text = text;
            
            if (animateAddedScoreFeedback != null)
                StopCoroutine(animateAddedScoreFeedback);
        
            StartCoroutine(AnimateAddedScoreFeedback());
        }

        private Coroutine animateAddedScoreFeedback;
        IEnumerator AnimateAddedScoreFeedback()
        {
            for (int i = 0; i < 5; i++)
            {
                addedScoreFeedbackTransform.transform.localScale = new Vector3(Random.Range(0.75f, 1.5f),
                    Random.Range(0.75f, 1.5f), Random.Range(0.75f, 1.5f));
                yield return new WaitForSeconds(0.1f);
            }
            addedScoreFeedbackTransform.transform.localScale = Vector3.one;

            float t = 0;
            while (t < 3)
            {
                t += Time.deltaTime;
                addedScoreFeedbackTransform.transform.localScale = new Vector3(1, Mathf.Lerp(1,0, t/3), 1);
                yield return null;
            }
        }

        public void RemoveScore(int amount)
        {
            CurrentGold -= amount;
        
            PlayerPrefs.SetInt("currentScore", CurrentGold);
            currentGoldText.text = "DOLAS: " + CurrentGold;
            PlayerPrefs.Save();
        }
        public void UpdateGold()
        {
            currentGoldText.text = "DOLAS: " + CurrentGold;
        }

    }
}

[Serializable]
public enum ScoringActionType
{
    NULL,
    KillRangedIdle,
    KillRangedOnMove,
    KillRangedOnRun,
    KillRangedOnJump,
    KillMeleeIdle,
    KillMeleeOnMove,
    KillMeleeOnRun,
    KillMeleeOnJump,
    KillLeaningRangedIdle,
    KillLeaningRangedOnMove,
    KillLeaningRangedOnRun,
    KillLeaningRangedOnJump,
    KillLeaningMeleeIdle,
    KillLeaningMeleeOnMove,
    KillLeaningMeleeOnRun,
    KillLeaningMeleeOnJump,
    KillExplosion,
    TileDestroyed,
    EnemyBumped,
    BarrelBumped,
    PropBumped
}