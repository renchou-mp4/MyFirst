using UnityEngine;

namespace yxy
{
    /// <summary>
    /// ScrollViewItem接口，所有ScrollView的子物体都需要继承此接口
    /// </summary>
    public interface IScrollRectItem
    {
        /// <summary>
        /// 设置Item数据
        /// </summary>
        /// <param name="data">Item对应的数据</param>
        /// <param name="index">Item在ScrollView中的索引</param>
        void SetData(object data, int index);

        /// <summary>
        /// 获取Item的大小
        /// </summary>
        /// <returns>Item的大小</returns>
        Vector2 GetItemSize();

        /// <summary>
        /// Item被回收时调用
        /// </summary>
        void OnRecycle();

        /// <summary>
        /// Item被激活时调用
        /// </summary>
        void OnActivate();
    }
}