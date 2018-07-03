using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Screen Transition")]
public class TeleportScreenTransitionImageEffect : MonoBehaviour {

    /// Provides a shader property that is set in the inspector
    /// and a material instantiated from the shader
    public Shader shader;

    [Range(0, 1.0f)]
    public float maskValue;
    public Color maskColor = Color.black;
    public Texture2D maskTexture;
    public bool maskInvert;

    private Material m_Material;
    private bool m_maskInvert;

    public float maskColorAlpha{
        get{
            return maskColor.a; 
        }
        set{
            maskColor.a = value; 
        }
    }

    Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_Material;
        }
    }

	// Use this for initialization
	void Start () {
        // Disable if we don't support image effects
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        shader = Shader.Find("NightAtTheMuseum/ScreenTransitionImageEffect");

        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (shader == null || !shader.isSupported)
        {
            enabled = false;
        }
	}
	
    void OnDisable()
    {
        if (m_Material)
        {
            DestroyImmediate(m_Material);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!enabled)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetColor("_MaskColor", maskColor);
        material.SetFloat("_MaskValue", maskValue);
        material.SetTexture("_MainTex", source);
        material.SetTexture("_MaskTex", maskTexture);

        if (material.IsKeywordEnabled("INVERT_MASK") != maskInvert)
        {
            if (maskInvert)
                material.EnableKeyword("INVERT_MASK");
            else
                material.DisableKeyword("INVERT_MASK");
        }

        Graphics.Blit(source, destination, material);
    }
}
