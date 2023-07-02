using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopCamera : MonoBehaviour
{
    public Camera center, left, right, fix, fLeft, fRight;
    private int mode = 5;
    private Rect rCenter, rLeft, rRight;
    // Start is called before the first frame update
    void Start()
    {
        rCenter = center.rect;
        rLeft = left.rect;
        rRight = right.rect;

        left.enabled = false;
        right.enabled = false;
        fLeft.enabled = false;
        fRight.enabled = false;
        center.enabled = true;
        fix.enabled = true;
        center.rect = rLeft;
        fix.rect = rRight;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
            switch (mode++ % 7)
            {
                case 0:
                    left.enabled = true;
                    right.enabled = true;
                    left.rect = rRight;
                    right.rect = rLeft;
                    break;
                case 1:
                    left.rect = rLeft;
                    right.rect = rRight;
                    break;
                case 2:
                    left.enabled = false;
                    right.enabled = false;
                    fix.enabled = true;
                    break;
                case 3:
                    fix.enabled = false;
                    fLeft.enabled = true;
                    fRight.enabled = true;
                    fLeft.rect = rRight;
                    fRight.rect = rLeft;
                    break;
                case 4:
                    fLeft.rect = rLeft;
                    fRight.rect = rRight;
                    break;
                case 5:
                    fLeft.enabled = false;
                    fRight.enabled = false;
                    fix.enabled = true;
                    center.rect = rLeft;
                    fix.rect = rRight;
                    break;
                case 6:
                    fix.enabled = false;
                    center.rect = rCenter;
                    fix.rect = rCenter;
                    break;
            }
    }
}
