using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Valve.VR;
using UnityEngine.UI;
using UnityEngine.XR;
using WebXR;

public class DisplayMove : MonoBehaviour
{
    // public SteamVR_Input_Sources hand;
    // public SteamVR_Action_Boolean grab;
    // public SteamVR_Action_Pose pose;
    public Transform handL, handR;
    [SerializeField] WebXRController controllerL, controllerR;
    public Vector3 lastPosL, lastPosR;
    public Quaternion lastRotL, lastRotR;
    public Slider size;
    private Quaternion relarot;
    void Update()
    {
        // if (grab.GetState(handL)) {
            // transform.position += (pose.GetLocalPosition(handL) - pose.GetLastLocalPosition(handL))*size.value;
            // relarot = pose.GetLocalRotation(handL) * Quaternion.Inverse(pose.GetLastLocalRotation(handL));
            // relarot.x = 0; relarot.z = 0;
            // transform.rotation *= relarot;
        // }
        if (controllerL.GetButton(WebXRController.ButtonTypes.Grip)) {
            transform.localPosition += (handL.localPosition - lastPosL)*size.value;
            relarot = handL.localRotation * Quaternion.Inverse(lastRotL);
            relarot.x = 0; relarot.z = 0;
            transform.rotation *= relarot;
        }
        else if (controllerR.GetButton(WebXRController.ButtonTypes.Grip)) {
            transform.localPosition += (handR.localPosition - lastPosR)*size.value;
            relarot = handR.localRotation * Quaternion.Inverse(lastRotR);
            relarot.x = 0; relarot.z = 0;
            transform.rotation *= relarot;
        }
        lastPosL = handL.localPosition;
        lastRotL = handL.localRotation;
        lastPosR = handR.localPosition;
        lastRotR = handR.localRotation;
    }
}