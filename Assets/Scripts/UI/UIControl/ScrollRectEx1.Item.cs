using GameFramework.ObjectPool;
using UnityEngine;

namespace yxy
{
    public abstract class ScrollRectItemObject1 : ObjectBase
    {
        public abstract Vector2 GetItemSize();
        public abstract void SetData(object data, int index);
        protected override void Release(bool isShutdown)
        {
            throw new System.NotImplementedException();
        }
    }
}