using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderCustom : Slider
{
    protected override void Reset()
    {
        base.Reset();
        interactable = false;
    }
}
