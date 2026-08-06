using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 循环列表Item数据类
/// </summary>
public class LoopListItem : MonoBehaviour
{
    private RectTransform _cacheTransform;

    public RectTransform CacheTransform
    {
        get
        {
            if (_cacheTransform == null)
            {
                _cacheTransform = gameObject.GetComponent<RectTransform>();
            }
            return _cacheTransform;
        }
    }

    public LoopListView ParentView { get; set; }

    public int Index { get; set; }

    public float Height => CacheTransform.rect.height;

    public float Width => CacheTransform.rect.width;

    public float Space { get; set; }

    public string PrefabName { get; set; }

    public float PosOffset { get; set; }

    public int FrameCountOnCreate { get; set; }

    public float TopY
    {
        get
        {
            if (ParentView.LoopListDir == ELoopListDir.TopToBottom)
                return CacheTransform.anchoredPosition3D.y;
            if (ParentView.LoopListDir == ELoopListDir.BottomToTop)
                return CacheTransform.anchoredPosition3D.y + CacheTransform.rect.height;
            return 0;
        }
    }

    public float BottomY
    {
        get
        {
            if (ParentView.LoopListDir == ELoopListDir.TopToBottom)
                return CacheTransform.anchoredPosition3D.y - CacheTransform.rect.height;
            if (ParentView.LoopListDir == ELoopListDir.BottomToTop)
                return CacheTransform.anchoredPosition3D.y;
            return 0;
        }
    }

    public float LeftX
    {
        get
        {
            if (ParentView.LoopListDir == ELoopListDir.RightToLeft)
                return CacheTransform.anchoredPosition3D.x;
            if (ParentView.LoopListDir == ELoopListDir.LeftToRight)
                return CacheTransform.anchoredPosition3D.x - CacheTransform.rect.width;
            return 0;
        }
    }

    public float RightX
    {
        get
        {
            if (ParentView.LoopListDir == ELoopListDir.LeftToRight)
                return CacheTransform.anchoredPosition3D.x + CacheTransform.rect.width;
            if (ParentView.LoopListDir == ELoopListDir.RightToLeft)
                return CacheTransform.anchoredPosition3D.x;
            return 0;
        }
    }

    public float ItemSize
    {
        get => ParentView.IsVertical ? Height : Width;
    }

    public float ItemSizeWithSpace
    {
        get => ItemSize + Space;
    }
}
