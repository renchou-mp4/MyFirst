using UnityEngine;

namespace yxy
{
    public interface IScrollRectItem1
    {
        public Vector2 GetItemSize();

        public int GetPrefabIndex();

        public void SetData(object data, int index);
    }
}