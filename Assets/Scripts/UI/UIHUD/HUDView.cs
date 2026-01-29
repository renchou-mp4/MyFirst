using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class HUDView : UIFormLogic
    {
        [SerializeField]
        private ButtonCustom Btn_Setting;
        [SerializeField]
        private ScrollRectEx Sr_ItemView;


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Btn_Setting.onClick.AddListener(OnClickSetting);

            Sr_ItemView.SetData(new List<object>()
            {
                "item1",
                "item2",
                "item3",
                "item4",
                "item5",
                "item6",
                "item7",
                "item8",
                "item9",
                "item10",
            });
        }

        private void OnClickSetting()
        {
            GameEntry.UI.OpenUIForm("Assets/Prefabs/UI/SettingView.prefab", "Dialog");
        }
    }
}
