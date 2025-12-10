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
    private Dictionary<int, CommonScrollViewItem> _items = new();
    private Dictionary<int, Vector2> _itemSize = new(); //item大小
    private IObjectPool<CommonScrollViewItem> _itemPool;
    private ScrollRect _scrollRect;
    private ScrollDir _scrollDir = ScrollDir.Vertical;
    private float _contentTotalLength; //内容总长度
    private int _ItemCount; //item数量


    [Header("Item配置")] 
    public List<GameObject> _ItemPrefabs = new(); //item预制体
    public Vector2[] _ItemSizeTopThree = new Vector2[3]; //前3个Item大小，0为默认大小
    public bool _Interactable = true; //是否可交互

    [EnumToggleButtons]
    [ShowInInspector]
    public ScrollDir _ScrollDir //滚动方向
    {
        get => _scrollDir;
        set
        {
            _scrollDir = value;
            SetScrollDir(value);
        }
    }


    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _itemPool =GameEntry.ObjectPool.CreateSingleSpawnObjectPool<CommonScrollViewItem>("ScrollViewItemPool",100);
    }

    private void Init(int itemCount)
    {
        if (_ItemPrefabs.Count <= 0) return;
        _ItemCount = itemCount;

        //获取Item脚本和大小
        for (int i = 0; i < _ItemCount; i++)
        {
            var item = _ItemPrefabs[i].GetComponent<CommonScrollViewItem>();
            if (item == null) continue;

            item._ItemId = i;
            _items.Add(i, item);
            _itemSize.Add(i, _ItemPrefabs[i].GetComponent<RectTransform>().sizeDelta);
        }

        //计算Content总长度
        CalculateContentSize();
    }

    private void CalculateContentSize()
    {
        for (int i = 0; i < _ItemCount; i++)
        {
            if (i < 3)
            {
                _contentTotalLength += _scrollDir == ScrollDir.Horizontal ? _ItemSizeTopThree[i].x : _ItemSizeTopThree[i].y;
            }
            else
            {
                _contentTotalLength += _scrollDir == ScrollDir.Horizontal ? _itemSize[i].x : _itemSize[i].y;
            }
        }

        _scrollRect.content.sizeDelta = _scrollDir == ScrollDir.Horizontal ? 
            new Vector2(_contentTotalLength, _scrollRect.content.sizeDelta.y) : 
            new Vector2(_scrollRect.content.sizeDelta.x, _contentTotalLength);
    }

    public void ShowItems()
    {
        
    }

    private void SetScrollDir(ScrollDir dir)
    {
        if (_scrollRect == null)
            _scrollRect = GetComponent<ScrollRect>();
        _scrollRect.horizontal = _Interactable && _ScrollDir == ScrollDir.Horizontal;
        _scrollRect.vertical = _Interactable && _ScrollDir == ScrollDir.Vertical;
        _scrollDir = dir;
    }
}