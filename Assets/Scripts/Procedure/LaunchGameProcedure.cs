using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace yxy
{
    public class LaunchGameProcedure : ProcedureBase
    {
        /// 是否已完成初始化
        private bool _isInit = false;

        protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);
            Log.Info("Procedure Init ----- LaunchGame");
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("Procedure Enter ----- LaunchGame");

            foreach (var enumName in EnumHelper.GetEnumNames<EUIGroups>())
            {
                GameEntry.UI.AddUIGroup(enumName);
            }

            _isInit = true;
        }

        protected override void OnUpdate(
            IFsm<IProcedureManager> procedureOwner,
            float elapseSeconds,
            float realElapseSeconds
        )
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            Log.Info("Procedure Update ----- LaunchGame");

            if (_isInit)
            {
                ChangeState<PreloadProcedure>(procedureOwner);
            }
        }
    }
}
