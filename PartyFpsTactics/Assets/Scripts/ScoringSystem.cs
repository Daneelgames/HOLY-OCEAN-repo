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
    public int currentScore = 0;
    public int currentScoreInCombo = 0;
    public int currentMultiplier = 1;
    [Range(1, 10)] public float scoreCooldownMax = 5;
    public float scoreCooldownCurrent = 0;

    [Header("UI")] 
    public Text currentScoreText;
    public Text comboText;
    public Image scoreCooldownFeedback;
    public Text actionNameFeedbackPrefab;
    public Transform actionNameFeedbacksFolder;
    public Vector2 actionNameFeedbackMinMaxX = new Vector2(-500, 500);
    public Vector2 actionNameFeedbackMinMaxY = new Vector2(-500, 500);

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterAction(ScoringActionType scoringAction)
    {
        for (int i = 0; i < Scores.Count; i++)
        {
            var actionScore = Scores[i];
            if (actionScore.scoringActionType == scoringAction)
            {
                // ACTION FEEDBACK
                var newActionName = Instantiate(actionNameFeedbackPrefab, actionNameFeedbacksFolder);
                newActionName.text = scoringAction.ToString();
                newActionName.rectTransform.anchoredPosition = new Vector2(Random.Range(actionNameFeedbackMinMaxY.x, actionNameFeedbackMinMaxY.y), Random.Range(actionNameFeedbackMinMaxY.x, actionNameFeedbackMinMaxY.y));
                Destroy(newActionName.gameObject,3);
                
                if (scoreCooldownCoroutine == null)
                    scoreCooldownCoroutine = StartCoroutine(ScoreCooldown());
                else
                    scoreCooldownCurrent = scoreCooldownMax;
                
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
        currentScore += currentScoreInCombo * currentMultiplier;
        currentScoreText.text = "SCORE: " + currentScore;
        comboText.text = String.Empty;
        currentScoreInCombo = 0;
        currentMultiplier = 1;
        scoreCooldownCoroutine = null;
    }

    public void CooldownToZero()
    {
        scoreCooldownCurrent = 0;
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