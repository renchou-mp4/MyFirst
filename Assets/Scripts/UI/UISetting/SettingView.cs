using UnityEngine;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class SettingView : UIFormLogic
    {
        /// 滑动条组件
        [SerializeField]
        private CommonSlider _slider;

        /// 返回按鈕
        [SerializeField]
        private ButtonCustom Btn_Return;

        /// 设置按鈕
        [SerializeField]
        private ButtonCustom Btn_Setting;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _slider.Init(
                new CommonSliderData(
                    btnLeftAction: null,
                    btnRightAction: null,
                    maxValue: 10,
                    minValue: 0,
                    defaultValue: 5,
                    interactableSlider: true,
                    interactableInput: false
                )
            );

            Btn_Setting.onClick.AddListener(OnClickSetting);
            Btn_Return.onClick.AddListener(OnClickReturn);
        }

        private void OnClickReturn()
        {
            GameEntry.UI.CloseUIForm(this.UIForm);
        }

        private void OnClickSetting()
        {
            OnClickReturn();
        }
    }
}
