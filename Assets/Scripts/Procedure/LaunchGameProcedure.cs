using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;
using yxy;

public class LaunchGameProcedure : ProcedureBase
{
    protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnInit(procedureOwner);
        Log.Info("Procedure Init ----- LaunchGame");
    }

    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        Log.Info("Procedure Enter ----- LaunchGame");

        foreach (var enumName in EnumHelper.GetEnumNames<GameConstants.UIGroups>())
        {
            GameEntry.GetComponent<UIComponent>().AddUIGroup(enumName);
        }
        
        GameEntry.GetComponent<UIComponent>().OpenUIForm("Assets/Prefabs/UI/LaunchGameView.prefab","Dialog");
        
    }
}
