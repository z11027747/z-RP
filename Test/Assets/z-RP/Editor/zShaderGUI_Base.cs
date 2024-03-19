using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class zShaderGUI_Base : ShaderGUI
{
    protected MaterialEditor editor;
    protected Object[] materials;
    protected MaterialProperty[] properties;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;
    }
    
    bool HasProperty(string name) => FindProperty(name, properties, false) != null;

    bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, properties, false);
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }
        return false;
    }

    protected void SetKeyword(string keyword, bool enabled)
    {
        if (enabled)
        {
            foreach (Material m in materials)
            {
                m.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material m in materials)
            {
                m.DisableKeyword(keyword);
            }
        }
    }

    protected void SetProperty(string name, string keyword, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }
    }

    protected RenderQueue RenderQueue
    {
        set
        {
            foreach (Material m in materials)
            {
                m.renderQueue = (int)value;
            }
        }
    }

    protected bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }
        return false;
    }

    protected bool Clipping
    {
        set => SetProperty("_Clipping", "_CLIPPING", value);
    }
    protected BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float)value);
    }

    protected BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float)value);
    }

    protected bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }

}