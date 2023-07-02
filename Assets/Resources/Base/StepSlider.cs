using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using Valve.VR.InteractionSystem;
public class StepSlider : Slider
{
    public float step = 0;
    // private SliderDrive sliderDrive;
    protected override void Set(float input, bool sendCallback)
    {
        if (step > 0) base.Set(Mathf.Round(input / step) * step, sendCallback);
        else base.Set(input, sendCallback);
    }

    public void UpdateMapping() {
        // if (sliderDrive == null)
        // {
            // sliderDrive = GetComponentInChildren<SliderDrive>();
            // if (sliderDrive == null)
            // {
                // Debug.Log("lost");
                // sliderDrive = gameObject.AddComponent<SliderDrive>();
            // }
        // }
        // sliderDrive.PassiveUpdate();
    }
}