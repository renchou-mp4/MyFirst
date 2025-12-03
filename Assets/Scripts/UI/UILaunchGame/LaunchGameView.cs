using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class LaunchGameView : UIFormLogic
{
    public Button _StartButton;
    private Action _clickAction;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        _clickAction = userData as Action;
        _StartButton.onClick.AddListener(OnClickStart);
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        Log.Info("OpenView-------LaunchGameView");
    }

    private void OnClickStart()
    {
        _clickAction?.Invoke();
        GameEntry.Scene.LoadScene("Assets/Scenes/MainGame.unity");
        GameEntry.UI.CloseUIForm(this.UIForm);
    }
}
