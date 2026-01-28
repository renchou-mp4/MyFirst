using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class UIGroupHelper : UIGroupHelperBase
    {
        private Canvas _canvas;

        public const int DepthFactor = 100;

        public override void SetDepth(int depth)
        {
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = depth * DepthFactor;
        }

        private void Awake()
        {
            _canvas = this.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        private void Start()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

    }
}

