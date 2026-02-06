
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

    public class ScrollRectExData
    {
        public object Data;
        public Vector2 Size;
        public int PrefabIndex;
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

        [SerializeField] private Dictionary<string, IObjectPool<ScrollRectItemObject1>> _itemPoolDic = new(); /// 对象池字典<名称，对象池>
        [SerializeField] private List<string> _itemPoolNames = new();                     /// 对象池名称列表
        [SerializeField] private float _itemPoolExpireTime = float.MaxValue;     /// 对象池过期时间
        [SerializeField] private int _itemPoolCapacity = int.MaxValue;           /// 对象池容量
        [SerializeField] private int _itemPriority = 0;                          /// 对象池优先级 
        [SerializeField] private int _preloadCount = 3;                          /// 预加载数量


        [Header("Item预制体相关")]

        [SerializeField] private GameObject[] Go_ItemPrefabs;                    /// 列表项预制体
        [SerializeField] private Vector2 _itemAnchorMin = Vector2.one * -1;      /// item锚点最小值
        [SerializeField] private Vector2 _itemAnchorMax = Vector2.one * -1;      /// item锚点最大值


        [Header("数据相关")]

        [SerializeField] private List<ScrollRectExData> _dataList = new();          /// 数据列表
        [SerializeField] private Dictionary<GameObject, ScrollRectItemObject1> _itemDict = new(); /// Item字典

        [SerializeField] private float _itemSpace = 0;                           /// Item间距
        [SerializeField] private Vector2 _itemPadding = Vector2.zero;            /// Item边距


        void Awake()
        {
            InitScrollRect();
        }

        public void SetData(List<ScrollRectExData> dataList)
        {
            if (dataList == null)
            {
                Log.Error($"ItemPool_{gameObject.name}: SetData传入的数据是空! ");
                return;
            }

            _dataList = dataList;
            InitItemPoolList();

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
        private void InitItemPoolList()
        {
            if (Go_ItemPrefabs == null || Go_ItemPrefabs.Length <= 0)
            {
                Log.Error($"ItemPool_{gameObject.name}: Item Prefab 是空！");
                return;
            }

            //若存在旧对象池则先销毁
            if (_itemPoolDic is { Count: > 0 })
            {
                foreach (var poolKV in _itemPoolDic)
                {
                    if (poolKV.Value != null && GameEntry.ObjectPool.HasObjectPool<ScrollRectItemObject1>(poolKV.Key))
                    {
                        GameEntry.ObjectPool.DestroyObjectPool<ScrollRectItemObject1>(poolKV.Key);
                    }
                }
                _itemPoolDic.Clear();
            }


            //创建新的对象池列表
            _itemPoolNames.Clear();
            for (int i = 0; i < Go_ItemPrefabs.Length; i++)
            {
                string poolName = $"ItemPool_{GetInstanceID()}_{Go_ItemPrefabs[i].name}";
                _itemPoolNames.Add(poolName);
                _itemPoolDic.Add(poolName,
                    GameEntry.ObjectPool.CreateSingleSpawnObjectPool<ScrollRectItemObject1>(
                        poolName,
                        _itemPoolCapacity,
                        _itemPoolExpireTime,
                        _itemPriority));
            }
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
                if (_dataList[i] == null)
                {
                    Log.Error($"ItemPool_{gameObject.name}: 数据列表中存在空数据! 索引: {i}");
                    continue;
                }
                itemSize = _dataList[i].Size;
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

        private int CalculateVisibleCount()
        {
            float viewportSize = _scrollDir == EScrollDir1.Vertical ? Rtf_Viewport.rect.height : Rtf_Viewport.rect.width;
            int count = 0;
            for (int i = 0; i < _dataList.Count && viewportSize > 0; i++)
            {
                viewportSize -= _scrollDir == EScrollDir1.Vertical ? _dataList[i].Size.y : _dataList[i].Size.x;
                count++;
            }
            return count;
        }

        /// <summary>
        /// 初始化Item
        /// </summary>
        private void InitItems()
        {
            if (Go_ItemPrefabs == null || Go_ItemPrefabs.Length <= 0)
            {
                Log.Error($"ItemPool_{gameObject.name}: Item Prefab 是空！");
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

            //初始化位置
            int showCount = CalculateVisibleCount() + _preloadCount;//预加载的不可见


            //初始化数据
            for (int i = 0; i < showCount; i++)
            {
                ScrollRectItemObject1 item = GetItem(i);
                item.SetData(_dataList[i], i);
            }
        }

        private ScrollRectItemObject1 GetItem(int index)
        {
            if (_itemPoolDic == null)
            {
                Log.Error($"ItemPool_{gameObject.name}: itemPoolDic 是空！");
                return null;
            }
            if (_dataList[index] == null)
            {
                Log.Error($"ItemPool_{gameObject.name}: 数据列表中存在空数据! 索引: {index}");
                return null;
            }

            int prefabIndex = _dataList[index].PrefabIndex;
            string poolName = _itemPoolNames[prefabIndex];

            if (!_itemPoolDic.ContainsKey(poolName))
            {
                Log.Error($"ItemPool_{gameObject.name}: 不存在名称为{poolName}的对象池！");
                return null;
            }

            ScrollRectItemObject1 item;
            if (_itemPoolDic[poolName].CanSpawn())
            {
                item = _itemPoolDic[poolName].Spawn();
            }
            else
            {
                if (Go_ItemPrefabs == null || Go_ItemPrefabs.Length <= prefabIndex)
                {
                    Log.Error($"ItemPool_{gameObject.name}: Item Prefab 数组越界！ prefabIndex: {prefabIndex}");
                    return null;
                }
                GameObject go = Instantiate(Go_ItemPrefabs[prefabIndex], Rtf_Content);
                item = ScrollRectItemObject1.Create(Go_ItemPrefabs[prefabIndex].name, go);
                _itemPoolDic[poolName].Register(item, false);
            }
            return item;
        }
    }
}