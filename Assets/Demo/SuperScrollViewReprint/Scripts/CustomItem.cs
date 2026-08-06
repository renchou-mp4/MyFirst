using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomItem : MonoBehaviour
{
    //---------------------------------------------------------
    //Unity组件
    //---------------------------------------------------------

    public Image Img_Bg;
    public TMP_Text Txt_Info;

    //---------------------------------------------------------
    //公有属性
    //---------------------------------------------------------

    public float Width { get; private set; }

    public float Height { get; private set; }

    private void Awake()
    {
        RectTransform rtf = GetComponent<RectTransform>();
        Width = rtf.rect.width;
        Height = rtf.rect.height;
    }

    public void SetData(string data, int index)
    {
        Txt_Info.SetText(data + "idx:" + index);
        if (index % 2 == 0)
        {
            Img_Bg.color = Color.gray;
        }
        else
        {
            Img_Bg.color = new Color(0.000f, 0.518f, 0.886f, 1.000f);
        }
    }
}
