using TMPro;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class ScrollRectTest : UIFormLogic, IScrollRectItem
    {
        [SerializeField]
        private TextMeshProUGUI Text;
        [SerializeField, Sirenix.OdinInspector.ReadOnly]
        private int _idx;

        public Vector2 GetItemSize()
        {
            return transform.GetComponent<RectTransform>().sizeDelta;
        }

        public void OnActivate()
        {
            Debug.Log("OnActivate: " + _idx);
        }

        public void SetData(object data, int index)
        {
            Text.text = data as string + " " + index;
            _idx = index;
        }

        void IScrollRectItem.OnRecycle()
        {
            OnRecycle();
            Debug.Log("OnRecycle: " + _idx);
        }
    }
}
