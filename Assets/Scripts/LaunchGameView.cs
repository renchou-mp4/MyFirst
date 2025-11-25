using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class LaunchGameView : UIFormLogic
{
    public Button _StartButton;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        _StartButton.onClick.AddListener(OnClickStart);
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        Log.Info("进入界面-------LaunchGameView");
    }

    private void OnClickStart()
    {
        if (GameEntry.GetComponent<ProcedureComponent>().CurrentProcedure is not LaunchGameProcedure procedure) return;
        
        procedure.LaunchGameChangeState<MainGameProcedure>();
        GameEntry.GetComponent<SceneComponent>().LoadScene("Assets/Scenes/GameScene.unity");
    }
}
