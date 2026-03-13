using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class ScrollRectItemPool : MonoBehaviour
    {

        private GameObject _itemPrefab;
        private Transform _itemParent;
        private string _prefabName;
        private int _preCreateCount = 1;
        private List<LoopListViewItem> _itemPool = new();
        private List<LoopListViewItem> _cacheItemPool = new();

        public ScrollRectItemPool(GameObject prefab, Transform itemParent, int preCreateCount)
        {
            _itemPrefab = prefab;
            _prefabName = prefab.name;
            _preCreateCount = preCreateCount;
            _itemParent = itemParent;
            for (int i = 0; i < _preCreateCount; i++)
            {
                LoopListViewItem item = Instantiate(_itemPrefab, _itemParent).GetComponent<LoopListViewItem>();
                _itemPool.Add(item);
                item.gameObject.SetSelfActive(true);
                item._ItemPrefabName = _prefabName;

                RectTransform rtf = item.GetComponent<RectTransform>();
                rtf.localScale = Vector3.one;
                rtf.anchoredPosition3D = Vector3.zero;
                rtf.localEulerAngles = Vector3.zero;
            }
        }

        public LoopListViewItem GetItem()
        {
            LoopListViewItem item = null;
            item._CacheRectTransform.anchoredPosition3D = default;
            item._CacheRectTransform.localEulerAngles = default;
            item._CacheRectTransform.localScale = default;
            item._CacheRectTransform.SetParent(_itemParent, false);
            item.gameObject.SetSelfActive(false);

            //没有缓存Item
            if (_cacheItemPool.Count <= 0)
            {
                item = Instantiate(_itemPrefab, _itemParent).GetComponent<LoopListViewItem>();
            }
            //有缓存Item
            else
            {
                item = _cacheItemPool[0];
                _cacheItemPool.RemoveAt(0);
            }
            _itemPool.Add(item);
            item.gameObject.SetSelfActive(true);
            return item;
        }

        public void Recycle(LoopListViewItem item)
        {
            if (item == null) return;
            if (!_itemPool.Contains(item))
            {
                Log.Error("要回收的Item不属于这个池子");
                return;
            }
            _itemPool.Remove(item);
            _cacheItemPool.Add(item);
            item.gameObject.SetSelfActive(false);
        }
    }
}
