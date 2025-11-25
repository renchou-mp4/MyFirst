using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;

public class UIGroupHelper : UIGroupHelperBase
{
    private Canvas  _canvas;
    
    public override void SetDepth(int depth)
    {
    }

    private void Awake()
    {
        _canvas = this.GetOrAddComponent<Canvas>();
    }

    private void Start()
    {
        int a = 1;
        a++;
    }
}
