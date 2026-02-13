using UnityEngine;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class Prefab_Image : UIFormLogic, IScrollRectItem1
    {
        public int GetPrefabIndex()
        {
            return 0;
        }

        public Vector2 GetItemSize()
        {
            return GetComponent<RectTransform>().sizeDelta;
        }

        public void SetData(object data, int index)
        {
            // 设置数据逻辑
            Log.Info($"Prefab_Image SetData: {data}, index: {index}");
        }
    }
}

