using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class LaunchGameView : UIFormLogic
{
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        Log.Info("进入界面-------LaunchGameView");
        GameEntry.GetComponent<SceneComponent>().LoadScene("Assets/Scenes/MainGame.unity");
    }
}
