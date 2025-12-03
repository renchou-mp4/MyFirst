using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewCustom : ScrollRect
{
     [TableList(ShowIndexLabels = true)]
     public List<CommonStageReward> _Items = new List<CommonStageReward>();
}
