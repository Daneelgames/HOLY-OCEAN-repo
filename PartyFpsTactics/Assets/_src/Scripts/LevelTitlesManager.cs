using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelTitlesManager : MonoBehaviour
{
    public Animator levelTitlesAnim;
    public float levelNameTime = 4;
    public Text levelNameText;

    public static LevelTitlesManager Instance;

    private bool play = false;
    
    private void Awake()
    {
        Instance = this;    
    }

    private void Start()
    {
        StartCoroutine(ShowIntroCoroutine());
        return;
        ShowIntro();
        Invoke(nameof(HideIntro), 1);
    }

    void ShowIntro()
    {
        StartCoroutine(ShowIntroCoroutine());
    }

    public void HideIntro()
    {
        play = false;
    }
    
    private IEnumerator ShowIntroCoroutine()
    {
        float time = 1f;
        play = true;
        levelTitlesAnim.SetBool("Intro", true);
        levelNameText.text = String.Empty;
        
        yield return new WaitForSeconds(0.3f);
        
        
        while (time > 0)
        {
            string newString = GameManager.Instance.UppercaseRandomly(ProgressionManager.Instance.CurrentLevel.levelName);
            levelNameText.text = newString;
            float r = Random.Range(0.05f, 0.75f);
            yield return new WaitForSeconds(r);
            /*
            if (GameManager.Instance.GetLevelType == GameManager.LevelType.Game && BuildingGenerator.Instance.Generated == false)
                continue;*/
            
            time -= r;
        }
        
        levelTitlesAnim.SetBool("Intro", false); 
        
        float t = 0;
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
