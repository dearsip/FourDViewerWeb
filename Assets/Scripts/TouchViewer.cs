using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchViewer : MonoBehaviour
{
    public GameObject sprite;
    private List<GameObject> sprites = new List<GameObject>();
    private RectTransform rectTransform;
    private Vector2 localPoint;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        int touchCount = Input.touchCount + (Input.GetMouseButton(0) ? 1 : 0);
        for (int i = 0; i < touchCount; i++)
        {
            Vector2 position = i == Input.touchCount ? Input.mousePosition : Input.GetTouch(i).position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                position,
                null,
                out localPoint
            );
            if (i > sprites.Count - 1)
            {
                GameObject newSprite = Instantiate(sprite, transform);
                newSprite.SetActive(true);
                sprites.Add(newSprite);
            }
            sprites[i].GetComponent<RectTransform>().anchoredPosition = localPoint;
        }
        for (int i = touchCount; i < sprites.Count; i++)
        {
            Destroy(sprites[touchCount]);
            sprites.RemoveAt(touchCount);
        }
    }
}
