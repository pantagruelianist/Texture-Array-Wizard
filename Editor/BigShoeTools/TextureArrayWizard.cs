using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//By C.S. Woodward in 2022, prepared for public use in 2024. 
//Version 1.65, added scroll for array members and some cosmetic branding changes as I wanted to figure out how to do them
//future features will include member reordering, size forcing, further parameters to textures, normal map array settings. 

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
        // Load the header and title images from the Assets folder
        bigShoeHeader = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/BigShoeTools/HeaderForTextureArrayWizard.png");
        parameterImg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/BigShoeTools/texturearrayparameters.png");
    }

    private void OnGUI()
    {
        GUIStyle imageStyle = new GUIStyle();
        imageStyle.margin = new RectOffset(0, 0, 0, 0);
        imageStyle.padding = new RectOffset(0, 0, 0, 0);

        // set up in an if just in case you, the knucklehead end-user decide to not keep the shit where it needs to be. 
        if (bigShoeHeader != null)
        {
            GUILayout.Box(bigShoeHeader, imageStyle);
        }
        if (parameterImg != null)
        {
            GUILayout.Box(parameterImg, imageStyle);
        }

        // texture properties... 
        size = EditorGUILayout.IntField("Array Size", size);
        textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);
        textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", textureFormat);

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
                    //do a copy from texture array... 
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
        }

        // reset with new tex array... 
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

    //force readable because I know knuckleheads be just dumping in texture willy nilly... 
    private void SetTextureReadable(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path);
        }
    }

    private void ResetFields()
    {
        textureArray = null;
        size = 4;
        textureWidth = 256;
        textureHeight = 256;
        textureFormat = TextureFormat.RGBA32;
        textures = null;

        // reset the whole util... 
        EditorUtility.SetDirty(this);
        Repaint();
    }
}









