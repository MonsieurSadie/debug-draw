using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontClearPostProcess : MonoBehaviour
{
    // A Material with the Unity shader you want to process the image with
    Material mat;
    public RenderTexture target;

    void Start()
    {
    	mat = new Material(Shader.Find("Unlit/DontClearShader"));
    	mat.SetTexture("_PrevScreenTex", target);
    	mat.SetColor("_ClearColor", Color.white);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Read pixels from the source RenderTexture, apply the material, copy the updated results to the destination RenderTexture
        Graphics.Blit(src, dest, mat);
    }
}
