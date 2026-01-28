using UnityGameFramework.Runtime;

namespace yxy
{
    public class SettingView : UIFormLogic
    {
        public CommonSlider _Slider;
        public ButtonCustom _Btn_Return;
        public ButtonCustom _Btn_Setting;


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _Slider.Init(new CommonSliderData
                (
                    btnLeftAction: null,
                    btnRightAction: null,
                    maxValue: 10,
                    minValue: 0,
                    defaultValue: 5,
                    interactableSlider: true,
                    interactableInput: false
                )
            );

            _Btn_Setting.onClick.AddListener(OnClickSetting);
            _Btn_Return.onClick.AddListener(OnClickReturn);
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
