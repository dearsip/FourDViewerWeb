using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Valve.VR;
using UnityEngine.UI;
using UnityEngine.XR;
using WebXR;

// Display の位置を動かす。
public class DisplayMove : MonoBehaviour
{
    // public SteamVR_Input_Sources hand;
    // public SteamVR_Action_Boolean grab;
    // public SteamVR_Action_Pose pose;
    public Transform hand;
    [SerializeField] WebXRController controller;
    public Vector3 lastPos;
    public Quaternion lastRot;
    private bool button;
    public Slider size;
    private Quaternion relarot;
    void Update()
    {
        // if (grab.GetState(hand)) {
            // transform.position += (pose.GetLocalPosition(hand) - pose.GetLastLocalPosition(hand))*size.value;
            // relarot = pose.GetLocalRotation(hand) * Quaternion.Inverse(pose.GetLastLocalRotation(hand));
            // relarot.x = 0; relarot.z = 0;
            // transform.rotation *= relarot;
        // }
        if (controller.GetButton(WebXRController.ButtonTypes.Grip)) {
            transform.localPosition += (hand.localPosition - lastPos)*size.value;
            relarot = hand.localRotation * Quaternion.Inverse(lastRot);
            relarot.x = 0; relarot.z = 0;
            transform.rotation *= relarot;
        }
        lastPos = hand.localPosition;
        lastRot = hand.localRotation;
    }
}