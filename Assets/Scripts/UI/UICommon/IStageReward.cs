using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IStageReward :IScrollViewItem
{
    public ImageCustom _Img_Icon { get; set; }
    public ImageCustom _Img_NodeBg{ get; set; }
    public ImageCustom _Img_Node{ get; set; }
    public TextMeshProUGUI _Txt_Count{ get; set; }
}
