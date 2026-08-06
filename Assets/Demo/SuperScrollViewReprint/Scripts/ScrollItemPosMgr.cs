using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScrollItemPosController
{
    public const int MaxItemCountPerGroup = 100;

    public float TotalSize = 0;

    private float _itemDefaultSize = 20;

    private int _dirtyStartIndex = int.MaxValue;

    private int _maxNotEmptyGroupIndex = 0;

    private List<ItemSizeGroup> _groups = new();

    public ScrollItemPosController(float itemDefaultSize)
    {
        _itemDefaultSize = itemDefaultSize;
    }

    public void SetMaxItemCount(int maxItemCount)
    {
        int groupCount = maxItemCount / MaxItemCountPerGroup;
        groupCount = maxItemCount % MaxItemCountPerGroup == 0 ? groupCount : groupCount + 1;

        if (groupCount > _groups.Count)
        {
            //原本的group数量不足
        }
        for (int i = 0; i < groupCount; i++)
        {
            _groups[i] = new ItemSizeGroup(i, _itemDefaultSize);
            _groups[i].Init();
        }

        _maxNotEmptyGroupIndex = _groups.Count - 1;
    }
}
