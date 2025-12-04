
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollViewEx : SerializedMonoBehaviour
{
     public Dictionary<int,IScrollViewItem> _PrefabItems = new ();
}
