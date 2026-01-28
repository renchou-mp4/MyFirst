using UnityEngine;

namespace yxy
{
    public static class UnityAPIHelper
    {

        /// <summary>
        /// 自定义SetActive
        /// </summary>
        ///Unity 中 Destroy(go) 后，go 在 C# 层不会立即变为 null，但 Unity 重载了 == 运算符：当对象被销毁时，if (go) 返回 false
        ///如果跳过null检查直接调用 go.SetActive() → 抛出 MissingReferenceException，因为 go 已被销毁，但在 C# 层 go仍然存在
        public static void SetSelfActive(this GameObject go, bool active)
        {
            if (!go || go.activeSelf == active) return;
            go.SetActive(active);
        }
    }
}