using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ScoringSystem : MonoBehaviour
{
    public static ScoringSystem Instance;

    // каждый килл дает множитель
    // вид килла определяет количество очков
   

    public List<ActionScore> Scores;
    int currentScore = 0;

    public int CurrentScore
    {
        get => currentScore;
        set => currentScore = value;
    }

    public int currentScoreInCombo = 0;
    public int currentMultiplier = 1;
    [Range(1, 10)] public float scoreCooldownMax = 5;
    public float scoreCooldownCurrent = 0;

    [Header("UI")] 
    public Text currentScoreText;
    public Text comboText;
    public Text actionNameText;
    public Image scoreCooldownFeedback;

    private void Awake()
    {
        Instance = this;
        
        if (PlayerPrefs.HasKey("currentScore"))
        {
            CurrentScore = PlayerPrefs.GetInt("currentScore");
            currentScoreText.text = "SCORE: " + CurrentScore;
        }

        if (CurrentScore < 0)
            CurrentScore = 0;
    }

    public void RegisterAction(ScoringActionType scoringAction, float addToCooldown = 5)
    {
        if (PlayerMovement.Instance.hc.health <= 0)
            return;
        
        for (int i = 0; i < Scores.Count; i++)
        {
            var actionScore = Scores[i];
            if (actionScore.scoringActionType == scoringAction)
            {
                // ACTION FEEDBACK
                actionNameText.text = scoringAction.ToString();
                StartCoroutine(AnimateActionNameFeedback());
                
                if (scoreCooldownCoroutine == null)
                    scoreCooldownCoroutine = StartCoroutine(ScoreCooldown());
                else
                    scoreCooldownCurrent = Mathf.Clamp(scoreCooldownCurrent + addToCooldown, 0, scoreCooldownMax);
                
                currentScoreInCombo += actionScore.score;
                
                if (MultiplyAction(actionScore.scoringActionType))
                    currentMultiplier++;

                
                string multiplierString = String.Empty; 
                if (currentMultiplier > 0)
                    multiplierString = " * " + currentMultiplier;

                comboText.text = "COMBO: " + currentScoreInCombo + multiplierString;
                
                break;
            }
        }
    }

    IEnumerator AnimateActionNameFeedback()
    {
        float t = 0;
        while (t < 0.1f)
        {
            actionNameText.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.5f, t/0.1f);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0;
        while (t < 0.2f)
        {
            actionNameText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t/0.2f);
            t += Time.deltaTime;
            yield return null;
        }
    }
    
    private Coroutine scoreCooldownCoroutine;
    IEnumerator ScoreCooldown()
    {
        scoreCooldownCurrent = scoreCooldownMax;
        while (scoreCooldownCurrent > 0)
        {
            scoreCooldownCurrent -= Time.deltaTime;
            scoreCooldownFeedback.fillAmount = scoreCooldownCurrent / scoreCooldownMax;
            yield return null;
        }

        scoreCooldownFeedback.fillAmount = 0;
        CurrentScore += currentScoreInCombo * currentMultiplier;
        PlayerPrefs.SetInt("currentScore", CurrentScore);
        PlayerPrefs.Save();
        currentScoreText.text = "SCORE: " + CurrentScore;
        actionNameText.text = String.Empty;
        comboText.text = String.Empty;
        currentScoreInCombo = 0;
        currentMultiplier = 1;
        scoreCooldownCoroutine = null;
    }

    public void RemoveScore(int amount)
    {
        CurrentScore -= amount;
        
        PlayerPrefs.SetInt("currentScore", CurrentScore);
        PlayerPrefs.Save();
    }
    public void CooldownToZero()
    {
        scoreCooldownCurrent = 0;
    }
    public void UpdateScore()
    {
        scoreCooldownCurrent = 0;
        currentScoreText.text = "SCORE: " + CurrentScore;
    }

    bool MultiplyAction(ScoringActionType action)
    {
        switch (action)
        {
            case ScoringActionType.KillExplosion:
                return true;
            case ScoringActionType.KillRangedIdle:
                return true;
            case ScoringActionType.KillRangedOnMove:
                return true;
            case ScoringActionType.KillRangedOnRun:
                return true;
            case ScoringActionType.KillRangedOnJump:
                return true;
            case ScoringActionType.KillMeleeIdle:
                return true;
            case ScoringActionType.KillMeleeOnMove:
                return true;
            case ScoringActionType.KillMeleeOnRun:
                return true;
            case ScoringActionType.KillMeleeOnJump:
                return true;
            case ScoringActionType.KillLeaningRangedIdle:
                return true;
            case ScoringActionType.KillLeaningRangedOnMove:
                return true;
            case ScoringActionType.KillLeaningRangedOnRun:
                return true;
            case ScoringActionType.KillLeaningRangedOnJump:
                return true;
            case ScoringActionType.KillLeaningMeleeIdle:
                return true;
            case ScoringActionType.KillLeaningMeleeOnMove:
                return true;
            case ScoringActionType.KillLeaningMeleeOnRun:
                return true;
            case ScoringActionType.KillLeaningMeleeOnJump:
                return true;
        }

        return false;
    }
}

[Serializable]
public class ActionScore
{
    public ScoringActionType scoringActionType;
    public int score = 100;
}

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
    BarrelBumped
}