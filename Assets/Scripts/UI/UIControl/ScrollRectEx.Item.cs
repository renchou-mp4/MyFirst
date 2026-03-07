using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace yxy
{
    public class ScrollRectItemObject : ObjectBase
    {
        public GameObject Go_Item { get; private set; }

        public int PrefabIndex { get; private set; }
        public ScrollRectItemObject()
        {
        }

        public static ScrollRectItemObject Create(string name, GameObject go, int prefabIndex)
        {
            ScrollRectItemObject obj = ReferencePool.Acquire<ScrollRectItemObject>();
            obj.Go_Item = go;
            obj.PrefabIndex = prefabIndex;
            obj.Initialize(name, go);
            return obj;
        }

        protected override void Release(bool isShutdown)
        {
            Go_Item = null;
        }
    }
}