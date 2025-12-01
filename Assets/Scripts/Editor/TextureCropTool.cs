using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureCropTool : EditorWindow
{
    //配置参数
    private bool _processAlphaChannel = true;
    private bool _preserveOriginal = true;
    private bool _processMultiple = false;
    private int _padding = 2;
    private string _outputPath = "";
    
    private Texture2D _oldTexture;
    private Texture2D _newTexture;
    
    
    [MenuItem("Tools/图片裁剪")]
    public static void ShowWindow()
    {
        GetWindow<TextureCropTool>("图片裁剪");
    }

    private void OnGUI()
    {
        DrawSettings();
        DrawAction();
    }

    private void DrawSettings()
    {
        EditorGUILayout.LabelField("图片裁剪设置",EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        _processAlphaChannel = EditorGUILayout.Toggle("Process Alpha Channel", _processAlphaChannel);
        _preserveOriginal = EditorGUILayout.Toggle("Preserve Original", _preserveOriginal);
        _padding = EditorGUILayout.IntSlider("Padding", _padding, 0, 10);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("输出目录", _outputPath);
    }

    private void DrawAction()
    {
        _oldTexture = EditorGUILayout.ObjectField("选择图片：", _oldTexture, typeof(Texture2D), false) as Texture2D;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        Rect oldRect = GUILayoutUtility.GetRect(256, 256);
        if(_oldTexture is not null)
            GUI.DrawTexture(oldRect, _oldTexture);
        
        Rect newRect = GUILayoutUtility.GetRect(256, 256);
        if(_newTexture is not null)
            GUI.DrawTexture(newRect, _newTexture);
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        Handles.BeginGUI();
        
        Handles.EndGUI();
    }

    private const float _baseRect = 256;
    private ValueTuple<float,float> _ratio;
    
    private void GetBox(Rect target,float width,float height,Rect oldRect)
    {
        float widthRatio = target.width / width;
        float heightRatio = target.height / height;
        
    }

    private Rect GetSetImageRect()
    {
        float heightRatio = _oldTexture.height / _baseRect;

        if (heightRatio <= 1)
        {
            float h = 256;
            float w = _baseRect/_oldTexture.width * _oldTexture.height;
        }
        else
        {
            float h = 256;
            float w = _oldTexture.width / _baseRect * _oldTexture.width;
        }
        
        return new Rect()
    }
}
