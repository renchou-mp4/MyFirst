using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using yxy;

/// <summary>
/// 循环列表方向
/// </summary>
public enum ELoopListDir
{
    TopToBottom,
    BottomToTop,
    LeftToRight,
    RightToLeft,
}

/// <summary>
/// 对象池数据类
/// </summary>
[Serializable]
public class ScrollItemPoolData
{
    /// <summary>
    /// 预制体
    /// </summary>
    public GameObject Prefab;

    /// <summary>
    /// 预加载数量
    /// </summary>
    public int PreloadCount;

    /// <summary>
    /// Item距离间隔
    /// </summary>
    public float Space;

    /// <summary>
    /// Item非主方向的偏移
    /// </summary>
    public float PosOffset;
}

[Serializable]
public class LoopListInitData
{
    /// <summary>
    /// 第一个Item回收线
    /// </summary>
    public float RecycleLine0;

    /// <summary>
    /// 最后一个Item回收线
    /// </summary>
    public float RecycleLine1;

    /// <summary>
    /// 第一个Item生成线
    /// </summary>
    public float CreateLine0;

    /// <summary>
    /// 最后一个Item生成线
    /// </summary>
    public float CreateLine1;
}

[RequireComponent(typeof(ScrollRect))]
public class LoopListView : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //----------------------------------------------------------
    //Unity组件
    //----------------------------------------------------------
    private ScrollRect Sr_LoopList;
    private RectTransform Rtf_Viewport;
    private RectTransform Rtf_Content;

    //----------------------------------------------------------
    //公有属性
    //----------------------------------------------------------

    /// <summary>
    /// Item对象池数据，由用户在面板赋值
    /// </summary>
    public List<ScrollItemPoolData> ItemPoolDataList;

    /// <summary>
    /// Item排列方向
    /// </summary>
    public ELoopListDir LoopListDir;

    /// <summary>
    /// 是否垂直布局
    /// </summary>
    [HideInInspector]
    public bool IsVertical;

    //----------------------------------------------------------
    //私有成员
    //----------------------------------------------------------

    /// <summary>
    /// Item对象池字典
    /// </summary>
    private Dictionary<string, ScrollItemPool> _itemPoolDict = new();

    /// <summary>
    /// Item对象池列表
    /// </summary>
    private List<ScrollItemPool> _itemPoolList = new();

    /// <summary>
    /// 当前显示的Item列表，按展示顺序排列
    /// </summary>
    private List<LoopListItem> _itemShowList = new();

    /// <summary>
    /// Viewport四角的本地坐标缓存
    /// </summary>
    private Vector3[] _viewportLocalPos = new Vector3[4];

    /// <summary>
    /// Item四角的世界坐标缓存
    /// </summary>
    private Vector3[] _itemWorldPos = new Vector3[4];

    /// <summary>
    /// 上一帧Content的位置
    /// </summary>
    private Vector3 _lastFrameContentPos;

    /// <summary>
    /// 当前Content的速度
    /// </summary>
    private Vector2 _velocity;

    /// <summary>
    /// 拖拽事件数据缓存
    /// </summary>
    private PointerEventData _cachePointerEventData;

    /// <summary>
    /// 是否需要恢复Content速度
    /// </summary>
    private bool _isResumeVelocity;

    /// <summary>
    /// 第一个Item距viewport超过此距离后回收
    /// </summary>
    private float _recycleLine0 = 300;

    /// <summary>
    /// 最后一个Item距viewport超过此距离后回收
    /// </summary>
    private float _recycleLine1 = 300;

    /// <summary>
    /// 第一个item距viewport小于此距离后生成
    /// </summary>
    private float _createLine0 = 200;

    /// <summary>
    /// 最后一个Item距viewport小于此距离后生成
    /// </summary>
    private float _createLine1 = 200;

    /// <summary>
    /// Item总量
    /// </summary>
    private int _itemTotalCount;

    /// <summary>
    /// 当前生成的Item下标
    /// </summary>
    private int _curCreateItemIndex = -1;

    /// <summary>
    /// 当前已探明的可成功创建item的最小Index
    /// </summary>
    private int _minExploreBoundary;

    /// <summary>
    /// 当前已探明的可成功创建Item的最大Index
    /// </summary>
    private int _maxExploreBoundary;

    /// <summary>
    /// 是否需要探索最小Index
    /// </summary>
    private bool _isNeedExploreMin;

    /// <summary>
    /// 是否需要探索最大Index
    /// </summary>
    private bool _isNeedExploreMax;

    /// <summary>
    /// 帧数计数器
    /// </summary>
    private int _frameCount;

    /// <summary>
    /// 当前是否在拖拽
    /// </summary>
    private bool _isDrag;

    /// <summary>
    /// 是否已初始化完毕
    /// </summary>
    private bool _isInit;

    /// <summary>
    /// 调用方提供创建Item方法，主要用于注入自定义数据
    /// </summary>
    private Func<LoopListView, int, LoopListItem> _onGetItemByIndex;

    /// <summary>
    /// 开始拖拽回调
    /// </summary>
    private Action _onDragBeginEvent;

    /// <summary>
    /// 结束拖拽回调
    /// </summary>
    private Action _onDragEndEvent;

    /// <summary>
    /// 拖拽中回调
    /// </summary>
    private Action _onDragEvent;

    #region Unity生命周期

    void Awake()
    {
        Sr_LoopList = GetComponent<ScrollRect>();

        Rtf_Viewport = Sr_LoopList.viewport ?? Sr_LoopList.GetComponent<RectTransform>();

        Rtf_Content = Sr_LoopList.content;
        if (Rtf_Content == null)
        {
            Debug.LogError("LoopListView---Awake---Content为空！");
        }

        IsVertical = Sr_LoopList.vertical;
        Sr_LoopList.horizontal = !IsVertical;
    }

    void Update()
    {
        if (!_isInit)
        {
            return;
        }

        if (_isResumeVelocity)
        {
            _isResumeVelocity = false;

            if (IsVertical)
            {
                if (Sr_LoopList.velocity.y * _velocity.y > 0)
                {
                    Sr_LoopList.velocity = _velocity;
                }
            }
            else
            {
                if (Sr_LoopList.velocity.x * _velocity.x > 0)
                {
                    Sr_LoopList.velocity = _velocity;
                }
            }
        }

        UpdateLoopListView();
        ClearItemRecycleTmp();
        _lastFrameContentPos = Rtf_Content.anchoredPosition3D;
    }

    #endregion


    public void InitLoopListView(
        int itemTotalCount,
        Func<LoopListView, int, LoopListItem> OnGetItemByIndex,
        LoopListInitData initData = null
    )
    {
        _itemTotalCount = itemTotalCount;
        _onGetItemByIndex = OnGetItemByIndex;

        if (initData != null)
        {
            _createLine0 = initData.CreateLine0;
            _createLine1 = initData.CreateLine1;
            _recycleLine0 = initData.RecycleLine0;
            _recycleLine1 = initData.RecycleLine1;
        }

        if (_recycleLine0 <= _createLine0)
        {
            Debug.LogError(
                $"LoopListView---InitLoopListView---回收线0必须大于生成线0！recycle0:{_recycleLine0}  create0:{_createLine0}"
            );
        }
        if (_recycleLine1 <= _createLine1)
        {
            Debug.LogError(
                $"LoopListView---InitLoopListView---回收线1必须大于生成线1！recycle1:{_recycleLine1}  create1:{_createLine1}"
            );
        }

        SetPivot(Rtf_Content);
        SetAnchor(Rtf_Content);
        InitItemPool();

        _minExploreBoundary = 0;
        _maxExploreBoundary = 0;
        _isNeedExploreMin = true;
        _isNeedExploreMax = true;
        UpdateContentSize();

        _isInit = true;
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitItemPool()
    {
        _itemPoolDict.Clear();
        _itemPoolList.Clear();

        if (ItemPoolDataList.Count <= 0)
        {
            Debug.LogError("LoopListView---InitItemPool---没有对象池数据！请在面板中配置！");
            return;
        }
        for (int i = 0; i < ItemPoolDataList.Count; i++)
        {
            ScrollItemPoolData data = ItemPoolDataList[i];
            if (data == null)
            {
                Debug.LogError("LoopListView---InitItemPool---对象池数据为空！");
                continue;
            }

            if (data.Prefab == null)
            {
                Debug.LogError("LoopListView---InitItemPool---对象池数据中预制体为空！");
                continue;
            }

            if (_itemPoolDict.ContainsKey(data.Prefab.name))
            {
                Debug.LogError($"LoopListView---InitItemPool---【{data.Prefab.name}】对象池重复创建！");
                continue;
            }

            RectTransform rtf = data.Prefab.GetComponent<RectTransform>();
            if (rtf == null)
            {
                Debug.LogError($"LoopListView---InitItemPool---【{data.Prefab.name}】预制体上没有【RectTransform】组件！");
                continue;
            }

            SetPivot(rtf);
            SetAnchor(rtf);

            data.Prefab.GetOrAddComponent<LoopListItem>();

            ScrollItemPool pool = new ScrollItemPool();
            pool.InitPool(data.Prefab, data.PreloadCount, data.Space, data.PosOffset, Rtf_Content);
            _itemPoolList.Add(pool);
            _itemPoolDict.Add(data.Prefab.name, pool);
        }
    }

    private void UpdateLoopListView()
    {
        _frameCount++;

        if (IsVertical)
        {
            int curCheckCount = 0;
            int maxCheckCount = 9999;
            bool needCheck = true;

            //这里的循环是同帧内循环，确保回收和生成在同帧内至少都会调用一次。
            //限制检查次数,防止BUG导致卡死.(因回收和生成在不同帧导致Content实际大小频繁修改.条件：生成线>=回收线 且 Content卡在两个item的space之间)
            while (needCheck)
            {
                curCheckCount++;
                if (curCheckCount >= maxCheckCount)
                {
                    Debug.LogError($"LoopListView---UpdateLoopListView---垂直检查次数超过【{maxCheckCount}】次！请检查代码和配置！");
                    break;
                }
                needCheck = UpdateLoopListViewVertical();
            }
        }
        else
        {
            int curCheckCount = 0;
            int maxCheckCount = 9999;
            bool needCheck = true;

            while (needCheck)
            {
                curCheckCount++;
                if (curCheckCount >= maxCheckCount)
                {
                    Debug.LogError($"LoopListView---UpdateLoopListView---水平检查次数超过【{maxCheckCount}】次！请检查代码和配置！");
                    break;
                }
                needCheck = UpdateLoopListViewHorizantal();
            }
        }
    }

    private bool UpdateLoopListViewVertical()
    {
        //从上到下
        if (LoopListDir == ELoopListDir.TopToBottom)
        {
            //若此时Item数量为0，则创建第一个
            if (_itemShowList.Count == 0)
            {
                //保证newItem的位置在Viewport的顶部
                float pos =
                    Rtf_Content.anchoredPosition3D.y < 0 ? 0 : -Rtf_Content.anchoredPosition3D.y;
                LoopListItem newItem = IntervalGetItemByIndex(0);
                if (newItem == null)
                {
                    return false;
                }

                newItem.CacheTransform.anchoredPosition3D = new Vector3
                {
                    x = newItem.PosOffset,
                    y = pos,
                    z = 0,
                };
                _itemShowList.Add(newItem);
                UpdateContentSize();
                return true;
            }

            //判断边界
            Rtf_Viewport.GetLocalCorners(_viewportLocalPos);

            //先回收再补充
            LoopListItem firstItem = _itemShowList[0];
            //获取Item四角位置，顺序为：左下，左上，右上，右下
            firstItem.CacheTransform.GetWorldCorners(_itemWorldPos);
            Vector3 firstItem0 = Rtf_Viewport.InverseTransformPoint(_itemWorldPos[0]);
            Vector3 firstItem1 = Rtf_Viewport.InverseTransformPoint(_itemWorldPos[1]);

            if (
                !_isDrag
                && firstItem.FrameCountOnCreate != _frameCount
                && firstItem0.y > _viewportLocalPos[1].y + _recycleLine0
            )
            {
                //item左下角大于回收线
                _itemShowList.RemoveAt(0);
                RecycleItem(firstItem);
                UpdateContentSize();
                CheckIsNeedUpdateItemPos();
                return true;
            }

            LoopListItem lastItem = _itemShowList[^1];
            lastItem.CacheTransform.GetWorldCorners(_itemWorldPos);
            Vector3 lastItem0 = Rtf_Viewport.InverseTransformPoint(_itemWorldPos[0]);
            Vector3 lastItem1 = Rtf_Viewport.InverseTransformPoint(_itemWorldPos[1]);

            if (
                !_isDrag
                && lastItem.FrameCountOnCreate != _frameCount
                && lastItem1.y < _viewportLocalPos[0].y - _recycleLine1
            )
            {
                //item左上角小于回收线
                _itemShowList.RemoveAt(_itemShowList.Count - 1);
                RecycleItem(lastItem);
                UpdateContentSize();
                CheckIsNeedUpdateItemPos();
                return true;
            }

            //若先判断firstItem，由于是TopToBottom，firstItem上一个index = -1，导致未铺满整个viewport就结束检查。因此需要先判断lastItem
            if (lastItem0.y > _viewportLocalPos[0].y - _createLine1)
            {
                //item左下角大于生成线
                if (lastItem.Index > _maxExploreBoundary)
                {
                    //突破了最大探索边界
                    _maxExploreBoundary = lastItem.Index;
                    _isNeedExploreMax = true;
                }

                int newIndex = lastItem.Index + 1;
                if (newIndex <= _maxExploreBoundary || _isNeedExploreMax)
                {
                    LoopListItem newItem = IntervalGetItemByIndex(newIndex);
                    if (newItem == null)
                    {
                        _maxExploreBoundary = lastItem.Index;
                        _isNeedExploreMax = false;
                        CheckIsNeedUpdateItemPos();
                    }
                    else
                    {
                        _itemShowList.Add(newItem);
                        float y = lastItem.BottomY - lastItem.Space;
                        newItem.CacheTransform.anchoredPosition3D = new Vector3
                        {
                            x = newItem.PosOffset,
                            y = y,
                            z = 0,
                        };

                        UpdateContentSize();
                        CheckIsNeedUpdateItemPos();
                        if (newIndex > _maxExploreBoundary)
                        {
                            _maxExploreBoundary = newIndex;
                        }
                        return true;
                    }
                }
            }

            if (firstItem1.y < _viewportLocalPos[1].y + _createLine0)
            {
                //item左上角小于生成线
                if (firstItem.Index < _minExploreBoundary)
                {
                    //突破了最小探索边界
                    _minExploreBoundary = firstItem.Index;
                    _isNeedExploreMin = true;
                }

                int newIndex = firstItem.Index - 1;
                if (newIndex >= _minExploreBoundary || _isNeedExploreMin)
                {
                    LoopListItem newItem = IntervalGetItemByIndex(newIndex);
                    if (newItem == null)
                    {
                        _minExploreBoundary = firstItem.Index;
                        _isNeedExploreMin = false;
                    }
                    else
                    {
                        _itemShowList.Insert(0, newItem);
                        float y = firstItem.TopY + newItem.ItemSizeWithSpace;
                        newItem.CacheTransform.anchoredPosition3D = new Vector3
                        {
                            x = newItem.PosOffset,
                            y = y,
                            z = 0,
                        };

                        UpdateContentSize();
                        CheckIsNeedUpdateItemPos();

                        if (newIndex < _minExploreBoundary)
                        {
                            _minExploreBoundary = newIndex;
                        }
                        return true;
                    }
                }
            }
        }
        //从下到上
        else { }
        return false;
    }

    private bool UpdateLoopListViewHorizantal()
    {
        return false;
    }

    /// <summary>
    /// 更新ContentSize
    /// </summary>
    private void UpdateContentSize()
    {
        if (_itemShowList.Count <= 0)
        {
            return;
        }

        float size = GetContentSize();

        if (IsVertical)
        {
            if (Rtf_Content.rect.height != size)
            {
                //使用这个方法可以忽视锚点的影响直接将sizeDelta设置为指定大小,是绝对尺寸
                //真实尺寸 = sizeDelta + 父尺寸 * (anchorMax - anchorMin).只有当锚点重合sizeDelta才等于真实大小
                Rtf_Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            }
        }
        else
        {
            if (Rtf_Content.rect.width != size)
            {
                Rtf_Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            }
        }
    }

    /// <summary>
    /// 获取Content大小
    /// </summary>
    /// <returns></returns>
    private float GetContentSize()
    {
        if (_itemShowList.Count <= 0)
        {
            return 0;
        }
        if (_itemShowList.Count == 1)
        {
            return _itemShowList[0].ItemSize;
        }
        if (_itemShowList.Count == 2)
        {
            return _itemShowList[0].ItemSizeWithSpace + _itemShowList[1].ItemSize;
        }

        float size = 0;
        for (int i = 0; i < _itemShowList.Count - 1; i++)
        {
            size += _itemShowList[i].ItemSizeWithSpace;
        }
        size += _itemShowList[^1].ItemSize;

        return size;
    }

    /// <summary>
    /// 是否需要更新Item位置
    /// </summary>
    /// <returns></returns>
    private void CheckIsNeedUpdateItemPos()
    {
        if (_itemShowList.Count == 0)
            return;

        if (LoopListDir == ELoopListDir.TopToBottom)
        {
            LoopListItem firstItem = _itemShowList[0];
            LoopListItem lastItem = _itemShowList[^1];

            //第一个Item超出viewport || 第一个Item前没有其他Item且Item的位置不在顶部
            if (
                firstItem.TopY > 0
                || (firstItem.Index == _minExploreBoundary && firstItem.TopY != 0)
            )
            {
                UpdateAllShownItemPos();
                return;
            }

            //位置数据一致性校验，Content大小严格为Items总高度+Space，若不一致则需要重新排列
            float ContentY = GetContentSize();
            if (
                (-lastItem.BottomY) > ContentY
                || (lastItem.Index == _maxExploreBoundary && (-lastItem.BottomY) != ContentY)
            )
            {
                UpdateAllShownItemPos();
                return;
            }
        }
    }

    /// <summary>
    /// 更新已显示的Item位置
    /// </summary>
    private void UpdateAllShownItemPos()
    {
        if (_itemShowList.Count == 0)
            return;

        //计算当前Content的速度
        float deltaTime = Time.deltaTime;
        float minDeltaTime = 1.0f / 120.0f;
        deltaTime = deltaTime < minDeltaTime ? minDeltaTime : deltaTime;
        _velocity = (Rtf_Content.anchoredPosition3D - _lastFrameContentPos) / deltaTime;

        if (LoopListDir == ELoopListDir.TopToBottom)
        {
            //若当前Item[0]之前的Item（现在已回收）曾改变大小，ItemPosMgr中的数据已改变，但当前Item[0]的位置是由改变前的大小累加得到，与正确位置产生了偏差，因此需要修正Content位置，以防止当前viewport中显示的Item发生位置跳变
            float pos = 0;
            float pos1 = _itemShowList[0].CacheTransform.anchoredPosition3D.y;
            float diff = pos - pos1;
            float curY = pos;

            //更新items位置
            for (int i = 0; i < _itemShowList.Count; i++)
            {
                LoopListItem item = _itemShowList[i];
                item.CacheTransform.anchoredPosition3D = new Vector3(item.PosOffset, curY, 0);
                curY -= item.ItemSizeWithSpace;
            }

            //修正Content位置偏移
            if (diff != 0)
            {
                Vector3 contentPos = Rtf_Content.anchoredPosition3D;
                contentPos.y -= diff;
                Rtf_Content.anchoredPosition3D = contentPos;
            }
        }

        if (_isDrag)
        {
            //重置拖拽基点,防止下一帧OnDrag抹掉修正(OnDrag在Update之前,到这里时已经更新为当前帧)
            Sr_LoopList.OnBeginDrag(_cachePointerEventData);
            //重算边界,让后续速度基于新的边界数据
            Sr_LoopList.Rebuild(CanvasUpdate.PostLayout);
            //还原惯性速度,保证手感连续
            Sr_LoopList.velocity = _velocity;
            //防止Rebuild和LateUpdate修改了速度.只在速度没反向才恢复,避免在边界回弹时强行拉成反方向
            _isResumeVelocity = true;
        }
    }

    /// <summary>
    /// 对外接口，提供初始化后的Item
    /// </summary>
    /// <returns></returns>
    public LoopListItem GetNewItem(string prefabName)
    {
        if (!_itemPoolDict.ContainsKey(prefabName))
        {
            Debug.LogError($"LoopListView---GetNewItem---【{prefabName}】不存在该预制体对象池！请检查面板配置！");
            return null;
        }
        return _itemPoolDict[prefabName].GetItem(_curCreateItemIndex);
    }

    /// <summary>
    /// 内部使用，根据Index创建Item。
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// 获取新Item流程：IntervalGetItemByIndex->_onGetItemByIndex(注入数据)->GetNewItem
    private LoopListItem IntervalGetItemByIndex(int index)
    {
        if (index < 0 || index >= _itemTotalCount)
        {
            return null;
        }
        _curCreateItemIndex = index;

        LoopListItem item = _onGetItemByIndex(this, index);
        if (item == null)
        {
            Debug.LogError($"LoopListView---IntervalGetItemByIndex---创建Item失败！Index:{index}");
            return null;
        }
        item.Index = index;
        item.ParentView = this;
        item.FrameCountOnCreate = _frameCount;
        return item;
    }

    /// <summary>
    /// 设置轴点
    /// </summary>
    private void SetPivot(RectTransform rtf)
    {
        Vector2 pivot = rtf.pivot;
        switch (LoopListDir)
        {
            case ELoopListDir.TopToBottom:
                pivot.y = 1;
                break;
            case ELoopListDir.BottomToTop:
                pivot.y = 0;
                break;
            case ELoopListDir.LeftToRight:
                pivot.x = 0;
                break;
            case ELoopListDir.RightToLeft:
                pivot.x = 1;
                break;
        }
        rtf.pivot = pivot;
    }

    /// <summary>
    /// 设置锚点
    /// </summary>
    private void SetAnchor(RectTransform rtf)
    {
        Vector2 anchorMax = rtf.anchorMax;
        Vector2 anchorMin = rtf.anchorMin;

        switch (LoopListDir)
        {
            case ELoopListDir.TopToBottom:
                anchorMax.y = 1;
                anchorMin.y = 1;
                break;
            case ELoopListDir.BottomToTop:
                anchorMax.y = 0;
                anchorMin.y = 0;
                break;
            case ELoopListDir.LeftToRight:
                anchorMax.x = 0;
                anchorMin.x = 0;
                break;
            case ELoopListDir.RightToLeft:
                anchorMax.x = 1;
                anchorMin.x = 1;
                break;
        }

        rtf.anchorMax = anchorMax;
        rtf.anchorMin = anchorMin;
    }

    #region 转发对象池操作

    /// <summary>
    /// 转发对象池回收Item操作
    /// </summary>
    /// <param name="item"></param>
    private void RecycleItem(LoopListItem item)
    {
        if (item == null)
        {
            Debug.LogError("LoopListView---RecycleItem---回收的指定对象为空！");
            return;
        }

        if (item.PrefabName.IsNullOrEmpty())
        {
            Debug.LogError("LoopListView---RecycleItem---回收对象的预制体名称为空！");
            return;
        }

        if (!_itemPoolDict.TryGetValue(item.PrefabName, out ScrollItemPool pool))
        {
            Debug.LogError($"LoopListView---RecycleItem---回收的指定对象没有对应的对象池！【{item.PrefabName}】");
            return;
        }

        pool.RecycleItem(item);
    }

    /// <summary>
    /// 转发对象池清理临时回收Item操作,每帧都回收(设置状态为false)
    /// </summary>
    private void ClearItemRecycleTmp()
    {
        for (int i = 0; i < _itemPoolList.Count; i++)
        {
            _itemPoolList[i].ClearItemRecycleTmp();
        }
    }

    #endregion

    #region 拖动接口实现

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        CacheDragPointerEventData(eventData);
        _onDragEvent?.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        _isDrag = true;
        CacheDragPointerEventData(eventData);
        _onDragBeginEvent?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        _isDrag = false;
        _onDragEndEvent?.Invoke();
    }

    private void CacheDragPointerEventData(PointerEventData data)
    {
        if (_cachePointerEventData == null)
        {
            _cachePointerEventData = new PointerEventData(EventSystem.current);
        }
        //这里只存储四个值是只需要这几个，防止data中其他的值污染ScrollRect
        _cachePointerEventData.button = data.button;
        _cachePointerEventData.position = data.position;
        _cachePointerEventData.pointerPressRaycast = data.pointerPressRaycast;
        _cachePointerEventData.pointerCurrentRaycast = data.pointerCurrentRaycast;
    }

    #endregion
}
