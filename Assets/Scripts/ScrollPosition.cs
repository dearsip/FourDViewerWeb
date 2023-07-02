using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ScrollPosition : MonoBehaviour
{
    public Scrollbar scrollbar;
    public bool start;
    RectTransform rectTransform;
    Vector2 min = new Vector2(0, 0);
    Vector2 max = new Vector2(0, 1);
    // Start is called before the first frame update
    void Start()
    {
        this.rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            max.y = scrollbar.size;
            rectTransform.anchorMax = max;
        }
        else
        {
            min.y = 1 - scrollbar.size;
            rectTransform.anchorMin = min;
        }
    }
}
