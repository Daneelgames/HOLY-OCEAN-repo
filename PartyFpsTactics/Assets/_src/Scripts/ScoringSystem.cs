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
        [BoxGroup("MOJO")][SerializeField] [ReadOnly] private int currentMojoLevel = 0;
        public int GetCurrentMojoLevel => currentMojoLevel;
        [BoxGroup("MOJO")][SerializeField] [ReadOnly] private float currentDamageInCombo;
        [BoxGroup("MOJO")][SerializeField] private float comboReduceSpeed = 10;
        [BoxGroup("MOJO")][SerializeField] private float comboReduceCooldown = 1;
        [BoxGroup("MOJO")][SerializeField] [ReadOnly] private float currentComboReduceCooldown;
        
        [Serializable]
        public struct MojoLevel
        {
            [Header("for level up")]
            public int minDamage;
            
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
                    currentDamageInCombo -= Time.deltaTime * comboReduceSpeed;
                }

                if (currentMojoLevel > 0)
                {
                    if (currentDamageInCombo <= _mojoLevels[currentMojoLevel - 1].minDamage)
                    {
                        // dont drop levels
                        currentDamageInCombo = _mojoLevels[currentMojoLevel - 1].minDamage + 1;
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


            if (currentDamageInCombo >= _mojoLevels[currentMojoLevel].minDamage)
                IncreaseMojoLevel();
        }

        void IncreaseMojoLevel()
        {
            var newMojo = currentMojoLevel + 1;
            newMojo = Mathf.Clamp(newMojo, 0, _mojoLevels.Count-1);
            if (newMojo == currentMojoLevel)
                return;

            currentMojoLevel = newMojo;
            UpdateMojoLevelUi();
            Game.LocalPlayer.Health.RestoreHealth();
            UpdateMojoInventory();
        }
        public void DecreaseMojoLevel()
        {
            currentMojoLevel--;
            currentMojoLevel = Mathf.Clamp(currentMojoLevel, 0, _mojoLevels.Count-1);
            currentDamageInCombo = _mojoLevels[currentMojoLevel].minDamage;
            UpdateMojoLevelUi();
            UpdateMojoInventory();
        }

        public void UpdateMojoInventory()
        {
            Game.LocalPlayer.Inventory.DropAll(false, false);
            
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevel].HeadTool);
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevel].HandLTool);
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevel].HandRTool);
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevel].BodyTool);
            Game.LocalPlayer.Inventory.AddAndEquipTool(_mojoLevels[currentMojoLevel].LegsTool);
        }

        void UpdateMojoLevelUi()
        {
            comboLevelText.text = "MOJO " + currentMojoLevel;
        }

        [Button]
        public void GetCurrentComboFillAmount()
        {
            Debug.LogError("GetCurrentComboFillAmount " + GetComboFillAmount());
        }

        float GetComboFillAmount()
        {
            float max = _mojoLevels[currentMojoLevel].minDamage;
            float min = 0;
            if (currentMojoLevel > 0)
                min = _mojoLevels[currentMojoLevel - 1].minDamage;
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

        public void ItemFoundSound()
        {
            scoreAddedAu.pitch = Random.Range(0.9f, 1.1f);
            scoreAddedAu.Play();
        }
        public void ItemFoundSoundLowPitch()
        {
            scoreAddedAu.pitch = Random.Range(0.3f, 0.5f);
            scoreAddedAu.Play();
        }
        
        public void AddGold(int amount)
        {
            if (amount == 0)
                return;
            
            ItemFoundSound();
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