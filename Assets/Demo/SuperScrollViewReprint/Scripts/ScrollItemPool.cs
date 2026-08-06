using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using yxy;

public class ScrollItemPool
{
    //---------------------------------------------------------
    //私有属性
    //---------------------------------------------------------

    /// <summary>
    /// Item对象池-临时回收（未设置Active(false)）
    /// </summary>
    private List<LoopListItem> _itemsPoolTmp = new();

    /// <summary>
    /// Item对象池-已回收（已设置Active(false)）
    /// </summary>
    private List<LoopListItem> _itemsPool = new();

    /// <summary>
    /// 预制体
    /// </summary>
    private GameObject _prefab;

    /// <summary>
    /// Item父节点
    /// </summary>
    private Transform _parent;

    /// <summary>
    /// 预制体名称
    /// </summary>
    private string _prefabName;

    /// <summary>
    /// 预加载数量
    /// </summary>
    private int _preloadCount;

    /// <summary>
    /// item间隔
    /// </summary>
    private float _space;

    /// <summary>
    /// 位置偏移
    /// </summary>
    private float _posOffset;

    /// <summary>
    /// 初始化对象池
    /// </summary>
    public void InitPool(
        GameObject prefab,
        int preloadCount,
        float space,
        float posOffset,
        Transform parent
    )
    {
        _prefab = prefab;
        _prefabName = prefab.name;
        _preloadCount = preloadCount;
        _space = space;
        _posOffset = posOffset;
        _parent = parent;

        if (_itemsPool.Count > 0 || _itemsPoolTmp.Count > 0)
        {
            DestoryAllItem();
        }

        for (int i = 0; i < _preloadCount; i++)
        {
            LoopListItem item = CreateItem();
            RecycleItemReal(item);
        }
    }

    /// <summary>
    /// 获取Item
    /// </summary>
    /// <returns></returns>
    public LoopListItem GetItem(int itemIndex)
    {
        LoopListItem item = null;

        //优先使用临时回收的Item：命中index则直接返回（已Active，跳过SetActive开销）；未命中则取末尾任意一个并SetActive(true)
        if (_itemsPoolTmp.Count > 0)
        {
            int tmpCount = _itemsPoolTmp.Count;
            for (int i = 0; i < tmpCount; i++)
            {
                if (_itemsPoolTmp[i].Index == itemIndex)
                {
                    item = _itemsPoolTmp[i];
                    _itemsPoolTmp.RemoveAt(i);
                    return item;
                }
            }
            //未命中index：从末尾取一个，SetActive(true)
            item = _itemsPoolTmp[tmpCount - 1];
            _itemsPoolTmp.RemoveAt(tmpCount - 1);
            item.gameObject.SetSelfActive(true);
            return item;
        }

        //临时回收的ItemPoolTmp中没有对应的Item。从ItemPool中获取Item
        if (_itemsPool.Count > 0)
        {
            item = _itemsPool[^1];
            item.gameObject.SetSelfActive(true);
            _itemsPool.RemoveAt(_itemsPool.Count - 1);
            return item;
        }

        //ItemPool中没有可用Item，创建新Item
        return CreateItem();
    }

    /// <summary>
    /// 创建新的Item
    /// </summary>
    /// <returns></returns>
    private LoopListItem CreateItem()
    {
        GameObject go = GameObject.Instantiate(_prefab, _parent);
        if (go == null)
        {
            Debug.LogError("创建LoopListItem失败！");
            return null;
        }
        ResetItem(go.transform);

        LoopListItem item = go.GetOrAddComponent<LoopListItem>();
        if (item == null)
        {
            Debug.LogError("获取LoopListItem组件失败！");
            GameObject.Destroy(go);
            return null;
        }

        item.PrefabName = _prefabName;
        item.PosOffset = _posOffset;
        item.Space = _space;

        item.gameObject.SetSelfActive(true);
        return item;
    }

    /// <summary>
    /// 重置Item
    /// </summary>
    /// <param name="trans"></param>
    private void ResetItem(Transform trans)
    {
        trans.localScale = Vector3.one;
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 回收Item但不设置Active(false)
    /// </summary>
    /// <param name="item"></param>
    public void RecycleItem(LoopListItem item)
    {
        if (item == null)
        {
            Debug.LogError("回收的LoopListItem为空！");
            return;
        }
        _itemsPoolTmp.Add(item);
    }

    /// <summary>
    /// 回收Item并设置Active(false)
    /// </summary>
    /// <param name="item"></param>
    public void RecycleItemReal(LoopListItem item)
    {
        if (item == null)
        {
            Debug.LogError("回收的LoopListItem为空！");
            return;
        }

        _itemsPool.Add(item);
        item.gameObject.SetSelfActive(false);
    }

    /// <summary>
    /// 清空已回收但未设置Active(false)的Item
    /// </summary>
    public void ClearItemRecycleTmp()
    {
        for (int i = 0; i < _itemsPoolTmp.Count; i++)
        {
            RecycleItemReal(_itemsPoolTmp[i]);
        }

        _itemsPoolTmp.Clear();
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void DestoryAllItem()
    {
        for (int i = 0; i < _itemsPool.Count; i++)
        {
            GameObject.Destroy(_itemsPool[i].gameObject);
        }
        for (int i = 0; i < _itemsPoolTmp.Count; i++)
        {
            GameObject.Destroy(_itemsPoolTmp[i].gameObject);
        }

        _prefab = null;
        _prefabName = string.Empty;
        _preloadCount = 0;
        _space = 0;
        _posOffset = 0;
        _itemsPool.Clear();
        _itemsPoolTmp.Clear();
    }
}
