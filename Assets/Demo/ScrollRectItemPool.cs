using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class ScrollRectItemPool
    {

        private GameObject _itemPrefab;
        private Transform _itemParent;
        private string _prefabName;
        private float _prefabPosOffset;
        private float _prefabPosPadding;
        private int _preCreateCount;

        /// <summary>
        /// 未激活未使用Item池
        /// </summary>
        private List<LoopListViewItem> _itemPool = new();
        /// <summary>
        /// 已激活未使用Item池，临时存放
        /// </summary>
        private List<LoopListViewItem> _tmpItemPool = new();

        public ScrollRectItemPool(GameObject prefab, Transform itemParent, float prefabPosOffset = 0, float prefabPosPadding = 0, int preCreateCount = 0)
        {
            _itemPrefab = prefab;
            _itemParent = itemParent;
            _prefabName = prefab.name;
            _prefabPosOffset = prefabPosOffset;
            _prefabPosPadding = prefabPosPadding;
            _preCreateCount = preCreateCount;

            for (int i = 0; i < _preCreateCount; i++)
            {
                LoopListViewItem item = GameObject.Instantiate(_itemPrefab, _itemParent).GetComponent<LoopListViewItem>();
                item.gameObject.SetSelfActive(false);
                _itemPool.Add(item);
            }

        }

        public LoopListViewItem GetItem()
        {
            LoopListViewItem item;

            //有已激活的Item
            if (_tmpItemPool.Count > 0)
            {
                item = _tmpItemPool[^1];
                _tmpItemPool.RemoveAt(_tmpItemPool.Count - 1);
                //TODO debug使用，需要删除
                item.gameObject.SetSelfActive(true);

                item._CacheRectTransform.localScale = Vector3.one;
                item._CacheRectTransform.anchoredPosition3D = Vector3.zero;
                item._CacheRectTransform.localEulerAngles = Vector3.zero;
                return item;
            }

            //有未激活的Item
            if (_itemPool.Count > 0)
            {
                item = _itemPool[^1];
                _itemPool.RemoveAt(_itemPool.Count - 1);
                item.gameObject.SetSelfActive(true);

                item._CacheRectTransform.localScale = Vector3.one;
                item._CacheRectTransform.anchoredPosition3D = Vector3.zero;
                item._CacheRectTransform.localEulerAngles = Vector3.zero;
                return item;
            }
            item = CreateItem();

            return item;
        }

        public LoopListViewItem CreateItem()
        {
            LoopListViewItem item = GameObject.Instantiate(_itemPrefab, _itemParent).GetComponent<LoopListViewItem>();
            item.gameObject.SetSelfActive(true);
            item._CacheRectTransform.localScale = Vector3.one;
            item._CacheRectTransform.anchoredPosition3D = Vector3.zero;
            item._CacheRectTransform.localEulerAngles = Vector3.zero;
            item._ItemPrefabName = _prefabName;
            item._ItemPosOffset = _prefabPosOffset;
            item._ItemPadding = _prefabPosPadding;
            return item;
        }

        public void Recycle(LoopListViewItem item)
        {
            if (item._ItemPrefabName != _prefabName)
            {
                Debug.LogError("要回收的Item不属于这个池子");
                return;
            }
            _itemPool.Add(item);
            item.gameObject.SetSelfActive(false);
        }

        public void TmpRecycle(LoopListViewItem item)
        {
            if (item._ItemPrefabName != _prefabName)
            {
                Debug.LogError("要回收的Item不属于这个池子");
                return;
            }
            _tmpItemPool.Add(item);
            //TODO debug使用，需要删除
            item.gameObject.SetSelfActive(false);
        }

        public void ClearTmpRecycle()
        {
            for (int i = 0; i < _tmpItemPool.Count; i++)
            {
                Recycle(_tmpItemPool[i]);
            }
        }

        public void DestroyAllItem()
        {
            ClearTmpRecycle();
            for (int i = 0; i < _itemPool.Count; i++)
            {
                GameObject.DestroyImmediate(_itemPool[i].gameObject);
            }
            _itemPool.Clear();
        }
    }
}
