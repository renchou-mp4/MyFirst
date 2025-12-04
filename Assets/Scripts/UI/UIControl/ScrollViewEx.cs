
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public enum ScrollViewDirection
{
     None = 0,
     Horizontal,
     Vertical,
}

[RequireComponent(typeof(ScrollRect))]
public class ScrollViewEx : SerializedMonoBehaviour
{
     private Dictionary<int,IScrollViewItem> _Items = new();
     private Dictionary<int,Vector2> _ItemSize = new();                     //item大小
     private ScrollRect _ScrollRect;
     private float _ContentTotalH = 0;                                     //内容总高度
     
     
     [Header("Item配置")]
     
     [Tooltip("Item预制体")]
     public List<GameObject> _ItemPrefabs = new();                         //item预制体
     [Tooltip("前3个Item大小，0为默认大小")]
     public Vector2[] _ItemSizeTopThree =  new Vector2[3];                 //前3个Item大小，0为默认大小
     [Tooltip("滚动方向")]
     public ScrollViewDirection _Direction = ScrollViewDirection.Vertical; //滚动方向


     private void Awake()
     {
          _ScrollRect = GetComponent<ScrollRect>();
     }

     private void Init(int itemCount)
     {
          if(_ItemPrefabs.Count<=0) return;
          
          //获取Item脚本和大小
          for (int i = 0;i< itemCount;i++)
          {
               var item = _ItemPrefabs[i].GetComponent<IScrollViewItem>();
               if(item==null) continue;
               
               item._ItemId = i;
               _Items.Add(i,item);
               _ItemSize.Add(i,_ItemPrefabs[i].GetComponent<RectTransform>().sizeDelta);
          }
     }
     
     public void ShowItems()
     {
          
     }
     
     public void SetDirection(ScrollViewDirection direction)
     {
          GetComponent<ScrollRect>().horizontal = direction!=ScrollViewDirection.None && direction==ScrollViewDirection.Horizontal;
          GetComponent<ScrollRect>().vertical = direction!=ScrollViewDirection.None && direction==ScrollViewDirection.Vertical;
     }
}
