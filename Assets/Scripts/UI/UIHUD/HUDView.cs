using UnityGameFramework.Runtime;

namespace yxy
{
    public class HUDView : UIFormLogic
    {
        public ButtonCustom _Btn_Setting;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _Btn_Setting.onClick.AddListener(OnClickSetting);
        }

        private void OnClickSetting()
        {
            GameEntry.UI.OpenUIForm("Assets/Prefabs/UI/SettingView.prefab", "Dialog");
        }
    }
}
