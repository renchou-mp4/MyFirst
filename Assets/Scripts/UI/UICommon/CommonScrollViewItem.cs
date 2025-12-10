using System.Collections;
using System.Collections.Generic;
using GameFramework.ObjectPool;
using UnityEngine;

public abstract class CommonScrollViewItem : ObjectBase,IScrollViewItem
{
    public abstract int _ItemId { get; set; }

    public abstract void Create();

    protected override void Release(bool isShutdown)
    {
        _ItemId = 0;
    }
}
