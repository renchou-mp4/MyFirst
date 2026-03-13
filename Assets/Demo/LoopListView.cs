using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace yxy
{

    /// Item预制体数据
    [System.Serializable]
    public class ItemPrefabData
    {
        public GameObject _ItemPrefab;
        /// <summary>
        /// Item间距
        /// </summary>
        public float _Padding;
        /// <summary>
        /// Item偏移量
        /// </summary>
        public float _Offset;
        /// <summary>
        /// 预创建数量
        /// </summary>
        public int _PreCreateCount;
    }

    [RequireComponent(typeof(ScrollRect))]
    public class LoopListView : SerializedMonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        #region  Unity组件

        private ScrollRect Sr_ScrollRect;
        private RectTransform Rtf_Content;
        private RectTransform Rtf_ViewPort;

        #endregion


        #region  公有成员

        // Item预制体数据列表
        public List<ItemPrefabData> _ItemPrefabDataList = new();
        /// <summary>
        /// 创建前面Item的边界线，超过这个边界线就创建Item
        /// </summary>
        public float _CreateItemLineFirst = 300f;
        /// <summary>
        /// 创建后面Item的边界线，超过这个边界线就创建Item
        /// </summary>
        public float _CreateItemLineLast = 300f;
        /// <summary>
        /// 回收前面Item的边界线，超过这个边界线就回收Item
        /// </summary>
        public float _RecycleItemLineFirst = 300f;
        /// <summary>
        /// 回收后面Item的边界线，超过这个边界线就回收Item
        /// </summary>
        public float _RecycleItemLineLast = 300f;

        public bool _IsVertical = true;

        #endregion


        #region 私有成员

        /// <summary>
        /// Item列表
        /// </summary>
        private List<LoopListViewItem> _itemList = new();
        /// <summary>
        /// Item对象池 字典，key为预制体名称，value为对应的Item池
        /// </summary>
        private Dictionary<string, ScrollRectItemPool> _itemPoolDic = new();
        private System.Func<LoopListView, int, LoopListViewItem> _getItemFunc;
        /// <summary>
        /// 缓存Item四角的世界坐标，顺序为左下、左上、右上、右下
        /// </summary>
        private Vector3[] _itemWorldCorners = new Vector3[4];
        /// <summary>
        /// 缓存ViewPort四角的本地坐标，顺序为左下、左上、右上、右下
        /// </summary>
        private Vector3[] _viewPortLocalCorners = new Vector3[4];

        #endregion

        void Awake()
        {
            Sr_ScrollRect = GetComponent<ScrollRect>();
            Rtf_Content = Sr_ScrollRect.content;
            Rtf_ViewPort = Sr_ScrollRect.viewport;
            Rtf_ViewPort.GetLocalCorners(_viewPortLocalCorners);
        }

        void Update()
        {
            CheckBorder();
        }

        public void InitListView(int totalItemCount, System.Func<LoopListView, int, LoopListViewItem> getItemFunc)
        {
            _getItemFunc = getItemFunc;

            InitItemPool();
        }

        private void InitItemPool()
        {
            foreach (var prefabData in _ItemPrefabDataList)
            {
                if (prefabData == null)
                {
                    Log.Error("预制体数据为空！");
                    continue;
                }
                if (!_itemPoolDic.ContainsKey(prefabData._ItemPrefab.name))
                {
                    _itemPoolDic.Add(prefabData._ItemPrefab.name, new ScrollRectItemPool(prefabData._ItemPrefab, Rtf_Content, prefabData._PreCreateCount));
                }
            }
        }

        public LoopListViewItem CreateItem(string itemPrefabName)
        {
            if (itemPrefabName.IsNullOrEmpty())
            {
                Log.Error("预制体名称不能为空！");
                return null;
            }
            if (!_itemPoolDic.TryGetValue(itemPrefabName, out ScrollRectItemPool pool))
            {
                Log.Error("对象池未包含该预制体！");
                return null;
            }
            LoopListViewItem item = pool.GetItem();
            RectTransform rtf = item.GetComponent<RectTransform>();
            rtf.parent = Rtf_Content;
            rtf.localScale = default;
            rtf.anchoredPosition3D = default;
            rtf.localEulerAngles = default;
            item._ParentListView = this;
            return item;
        }

        private void CheckBorder()
        {
            if (_itemList.Count == 0) return;

            LoopListViewItem firstItem = _itemList[0];
            LoopListViewItem lastItem = _itemList[_itemList.Count - 1];

            //获取第一个Item的左下相对ViewPort的坐标
            firstItem._CacheRectTransform.GetWorldCorners(_itemWorldCorners);
            Vector3 firstLocalPos = _itemWorldCorners[0];
            Vector3 firstrelativePos = Rtf_ViewPort.InverseTransformPoint(firstLocalPos);

            //获取最后一个Item的左下相对ViewPort的坐标
            lastItem._CacheRectTransform.GetWorldCorners(_itemWorldCorners);
            Vector3 lastLocalPos = _itemWorldCorners[1];
            Vector3 lastRelativePos = Rtf_ViewPort.InverseTransformPoint(lastLocalPos);

            if (firstrelativePos.y - _viewPortLocalCorners[1].y > _RecycleItemLineFirst)
            {
                //回收第一个Item
                _itemList.RemoveAt(0);
                _itemPoolDic[firstItem.name].Recycle(firstItem);

            }
            if (_viewPortLocalCorners[0].y - lastRelativePos.y > _RecycleItemLineLast)
            {
                //回收最后一个Item
                _itemList.RemoveAt(_itemList.Count - 1);
                _itemPoolDic[lastItem.name].Recycle(lastItem);
            }

            UpdateContentSize();
            CheckIsNeedUpdateItemPos();
        }

        private void UpdateContentSize()
        {
            float size = 0;
            foreach (var item in _itemList)
            {
                size += item._ItemSize;
            }

            if (_IsVertical)
            {
                if (size == Rtf_Content.rect.height) return;
                Rtf_Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }
            else
            {
                if (size == Rtf_Content.rect.width) return;
                Rtf_Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            }

        }

        private void CheckIsNeedUpdateItemPos()
        {
            LoopListViewItem firstItem = _itemList[0];
            LoopListViewItem lastItem = _itemList[_itemList.Count - 1];

            if (firstItem.TopY != 0 || -lastItem.BottomY != Rtf_ViewPort.rect.height)
            {
                UpdateAllShownItemPos();
                return;
            }
        }

        private void UpdateAllShownItemPos()
        {
            if (_itemList.Count == 0) return;
            float pos = 0;
            LoopListViewItem item;

            float dis = _itemList[0].TopY;
            for (int i = 0; i < _itemList.Count; i++)
            {
                item = _itemList[i];
                item._CacheRectTransform.anchoredPosition3D = new Vector3(item._ItemPosOffset, pos, 0);
                pos += item._ItemSize + item._ItemPadding;
            }
            Vector3 originPos = Rtf_Content.anchoredPosition3D;
            originPos.y -= dis;
            Rtf_Content.anchoredPosition3D = originPos;

        }

        public void OnDrag(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }
    }
}


