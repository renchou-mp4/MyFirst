
using System.Collections.Generic;
using GameFramework.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace yxy
{
    public enum EScrollDir1
    {
        /// <summary>
        /// 垂直滚动
        /// </summary>
        Vertical,
        /// <summary>
        /// 水平滚动
        /// </summary>
        Horizontal,
    }


    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectEx1 : SerializedMonoBehaviour
    {
        [Header("ScrollRect相关")]

        private ScrollRect Sr_ScrollRect;
        private RectTransform Rtf_Content;
        private RectTransform Rtf_Viewport;
        private EScrollDir1 _scrollDir = EScrollDir1.Vertical;    /// 滚动方向
        private float _contentSize = 0;                          /// 内容尺寸


        [Header("对象池相关")]

        [SerializeField] private IObjectPool<ScrollRectItemObject1> _itemPool;
        [SerializeField] private string _itemPoolName = "ScrollRectItemObject1"; /// 对象池名称
        [SerializeField] private float _itemPoolExpireTime = float.MaxValue;     /// 对象池过期时间
        [SerializeField] private int _itemPoolCapacity = int.MaxValue;           /// 对象池容量
        [SerializeField] private int _itemPriority = 0;                          /// 对象池优先级 
        [SerializeField] private int _preloadCount = 3;                          /// 预加载数量


        [Header("Item预制体相关")]

        [SerializeField] private GameObject[] Go_ItemPrefabs;                    /// 列表项预制体
        [SerializeField] private Vector2 _itemAnchorMin = Vector2.one * -1;      /// item锚点最小值
        [SerializeField] private Vector2 _itemAnchorMax = Vector2.one * -1;      /// item锚点最大值


        [Header("数据相关")]

        [SerializeField] private List<ScrollRectItemObject1> _dataList;          /// 数据列表
        [SerializeField] private Dictionary<GameObject, ScrollRectItemObject1> _itemDict = new(); /// Item字典

        [SerializeField] private float _itemSpace = 0;                           /// Item间距
        [SerializeField] private Vector2 _itemPadding = Vector2.zero;            /// Item边距


        void Awake()
        {
            InitScrollRect();
        }

        public void SetData(List<ScrollRectItemObject1> dataList)
        {
            if (dataList == null)
            {
                Log.Error($"{_itemPoolName}: SetData传入的数据是空！");
                return;
            }

            _dataList = dataList;
            InitItemPool();

            CalculateContentSize();
            InitItems();

        }

        /// <summary>
        /// 初始化ScrollRect
        /// </summary>
        private void InitScrollRect()
        {
            Sr_ScrollRect = GetComponent<ScrollRect>();
            Rtf_Content = Sr_ScrollRect.content;
            Rtf_Viewport = Sr_ScrollRect.viewport;

            _scrollDir = Sr_ScrollRect.vertical ? EScrollDir1.Vertical : EScrollDir1.Horizontal;
            Sr_ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitItemPool()
        {
            _itemPoolName += "_" + GetInstanceID();

            //若存在旧对象池则先销毁
            if (_itemPool != null && GameEntry.ObjectPool.HasObjectPool<ScrollRectItemObject1>(_itemPoolName))
            {
                GameEntry.ObjectPool.DestroyObjectPool<ScrollRectItemObject1>(_itemPoolName);
            }

            _itemPool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<ScrollRectItemObject1>(_itemPoolName, _itemPoolCapacity, _itemPoolExpireTime, _itemPriority);
        }


        private void OnScrollValueChanged(Vector2 normalizedPosition)
        {
            //Debug.Log($"ScrollRectEx1 OnScrollValueChanged: {normalizedPosition}");
        }

        /// <summary>
        /// 计算Content大小
        /// </summary>
        private void CalculateContentSize()
        {
            Vector2 itemSize;
            float totalSize = _itemPadding.x + _itemPadding.y - _itemSpace; //初始上下边距，提前减去最后一个间距

            for (int i = 0; i < _dataList.Count; i++)
            {
                itemSize = _dataList[i].GetItemSize();
                totalSize += (_scrollDir == EScrollDir1.Vertical ? itemSize.y : itemSize.x) + _itemSpace;
            }

            if (_scrollDir == EScrollDir1.Vertical)
            {
                Rtf_Content.sizeDelta = new Vector2(Rtf_Content.sizeDelta.x, totalSize);
            }
            else
            {
                Rtf_Content.sizeDelta = new Vector2(totalSize, Rtf_Content.sizeDelta.y);
            }
            _contentSize = totalSize;
        }

        /// <summary>
        /// 初始化Item
        /// </summary>
        private void InitItems()
        {
            if (Go_ItemPrefabs == null || Go_ItemPrefabs.Length <= 0)
            {
                Log.Error($"{_itemPoolName}: Item Prefab 是空！");
                return;
            }

            //初始化锚点
            Vector2 defaultAnchorMin;
            Vector2 defaultAnchorMax;
            if (_scrollDir == EScrollDir1.Vertical)
            {
                defaultAnchorMin = new Vector2(0.5f, 1);
                defaultAnchorMax = new Vector2(0.5f, 1);

            }
            else
            {
                defaultAnchorMin = new Vector2(0, 0.5f);
                defaultAnchorMax = new Vector2(0, 0.5f);
            }
            _itemAnchorMin = _itemAnchorMin == Vector2.zero * -1 ? defaultAnchorMin : _itemAnchorMin;
            _itemAnchorMax = _itemAnchorMax == Vector2.zero * -1 ? defaultAnchorMax : _itemAnchorMax;

            //初始化数据
            for (int i = 0; i < CalculateVisibleCount(); i++)
            {

            }
        }

        private void GetItem()
        {

        }

        private int CalculateVisibleCount()
        {
            float viewportSize = _scrollDir == EScrollDir1.Vertical ? Rtf_Viewport.rect.height : Rtf_Viewport.rect.width;
            int count = 0;
            for (int i = 0; i < _dataList.Count && viewportSize > 0; i++)
            {
                viewportSize -= _scrollDir == EScrollDir1.Vertical ? _dataList[i].GetItemSize().y : _dataList[i].GetItemSize().x;
                count++;
            }
            return count;
        }
    }
}