using UnityEngine;

namespace yxy
{
    public class LoopListViewItem : MonoBehaviour
    {
        private RectTransform Rtf_CacheRectTransform;
        public RectTransform _CacheRectTransform
        {
            get
            {
                if (Rtf_CacheRectTransform == null)
                {
                    Rtf_CacheRectTransform = GetComponent<RectTransform>();
                }
                return Rtf_CacheRectTransform;
            }
        }

        private LoopListView _parentListView;
        public LoopListView _ParentListView
        {
            get => _parentListView;
            set => _parentListView = value;
        }

        private int _createFrame = 0;
        public int _CreateFrame
        {
            get => _createFrame;
            set => _createFrame = value;
        }

        private string _itemPrefabName;
        public string _ItemPrefabName
        {
            get => _itemPrefabName;
            set => _itemPrefabName = value;
        }

        private int _itemIndex;
        public int _ItemIndex
        {
            get => _itemIndex;
            set => _itemIndex = value;
        }


        private float _itemPosOffset;
        /// <summary>
        /// 另一个方向的偏移量，垂直列表就是水平偏移，水平列表就是垂直偏移
        /// </summary>l
        public float _ItemPosOffset
        {
            get => _itemPosOffset;
            set => _itemPosOffset = value;
        }

        private float _itemPadding;
        public float _ItemPadding
        {
            get => _itemPadding;
            set => _itemPadding = value;
        }

        public float _ItemSize
        {
            get
            {
                return _ParentListView._isVertical ? _CacheRectTransform.rect.height : _CacheRectTransform.rect.width;
            }
        }

        public float TopY
        {
            get
            {
                return _CacheRectTransform.anchoredPosition3D.y;
            }
        }

        public float BottomY
        {
            get
            {
                return _CacheRectTransform.anchoredPosition3D.y - _CacheRectTransform.rect.height;
            }
        }
    }
}
