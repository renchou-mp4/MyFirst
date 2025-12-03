using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public struct CommonSliderData
{
    public readonly UnityAction _btnLeftAction;
    public readonly UnityAction _btnRightAction;
    public readonly int _maxValue;
    public readonly int _minValue;
    public readonly int _defaultValue;
    public readonly bool _interactableSlider;
    public readonly bool _interactableInput;

    public CommonSliderData(
        UnityAction btnLeftAction, 
        UnityAction btnRightAction, 
        int maxValue, 
        int minValue,
        int defaultValue = 0,
        bool interactableSlider = true,
        bool interactableInput = true)
    {
        _btnLeftAction = btnLeftAction;
        _btnRightAction = btnRightAction;
        _maxValue = maxValue;
        _minValue = minValue;
        _defaultValue = defaultValue;
        _interactableSlider = interactableSlider;
        _interactableInput = interactableInput;
    }
}

public class CommonSlider : MonoBehaviour
{
    private CommonSliderData _data;
    
    public ButtonCustom _Btn_Left;
    public ButtonCustom _Btn_Right;
    public SliderCustom _Slider;
    public TMP_InputField _Input_Count;
    
    
    public void Init(CommonSliderData data)
    {
        _data = data;
        _Input_Count.interactable = _data._interactableInput;
        _Slider.interactable = _data._interactableSlider;
        _Slider.wholeNumbers = true;
        _Slider.maxValue = _data._maxValue;
        _Slider.minValue = _data._minValue;
        
        _Btn_Left?.onClick.AddListener(OnClickBtnLeft);
        _Btn_Right?.onClick.AddListener(OnClickBtnRight);
        if (_data._btnLeftAction != null || _data._btnRightAction != null)
        {
            _Btn_Left?.onClick.AddListener(_data._btnLeftAction);
            _Btn_Right?.onClick.AddListener(_data._btnRightAction);
        }
        
        _Slider.onValueChanged.AddListener(OnValueChanged);
        _Slider.value = _data._defaultValue;

        ChangeInputText();
    }
    
    private void OnClickBtnLeft()
    {
        _Slider.value = _Slider.value - 1 < _data._minValue ? _data._minValue : _Slider.value - 1;
        ChangeInputText();
    }

    private void OnClickBtnRight()
    {
        _Slider.value = _Slider.value + 1 > _data._maxValue ? _data._maxValue : _Slider.value + 1;
        ChangeInputText();
    }

    private void OnValueChanged(float changeValue)
    {
        _Slider.value = changeValue;
        ChangeInputText();
    }

    private void ChangeInputText()
    {
        _Input_Count.text = $"{_Slider.value}/{_data._maxValue}";
    }
}
