
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public enum ScrollDir
{
     None = 0,
     Horizontal,
     Vertical,
}

[RequireComponent(typeof(ScrollRect))]
public class ScrollViewEx : SerializedMonoBehaviour
{
     private Dictionary<int,IScrollViewItem> _items = new();
     private Dictionary<int,Vector2> _itemSize = new();                     //item大小
     private ScrollRect _scrollRect;
     private ScrollDir _scrollDir = ScrollDir.Vertical;
     private float _ContentTotalLength = 0;                                 //内容总长度
     
     
     [Header("Item配置")]
     
     public List<GameObject> _ItemPrefabs = new();                         //item预制体
     public Vector2[] _ItemSizeTopThree =  new Vector2[3];                 //前3个Item大小，0为默认大小
     
     [EnumToggleButtons,ShowInInspector]
     public ScrollDir _ScrollDir {
          get => _scrollDir;
          set
          {
               _scrollDir = value;
               SetScrollDir(value);
          }
     } //滚动方向
     
     [ShowIf("_ScrollDir",ScrollDir.None)]
     public bool _Horizontal = true;//计算item时的方向


     private void Awake()
     {
          _scrollRect = GetComponent<ScrollRect>();
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
               _items.Add(i,item);
               _itemSize.Add(i,_ItemPrefabs[i].GetComponent<RectTransform>().sizeDelta);
          }


          if (_ScrollDir == ScrollDir.None)
          {
               if (!_Horizontal)
               {
                    for (int i = 0; i < itemCount; i++)
                    {
                         if (i < 3)
                         {
                              if(_ItemSizeTopThree[i].y != 0)
                                   _ContentTotalLength += _ItemSizeTopThree[i].y;
                              else
                                   _ContentTotalLength += _itemSize[i].y;
                         }
                         else
                         {
                              _ContentTotalLength+= _itemSize[i].y;
                         }
               
                    }
               }
               else
               {
                    for (int i = 0; i < itemCount; i++)
                    {
                         if (i < 3)
                         {
                              if(_ItemSizeTopThree[i].x != 0)
                                   _ContentTotalLength += _ItemSizeTopThree[i].x;
                              else
                                   _ContentTotalLength += _itemSize[i].x;
                         }
                         else
                         {
                              _ContentTotalLength+= _itemSize[i].x;
                         }
               
                    }
               }
          }
          else if(_scrollDir == ScrollDir.Horizontal)
          {
               for (int i = 0; i < itemCount; i++)
               {
                    if (i < 3)
                    {
                         if (_ItemSizeTopThree[i].x != 0)
                              _ContentTotalLength += _ItemSizeTopThree[i].x;
                         else
                              _ContentTotalLength += _itemSize[i].x;
                    }
                    else
                    {
                         _ContentTotalLength += _itemSize[i].x;
                    }

               }
          }
          
     }
     
     public void ShowItems()
     {
          
     }

     private void SetScrollDir(ScrollDir dir)
     {
          if(_scrollRect == null)
               _scrollRect = GetComponent<ScrollRect>();
          _scrollRect.horizontal = _ScrollDir!=ScrollDir.None && _ScrollDir == ScrollDir.Horizontal;
          _scrollRect.vertical = _ScrollDir!=ScrollDir.None && _ScrollDir == ScrollDir.Vertical;
          _scrollDir = dir;
     }
}
