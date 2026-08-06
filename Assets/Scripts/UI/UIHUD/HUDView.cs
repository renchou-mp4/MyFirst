using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class HUDView : UIFormLogic
    {
        /// 设置界面按鈕
        [SerializeField]
        private ButtonCustom Btn_Setting;

        /// 列表满动视图
        [SerializeField]
        private ScrollRectEx _itemView;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Btn_Setting.onClick.AddListener(OnClickSetting);

            List<ScrollRectExData> dataList = new List<ScrollRectExData>();
            for (int i = 0; i < 20; i++)
            {
                dataList.Add(new ScrollRectExData() { Data = $"item{i + 1}", PrefabIndex = 0 });
            }

            _itemView.SetData(dataList);
        }

        private void OnClickSetting()
        {
            GameEntry.UI.OpenUIForm("Assets/Prefabs/UI/SettingView.prefab", "Dialog");
        }
    }
}
