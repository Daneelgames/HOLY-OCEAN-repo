using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelTitlesManager : MonoBehaviour
{
    public Animator levelTitlesAnim;
    public float levelNameTime = 4;
    public Text levelNameText;
    private IEnumerator Start()
    {
        levelNameText.text = String.Empty;
        
        yield return new WaitForSeconds(0.3f);
        
        float t = 0;
        while (t < levelNameTime)
        {
            string newString = GameManager.Instance.UppercaseRandomly(ProgressionManager.Instance.CurrentLevel.levelName);
            levelNameText.text = newString;
            float r = Random.Range(0.05f, 0.75f);
            t += r;
            yield return new WaitForSeconds(r);
        }
        t = 0;
        while (t < 1)
        {
            string newString = GameManager.Instance.RemoveRandomLetters(ProgressionManager.Instance.CurrentLevel.levelName);
            levelNameText.text = newString;
            t += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        levelNameText.text = String.Empty;
        

    }
}
