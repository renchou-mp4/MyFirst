using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

[Serializable]
public class CommonStageReward : MonoBehaviour,IStageReward
{
    public int _ItemId { get; set; }
    public ImageCustom _Img_Icon { get; set; }
    public ImageCustom _Img_NodeBg { get; set; }
    public ImageCustom _Img_Node { get; set; }
    public TextMeshProUGUI _Txt_Count { get; set; }
}
