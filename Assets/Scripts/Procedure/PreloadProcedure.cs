using System;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class PreloadProcedure : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("Procedure Enter ----- Preload");
            GameEntry.UI.OpenUIForm("Assets/Prefabs/UI/LaunchGameView.prefab", "Dialog", new Action(() =>
            {
                ChangeState<MainGameProcedure>(procedureOwner);
            }));
        }
    }
}
