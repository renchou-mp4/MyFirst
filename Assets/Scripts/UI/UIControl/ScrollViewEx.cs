using System.Collections.Generic;
using GameFramework.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public enum ScrollDir
{
    Horizontal,
    Vertical
}

[RequireComponent(typeof(ScrollRect))]
public class ScrollViewEx : SerializedMonoBehaviour
{
    
}