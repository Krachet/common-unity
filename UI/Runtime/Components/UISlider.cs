using UnityEngine;
using UnityEngine.UI;

public class UISlider : Slider
{
    public override float value {
        set
        {
            base.value = Mathf.Clamp(value, minValue, maxValue);
            fillRect.gameObject.SetActive(base.value != 0);
        }
        get => base.value;
    }
}