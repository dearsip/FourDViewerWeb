using UnityEngine;

public class ToggleView : MonoBehaviour
{
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
