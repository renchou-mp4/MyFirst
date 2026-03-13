using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSimple : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI TMP_text;
    [SerializeField]
    private Image Img_Bg;
    [SerializeField]
    private Button Btn_Item;

    private int _index;
    private object _data;
    private Action<int> _onClickItem;

    public void Init()
    {
        Btn_Item?.onClick.AddListener(OnClickItem);
    }

    public void SetItemData(string context, int index, Action<int> onClickItem)
    {
        _index = index;
        _data = context;
        _onClickItem = onClickItem;

        if (TMP_text.text != context)
        {
            TMP_text.text = context;
        }

        Img_Bg.color = index % 2 == 0 ? Color.white : Color.gray;
    }

    private void OnClickItem()
    {
        _onClickItem?.Invoke(_index);
    }
}
