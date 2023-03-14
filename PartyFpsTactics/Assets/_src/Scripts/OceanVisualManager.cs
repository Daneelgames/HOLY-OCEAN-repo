using System;
using System.Collections;
using System.Collections.Generic;
using Crest;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class OceanVisualManager : MonoBehaviour
{
    [SerializeField] private OceanRenderer _oceanRenderer;
    private Material oceanMaterial;
    [SerializeField] private List<OceanVisual> _oceanVisuals;
    [SerializeField] private int currentOceanVisual;
    [SerializeField] private EnviroSky _enviroSky;
    [Serializable]
    public struct OceanVisual
    {
        public Color _Diffuse;
        public Color _DiffuseGrazing;
        public Color _FoamBubbleColor;
        public Color _FoamWhiteColor;
        public Color _SkyBase;
        public Color _SkyTowardsSun;

        public Gradient SkySimpleColor;
        public Gradient SkySimpleHorizonColor;
        public Gradient SkySimpleHorizonBackColor;
        public Gradient SkySimpleSunColor;
        public Color SkyMoonColorSunColor;
        public Color SkyMoonGlowColorSunColor;
    }

    private void Start()
    {
        return;
        oceanMaterial = _oceanRenderer.OceanMaterial;
        
        StartCoroutine(SetWorldColorsOverTime());
    }


    IEnumerator SetWorldColorsOverTime()
    {
        
        while (true)
        {
            currentOceanVisual = Random.Range(0, _oceanVisuals.Count);
        
            SetOceanVisual();
            SetSkyVisual();
            
            yield break;
            yield return new WaitForSeconds(30);
                
            
            /*
            int randomNextColorsIndex = Random.Range(0, _oceanVisuals.Count);
            float t = 0;
            float transitionTime = 30;
            var transitionColorA = HashCurrentColor();
            var transitionColorB = _oceanVisuals[randomNextColorsIndex];
            
            SetOceanVisual();
            SetSkyVisual();*/
            /*
            while (t < transitionTime)
            {
                yield return new WaitForSeconds(0.1f);
                t += 0.1f;
                var currentSmooth = t / transitionTime;
                
                _enviroSky.skySettings.simpleSkyColor = new Gradient().colorKeys .Lerp(transitionColorA.SkySimpleColor, transitionColorB.SkySimpleColor, currentSmooth);
                _enviroSky.skySettings.simpleHorizonColor = _oceanVisuals[currentOceanVisual].SkySimpleHorizonColor;
                _enviroSky.skySettings.simpleHorizonBackColor = _oceanVisuals[currentOceanVisual].SkySimpleHorizonBackColor;
                _enviroSky.skySettings.simpleSunColor = _oceanVisuals[currentOceanVisual].SkySimpleSunColor;
                _enviroSky.skySettings.moonColor = _oceanVisuals[currentOceanVisual].SkyMoonColorSunColor;
                _enviroSky.skySettings.moonGlowColor = _oceanVisuals[currentOceanVisual].SkyMoonGlowColorSunColor;
                
                _oceanRenderer.OceanMaterial.SetColor("_Diffuse", _oceanVisuals[currentOceanVisual]._Diffuse);
                _oceanRenderer.OceanMaterial.SetColor("_DiffuseGrazing", _oceanVisuals[currentOceanVisual]._DiffuseGrazing);
                _oceanRenderer.OceanMaterial.SetColor("_FoamBubbleColor", _oceanVisuals[currentOceanVisual]._FoamBubbleColor);
                _oceanRenderer.OceanMaterial.SetColor("_FoamWhiteColor", _oceanVisuals[currentOceanVisual]._FoamWhiteColor);
                _oceanRenderer.OceanMaterial.SetColor("_SkyBase", _oceanVisuals[currentOceanVisual]._SkyBase);
                _oceanRenderer.OceanMaterial.SetColor("_SkyTowardsSun", _oceanVisuals[currentOceanVisual]._SkyTowardsSun);
            }   */
        }
    }

    OceanVisual HashCurrentColor()
    {
        OceanVisual newVisual = new OceanVisual();
        newVisual._Diffuse = _oceanRenderer.OceanMaterial.GetColor("_Diffuse"); 
        newVisual._DiffuseGrazing = _oceanRenderer.OceanMaterial.GetColor("_DiffuseGrazing"); 
        newVisual._FoamBubbleColor = _oceanRenderer.OceanMaterial.GetColor("_FoamBubbleColor"); 
        newVisual._FoamWhiteColor = _oceanRenderer.OceanMaterial.GetColor("_FoamWhiteColor"); 
        newVisual._SkyBase = _oceanRenderer.OceanMaterial.GetColor("_SkyBase"); 
        newVisual._SkyTowardsSun = _oceanRenderer.OceanMaterial.GetColor("_SkyTowardsSun"); 
        
        newVisual.SkySimpleColor = _enviroSky.skySettings.simpleSkyColor; 
        newVisual.SkySimpleHorizonColor = _enviroSky.skySettings.simpleHorizonColor;
        newVisual.SkySimpleHorizonBackColor = _enviroSky.skySettings.simpleHorizonBackColor;
        newVisual.SkySimpleSunColor = _enviroSky.skySettings.simpleSunColor;
        newVisual.SkyMoonColorSunColor = _enviroSky.skySettings.moonColor;
        newVisual.SkyMoonGlowColorSunColor = _enviroSky.skySettings.moonGlowColor;
        
        return newVisual;
    }
    
    public void SetSkyVisual()
    {
        return;
        //_enviroSky.skySettings
        _enviroSky.skySettings.simpleSkyColor = _oceanVisuals[currentOceanVisual].SkySimpleColor;
        _enviroSky.skySettings.simpleHorizonColor = _oceanVisuals[currentOceanVisual].SkySimpleHorizonColor;
        _enviroSky.skySettings.simpleHorizonBackColor = _oceanVisuals[currentOceanVisual].SkySimpleHorizonBackColor;
        _enviroSky.skySettings.simpleSunColor = _oceanVisuals[currentOceanVisual].SkySimpleSunColor;
        _enviroSky.skySettings.moonColor = _oceanVisuals[currentOceanVisual].SkyMoonColorSunColor;
        _enviroSky.skySettings.moonGlowColor = _oceanVisuals[currentOceanVisual].SkyMoonGlowColorSunColor;
    }

    [Button]
    public void SetOceanVisual(int index)
    {
        return;
        currentOceanVisual = index;
        if (currentOceanVisual >= _oceanVisuals.Count || currentOceanVisual < 0)
            currentOceanVisual = 0;
        
        SetOceanVisual();
        SetSkyVisual();
    }

    public void SetOceanVisual()
    {
        return;
        _oceanRenderer.OceanMaterial.SetColor("_Diffuse", _oceanVisuals[currentOceanVisual]._Diffuse);
        _oceanRenderer.OceanMaterial.SetColor("_DiffuseGrazing", _oceanVisuals[currentOceanVisual]._DiffuseGrazing);
        _oceanRenderer.OceanMaterial.SetColor("_FoamBubbleColor", _oceanVisuals[currentOceanVisual]._FoamBubbleColor);
        _oceanRenderer.OceanMaterial.SetColor("_FoamWhiteColor", _oceanVisuals[currentOceanVisual]._FoamWhiteColor);
        _oceanRenderer.OceanMaterial.SetColor("_SkyBase", _oceanVisuals[currentOceanVisual]._SkyBase);
        _oceanRenderer.OceanMaterial.SetColor("_SkyTowardsSun", _oceanVisuals[currentOceanVisual]._SkyTowardsSun);
    }
}
