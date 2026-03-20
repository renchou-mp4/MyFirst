using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    //TODO 锚点设置
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

        [HideInInspector]
        public bool _isVertical = true;

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
        private System.Action _onDragBegin;
        private System.Action _onDrag;
        private System.Action _onDragEnd;
        private PointerEventData _pointerEventData;
        /// <summary>
        /// 缓存Item四角的世界坐标，顺序为左下、左上、右上、右下
        /// </summary>
        private Vector3[] _itemWorldCorners = new Vector3[4];
        /// <summary>
        /// 缓存ViewPort四角的本地坐标，顺序为左下、左上、右上、右下
        /// </summary>
        private Vector3[] _viewPortLocalCorners = new Vector3[4];
        /// <summary>
        /// 当前刷新ListView的次数，每帧+1
        /// </summary>
        private int _curFrame = 0;

        private int _totalItemCount = 0;

        private bool _isDrag;

        private Vector3 _lastFrameContentPos;

        private Vector3 _adjustVec;
        private bool _needAdjustVec;

        #endregion

        void Awake()
        {
            Sr_ScrollRect = GetComponent<ScrollRect>();
            Rtf_Content = Sr_ScrollRect.content;
            Rtf_ViewPort = Sr_ScrollRect.viewport;
            Rtf_ViewPort.GetLocalCorners(_viewPortLocalCorners);
            _isVertical = Sr_ScrollRect.vertical;
            Sr_ScrollRect.horizontal = !Sr_ScrollRect.vertical;
        }

        void Update()
        {
            if (_needAdjustVec)
            {
                _needAdjustVec = false;
                if (_isVertical)
                {
                    if (Sr_ScrollRect.velocity.y * _adjustVec.y > 0)
                    {
                        Sr_ScrollRect.velocity = _adjustVec;
                    }
                }
                else
                {
                    if (Sr_ScrollRect.velocity.x * _adjustVec.x > 0)
                    {
                        Sr_ScrollRect.velocity = _adjustVec;
                    }
                }

            }
            UpdateListView();

            _lastFrameContentPos = Rtf_Content.anchoredPosition3D;
        }

        public void InitListView(int totalItemCount, System.Func<LoopListView, int, LoopListViewItem> getItemFunc)
        {
            _totalItemCount = totalItemCount;
            _getItemFunc = getItemFunc;

            InitItemPool();
        }

        private void InitItemPool()
        {
            _itemPoolDic.Clear();
            foreach (var prefabData in _ItemPrefabDataList)
            {
                if (prefabData == null)
                {
                    Debug.LogError("预制体数据为空！");
                    continue;
                }
                if (!_itemPoolDic.ContainsKey(prefabData._ItemPrefab.name))
                {
                    ScrollRectItemPool itemPool = new(prefabData._ItemPrefab, Rtf_Content, prefabData._PreCreateCount);
                    _itemPoolDic.Add(prefabData._ItemPrefab.name, itemPool);

                    for (int i = 0; i < prefabData._PreCreateCount; i++)
                    {
                        LoopListViewItem item = CreateItem(prefabData._ItemPrefab.name);
                        _itemList.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// 从对象池中获取Item
        /// </summary>
        /// <param name="itemPrefabName"></param>
        /// <returns></returns>
        public LoopListViewItem CreateItem(string itemPrefabName)
        {
            if (itemPrefabName.IsNullOrEmpty())
            {
                Debug.LogError("预制体名称不能为空！");
                return null;
            }
            if (!_itemPoolDic.TryGetValue(itemPrefabName, out ScrollRectItemPool pool))
            {
                Debug.LogError("对象池未包含该预制体！");
                return null;
            }
            LoopListViewItem item = pool.GetItem();
            item._CreateFrame = _curFrame;
            item._ParentListView = this;

            return item;
        }

        private LoopListViewItem CreateItemInternal(int index)
        {
            LoopListViewItem item = _getItemFunc?.Invoke(this, index);
            if (item == null) return null;
            item._ItemIndex = index;
            return item;
        }

        private void UpdateListView()
        {
            _curFrame++;

            bool needContinueCheak = true;
            int curCheckCount = 0;
            int maxCheckCount = 9999;

            if (curCheckCount > maxCheckCount)
            {
                Debug.LogError("ListView循环检查次数超标！请检查代码");
                Application.Quit();
                return;
            }

            while (needContinueCheak)
            {
                curCheckCount++;
                if (_isVertical)
                {
                    needContinueCheak = UpdateListViewVertical();
                }
                else
                {
                    needContinueCheak = UpdateListViewHorizantal();
                }
            }
        }

        private bool UpdateListViewVertical()
        {
            if (_totalItemCount == 0 && _itemList.Count > 0)
            {
                RecycleAllItem();
                return false;
            }

            //1.若itemList数量为0，直接生成一个item
            if (_itemList.Count == 0)
            {
                LoopListViewItem item = CreateItemInternal(0);
                if (item == null) return false;
                _itemList.Add(item);
                item._CacheRectTransform.anchoredPosition3D = new Vector3(item._ItemPosOffset, 0, 0);
                UpdateContentSize();
                return true;
            }

            LoopListViewItem firstItem = _itemList[0];
            LoopListViewItem lastItem = _itemList[_itemList.Count - 1];

            //获取第一个Item的左下相对ViewPort的坐标
            firstItem._CacheRectTransform.GetWorldCorners(_itemWorldCorners);
            Vector3 firstPos0 = Rtf_ViewPort.InverseTransformPoint(_itemWorldCorners[0]);
            Vector3 firstPos1 = Rtf_ViewPort.InverseTransformPoint(_itemWorldCorners[1]);

            //获取最后一个Item的左下相对ViewPort的坐标
            lastItem._CacheRectTransform.GetWorldCorners(_itemWorldCorners);
            Vector3 lastPos0 = Rtf_ViewPort.InverseTransformPoint(_itemWorldCorners[0]);
            Vector3 lastPos1 = Rtf_ViewPort.InverseTransformPoint(_itemWorldCorners[1]);

            //2.回收item
            if (!_isDrag && firstItem._CreateFrame != _curFrame && firstPos0.y - _viewPortLocalCorners[1].y > _RecycleItemLineFirst)
            {
                //回收第一个Item
                _itemList.RemoveAt(0);
                TmpRecycle(firstItem);
                Debug.LogError($"回收第一个Item：{firstItem._ItemIndex},Pos:{firstItem._CacheRectTransform.anchoredPosition3D}");
                return true;
            }
            if (!_isDrag && lastItem._CreateFrame != _curFrame && _viewPortLocalCorners[0].y - lastPos1.y > _RecycleItemLineLast)
            {
                //回收最后一个Item
                _itemList.RemoveAt(_itemList.Count - 1);
                TmpRecycle(lastItem);
                Debug.LogError($"回收最后一个Item：{lastItem._ItemIndex},Pos:{lastItem._CacheRectTransform.anchoredPosition3D}");
                return true;
            }

            //3.生成item
            if (_viewPortLocalCorners[0].y - lastPos0.y < _CreateItemLineLast)
            {
                //生成最后一个Item
                int nIndex = lastItem._ItemIndex + 1;
                if (nIndex >= _totalItemCount) return false;
                LoopListViewItem newItem = CreateItemInternal(nIndex);
                if (newItem == null)
                {
                    CheckIsNeedUpdateItemPos();
                    return false;
                }
                _itemList.Add(newItem);
                float y = lastItem._CacheRectTransform.anchoredPosition3D.y - lastItem._CacheRectTransform.rect.height - lastItem._ItemPadding;
                newItem._CacheRectTransform.anchoredPosition3D = new Vector3(newItem._ItemPosOffset, y, 0);
                Debug.LogError($"生成最后一个Item：{newItem._ItemIndex},Pos:{newItem._CacheRectTransform.anchoredPosition3D}");
                UpdateContentSize();
                CheckIsNeedUpdateItemPos();
                return true;
            }
            if (firstPos1.y - _viewPortLocalCorners[1].y < _CreateItemLineFirst)
            {
                //生成第一个Item
                int nIndex = firstItem._ItemIndex - 1;
                if (nIndex < 0) return false;
                LoopListViewItem newItem = CreateItemInternal(nIndex);
                if (newItem == null)
                {
                    CheckIsNeedUpdateItemPos();
                    return false;
                }
                _itemList.Insert(0, newItem);
                float y = firstItem._CacheRectTransform.anchoredPosition3D.y + newItem._CacheRectTransform.rect.height + newItem._ItemPadding;
                newItem._CacheRectTransform.anchoredPosition3D = new Vector3(newItem._ItemPosOffset, y, 0);
                Debug.LogError($"生成第一个Item：{newItem._ItemIndex},Pos:{newItem._CacheRectTransform.anchoredPosition3D}");
                UpdateContentSize();
                CheckIsNeedUpdateItemPos();
                return true;
            }

            return false;
        }

        private bool UpdateListViewHorizantal()
        {
            return false;
        }

        private float GetContentSize()
        {
            float size = 0;
            foreach (var item in _itemList)
            {
                size += item._ItemSize + item._ItemPadding;
            }
            return size;
        }

        private void UpdateContentSize()
        {
            float size = GetContentSize();

            if (_isVertical)
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
            if (_itemList.Count <= 0)
            {
                Debug.Log($"Count:{_itemList.Count}");
                return;
            }
            LoopListViewItem firstItem = _itemList[0];
            LoopListViewItem lastItem = _itemList[_itemList.Count - 1];

            if (firstItem.TopY > 0)
            {
                UpdateAllShownItemPos();
                Debug.Log("重新排版0");
                return;
            }

            float contentSize = GetContentSize();
            if (-lastItem.BottomY > contentSize)
            {
                UpdateAllShownItemPos();
                Debug.Log("重新排版1");
                return;
            }
        }

        private void UpdateAllShownItemPos()
        {
            if (_itemList.Count == 0) return;
            float deltaTime = Time.deltaTime;
            const float minDeltaTime = 1.0f / 120.0f;
            if (deltaTime < minDeltaTime)
            {
                deltaTime = minDeltaTime;
            }
            _adjustVec = (Rtf_Content.anchoredPosition3D - _lastFrameContentPos) / deltaTime;
            float pos = 0;

            LoopListViewItem item;

            float dis = -_itemList[0].TopY;
            for (int i = 0; i < _itemList.Count; i++)
            {
                item = _itemList[i];
                item._CacheRectTransform.anchoredPosition3D = new Vector3(item._ItemPosOffset, pos, 0);
                pos -= item._ItemSize + item._ItemPadding;
            }
            Vector3 originPos = Rtf_Content.anchoredPosition3D;
            originPos.y -= dis;
            Rtf_Content.anchoredPosition3D = originPos;

            if (_isDrag)
            {
                Sr_ScrollRect.OnBeginDrag(_pointerEventData);
                Sr_ScrollRect.Rebuild(CanvasUpdate.PostLayout);
                Sr_ScrollRect.velocity = _adjustVec;
                _needAdjustVec = true;
            }

        }

        #region 对象池回收

        private void Recycle(LoopListViewItem item)
        {
            if (item == null) return;
            if (item._ItemPrefabName.IsNullOrEmpty()) return;
            if (_itemPoolDic.TryGetValue(item._ItemPrefabName, out ScrollRectItemPool itemPool))
            {
                itemPool.Recycle(item);
            }
        }

        private void TmpRecycle(LoopListViewItem item)
        {
            if (item == null) return;
            if (item._ItemPrefabName.IsNullOrEmpty()) return;
            if (_itemPoolDic.TryGetValue(item._ItemPrefabName, out ScrollRectItemPool itemPool))
            {
                itemPool.TmpRecycle(item);
            }
        }

        private void ClearAllTmpRecycle()
        {
            foreach (var itemPool in _itemPoolDic.Values)
            {
                itemPool.ClearTmpRecycle();
            }
        }

        private void RecycleAllItem()
        {
            foreach (var item in _itemList)
            {
                Recycle(item);
            }
            _itemList.Clear();
        }

        #endregion

        #region 拖拽接口实现

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isDrag = true;
            CacheDragPointerEventData(eventData);
            _onDragBegin?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            CacheDragPointerEventData(eventData);
            _onDrag?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isDrag = false;
            _onDragEnd?.Invoke();
            ClearAllTmpRecycle();
        }

        void CacheDragPointerEventData(PointerEventData eventData)
        {
            if (_pointerEventData == null)
            {
                _pointerEventData = new PointerEventData(EventSystem.current);
            }
            _pointerEventData.button = eventData.button;
            _pointerEventData.position = eventData.position;
            _pointerEventData.pointerPressRaycast = eventData.pointerPressRaycast;
            _pointerEventData.pointerCurrentRaycast = eventData.pointerCurrentRaycast;
        }

        #endregion
    }
}


