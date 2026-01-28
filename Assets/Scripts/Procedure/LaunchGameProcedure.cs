using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class LaunchGameProcedure : ProcedureBase
    {
        private bool isInit = false;

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
                GameEntry.UI.AddUIGroup(enumName);
            }

            isInit = true;
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            Log.Info("Procedure Update ----- LaunchGame");

            if (isInit)
            {
                ChangeState<PreloadProcedure>(procedureOwner);
            }
        }
    }
}

