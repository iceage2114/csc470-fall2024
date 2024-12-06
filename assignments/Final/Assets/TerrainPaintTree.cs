using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPaintTree : MonoBehaviour
{
    public Renderer meshRenderer; // Reference to the tree's mesh renderer
    public TreePrototype treePrototype; // Terrain tree prototype reference

    void Start()
    {
        // Ensure the mesh renderer is properly configured
        meshRenderer = GetComponent<Renderer>();
        
        // Optional: Configure renderer settings for terrain painting
        if (meshRenderer != null)
        {
            // Ensure the material supports terrain paint blending
            meshRenderer.material.shader = Shader.Find("Nature/Tree");
            
            // Set up proper rendering mode for terrain trees
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;
        }
    }
}