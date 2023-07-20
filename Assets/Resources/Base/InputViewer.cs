using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using WebXR;

public class InputViewer : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;
    // public SteamVR_Input_Sources hand;
    // public SteamVR_Action_Boolean move, grip;
    public Transform pose, screen;
    [SerializeField] WebXRController controller;
    private Vector3 fromPos, fromForward, dPos, origin, reg;
    private Quaternion fromRot, dRot;
    public bool isMover;
    private bool limit3D, disableLeftAndRight;
    public Toggle toggleLimit3D;
    private enum State { INACTIVE, HANDISACTIVE, KEYISACTIVE }
    private State state = State.INACTIVE;
    private bool activate;

    private const float threshold = 0.1f;
    private const float dist = 0.06f;
    private const float ringScale = 0.05f;
    private static readonly Vector3 root = new Vector3(0f, 0f, 0.1f);
    private float fLen = 2f/Mathf.Sqrt(5f);

    public void ToggleLimit3D (bool isOn) {
        limit3D = isOn;
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        vertices = new Vector3[arrowVector.Length + ringVector.Length];
        colors = new Color[arrowVector.Length + ringVector.Length];
        triangles = new int[arrowTriangles.Length + ringTriangles.Length];
        mesh.Clear();

        Array.Copy(arrowTriangles, triangles, arrowTriangles.Length);
        for (int i = 0; i < ringTriangles.Length; i++)
            triangles[arrowTriangles.Length+i] = ringTriangles[i]+arrowVector.Length;
        for (int i = 0; i < colors.Length; i++) colors[i] = color;
        GetComponent<MeshFilter>().sharedMesh = mesh;

        // if (isMover) grip.AddOnStateDownListener(LeftGrip, hand);
    }

    // private void LeftGrip(SteamVR_Action_Boolean fromBoolean, SteamVR_Input_Sources fromSource)
    // {
        // disableLeftAndRight = !disableLeftAndRight;
    // }
    // private void LeftGrip()
    // {
        // disableLeftAndRight = !disableLeftAndRight;
    // }

    // Update is called once per frame
    void Update()
    {
        if (controller.GetButtonDown(WebXRController.ButtonTypes.ButtonA)) {
            activate = true;
            state = State.HANDISACTIVE;
        } else if (KeyStateDown()) { // we do not consider conflicts
            activate = true;
            state = State.KEYISACTIVE;
        }
        if (activate) {
            fromPos = pose.position;
            fromRot = pose.rotation;
            fromForward = (pose.forward+pose.up*.5f)*fLen;
            // origin = fromPos + fromForward * dist;
            origin = fromPos + fromRot * root;
            dPos = Vector3.zero;
            dRot = Quaternion.identity;
            activate = false;
        }
        
        switch (state) {
            case State.HANDISACTIVE: CalcHand(); break;
            case State.KEYISACTIVE: CalcKey(); break;
        }
        if (state!=State.INACTIVE) {
            DrawVector();
            DrawQuaternion();

            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
        
        // if (move.GetStateUp(hand) || KeyStateUp()) {
        if (controller.GetButtonUp(WebXRController.ButtonTypes.ButtonA)) {
            mesh.Clear();
            state = State.INACTIVE;
        }
    }

    private void CalcHand() {
            dPos = pose.position - fromPos;
            dRot = pose.rotation * Quaternion.Inverse(fromRot);
            if (isMover) RotationProject(ref dRot, fromForward);
            if (limit3D) {
                reg = Vector3.Dot(dPos, screen.transform.forward) * screen.transform.forward;
                dPos = dPos - reg;
                if (!isMover) RotationProject(ref dRot, screen.transform.forward);
            }
    }

    private void DrawVector() {
        // if (disableLeftAndRight && isMover) for (int i = 0; i < arrowVector.Length-1; i++) vertices[i] = Vector3.zero;
        // else {
            float f = dPos.magnitude;
            for (int i = 0; i < arrowVector.Length; i++) {
                vertices[i]    = arrowVector[i] * f;
                vertices[i].x *= threshold / Mathf.Max(threshold,f);
                vertices[i].y *= threshold / Mathf.Max(threshold,f);
                vertices[i]    = Quaternion.FromToRotation(Vector3.forward, dPos.normalized) * vertices[i];
                vertices[i]   += origin;
                //colors  [i]    = color;
            }
        // }
    }

    private void DrawQuaternion() {
        float f = Mathf.Acos(Mathf.Abs(dRot.w)) * ringScale;
        Vector3 axis = (new Vector3(dRot.x,dRot.y,dRot.z).normalized)*Mathf.Sign(dRot.w);
        if (f == 0) for (int i = 0; i < ringVector.Length; i++) 
            vertices[arrowVector.Length+i]  = Vector3.zero;
        else for (int i = 0; i < ringVector.Length; i++) {
            vertices[arrowVector.Length+i]  = Quaternion.FromToRotation(Vector3.forward, axis) * 
                                              Quaternion.AngleAxis(f * 1800, Vector3.forward) *
                                              ringVector[i] * f;
            vertices[arrowVector.Length+i] += origin;
            //colors  [arrowVector.Length+i]    = color;
        }
    }

    private void RotationProject(ref Quaternion q, Vector3 d) {
        reg = new Vector3(q.x, q.y, q.z);
        reg = Vector3.Dot(reg, d) * d;
        q.x = reg.x; q.y = reg.y; q.z = reg.z; 
        q.w = Mathf.Sqrt(1-Mathf.Pow(q.x,2)-Mathf.Pow(q.y,2)-Mathf.Pow(q.z,2)) * Mathf.Sign(q.w);
    }

    private bool KeyStateDown() {
        if (!Input.GetKey(KeyCode.LeftControl)&&!Input.GetKey(KeyCode.RightControl)) return false;
        if (isMover)
            return Input.GetKeyDown(Core.KEY_SLIDELEFT )||
                   Input.GetKeyDown(Core.KEY_SLIDERIGHT)||
                   Input.GetKeyDown(Core.KEY_SLIDEUP   )||
                   Input.GetKeyDown(Core.KEY_SLIDEDOWN )||
                   Input.GetKeyDown(Core.KEY_SLIDEIN   )||
                   Input.GetKeyDown(Core.KEY_SLIDEOUT  )||
                   Input.GetKeyDown(Core.KEY_FORWARD   )||
                   Input.GetKeyDown(Core.KEY_BACK      );
        else
            return Input.GetKeyDown(Core.KEY_TURNLEFT )||
                   Input.GetKeyDown(Core.KEY_TURNRIGHT)||
                   Input.GetKeyDown(Core.KEY_TURNUP   )||
                   Input.GetKeyDown(Core.KEY_TURNDOWN )||
                   Input.GetKeyDown(Core.KEY_TURNIN   )||
                   Input.GetKeyDown(Core.KEY_TURNOUT  );
    }

    private void CalcKey() {
        if (isMover) {
            if (Input.GetKey(Core.KEY_SLIDELEFT )) dPos += -Time.deltaTime*0.2f * screen.transform.right;
            if (Input.GetKey(Core.KEY_SLIDERIGHT)) dPos +=  Time.deltaTime*0.2f * screen.transform.right;
            if (Input.GetKey(Core.KEY_SLIDEUP   )) dPos +=  Time.deltaTime*0.2f * screen.transform.up;
            if (Input.GetKey(Core.KEY_SLIDEDOWN )) dPos += -Time.deltaTime*0.2f * screen.transform.up;
            if (Input.GetKey(Core.KEY_SLIDEIN   )) dPos +=  Time.deltaTime*0.2f * screen.transform.forward;
            if (Input.GetKey(Core.KEY_SLIDEOUT  )) dPos += -Time.deltaTime*0.2f * screen.transform.forward;
            if (Input.GetKey(Core.KEY_FORWARD   )) dRot *=  Quaternion.AngleAxis(-Time.deltaTime*180, fromForward);
            if (Input.GetKey(Core.KEY_BACK      )) dRot *=  Quaternion.AngleAxis( Time.deltaTime*180, fromForward);
        } else {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                if (Input.GetKey(Core.KEY_SPINLEFT )) dRot *=  Quaternion.AngleAxis( Time.deltaTime*180, screen.transform.right);
                if (Input.GetKey(Core.KEY_SPINRIGHT)) dRot *=  Quaternion.AngleAxis(-Time.deltaTime*180, screen.transform.right);
                if (Input.GetKey(Core.KEY_SPINUP   )) dRot *=  Quaternion.AngleAxis(-Time.deltaTime*180, screen.transform.up);
                if (Input.GetKey(Core.KEY_SPINDOWN )) dRot *=  Quaternion.AngleAxis( Time.deltaTime*180, screen.transform.up);
                if (Input.GetKey(Core.KEY_SPININ   )) dRot *=  Quaternion.AngleAxis(-Time.deltaTime*180, screen.transform.forward);
                if (Input.GetKey(Core.KEY_SPINOUT  )) dRot *=  Quaternion.AngleAxis( Time.deltaTime*180, screen.transform.forward);
            } else {
                if (Input.GetKey(Core.KEY_TURNLEFT )) dPos += -Time.deltaTime*0.2f * screen.transform.right;
                if (Input.GetKey(Core.KEY_TURNRIGHT)) dPos +=  Time.deltaTime*0.2f * screen.transform.right;
                if (Input.GetKey(Core.KEY_TURNUP   )) dPos +=  Time.deltaTime*0.2f * screen.transform.up;
                if (Input.GetKey(Core.KEY_TURNDOWN )) dPos += -Time.deltaTime*0.2f * screen.transform.up;
                if (Input.GetKey(Core.KEY_TURNIN   )) dPos +=  Time.deltaTime*0.2f * screen.transform.forward;
                if (Input.GetKey(Core.KEY_TURNOUT  )) dPos += -Time.deltaTime*0.2f * screen.transform.forward;
            }
        }
    }

    private bool KeyStateUp() {
        if (Input.GetKeyUp(KeyCode.LeftControl)||Input.GetKeyUp(KeyCode.RightControl)) return true;
        if (isMover)
            return Input.GetKeyUp(Core.KEY_SLIDELEFT )||
                   Input.GetKeyUp(Core.KEY_SLIDERIGHT)||
                   Input.GetKeyUp(Core.KEY_SLIDEUP   )||
                   Input.GetKeyUp(Core.KEY_SLIDEDOWN )||
                   Input.GetKeyUp(Core.KEY_SLIDEIN   )||
                   Input.GetKeyUp(Core.KEY_SLIDEOUT  )||
                   Input.GetKeyUp(Core.KEY_FORWARD   )||
                   Input.GetKeyUp(Core.KEY_BACK      );
        else
            return Input.GetKeyUp(Core.KEY_TURNLEFT )||
                   Input.GetKeyUp(Core.KEY_TURNRIGHT)||
                   Input.GetKeyUp(Core.KEY_TURNUP   )||
                   Input.GetKeyUp(Core.KEY_TURNDOWN )||
                   Input.GetKeyUp(Core.KEY_TURNIN   )||
                   Input.GetKeyUp(Core.KEY_TURNOUT  );
    }

    private readonly Vector3[] arrowVector = {
        new Vector3 ( 0   , 0   , 0   ), 

        new Vector3 ( 0.1f, 0   , 0.6f),
        new Vector3 ( 0   ,-0.1f, 0.6f),
        new Vector3 (-0.1f, 0   , 0.6f),
        new Vector3 ( 0   , 0.1f, 0.6f),

        new Vector3 ( 0.2f, 0   , 0.5f),
        new Vector3 ( 0   ,-0.2f, 0.5f),
        new Vector3 (-0.2f, 0   , 0.5f),
        new Vector3 ( 0   , 0.2f, 0.5f),

        new Vector3 ( 0   , 0   , 1   ),
    };

    private readonly int[] arrowTriangles = {
        0, 1, 2,  0, 2, 3,  0, 3, 4,  0, 4, 1,
        1, 5, 2,  6, 2, 5,  2, 6, 3,  7, 3, 6,
        3, 7, 4,  8, 4, 7,  4, 8, 1,  5, 1, 8,
        5, 9, 6,  6, 9, 7,  7, 9, 8,  8, 9, 5,
    };

    private readonly Vector3[] ringVector = {
        new Vector3 ( 1   , 0   , 0   ), 
        new Vector3 ( 0.6f,-0.6f, 0   ),
        new Vector3 ( 0.7f,-0.7f, 0.2f),
        new Vector3 ( 0.8f,-0.8f, 0   ),
        new Vector3 ( 0   ,-0.8f, 0   ),
        new Vector3 ( 0   ,-1   , 0.3f),
        new Vector3 ( 0   ,-1.2f, 0   ),

        new Vector3 ( 0   ,-1   , 0   ), 
        new Vector3 (-0.6f,-0.6f, 0   ),
        new Vector3 (-0.7f,-0.7f, 0.2f),
        new Vector3 (-0.8f,-0.8f, 0   ),
        new Vector3 (-0.8f, 0   , 0   ),
        new Vector3 (-1   , 0   , 0.3f),
        new Vector3 (-1.2f, 0   , 0   ),

        new Vector3 (-1   , 0   , 0   ), 
        new Vector3 (-0.6f, 0.6f, 0   ),
        new Vector3 (-0.7f, 0.7f, 0.2f),
        new Vector3 (-0.8f, 0.8f, 0   ),
        new Vector3 ( 0   , 0.8f, 0   ),
        new Vector3 ( 0   , 1   , 0.3f),
        new Vector3 ( 0   , 1.2f, 0   ),

        new Vector3 ( 0   , 1   , 0   ), 
        new Vector3 ( 0.6f, 0.6f, 0   ),
        new Vector3 ( 0.7f, 0.7f, 0.2f),
        new Vector3 ( 0.8f, 0.8f, 0   ),
        new Vector3 ( 0.8f, 0   , 0   ),
        new Vector3 ( 1   , 0   , 0.3f),
        new Vector3 ( 1.2f, 0   , 0   ),
    };

    private readonly int[] ringTriangles = {
         0,  1,  2,   0,  2,  3,   0,  3,  1,
         1,  4,  2,   5,  2,  4,   2,  5,  3,   6,  3,  5,   3,  6,  1,   4,  1,  6,   6,  5,  4,

         7,  8,  9,   7,  9, 10,   7, 10,  8,
         8, 11,  9,  12,  9, 11,   9, 12, 10,  13, 10, 12,  10, 13,  8,  11,  8, 13,  13, 12, 11,

        14, 15, 16,  14, 16, 17,  14, 17, 15,
        15, 18, 16,  19, 16, 18,  16, 19, 17,  20, 17, 19,  17, 20, 15,  18, 15, 20,  20, 19, 18,

        21, 22, 23,  21, 23, 24,  21, 24, 22,
        22, 25, 23,  26, 23, 25,  23, 26, 24,  27, 24, 26,  24, 27, 22,  25, 22, 27,  27, 26, 25,
    };

    private readonly Color color = Color.white;
}