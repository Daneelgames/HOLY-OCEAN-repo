using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlatEventManager : MonoBehaviour
{
    private int currentLevel = 0;
    public List<LevelEvents> LevelEventsList;
    public List<Transform> spawnPlaces;

    public static FlatEventManager Instance;
    private bool playerAnswered = false;

    public bool CanAnswer => !playerAnswered;


enum LastPlayerAnswer
    {
        Negative, Positive
    }

    private LastPlayerAnswer _lastPlayerAnswer = LastPlayerAnswer.Negative;

    void Start()
    {
        Instance = this;
        StartCoroutine(RunEvents(LevelEventsList[Mathf.Clamp(currentLevel, 0,LevelEventsList.Count-1)]));
        // test version
    }

    IEnumerator RunEvents(LevelEvents levelEvents)
    {
        for (int i = 0; i < levelEvents.eventsList.Count; i++)
        {
            var _event = levelEvents.eventsList[i];

            yield return new WaitForSeconds(_event.delayIn);
            
            switch (_event.scriptedEventType)
            {
                case ScriptedEventType.StartDialogue:
                    yield return StartCoroutine(RunDialogue(_event.dialogueToStart));
                    break;
                case ScriptedEventType.SpawnObject:
                    yield return StartCoroutine(RunSpawn(_event));
                    break;
                case ScriptedEventType.AddScore:
                    ScoringSystem.Instance.AddScore(_event.scoreToAdd);
                    break;
                case ScriptedEventType.PlaySound:
                    var newGo = new GameObject("Sound " + _event.soundToPlay.name);
                    var au = newGo.AddComponent<AudioSource>();
                    au.pitch = Random.Range(_event.auPitchMinMax.x, _event.auPitchMinMax.y);
                    au.clip = _event.soundToPlay;
                    au.playOnAwake = false;
                    au.loop = false;
                    au.Play();
                    Destroy(newGo, _event.soundToPlay.length);
                    break;
            }
            yield return new WaitForSeconds(_event.delayOut);
        }
    }

    IEnumerator RunDialogue(PhoneDialogue dialogue)
    {
        for (int i = 0; i < dialogue.phrases.Count; i++)
        {
            var phrase = dialogue.phrases[i];
            yield return new WaitForSeconds(phrase.delayIn);
            PhoneInterface.Instance.NewMessage(phrase.messengerName,phrase.messageText, true);

            if (!phrase.waitForPlayerAnswer)
                continue;
            
            playerAnswered = false;
            PhoneInterface.Instance.TogglePlayerAnswerButtons(true);
            while (!playerAnswered)
            {
                yield return null;
            }
            PhoneInterface.Instance.TogglePlayerAnswerButtons(false);
            
            if (_lastPlayerAnswer == LastPlayerAnswer.Positive && phrase.answerOnPositive)
            {
                yield return new WaitForSeconds(phrase.delayBeforeReactionOnPositiveAnswer);
                PhoneInterface.Instance.NewMessage(phrase.messengerName, phrase.answerOnPositiveText, false);
                yield return new WaitForSeconds(phrase.delayAfterReactionOnPositiveAnswer);
            }
            else if (_lastPlayerAnswer == LastPlayerAnswer.Negative && phrase.answerOnNegative)
            {
                yield return new WaitForSeconds(phrase.delayBeforeReactionOnNegativeAnswer);
                PhoneInterface.Instance.NewMessage(phrase.messengerName, phrase.answerOnNegativeText, false);
                yield return new WaitForSeconds(phrase.delayAfterReactionOnNegativeAnswer);
            }
            
            if (i >= dialogue.phrases.Count - 1)
                PhoneInterface.Instance.NewMessage(String.Empty, String.Empty, true);
        }
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
        Transform targetSpawnTransform;
        List<Transform> availableTransforms = new List<Transform>();
        for (int i = 0; i < spawnPlaces.Count; i++)
        {
            if (!GameManager.Instance.IsPositionInPlayerFov(spawnPlaces[i].position))
            {
                availableTransforms.Add(spawnPlaces[i]);
            }
        }

        if (availableTransforms.Count > 0)
            targetSpawnTransform = availableTransforms[Random.Range(0, availableTransforms.Count)];
        else
            targetSpawnTransform = spawnPlaces[Random.Range(0, spawnPlaces.Count)];

        Instantiate(_event.prefabToSpawn, targetSpawnTransform.position, targetSpawnTransform.rotation);
        yield return null;
    }
}