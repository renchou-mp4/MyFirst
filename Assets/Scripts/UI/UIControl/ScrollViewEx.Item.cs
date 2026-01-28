using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace yxy
{


    // 使用partial类将相关类分离到不同文件
    public partial class ScrollViewEx
    {
        /// <summary>
        /// ScrollView Item对象池包装类
        /// </summary>
        private class ScrollViewItemObject : ObjectBase
        {
            private GameObject Go_Item;

            public ScrollViewItemObject()
            {
            }

            public static ScrollViewItemObject Create(string name, GameObject go)
            {
                ScrollViewItemObject scrollViewItemObject = ReferencePool.Acquire<ScrollViewItemObject>();
                scrollViewItemObject.Initialize(name, go);
                scrollViewItemObject.Go_Item = go;
                return scrollViewItemObject;
            }

            public GameObject GameObject
            {
                get { return Go_Item; }
            }

            protected override void Release(bool isShutdown)
            {
                if (Go_Item != null)
                {
                    if (isShutdown)
                    {
                        GameObject.Destroy(Go_Item);
                    }
                    Go_Item = null;
                }
            }

            public override void Clear()
            {
                base.Clear();
                Go_Item = null;
            }
        }
    }
}