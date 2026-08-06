using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace yxy
{
    public struct CommonSliderData
    {
        /// 左按鈕点击回调
        public readonly UnityAction BtnLeftAction;

        /// 右按鈕点击回调
        public readonly UnityAction BtnRightAction;

        /// 最大値
        public readonly int MaxValue;

        /// 最小値
        public readonly int MinValue;

        /// 默认値
        public readonly int DefaultValue;

        /// Slider 是否可交互
        public readonly bool InteractableSlider;

        /// 输入框是否可交互
        public readonly bool InteractableInput;

        public CommonSliderData(
            UnityAction btnLeftAction,
            UnityAction btnRightAction,
            int maxValue,
            int minValue,
            int defaultValue = 0,
            bool interactableSlider = true,
            bool interactableInput = true
        )
        {
            BtnLeftAction = btnLeftAction;
            BtnRightAction = btnRightAction;
            MaxValue = maxValue;
            MinValue = minValue;
            DefaultValue = defaultValue;
            InteractableSlider = interactableSlider;
            InteractableInput = interactableInput;
        }
    }

    public class CommonSlider : MonoBehaviour
    {
        // ── 序列化字段 (Serialized Fields) ───────────────────────

        /// 左按鈕
        [SerializeField]
        private ButtonCustom Btn_Left;

        /// 右按鈕
        [SerializeField]
        private ButtonCustom Btn_Right;

        /// 滑动条
        [SerializeField]
        private SliderCustom Sld_Slider;

        /// 数量输入框
        [SerializeField]
        private TMP_InputField _inputCount;

        // ── 私有字段 (Private Fields) ───────────────────────────

        /// 当前数据
        private CommonSliderData _data;

        /// <summary>
        /// 初始化滑动条组件
        /// </summary>
        public void Init(CommonSliderData data)
        {
            _data = data;
            _inputCount.interactable = _data.InteractableInput;
            Sld_Slider.interactable = _data.InteractableSlider;
            Sld_Slider.wholeNumbers = true;
            Sld_Slider.maxValue = _data.MaxValue;
            Sld_Slider.minValue = _data.MinValue;

            Btn_Left?.onClick.AddListener(OnClickBtnLeft);
            Btn_Right?.onClick.AddListener(OnClickBtnRight);
            if (_data.BtnLeftAction != null || _data.BtnRightAction != null)
            {
                Btn_Left?.onClick.AddListener(_data.BtnLeftAction);
                Btn_Right?.onClick.AddListener(_data.BtnRightAction);
            }

            Sld_Slider.onValueChanged.AddListener(OnValueChanged);
            Sld_Slider.value = _data.DefaultValue;

            ChangeInputText();
        }

        private void OnClickBtnLeft()
        {
            Sld_Slider.value =
                Sld_Slider.value - 1 < _data.MinValue ? _data.MinValue : Sld_Slider.value - 1;
            ChangeInputText();
        }

        private void OnClickBtnRight()
        {
            Sld_Slider.value =
                Sld_Slider.value + 1 > _data.MaxValue ? _data.MaxValue : Sld_Slider.value + 1;
            ChangeInputText();
        }

        private void OnValueChanged(float changeValue)
        {
            Sld_Slider.value = changeValue;
            ChangeInputText();
        }

        private void ChangeInputText()
        {
            _inputCount.text = $"{Sld_Slider.value}/{_data.MaxValue}";
        }
    }
}
