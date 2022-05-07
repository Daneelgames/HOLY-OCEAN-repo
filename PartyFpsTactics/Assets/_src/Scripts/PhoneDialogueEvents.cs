using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhoneDialogueEvents : MonoBehaviour
{
    [Header("Этот класс только запускает диалоги")]
    public static PhoneDialogueEvents Instance;
    private bool inCutScene = false;
    public bool CanAnswer => !playerAnswered;
    public bool InCutScene => inCutScene;
    private bool playerAnswered = false;
    public HealthController currentTalknigNpc;
    
    enum LastPlayerAnswer
    {
        Negative, Positive
    }
    private LastPlayerAnswer _lastPlayerAnswer = LastPlayerAnswer.Negative;
    void Awake()
    {
        Instance = this;
    }

    public void RunNpcDialogueCutscene(Dialogue dialogue, HealthController npcHc, InteractiveObject destroyInteractorAfterDialogueCompleted, int scoreToAddOnDialogueCompleted, bool setNextLevelOnCompletion)
    {
        playerAnswered = false;
        inCutScene = false;
        currentTalknigNpc = null;
            
        if (NpcDialogueCutsceneCoroutine != null)
            StopCoroutine(NpcDialogueCutsceneCoroutine);
        
        NpcDialogueCutsceneCoroutine = StartCoroutine(NpcDialogueCutscene(dialogue, npcHc, destroyInteractorAfterDialogueCompleted, scoreToAddOnDialogueCompleted, setNextLevelOnCompletion));
    }

    private Coroutine NpcDialogueCutsceneCoroutine;
    IEnumerator NpcDialogueCutscene(Dialogue dialogue, HealthController npcHc, InteractiveObject destroyInteractorAfterDialogueCompleted, int scoreToAddOnDialogueCompleted, bool setNextLevelOnCompletion)
    {
        currentTalknigNpc = npcHc;
        inCutScene = true;
        //npcHc.IsImmortal = true;
        //Player.Health.IsImmortal = true;
        DialogueWindowInterface.Instance.ToggleDialogueWindow(true, npcHc);
        for (int i = 0; i < dialogue.phrases.Count; i++)
        {;
            yield return null;
            
            var phrase = dialogue.phrases[i];
            if (!Input.GetKey(KeyCode.Tab))
                yield return new WaitForSeconds(phrase.delayIn);
            DialogueWindowInterface.Instance.NewMessage(phrase.messengerName, phrase.messageText, true);

            if (!phrase.waitForPlayerAnswer)
            {
                continue;
            }

            playerAnswered = false;
            DialogueWindowInterface.Instance.TogglePlayerAnswerButtons(true);
            while (!playerAnswered)
            {
                yield return null;
            }

            DialogueWindowInterface.Instance.TogglePlayerAnswerButtons(false);

            if (_lastPlayerAnswer == LastPlayerAnswer.Positive && phrase.answerOnPositive)
            {
                if (!Input.GetKey(KeyCode.Tab))
                    yield return new WaitForSeconds(phrase.delayBeforeReactionOnPositiveAnswer);
                DialogueWindowInterface.Instance.NewMessage(phrase.messengerName, phrase.answerOnPositiveText, false);
                
                if (!Input.GetKey(KeyCode.Tab))
                    yield return new WaitForSeconds(phrase.delayAfterReactionOnPositiveAnswer);
                
                if (phrase.openShopOnPositive)
                {
                    Shop.Instance.SetToolsList(npcHc.AiShop.toolsToSell);
                    Shop.Instance.OpenShop(0);
                }
            }
            else if (_lastPlayerAnswer == LastPlayerAnswer.Negative && phrase.answerOnNegative)
            {
                if (!Input.GetKey(KeyCode.Tab))
                    yield return new WaitForSeconds(phrase.delayBeforeReactionOnNegativeAnswer);
                DialogueWindowInterface.Instance.NewMessage(phrase.messengerName, phrase.answerOnNegativeText, false);
                
                if (!Input.GetKey(KeyCode.Tab))
                    yield return new WaitForSeconds(phrase.delayAfterReactionOnNegativeAnswer);
            }

            if (i >= dialogue.phrases.Count - 1)
                DialogueWindowInterface.Instance.NewMessage(String.Empty, String.Empty, true);
        }
        
        // DIALOGUE SUCCESSGULLY COMPLETED
        if (destroyInteractorAfterDialogueCompleted)
            Destroy(destroyInteractorAfterDialogueCompleted.gameObject);
        
        //if (scoreToAddOnDialogueCompleted > 0)
        ScoringSystem.Instance.AddScore(scoreToAddOnDialogueCompleted);
        
        if (setNextLevelOnCompletion)
        {
            ProgressionManager.Instance.SetCurrentLevel(ProgressionManager.Instance.currentLevelIndex + 1);
            GameManager.Instance.StartProcScene();
        }    
        
        DialogueWindowInterface.Instance.ToggleDialogueWindow(false);
        playerAnswered = false;
        
        //Player.LookAround.SetCurrentCutsceneTargetTrasform(null);
        //Player.Health.IsImmortal = false;
        //npcHc.IsImmortal = false;
        inCutScene = false;
        
        currentTalknigNpc = null;
    }
    
    
    public void PlayerAnswered(bool positiveAnswer)
    {
        playerAnswered = true;
        if (positiveAnswer)
            _lastPlayerAnswer = LastPlayerAnswer.Positive;
        else
            _lastPlayerAnswer = LastPlayerAnswer.Negative;
    }

    public void NpcDied(HealthController hc)
    {
        if (currentTalknigNpc == null)
            return;
        
        if (currentTalknigNpc == hc)
        {
            DialogueWindowInterface.Instance.ToggleDialogueWindow(false);
        }
    }
    
    public void CloseCutscene()
    {
        playerAnswered = false;
        inCutScene = false;
        currentTalknigNpc = null;
    }
    
    IEnumerator RunSpawn(ScriptedEvent _event)
    {
        Instantiate(_event.prefabToSpawn, Vector3.zero, Quaternion.identity);
        yield return null;
    }

}
