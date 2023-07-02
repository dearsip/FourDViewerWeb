using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRInputField : MonoBehaviour
{
    public KeyboardScript keyboard;
    InputField inputField;
    public GameObject layout;

    private void Start()
    {
        inputField = GetComponent<InputField>();
    }

    private void Update()
    {
        if (inputField.isFocused)
        {
            keyboard.TextField = inputField;
            keyboard.ShowLayout(layout);
        }
        //else if (!inputField.isFocused && keyboard.TextField == inputField)
        //{
        //    keyboard.CloseAllLayouts();
        //    keyboard.TextField = null;
        //}
    }
}
