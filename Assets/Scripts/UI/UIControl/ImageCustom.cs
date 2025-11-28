using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif


[AddComponentMenu("UI/ImageCustom")]
public class ImageCustom : Image
{
    protected override void Reset()
    {
        base.Reset();
        raycastTarget = false;
    }
    
    #if UNITY_EDITOR
    
    /// <summary>
    /// 在右键菜单中添加自定义Image组件选项，和Inspector的AddComponent菜单不是一个
    /// </summary>
    /// <param name="menuCommand"></param>
    [MenuItem("GameObject/UI/ImageCustom", false, 2000)]
     public static void AddImage(MenuCommand menuCommand)
     {
         GameObject go = CreateUIRoot(menuCommand, "Image",new Vector3(100,100));
         go.AddComponent<ImageCustom>();
     }
    
    /// <summary>
    /// 创建自定义Image对象
    /// </summary>
    /// <param name="menuCommand"></param>
    /// <param name="name"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private static GameObject CreateUIRoot(MenuCommand menuCommand, string name, Vector3 size)
    {
         GameObject parent = menuCommand.context as GameObject;
         Canvas parentCanvas = null;
         
         //尝试获取Canvas
         if (parent != null)
         {
             parentCanvas = parent.GetComponentInParent<Canvas>();
             if (parentCanvas == null)
                 parentCanvas = Object.FindObjectOfType<Canvas>();
             if (parentCanvas == null)
                 parent = null;
         }
    
         //没有获取到Canvas则创建Parent并添加Canvas组件
         if (parentCanvas == null)
         {
             GameObject canvasObj =  new GameObject("Canvas");
             parentCanvas = canvasObj.AddComponent<Canvas>();
             parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
             
             canvasObj.AddComponent<CanvasScaler>();
             canvasObj.AddComponent<GraphicRaycaster>();
    
             canvasObj.layer = LayerMask.NameToLayer("UI");
             parent =  canvasObj;
         }
         
         //创建UI对象
         GameObject child = new GameObject(name);
         if(parent != null)
            child.transform.SetParent(parent.transform);
         child.transform.localScale = Vector3.one;
         child.transform.localPosition = Vector3.zero;
         child.transform.localRotation = Quaternion.identity;
    
         child.AddComponent<RectTransform>().sizeDelta = size;
         
        child.layer = LayerMask.NameToLayer("UI");
    
        Selection.activeGameObject = child;
         return child;
    }
    
    #endif
}
