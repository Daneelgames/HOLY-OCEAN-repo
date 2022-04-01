using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink.Health;
using MrPink.PlayerSystem;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralCutscenesManager : MonoBehaviour
{
    public static ProceduralCutscenesManager Instance;
    private bool inCutScene = false;
    public bool CanAnswer => !playerAnswered;
    public bool InCutScene => inCutScene;
    private bool playerAnswered = false;
    
    enum LastPlayerAnswer
    {
        Negative, Positive
    }
    private LastPlayerAnswer _lastPlayerAnswer = LastPlayerAnswer.Negative;
    void Awake()
    {
        Instance = this;
    }

    public void RunNpcDialogueCutscene(Dialogue dialogue, List<Transform> cameraTargetTransforms, HealthController npcHc)
    {
        StartCoroutine(NpcDialogueCutscene(dialogue, cameraTargetTransforms, npcHc));
    }

    IEnumerator NpcDialogueCutscene(Dialogue dialogue, List<Transform> cameraTargetTransforms, HealthController npcHc)
    {
        inCutScene = true;
        npcHc.IsImmortal = true;
        Player.Health.IsImmortal = true;
        var camTargetsTemp = new List<Transform>(cameraTargetTransforms);
        var randomTransform = camTargetsTemp[Random.Range(0, camTargetsTemp.Count)];
        DialogueWindowInterface.Instance.ToggleDialogueWindow(true);
        for (int i = 0; i < dialogue.phrases.Count; i++)
        {
            // CHOOSE CAMERA TARGET TRANSFORM
            if (camTargetsTemp.Count <= 1)
                camTargetsTemp = new List<Transform>(cameraTargetTransforms);
            randomTransform = camTargetsTemp[Random.Range(0, camTargetsTemp.Count)];
            camTargetsTemp.Remove(randomTransform);
            Player.LookAround.SetCurrentCutsceneTargetTrasform(randomTransform);
            
            var phrase = dialogue.phrases[i];
            yield return new WaitForSeconds(phrase.delayIn);
            DialogueWindowInterface.Instance.NewMessage(phrase.messengerName, phrase.messageText, true);

            if (!phrase.waitForPlayerAnswer)
                continue;

            playerAnswered = false;
            DialogueWindowInterface.Instance.TogglePlayerAnswerButtons(true);
            while (!playerAnswered)
            {
                yield return null;
            }

            DialogueWindowInterface.Instance.TogglePlayerAnswerButtons(false);

            if (_lastPlayerAnswer == LastPlayerAnswer.Positive && phrase.answerOnPositive)
            {
                yield return new WaitForSeconds(phrase.delayBeforeReactionOnPositiveAnswer);
                DialogueWindowInterface.Instance.NewMessage(phrase.messengerName, phrase.answerOnPositiveText, false);
                yield return new WaitForSeconds(phrase.delayAfterReactionOnPositiveAnswer);
            }
            else if (_lastPlayerAnswer == LastPlayerAnswer.Negative && phrase.answerOnNegative)
            {
                yield return new WaitForSeconds(phrase.delayBeforeReactionOnNegativeAnswer);
                DialogueWindowInterface.Instance.NewMessage(phrase.messengerName, phrase.answerOnNegativeText, false);
                yield return new WaitForSeconds(phrase.delayAfterReactionOnNegativeAnswer);
            }

            if (i >= dialogue.phrases.Count - 1)
                DialogueWindowInterface.Instance.NewMessage(String.Empty, String.Empty, true);
        }

        DialogueWindowInterface.Instance.ToggleDialogueWindow(false);
        Player.LookAround.SetCurrentCutsceneTargetTrasform(null);
        Player.Health.IsImmortal = false;
        playerAnswered = false;
        npcHc.IsImmortal = false;
        inCutScene = false;
    }
    
    
    public void PlayerAnswered(bool positiveAnswer)
    {
        playerAnswered = true;
        if (positiveAnswer)
            _lastPlayerAnswer = LastPlayerAnswer.Positive;
        else
            _lastPlayerAnswer = LastPlayerAnswer.Negative;
    }
    
    
    IEnumerator RunSpawn(ScriptedEvent _event)
    {
        Instantiate(_event.prefabToSpawn, Vector3.zero, Quaternion.identity);
        yield return null;
    }

}
