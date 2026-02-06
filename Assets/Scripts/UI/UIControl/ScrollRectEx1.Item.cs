using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace yxy
{
    public class ScrollRectItemObject1 : ObjectBase
    {
        public ScrollRectItemObject1()
        {
        }

        public static ScrollRectItemObject1 Create(string name, GameObject go)
        {
            ScrollRectItemObject1 obj = ReferencePool.Acquire<ScrollRectItemObject1>();
            obj.Initialize(name, go);
            return obj;
        }

        protected override void Release(bool isShutdown)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SetData(ScrollRectExData data, int index) { }

    }
}