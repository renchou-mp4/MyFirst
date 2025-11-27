using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class LaunchGameView : UIFormLogic
{
    public Button _StartButton;
    public Action _ClickAction;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        _ClickAction = userData as Action;
        _StartButton.onClick.AddListener(OnClickStart);
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        Log.Info("OpenView-------LaunchGameView");
    }

    private void OnClickStart()
    {
        _ClickAction?.Invoke();
        GameEntry.GetComponent<SceneComponent>().LoadScene("Assets/Scenes/MainGame.unity");
        GameEntry.GetComponent<UIComponent>().CloseUIForm(this.UIForm);
    }
}
