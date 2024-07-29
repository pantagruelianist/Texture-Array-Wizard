using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//By C.S. Woodward in 2022, prepared for public use in 2024. 
//Version 1.65, added scroll for array members and some cosmetic branding changes as I wanted to figure out how to do them
//added remove-add functionality within 1.65 as I forgot that I had that in my own notes. 
//future features will include member reordering, size forcing, further parameters to textures, normal map array settings. 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextureArrayWizard : EditorWindow
{
    private Texture2DArray textureArray; // the texture array... 
    private int size = 4; //default member-count
    private int textureWidth = 256; //default width
    private int textureHeight = 256; //default height
    private TextureFormat textureFormat = TextureFormat.RGBA32; //default go for texture format... 
    private Texture2D[] textures; //textures from the textureArray
    private Vector2 scrollPos;
    private Texture2D bigShoeHeader;
    private Texture2D parameterImg;

    [MenuItem("Tools/Big Shoe Development Suite/Texture Array Wizard")]
    public static void ShowWindow()
    {
        GetWindow<TextureArrayWizard>("Texture Array Wizard");
    }

    private void OnEnable()
    {
        
        bigShoeHeader = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/BigShoeTools/HeaderForTextureArrayWizard.png");
        parameterImg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/BigShoeTools/texturearrayparameters.png");
    }

    private void OnGUI()
    {
        GUIStyle imageStyle = new GUIStyle();
        imageStyle.margin = new RectOffset(0, 0, 0, 0);
        imageStyle.padding = new RectOffset(0, 0, 0, 0);

        if (bigShoeHeader != null)
        {
            GUILayout.Box(bigShoeHeader, imageStyle);
        }
        if (parameterImg != null)
        {
            GUILayout.Box(parameterImg, imageStyle);
        }

        EditorGUI.BeginDisabledGroup(textureArray != null);
        size = EditorGUILayout.IntField("Array Size", size);
        textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);
        textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", textureFormat);
        EditorGUI.EndDisabledGroup();

        GUI.backgroundColor = new Color32(48, 211, 223, 255);
        if (GUILayout.Button("Create New Texture2DArray"))
        {
            CreateNewTexture2DArray();
        }

        GUI.backgroundColor = Color.white;

        GUILayout.Label("Load Existing Texture2DArray", EditorStyles.boldLabel);
        var newTextureArray = (Texture2DArray)EditorGUILayout.ObjectField("Texture2DArray", textureArray, typeof(Texture2DArray), false);

        if (newTextureArray != textureArray)
        {
            textureArray = newTextureArray;
            if (textureArray != null)
            {
                size = textureArray.depth;
                textureWidth = textureArray.width;
                textureHeight = textureArray.height;
                textureFormat = textureArray.format;

                textures = new Texture2D[size];
                for (int i = 0; i < size; i++)
                {
                    textures[i] = new Texture2D(textureWidth, textureHeight, textureFormat, false);
                    Graphics.CopyTexture(textureArray, i, textures[i], 0);
                }
            }
        }

        if (textureArray != null)
        {
            if (textures == null || textures.Length != size)
            {
                textures = new Texture2D[size];
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add a Texture Slot"))
            {
                AddTexture();
            }
            if (GUILayout.Button("Remove a Texture Slot"))
            {
                RemoveTexture();
            }
            GUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            for (int i = 0; i < size; i++)
            {
                textures[i] = (Texture2D)EditorGUILayout.ObjectField("Texture " + i, textures[i], typeof(Texture2D), false);
            }
            EditorGUILayout.EndScrollView();

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Apply Textures"))
            {
                ApplyTextures();
            }

            GUI.backgroundColor = Color.white;
        }

        GUILayout.Space(20);

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Reset"))
        {
            ResetFields();
        }

        GUI.backgroundColor = Color.white;
    }

    private void CreateNewTexture2DArray()
    {
        textureArray = new Texture2DArray(textureWidth, textureHeight, size, textureFormat, false);
        textures = new Texture2D[size];

        for (int i = 0; i < size; i++)
        {
            textures[i] = new Texture2D(textureWidth, textureHeight, textureFormat, false);
            textures[i].filterMode = FilterMode.Point; // point filter... we'll deal with that later... just need to avoid mips rn 
            textures[i].wrapMode = TextureWrapMode.Repeat;
        }

        EditorUtility.SetDirty(this);
        Repaint();
    }

    private void ApplyTextures()
    {
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] != null)
            {
                SetTextureReadable(textures[i]);
                Graphics.CopyTexture(textures[i], 0, textureArray, i);
            }
        }
        EditorUtility.SetDirty(textureArray);
        AssetDatabase.SaveAssets();
    }

    private void SetTextureReadable(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.isReadable = true;
            importer.mipmapEnabled = false; // Ensure mipmaps are disabled
            AssetDatabase.ImportAsset(path);
        }
    }


    private void AddTexture()
    {
        size++;
        System.Array.Resize(ref textures, size);
        textures[size - 1] = new Texture2D(textureWidth, textureHeight, textureFormat, false);
        ResizeTextureArray();
    }

    private void RemoveTexture()
    {
        if (size > 1)
        {
            size--;
            System.Array.Resize(ref textures, size);
            ResizeTextureArray();
        }
    }

    private void ResizeTextureArray()
    {
        Texture2DArray newTextureArray = new Texture2DArray(textureWidth, textureHeight, size, textureFormat, false);
        for (int i = 0; i < Mathf.Min(textureArray.depth, size); i++)
        {
            Graphics.CopyTexture(textureArray, i, newTextureArray, i);
        }
        textureArray = newTextureArray;
        EditorUtility.SetDirty(textureArray);
        AssetDatabase.SaveAssets();
    }

    private void ResetFields()
    {
        textureArray = null;
        size = 4;
        textureWidth = 256;
        textureHeight = 256;
        textureFormat = TextureFormat.RGBA32;
        textures = null;

        EditorUtility.SetDirty(this);
        Repaint();
    }


}










