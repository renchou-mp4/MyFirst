using System.Collections.Generic;
using GameFramework.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace yxy
{
    public enum EScrollDir
    {
        Horizontal,
        Vertical
    }

    [RequireComponent(typeof(ScrollRect))]
    public partial class ScrollRectEx : SerializedMonoBehaviour
    {
        // Unity特有类型变量
        [SerializeField] private GameObject Go_ItemPrefab;
        [SerializeField] private RectTransform Tf_Content;
        private ScrollRect Sr_ScrollRect;

        // 配置变量
        [Header("滚动配置")]
        [SerializeField] private EScrollDir _ScrollDirection = EScrollDir.Vertical;
        [SerializeField] private float _Spacing = 0f;
        [SerializeField] private int _PreloadCount = 5;

        // 锚点配置变量
        [Header("锚点配置")]
        [SerializeField] private Vector2 _ContentAnchorMin = Vector2.zero;
        [SerializeField] private Vector2 _ContentAnchorMax = Vector2.one;
        [SerializeField] private Vector2 _ItemAnchorMin = Vector2.zero;
        [SerializeField] private Vector2 _ItemAnchorMax = Vector2.one;

        // 运行时数据
        [Header("运行时数据")]
        [ShowInInspector, ReadOnly] private List<object> _DataList = new List<object>();
        [ShowInInspector, ReadOnly] private List<RectTransform> _ActiveItems = new List<RectTransform>();
        [ShowInInspector, ReadOnly] private int _FirstVisibleIndex = 0;
        [ShowInInspector, ReadOnly] private int _LastVisibleIndex = 0;

        // 计算和状态变量
        private float _ContentSize;
        private float _ItemSize;
        private float _ViewPortSize;
        private int _MaxVisibleCount = 0;

        // 对象池相关变量
        private IObjectPool<ScrollRectItemObject> _ObjectPool;
        private string _PoolName;
        private Dictionary<GameObject, ScrollRectItemObject> _ItemMap = new Dictionary<GameObject, ScrollRectItemObject>();



        /// <summary>
        /// 初始化ScrollViewEx
        /// </summary>
        private void Awake()
        {
            Sr_ScrollRect = GetComponent<ScrollRect>();
            if (Sr_ScrollRect == null)
            {
                Log.Error("ScrollViewEx: ScrollRect component not found!");
                return;
            }

            if (Tf_Content == null)
            {
                Tf_Content = Sr_ScrollRect.content;
            }

            // 设置滚动方向
            Sr_ScrollRect.horizontal = _ScrollDirection == EScrollDir.Horizontal;
            Sr_ScrollRect.vertical = _ScrollDirection == EScrollDir.Vertical;

            // 注册滚动事件
            Sr_ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);

            // 初始化对象池
            InitializeObjectPool();

            // 应用Content锚点设置
            ApplyContentAnchors();

            // 计算Item大小
            if (Go_ItemPrefab != null)
            {
                if (Go_ItemPrefab.TryGetComponent<IScrollRectItem>(out var item))
                {
                    Vector2 size = item.GetItemSize();
                    _ItemSize = _ScrollDirection == EScrollDir.Horizontal ? size.x : size.y;
                }
            }

            // 计算视口大小
            _ViewPortSize = _ScrollDirection == EScrollDir.Horizontal ?
                Sr_ScrollRect.viewport.rect.width : Sr_ScrollRect.viewport.rect.height;
        }

        /// <summary>
        /// 应用Content锚点设置
        /// </summary>
        private void ApplyContentAnchors()
        {
            if (Tf_Content != null)
            {
                // 限制锚点值在0-1之间
                Vector2 clampedAnchorMin = new Vector2(
                    Mathf.Clamp01(_ContentAnchorMin.x),
                    Mathf.Clamp01(_ContentAnchorMin.y)
                );
                Vector2 clampedAnchorMax = new Vector2(
                    Mathf.Clamp01(_ContentAnchorMax.x),
                    Mathf.Clamp01(_ContentAnchorMax.y)
                );

                Tf_Content.anchorMin = clampedAnchorMin;
                Tf_Content.anchorMax = clampedAnchorMax;
            }
        }

        /// <summary>
        /// 应用Item锚点设置
        /// </summary>
        /// <param name="itemTrans">Item的RectTransform</param>
        private void ApplyItemAnchors(RectTransform itemTrans)
        {
            if (itemTrans != null)
            {
                // 限制锚点值在0-1之间
                Vector2 clampedAnchorMin = new Vector2(
                    Mathf.Clamp01(_ItemAnchorMin.x),
                    Mathf.Clamp01(_ItemAnchorMin.y)
                );
                Vector2 clampedAnchorMax = new Vector2(
                    Mathf.Clamp01(_ItemAnchorMax.x),
                    Mathf.Clamp01(_ItemAnchorMax.y)
                );

                itemTrans.anchorMin = clampedAnchorMin;
                itemTrans.anchorMax = clampedAnchorMax;
            }
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitializeObjectPool()
        {
            _PoolName = $"ScrollViewItemPool_{gameObject.GetInstanceID()}";

            // 检查对象池是否已存在，如果存在则销毁
            if (GameEntry.ObjectPool.HasObjectPool<ScrollRectItemObject>(_PoolName))
            {
                GameEntry.ObjectPool.DestroyObjectPool<ScrollRectItemObject>(_PoolName);
            }

            // 创建新的对象池
            _ObjectPool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<ScrollRectItemObject>(
                _PoolName, // 对象池名称
                10, // 初始容量
                3600f, // 对象过期时间（秒）
                0); // 对象池优先级
        }

        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="dataList">数据源列表</param>
        public void SetData(List<object> dataList)
        {
            _DataList.Clear();
            _DataList.AddRange(dataList);

            // 重置滚动位置
            Sr_ScrollRect.normalizedPosition = _ScrollDirection == EScrollDir.Horizontal ? Vector2.right : Vector2.up;

            // 回收所有活跃的Item
            RecycleAllItems();

            // 计算Content大小
            CalculateContentSize();

            // 计算最大可见数量
            CalculateMaxVisibleCount();

            // 更新可见的Item
            UpdateVisibleItems();
        }

        /// <summary>
        /// 计算Content大小
        /// </summary>
        private void CalculateContentSize()
        {
            int count = _DataList.Count;
            if (count == 0)
            {
                _ContentSize = 0;
                return;
            }

            _ContentSize = count * _ItemSize + (count - 1) * _Spacing;

            Vector2 size = Tf_Content.sizeDelta;
            if (_ScrollDirection == EScrollDir.Horizontal)
            {
                size.x = _ContentSize;
            }
            else
            {
                size.y = _ContentSize;
            }
            Tf_Content.sizeDelta = size;
        }

        /// <summary>
        /// 计算最大可见数量
        /// </summary>
        private void CalculateMaxVisibleCount()
        {
            if (_ItemSize <= 0)
            {
                _MaxVisibleCount = 0;
                return;
            }

            // 可视项目数 + ViewPort外上下两个方向的预加载项目数
            _MaxVisibleCount = Mathf.CeilToInt(_ViewPortSize / _ItemSize) + _PreloadCount * 2;
            _MaxVisibleCount = Mathf.Min(_MaxVisibleCount, _DataList.Count);
        }

        /// <summary>
        /// 更新可见的Item
        /// </summary>
        private void UpdateVisibleItems()
        {
            if (_DataList.Count == 0)
                return;

            // 计算当前可见区域的起始和结束位置
            float scrollPosition = _ScrollDirection == EScrollDir.Horizontal ?
                Tf_Content.anchoredPosition.x : -Tf_Content.anchoredPosition.y;

            int newFirstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(scrollPosition / (_ItemSize + _Spacing)) - _PreloadCount);
            int newLastVisibleIndex = Mathf.Min(_DataList.Count - 1,
                Mathf.CeilToInt((scrollPosition + _ViewPortSize) / (_ItemSize + _Spacing)) + _PreloadCount);

            // 如果可见范围没有变化，不需要更新
            if (newFirstVisibleIndex == _FirstVisibleIndex && newLastVisibleIndex == _LastVisibleIndex)
                return;

            _FirstVisibleIndex = newFirstVisibleIndex;
            _LastVisibleIndex = newLastVisibleIndex;

            // 回收不在可见范围内的Item
            List<RectTransform> itemsToRecycle = new List<RectTransform>();
            foreach (RectTransform item in _ActiveItems)
            {
                int itemIndex = GetItemIndex(item);
                if (itemIndex < _FirstVisibleIndex || itemIndex > _LastVisibleIndex)
                {
                    itemsToRecycle.Add(item);
                }
            }

            foreach (RectTransform item in itemsToRecycle)
            {
                RecycleItem(item);
            }

            // 创建或复用可见范围内的Item
            for (int i = _FirstVisibleIndex; i <= _LastVisibleIndex; i++)
            {
                if (!IsItemActive(i))
                {
                    CreateOrReuseItem(i);
                }
            }
        }

        /// <summary>
        /// 判断指定索引的Item是否已经激活
        /// </summary>
        private bool IsItemActive(int index)
        {
            foreach (RectTransform item in _ActiveItems)
            {
                if (GetItemIndex(item) == index)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 创建或复用Item
        /// </summary>
        private void CreateOrReuseItem(int index)
        {
            if (index < 0 || index >= _DataList.Count)
                return;

            ScrollRectItemObject itemObject = null;

            // 尝试从对象池获取Item
            if (_ObjectPool.CanSpawn())
            {
                itemObject = _ObjectPool.Spawn();
            }
            else
            {
                // 如果对象池没有可用对象，创建新对象
                GameObject newItemObj = Instantiate(Go_ItemPrefab);
                string itemName = $"Item_{index}";
                itemObject = ScrollRectItemObject.Create(itemName, newItemObj);
                _ObjectPool.Register(itemObject, true);
            }

            if (itemObject == null)
                return;

            GameObject itemObj = itemObject.GameObject;
            RectTransform itemTrans = itemObj.GetComponent<RectTransform>();

            // 添加到映射表
            if (!_ItemMap.ContainsKey(itemObj))
            {
                _ItemMap.Add(itemObj, itemObject);
            }

            // 设置Item的父节点和位置
            itemTrans.SetParent(Tf_Content);
            // 应用Item锚点设置
            ApplyItemAnchors(itemTrans);
            itemTrans.localScale = Vector3.one;
            itemTrans.localRotation = Quaternion.identity;
            itemObj.SetActive(true);

            // 设置Item的位置
            float position = index * (_ItemSize + _Spacing);
            Vector2 anchoredPos = itemTrans.anchoredPosition;
            if (_ScrollDirection == EScrollDir.Horizontal)
            {
                anchoredPos.x = position;
                anchoredPos.y = 0;
            }
            else
            {
                anchoredPos.x = 0;
                anchoredPos.y = -position;
            }
            itemTrans.anchoredPosition = anchoredPos;

            // 设置Item数据
            if (itemObj.TryGetComponent<IScrollRectItem>(out var scrollItem))
            {
                scrollItem.OnActivate();
                scrollItem.SetData(_DataList[index], index);
            }

            // 保存Item索引
            itemTrans.name = $"Item_{index}";

            // 添加到活跃列表
            _ActiveItems.Add(itemTrans);
        }

        /// <summary>
        /// 回收Item
        /// </summary>
        private void RecycleItem(RectTransform item)
        {
            if (item == null)
                return;

            GameObject go = item.gameObject;

            // 调用Item的回收方法
            if (item.TryGetComponent<IScrollRectItem>(out var scrollItem))
            {
                scrollItem.OnRecycle();
            }

            // 从活跃列表移除
            _ActiveItems.Remove(item);

            // 从映射表中查找对应的对象池对象
            if (_ItemMap.TryGetValue(go, out ScrollRectItemObject itemObject))
            {
                // 禁用GameObject并重置父节点
                go.SetActive(false);
                item.SetParent(null);

                // 归还到对象池
                _ObjectPool.Unspawn(itemObject);
            }
        }

        /// <summary>
        /// 回收所有Item
        /// </summary>
        private void RecycleAllItems()
        {
            while (_ActiveItems.Count > 0)
            {
                RecycleItem(_ActiveItems[0]);
            }
        }

        /// <summary>
        /// 获取Item的索引
        /// </summary>
        private int GetItemIndex(RectTransform item)
        {
            if (item == null)
                return -1;

            string[] parts = item.name.Split('_');
            if (parts.Length < 2)
                return -1;

            int index;
            if (int.TryParse(parts[1], out index))
            {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// 滚动值变化回调
        /// </summary>
        private void OnScrollValueChanged(Vector2 value)
        {
            UpdateVisibleItems();
        }

        /// <summary>
        /// 组件销毁时释放资源
        /// </summary>
        protected void OnDestroy()
        {
            // 回收所有活跃的Item
            RecycleAllItems();

            // 清理映射表
            _ItemMap.Clear();

            // 销毁对象池
            if (_ObjectPool != null && GameEntry.ObjectPool.HasObjectPool<ScrollRectItemObject>(_PoolName))
            {
                GameEntry.ObjectPool.DestroyObjectPool<ScrollRectItemObject>(_PoolName);
                _ObjectPool = null;
            }
        }

        /// <summary>
        /// 添加单个数据项
        /// </summary>
        public void AddItem(object data)
        {
            _DataList.Add(data);
            CalculateContentSize();
            UpdateVisibleItems();
        }

        /// <summary>
        /// 移除指定索引的数据项
        /// </summary>
        public void RemoveItem(int index)
        {
            if (index < 0 || index >= _DataList.Count)
                return;

            _DataList.RemoveAt(index);
            CalculateContentSize();
            UpdateVisibleItems();
        }

        /// <summary>
        /// 更新指定索引的数据项
        /// </summary>
        public void UpdateItem(int index, object newData)
        {
            if (index < 0 || index >= _DataList.Count)
                return;

            _DataList[index] = newData;

            // 如果Item已经激活，更新其数据
            foreach (RectTransform item in _ActiveItems)
            {
                if (GetItemIndex(item) == index)
                {
                    if (item.TryGetComponent<IScrollRectItem>(out var scrollItem))
                    {
                        scrollItem.SetData(newData, index);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            _DataList.Clear();
            RecycleAllItems();
            CalculateContentSize();

            _FirstVisibleIndex = 0;
            _LastVisibleIndex = 0;
        }

        #region 属性

        /// <summary>
        /// 获取或设置滚动方向
        /// </summary>
        public EScrollDir _ScrollDir
        {
            get { return _ScrollDirection; }
            set { _ScrollDirection = value; }
        }

        /// <summary>
        /// 获取或设置Item间距
        /// </summary>
        public float _ItemSpacing
        {
            get { return _Spacing; }
            set { _Spacing = value; }
        }

        /// <summary>
        /// 获取或设置预加载数量
        /// </summary>
        public int _LoadCount
        {
            get { return _PreloadCount; }
            set { _PreloadCount = value; }
        }

        #endregion
    }
}