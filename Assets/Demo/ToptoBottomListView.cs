using System.Collections.Generic;
using UnityEngine;
using yxy;

public class ToptoBottomListView : MonoBehaviour
{
    [SerializeField]
    private LoopListView _loopListView;

    private List<string> _datas = new();

    void Start()
    {
        TestData();
        _loopListView.InitListView(_datas.Count, OnGetItemByIndex);
    }

    private LoopListViewItem OnGetItemByIndex(LoopListView listView, int index)
    {
        if (index < 0 || index >= _datas.Count)
        {
            return null;
        }

        LoopListViewItem item = listView.CreateItem("Item");

        ItemSimple itemSimple = item.GetComponent<ItemSimple>();
        if (itemSimple != null)
        {
            itemSimple.Init();
            itemSimple.SetItemData(_datas[index], index, null);
        }

        return item;
    }

    private void TestData()
    {
        _datas.Clear();
        for (int i = 0; i < 100; i++)
        {
            _datas.Add("Test Data:" + i);
        }
    }
}
