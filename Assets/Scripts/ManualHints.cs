using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using Valve.VR;
// using Valve.VR.InteractionSystem;

public class ManualHints : MonoBehaviour
{
    // public Hand rightHand;
    // public Hand leftHand;
    // public SteamVR_Input_Sources hand;
    // public SteamVR_Action_Boolean trigger, move, menu, grab;
    private bool visibleHints;
    private float time;

    private const KeyCode HINT_LEFT_MENU = KeyCode.B;
    private const KeyCode HINT_LEFT_MOVE = KeyCode.V;
    private const KeyCode HINT_LEFT_TRIGGER = KeyCode.C;
    private const KeyCode HINT_LEFT_GRAB = KeyCode.X;
    private const KeyCode HINT_RIGHT_MENU = KeyCode.N;
    private const KeyCode HINT_RIGHT_MOVE = KeyCode.M;
    private const KeyCode HINT_RIGHT_TRIGGER = KeyCode.Comma;
    private const KeyCode HINT_RIGHT_GRAB = KeyCode.Period;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(HINT_LEFT_MENU)) ControllerButtonHints.ShowButtonHint(leftHand, menu);
        // if (Input.GetKeyDown(HINT_LEFT_MOVE)) ControllerButtonHints.ShowButtonHint(leftHand, move);
        // if (Input.GetKeyDown(HINT_LEFT_TRIGGER)) ControllerButtonHints.ShowButtonHint(leftHand, trigger);
        // if (Input.GetKeyDown(HINT_LEFT_GRAB)) ControllerButtonHints.ShowButtonHint(leftHand, grab);
        // if (Input.GetKeyDown(HINT_RIGHT_MENU)) ControllerButtonHints.ShowButtonHint(rightHand, menu);
        // if (Input.GetKeyDown(HINT_RIGHT_MOVE)) ControllerButtonHints.ShowButtonHint(rightHand, move);
        // if (Input.GetKeyDown(HINT_RIGHT_TRIGGER)) ControllerButtonHints.ShowButtonHint(rightHand, trigger);
        // if (Input.GetKeyDown(HINT_RIGHT_GRAB)) ControllerButtonHints.ShowButtonHint(rightHand, grab);

        // if (Input.GetKeyUp(HINT_LEFT_MENU)) ControllerButtonHints.HideButtonHint(leftHand, menu);
        // if (Input.GetKeyUp(HINT_LEFT_MOVE)) ControllerButtonHints.HideButtonHint(leftHand, move);
        // if (Input.GetKeyUp(HINT_LEFT_TRIGGER)) ControllerButtonHints.HideButtonHint(leftHand, trigger);
        // if (Input.GetKeyUp(HINT_LEFT_GRAB)) ControllerButtonHints.HideButtonHint(leftHand, grab);
        // if (Input.GetKeyUp(HINT_RIGHT_MENU)) ControllerButtonHints.HideButtonHint(rightHand, menu);
        // if (Input.GetKeyUp(HINT_RIGHT_MOVE)) ControllerButtonHints.HideButtonHint(rightHand, move);
        // if (Input.GetKeyUp(HINT_RIGHT_TRIGGER)) ControllerButtonHints.HideButtonHint(rightHand, trigger);
        // if (Input.GetKeyUp(HINT_RIGHT_GRAB)) ControllerButtonHints.HideButtonHint(rightHand, grab);
    }

}