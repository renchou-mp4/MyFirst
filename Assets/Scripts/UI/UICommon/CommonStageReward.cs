using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

[Serializable]
public class CommonStageReward : MonoBehaviour,IStageReward
{
    public ImageCustom _Img_Icon;
    public ImageCustom _Img_NodeBg;
    public ImageCustom _Img_Node;
    public TextMeshProUGUI _Txt_Count;
    
}
