using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorRandomizer : MonoBehaviour
{
    [SerializeField] private Color[] colors;
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    private void Start()
    {
        var color = colors[Random.Range(0, colors.Length)];

        for (var index = 0; index < meshRenderer.materials.Length; index++)
        {
            var material = meshRenderer.materials[index];
            
            material.SetColor(BaseColor, color);
        }
    }
}
