using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateMaterialDetails : MonoBehaviour
{
    public MeshRenderer mesh;
    private Material mat;
    public float speedX = 1;
    public float speedY = 1;

    private string mainTexString = "_MainTex";
    private void Start()
    {
        mat = mesh.material;
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetTextureOffset(mainTexString, new Vector2(speedX * Time.unscaledDeltaTime, speedY * Time.unscaledDeltaTime));
    }
}
