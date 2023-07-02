using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueToText : MonoBehaviour
{
    public Slider slider;
    InputField text;

    private void Start()
    {
        text = GetComponent<InputField>();
        slider.onValueChanged.AddListener((float f) => { text.text = slider.value.ToString(); });
    }
}
