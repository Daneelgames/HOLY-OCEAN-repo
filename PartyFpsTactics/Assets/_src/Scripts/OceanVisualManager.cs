using System;
using System.Collections;
using System.Collections.Generic;
using Crest;
using Sirenix.OdinInspector;
using UnityEngine;

public class OceanVisualManager : MonoBehaviour
{
    [SerializeField] private OceanRenderer _oceanRenderer;
    private Material oceanMaterial;
    [SerializeField] private List<OceanVisual> _oceanVisuals;
    [SerializeField] [ReadOnly] private int currentOceanVisual;

    [Serializable]
    public struct OceanVisual
    {
        public Color _Diffuse;
        public Color _DiffuseGrazing;
        public Color _FoamBubbleColor;
        public Color _FoamWhiteColor;
        public Color _SkyBase;
        public Color _SkyTowardsSun;
    }

    private void Start()
    {
        oceanMaterial = _oceanRenderer.OceanMaterial;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            currentOceanVisual++;

            if (currentOceanVisual >= _oceanVisuals.Count)
                currentOceanVisual = 0;

            SetOceanVisual();
        }
    }

    [Button]
    public void SetOceanVisual(int index)
    {
        currentOceanVisual = index;
        if (currentOceanVisual >= _oceanVisuals.Count || currentOceanVisual < 0)
            currentOceanVisual = 0;
        
        SetOceanVisual();
    }

    public void SetOceanVisual()
    {
        _oceanRenderer.OceanMaterial.SetColor("_Diffuse", _oceanVisuals[currentOceanVisual]._Diffuse);
        _oceanRenderer.OceanMaterial.SetColor("_DiffuseGrazing", _oceanVisuals[currentOceanVisual]._DiffuseGrazing);
        _oceanRenderer.OceanMaterial.SetColor("_FoamBubbleColor", _oceanVisuals[currentOceanVisual]._FoamBubbleColor);
        _oceanRenderer.OceanMaterial.SetColor("_FoamWhiteColor", _oceanVisuals[currentOceanVisual]._FoamWhiteColor);
        _oceanRenderer.OceanMaterial.SetColor("_SkyBase", _oceanVisuals[currentOceanVisual]._SkyBase);
        _oceanRenderer.OceanMaterial.SetColor("_SkyTowardsSun", _oceanVisuals[currentOceanVisual]._SkyTowardsSun);
    }
}
