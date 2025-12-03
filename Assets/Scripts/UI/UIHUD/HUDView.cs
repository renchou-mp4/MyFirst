using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

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
