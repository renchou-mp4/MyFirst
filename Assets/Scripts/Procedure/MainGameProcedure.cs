using GameFramework.Fsm;
using GameFramework.Procedure;

namespace yxy
{
    public class MainGameProcedure : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.UI.OpenUIForm("Assets/Prefabs/UI/UIHUD/HUDView.prefab", "HUD");
        }
    }
}
