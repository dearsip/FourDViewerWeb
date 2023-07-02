using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayText : MonoBehaviour
{
    public Transform head;
    public Text text;
    private CanvasGroup canvasGroup;
    private const float offsetForward = .3f;
    private const float offsetUp = -.15f;
    private readonly Quaternion offsetRotate = Quaternion.Euler(15,0,0);
    private float lastTime, fadeTime;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, head.transform.position + head.forward * offsetForward + head.up * offsetUp, 6.0f * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, offsetRotate*head.transform.rotation, 12.0f * Time.deltaTime);
        if (canvasGroup.alpha > 0) 
        { 
            if (lastTime > 0) lastTime -= Time.deltaTime;
            else canvasGroup.alpha -= fadeTime * Time.deltaTime;
        }
    }

    public void ShowText(string text, float lastTime = .5f, float fadeTime = .3f)
    {
        this.text.text = text;
        canvasGroup.alpha = .7f;
        this.lastTime = lastTime;
        this.fadeTime = fadeTime;
    }
}
