using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;

public class PreloadProcedure : ProcedureBase
{
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        Log.Info("Procedure Enter ----- Preload");
        GameEntry.UI.OpenUIForm("Assets/Prefabs/UI/LaunchGameView.prefab", "Dialog",new Action(() =>
        {
            ChangeState<MainGameProcedure>(procedureOwner);
        }));
    }
}
