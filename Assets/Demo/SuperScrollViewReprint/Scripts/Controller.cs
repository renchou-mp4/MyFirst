using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using yxy;

public class Controller : MonoBehaviour
{
    /// <summary>
    /// 循环列表
    /// </summary>
    public LoopListView LoopListView;

    /// <summary>
    /// item数据列表
    /// </summary>
    private List<string> _itemDataList = new();

    void Start()
    {
        if (LoopListView == null)
        {
            Debug.LogError("循环列表未赋值！");
            return;
        }

        TestData();
        LoopListView.InitLoopListView(_itemDataList.Count, OnGetItemByIndex);
    }

    private void TestData()
    {
        for (int i = 0; i < 100; i++)
        {
            _itemDataList.Add($"TestData{i}");
        }
    }

    /// <summary>
    /// 调用方提供创建Item方法，注入自定义数据
    /// </summary>
    /// <param name="view"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private LoopListItem OnGetItemByIndex(LoopListView view, int index)
    {
        if (view == null)
        {
            Debug.LogError("循环列表为空！");
            return null;
        }

        LoopListItem item = view.GetNewItem("CustomItem");
        if (item == null)
        {
            Debug.LogError($"获取Item失败！Index：{index}");
            return null;
        }

        CustomItem customItem = item.gameObject.GetComponent<CustomItem>();
        if (customItem == null)
        {
            Debug.LogError("自定义脚本为空！");
            return null;
        }
        customItem.SetData(_itemDataList[index], index);

        return item;
    }
}
