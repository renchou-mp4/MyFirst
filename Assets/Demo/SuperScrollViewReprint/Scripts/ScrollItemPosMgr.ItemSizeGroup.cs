using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public partial class ScrollItemPosController
{
    public class ItemSizeGroup
    {
        //---------------------------------------------------------
        //公有属性
        //---------------------------------------------------------
        public float GroupIndex = 0;

        public float GroupSize = 0;

        public float GroupStartPos = 0;

        public float GroupEndPos = 0;

        public float[] ItemSizeArray = null;

        public float[] ItemStartPosArray = null;

        public int ItemCount = 0;

        public bool IsDirty => _dirtyStartIndex < ItemCount;

        //---------------------------------------------------------
        //私有属性
        //---------------------------------------------------------
        private float _itemDefaultSize = 0;

        private int _dirtyStartIndex = MaxItemCountPerGroup;

        private int _maxNoZeroIndex = 0;

        public ItemSizeGroup(int index, float itemDefaultSize)
        {
            GroupIndex = index;
            _itemDefaultSize = itemDefaultSize;
            Init();
        }

        public void Init()
        {
            ItemSizeArray = new float[MaxItemCountPerGroup];
            if (_itemDefaultSize != 0)
            {
                for (int i = 0; i < MaxItemCountPerGroup; i++)
                {
                    ItemSizeArray[i] = _itemDefaultSize;
                }
            }
            ItemStartPosArray = new float[MaxItemCountPerGroup];
            ItemStartPosArray[0] = 0;
            ItemCount = MaxItemCountPerGroup;
            GroupSize = _itemDefaultSize * ItemCount;
            _dirtyStartIndex = _itemDefaultSize == 0 ? MaxItemCountPerGroup : 0;
        }

        /// <summary>
        /// 获取指定Item的起始位置
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float GetItemStartPos(int index)
        {
            return GroupStartPos + ItemStartPosArray[index];
        }

        /// <summary>
        /// 获取指定位置的Item下标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int GetItemIndexByPos(float pos)
        {
            if (pos < GroupStartPos || pos > GroupEndPos)
            {
                return -1;
            }

            //二分查找
            int low = 0;
            int height = ItemCount - 1;
            if (_itemDefaultSize == 0)
            {
                height = _maxNoZeroIndex;
            }

            while (low <= height)
            {
                int mid = (low + height) / 2;
                float startPos = ItemStartPosArray[mid];
                float endPos = startPos + ItemSizeArray[mid];
                if (pos >= startPos && pos <= endPos)
                {
                    return mid;
                }
                else if (pos < startPos)
                {
                    height = mid;
                }
                else if (pos > endPos)
                {
                    low = mid;
                }
            }
            return -1;
        }

        /// <summary>
        /// 设置指定Item的大小
        /// </summary>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public float SetItemSize(int index, float size)
        {
            if (index >= ItemCount)
            {
                Debug.Log($"index非法！index:{index} ItemCount:{ItemCount}");
                return 0;
            }

            if (index > 0 && index > _maxNoZeroIndex)
            {
                _maxNoZeroIndex = index;
            }

            float oldSize = ItemSizeArray[index];
            if (oldSize == size)
            {
                return 0;
            }

            ItemSizeArray[index] = size;

            if (index < _dirtyStartIndex)
            {
                _dirtyStartIndex = index;
            }

            float ds = size - oldSize;
            GroupSize += ds;
            return ds;
        }

        /// <summary>
        /// 设置组中Item数量
        /// </summary>
        /// <param name="count"></param>
        public void SetItemCount(int count)
        {
            if (count < 0 || count > MaxItemCountPerGroup)
            {
                Debug.LogError($"Count非法！【{count}】 每组最大count数：{MaxItemCountPerGroup}");
                return;
            }

            if (count < _maxNoZeroIndex)
            {
                _maxNoZeroIndex = count;
            }

            if (count == ItemCount)
            {
                return;
            }

            ItemCount = count;
            RecalculateGroupSize();
        }

        /// <summary>
        /// 重新计算Group大小
        /// </summary>
        public void RecalculateGroupSize()
        {
            GroupSize = 0;
            for (int i = 0; i < ItemCount; i++)
            {
                GroupSize += ItemSizeArray[i];
            }
        }

        /// <summary>
        /// 更新所有Item起始位置
        /// </summary>
        public void UpdateAllItemStartPos()
        {
            if (_dirtyStartIndex >= ItemCount)
                return;

            _dirtyStartIndex = _dirtyStartIndex < 1 ? 1 : _dirtyStartIndex;
            for (int i = _dirtyStartIndex; i < ItemCount - 1; i++)
            {
                ItemStartPosArray[i] = ItemStartPosArray[i - 1] + ItemSizeArray[i - 1];
            }
            _dirtyStartIndex = ItemCount;
        }

        /// <summary>
        /// 清除所有Item大小数据
        /// </summary>
        public void ClearOldSizeData()
        {
            for (int i = 0; i < MaxItemCountPerGroup; i++)
            {
                ItemSizeArray[i] = 0;
            }
        }
    }
}
