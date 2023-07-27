﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.UI;
using WebXR;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Core : MonoBehaviour
{
    public delegate void Command();
    private Options optDefault;
    private Options opt; // the next three are used only during load
    public int dim;
    private string gameDirectory;
    private string reloadFile;

    private OptionsAll oa;
    private Engine engine;

    private Mesh mesh;

    private bool engineAlignMode;
    // private bool active, excluded;
    // private int[] param;
    private float delta;
    private float timeMove, timeRotate, timeAlignMove, timeAlignRotate;
    // private int nMove, nRotate, nAlignMove, nAlignRotate;
    private float dMove, dRotate, dAlignMove, dAlignRotate;
    private bool start, started;
    private IMove target;
    public int quantity;
    private double[] saveOrigin;
    private double[][] saveAxis;
    public bool alignMode;
    private int ad0, ad1;
    private double tActive;
    private Align alignActive;
    public bool keepUpAndDown;

    private int interval;

    public Command command;
    public Command menuCommand;
    private Vector3 posLeft, lastPosLeft, fromPosLeft, posRight, lastPosRight, fromPosRight, dlPosLeft, dfPosLeft, dlPosRight, dfPosRight;
    private Quaternion rotLeft, lastRotLeft, fromRotLeft, rotRight, lastRotRight, fromRotRight, dlRotLeft, dfRotLeft, dlRotRight, dfRotRight, relarot;
    private bool leftMove, rightMove;
    private int maxTouchCount = 4;
    private bool[] operated;
    private List<int> fingerIds;
    private bool leftTouchButton, rightTouchButton;
    private bool alt { get { return opt.oh.alternativeControlIn3D && dim == 3; } }
    private enum TouchType { MoveForward1, MoveForward2, MoveLateral1, MoveLateral2, Turn1, Turn2, Spin1, Spin2, LeftTouchButton, RightTouchButton, CameraRotate, Align, Click, Remove, Add, Menu, None }
    private TouchType[] touchType;
    private bool[] touchEnded;
    private Vector2[] fromTouchPos, lastTouchPos, touchPos;
    public Image alignButton, clickButton, removeShapeButton, addShapesButton, sliceButton, strictButton, leftButton, rightButton;
    public Menu menuPanel;
    public Canvas menuCanvas, inputCanvas;
    private WebXRState xrState = WebXRState.NORMAL;
    public Camera fixedCamera, fixedCameraLeft, fixedCameraRight;
    public Transform cameraLookAt, mapPos;
    public bool focusOnMap = false;
    private readonly float cameraDistanceDefault = 0.56f;
    private Vector2 cameraRot;
    private readonly Vector2 cameraRotDefault = new Vector2(26f, 0f);
    private float verticalOffset = -0.01f;
    private float fNear, fFar, sfWidth, fHeight, fWidth, sNear, sFar, sHeight, sWidth;

    public Transform leftT, rightT;
    public Transform head;
    [SerializeField] WebXRController leftC;
    [SerializeField] WebXRController rightC;

    private Vector3 reg0, reg1;
    private double[] reg2, reg3, reg4, reg5, reg6, reg7, reg8;
    private double[] eyeVector;
    private double[] cursor;
    private double[][] cursorAxis;

    public OverlayText overlayText;
    public InputViewer IVLeft, IVRight;
    private bool skyBox = false;
    public GameObject environment, hint, lefthint1, lefthint2, rightHint1, rightHint2, threeDHint;

    // --- option accessors ---

    // some of these also implement IOptions

    private OptionsMap om()
    {
        // omCurrent is always non-null, so can be used directly
        return oa.opt.om4;
    }

    public OptionsColor oc()
    {
        if (oa.ocCurrent != null) return oa.ocCurrent;
        return oa.opt.oc4;
    }

    public OptionsView ov()
    {
        if (oa.ovCurrent != null) return oa.ovCurrent;
        return oa.opt.ov4;
    }

    private OptionsMotion ot()
    {
        return oa.opt.ot4;
    }

    public int getSaveType()
    {
        return engine.getSaveType();
    }

    public void saveMaze(IStore store) {

      store.putString(KEY_CHECK,VALUE_CHECK);

      store.putInteger(KEY_DIM,dim);
      store.putObject(KEY_OPTIONS_MAP,oa.omCurrent);
      store.putObject(KEY_OPTIONS_COLOR,oc()); // ocCurrent may be null
      store.putObject(KEY_OPTIONS_VIEW,ov());  // ditto
      store.putObject(KEY_OPTIONS_SEED,oa.oeCurrent);
      store.putBool(KEY_ALIGN_MODE,alignMode);

      engine.save(store,om());
   }

   // Start is called before the first frame update
    void Start()
    {
        posLeft = leftT.localPosition; rotLeft = leftT.localRotation;
        posRight = rightT.localPosition; rotRight = rightT.localRotation;

        optDefault = ScriptableObject.CreateInstance<Options>();
        opt = ScriptableObject.CreateInstance<Options>();
        doInit();
        // dim and rest of oa are initialized when new game started

        oa = new OptionsAll();
        oa.opt = opt;
        oa.omCurrent = new OptionsMap(0); // blank for copying into
        oa.oeNext = new OptionsSeed();

        eyeVector = new double[3];
        mesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        engine = new Engine(mesh);

        newGame(dim);

        reg2 = new double[3];
        reg3 = new double[4];
        reg4 = new double[4];
        reg5 = new double[3];
        reg6 = new double[4];

        operated = new bool[maxTouchCount];
        fingerIds = new List<int>();
        for (int i = 0; i < maxTouchCount; i++) fingerIds.Add(-1);
        touchType = new TouchType[maxTouchCount];
        for (int i = 0; i < maxTouchCount; i++) touchType[i] = TouchType.None;
        touchEnded = new bool[maxTouchCount];
        fromTouchPos = new Vector2[maxTouchCount];
        lastTouchPos = new Vector2[maxTouchCount];
        touchPos = new Vector2[maxTouchCount];
        CamraReset();

        FileBrowser.HideDialog();
        StartCoroutine(FileItem.Build());

        LeftDown(); RightDown();

        fNear = fixedCamera.nearClipPlane;
        fFar = fixedCamera.farClipPlane;
        fHeight = fNear * Mathf.Tan(fixedCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        fWidth = fHeight * fixedCamera.aspect;
        sNear = fixedCameraLeft.nearClipPlane;
        sFar = fixedCameraLeft.farClipPlane;
        sHeight = sNear * Mathf.Tan(fixedCameraLeft.fieldOfView * 0.5f * Mathf.Deg2Rad);
        sWidth = sHeight * fixedCameraLeft.aspect;
    }

    private void OnEnable() {
        WebXRManager.OnXRChange += OnXRChange;
    }

    private void OnDisable() {
        WebXRManager.OnXRChange -= OnXRChange;
    }

    private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
        xrState = state;
        if (xrState != WebXRState.NORMAL)
        {
            menuCanvas.enabled = false;
            inputCanvas.enabled = false;
            fixedCameraLeft.enabled = false;
            fixedCameraRight.enabled = false;
            opt.oh.stereo = false;
        }
        else menuCanvas.enabled = true;

        environment.SetActive(xrState != WebXRState.AR && !skyBox);
    }

    public void ToggleStereo(bool b)
    {
        opt.oh.stereo = b;
        if (opt.oh.stereo)
        {
            fixedCameraLeft.enabled = true;
            fixedCameraRight.enabled = true;
            fixedCamera.enabled = false;
        }
        else
        {
            fixedCameraLeft.enabled = false;
            fixedCameraRight.enabled = false;
            fixedCamera.enabled = true;
        }
    }

    private readonly Rect leftRect = new Rect(0, 0, 0.5f, 1);
    private readonly Rect rightRect = new Rect(0.5f, 0, 0.5f, 1);
    public void ToggleCross(bool b)
    {
        opt.oh.cross = b;
        if (opt.oh.cross)
        {
            fixedCameraLeft.rect = rightRect;
            fixedCameraRight.rect = leftRect;
        }
        else
        {
            fixedCameraLeft.rect = leftRect;
            fixedCameraRight.rect = rightRect;
        }
    }

    private void LeftDown() {
        posLeft = fromPosLeft = leftT.localPosition;
        rotLeft = fromRotLeft = leftT.localRotation;
    }

    private void RightDown() {
        posRight = fromPosRight = rightT.localPosition;
        rotRight = fromRotRight = rightT.localRotation;
    }

    private void RightClick()
    {
        if (engine.getSaveType() == IModel.SAVE_GEOM
         || engine.getSaveType() == IModel.SAVE_NONE)
        {
            if (command == null) command = click;
        }
        else { if (command == null) command = jump; }
    }

    private void doToggleLimit3D() {
        opt.oo.limit3D = !opt.oo.limit3D;
        overlayText.ShowText(opt.oo.limit3D ? "Restrict\noperations to 3D" : "Removr\n3D restrictions");
        IVLeft.ToggleLimit3D(opt.oo.limit3D);
        IVRight.ToggleLimit3D(opt.oo.limit3D);
    }

    private void openMenu()
    {
        if (xrState != WebXRState.NORMAL) return;
        menuCanvas.enabled = true;
        inputCanvas.enabled = false;
        menuPanel.Activate(oa, engine.retrieveModel() as ISelectShape);
    }

    public Slider size;
    public void changeSize() {
        float f = Mathf.Pow(2,size.value-1)*0.15f;
        transform.localScale = new Vector3(f,f,f);
    }

    public void newGame()
    {
        setOptions();
        newGame(0);
    }

    private void newGame(int dim)
    {
        if (dim != 0) this.dim = dim; // allow zero to mean "keep the same"

        OptionsMap.copy(oa.omCurrent, om());
        oa.ocCurrent = null; // use standard colors for dimension
        oa.ovCurrent = null; // ditto
        oa.oeCurrent = oa.oeNext;
        oa.oeCurrent.forceSpecified();
        oa.oeNext = new OptionsSeed();

        IModel model = new MapModel(this.dim, oa.omCurrent, oc(), oa.oeCurrent, ov(), null);
        engine.newGame(this.dim, model, ov(), /*oa.opt.os,*/ ot(), true);
        controllerReset();
    }

    private readonly Color enabledColor = new Color(1, 1, 1, 0.25f);
    private readonly Color inactiveColor = new Color(1, 1, 1, 0.125f);
    private readonly Color disabledColor = new Color(1, 1, 1, 0.0625f);
    private void controllerReset() {
        setKeepUpAndDown();

        updateOptions();
        setOptions();

        target = engine;
        command = null;
        saveOrigin = new double[this.dim];
        saveAxis = new double[this.dim][];
        for (int i = 0; i < this.dim; i++) saveAxis[i] = new double[this.dim];
        started = false;

        alignButton.color = ButtonEnabled(TouchType.Align) ? enabledColor : disabledColor;
        clickButton.color = ButtonEnabled(TouchType.Click) ? enabledColor : disabledColor;
        removeShapeButton.color = ButtonEnabled(TouchType.Remove) ? enabledColor : disabledColor;
        addShapesButton.color = ButtonEnabled(TouchType.Add) ? enabledColor : disabledColor;

        reg7 = new double[dim];
        reg8 = new double[dim];
    }

    private bool ButtonEnabled(TouchType t)
    {
        int saveType = engine.getSaveType();
        switch (t)
        {
            case TouchType.Align:
                return saveType == IModel.SAVE_MAZE || saveType == IModel.SAVE_GEOM || saveType == IModel.SAVE_NONE;
            case TouchType.Click:
                return saveType != IModel.SAVE_MAZE;
            case TouchType.Remove:
                return saveType == IModel.SAVE_GEOM || saveType == IModel.SAVE_NONE || saveType == IModel.SAVE_BLOCK;
            case TouchType.Add:
                return saveType != IModel.SAVE_MAZE && saveType != IModel.SAVE_ACTION;
            default:
                return false;
        }
    }

    public void resetWin() {
        engine.resetWin();
    }

    public void restartGame() {
        engine.restartGame();
        controllerReset();
    }

    public void CamraReset() {
        opt.oh.cameraDistanceScale = 1f;
        cameraRot = cameraRotDefault;
    }

    float now = 0;
    float last = 0;
    // float lastOneSec = 0;
    // float dOneSec = 0;
    // float fps;
    // bool nextFrame = true;
    // int frameCount = 0;
    void Update()
    {
        if (!menuCanvas.enabled) calcInputFrame();
        TouchInputFrame();
        // frameCount++;
        now = Time.realtimeSinceStartup;
        delta = Mathf.Clamp(now-last, 0.0001f, 0.5f);
        last = now;
        // dOneSec = now - lastOneSec;
        // if (dOneSec >= 1) {
            // fps = frameCount / dOneSec;
            // frameCount = 0;
            // lastOneSec = now;
            // //Debug.Log(fps);
        // }

        if (Input.GetKeyDown(KeyCode.Escape)) ToggleMenu();

        engine.ApplyMesh();
        if (!menuCanvas.enabled) calcInput();
        menuCommand?.Invoke();
        menuCommand = null;
        control();
        started = started || start;
        engine.renderAbsolute(eyeVector, opt.oo, delta, !menuCanvas.enabled && started);

        if (opt.oh.alternativeControlIn3D)
        {
            lefthint1.SetActive(false);
            lefthint2.SetActive(false);
            rightHint1.SetActive(false);
            rightHint2.SetActive(false);
            threeDHint.SetActive(true);
        }
        else
        {
            if (leftTouchButton) {
                rightHint1.SetActive(false);
                rightHint2.SetActive(true);
            }
            else {
                rightHint1.SetActive(true);
                rightHint2.SetActive(false);
            }
            if (rightTouchButton) {
                lefthint1.SetActive(false);
                lefthint2.SetActive(true);
            }
            else {
                lefthint1.SetActive(true);
                lefthint2.SetActive(false);
            }
            threeDHint.SetActive(false);
        }

        sliceButton.color = opt.oo.sliceDir > 0 ? enabledColor : inactiveColor;
        strictButton.color = opt.oo.limit3D ? enabledColor : inactiveColor;
        leftButton.color = !opt.oh.leftTouchToggleMode || leftTouchButton ? enabledColor : inactiveColor;
        rightButton.color = !opt.oh.rightTouchToggleMode || rightTouchButton ? enabledColor : inactiveColor;

        if (xrState == WebXRState.NORMAL) {
            // fixedCamera.transform.rotation = cameraLookAt.rotation * Quaternion.Euler(cameraRot.x, 0, 0);
            // transform.rotation = cameraLookAt.rotation * Quaternion.Euler(0, -cameraRot.y, 0);
            fixedCamera.transform.rotation = (focusOnMap ? mapPos : cameraLookAt).rotation * Quaternion.Euler(cameraRot);
            fixedCamera.transform.position = (focusOnMap ? mapPos : cameraLookAt).position + fixedCamera.transform.rotation * Vector3.back * opt.oh.cameraDistanceScale * cameraDistanceDefault;
            fixedCameraLeft.transform.rotation = fixedCamera.transform.rotation;
            fixedCameraLeft.transform.position = fixedCamera.transform.position + fixedCamera.transform.rotation * (Vector3.left * opt.oh.iPD * 0.5f);
            fixedCameraRight.transform.rotation = fixedCamera.transform.rotation;
            fixedCameraRight.transform.position = fixedCamera.transform.position + fixedCamera.transform.rotation * (Vector3.right * opt.oh.iPD * 0.5f);

            fixedCamera.projectionMatrix = PerspectiveOffCenter(-fWidth * opt.oh.fovscale / opt.oh.cameraDistanceScale, fWidth * opt.oh.fovscale / opt.oh.cameraDistanceScale, (-fHeight * ((opt.oh.fovscale - 1) * 1.9f + 1) + verticalOffset) / opt.oh.cameraDistanceScale, (fHeight * ((opt.oh.fovscale - 1) * 0.1f + 1) + verticalOffset) / opt.oh.cameraDistanceScale, fNear, fFar);
            fixedCameraLeft.projectionMatrix = PerspectiveOffCenter((-sWidth * ((opt.oh.fovscale - 1) * 0.1f + 1) + opt.oh.iPD * 0.5f * sNear  / cameraDistanceDefault) / opt.oh.cameraDistanceScale, (sWidth * ((opt.oh.fovscale - 1) * 1.9f + 1) + opt.oh.iPD * 0.5f * sNear  / cameraDistanceDefault) / opt.oh.cameraDistanceScale, (-sHeight * ((opt.oh.fovscale - 1) * 1.9f + 1) + verticalOffset) / opt.oh.cameraDistanceScale, (sHeight * ((opt.oh.fovscale - 1) * 0.1f + 1) + verticalOffset) / opt.oh.cameraDistanceScale, sNear, sFar);
            fixedCameraRight.projectionMatrix = PerspectiveOffCenter((-sWidth * ((opt.oh.fovscale - 1) * 1.9f + 1) - opt.oh.iPD * 0.5f * sNear  / cameraDistanceDefault) / opt.oh.cameraDistanceScale, (sWidth * ((opt.oh.fovscale - 1) * 0.1f + 1) - opt.oh.iPD  * 0.5f * sNear  / cameraDistanceDefault) / opt.oh.cameraDistanceScale, (-sHeight * ((opt.oh.fovscale - 1) * 1.9f + 1) + verticalOffset) / opt.oh.cameraDistanceScale, (sHeight * ((opt.oh.fovscale - 1) * 0.1f + 1) + verticalOffset) / opt.oh.cameraDistanceScale, sNear, sFar);
        }
    }

    private void Slice()
    {
        opt.oo.sliceDir = (opt.oo.sliceDir + 1) % ((opt.oo.sliceMode) ? 4 : 2);
        overlayText.ShowText("Slice mode change");
    }

    public void ToggleMenu()
    {
        if (menuCanvas.enabled == false) openMenu();
        else menuPanel.doCancel();
    }

    private int swipeDir = 0;
    private float tSwipe = 0.3f;
    private float alignTime;
    private void calcInputFrame() {
        if (leftC.GetButtonDown(WebXRController.ButtonTypes.ButtonA) || leftC.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
            LeftDown();
        if (rightC.GetButtonDown(WebXRController.ButtonTypes.ButtonA) || rightC.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
            RightDown();
        if (leftC.GetButtonUp(WebXRController.ButtonTypes.ButtonB))
            doToggleLimit3D();
        if (rightC.GetButtonDown(WebXRController.ButtonTypes.ButtonB))
            Slice();
        if (leftC.GetButtonDown(WebXRController.ButtonTypes.Trigger))
            OperateAlign();
        if (rightC.GetButtonDown(WebXRController.ButtonTypes.Trigger))
            RightClick();
        if (alignTime > 0) alignTime -= Time.deltaTime;

        Vector2 v = rightC.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
        if (Mathf.Abs(v.x) <= tSwipe) swipeDir = 0;
        if  (swipeDir >= 0 && v.x < -tSwipe && command == null) { command = removeShape; swipeDir = -1; }
        else if (swipeDir <= 0 && v.x > tSwipe && command == null) { command = addShapes; swipeDir = 1; }

        bool update = false;
        if (Input.GetKeyDown(KeyCode.G)) Slice();
        if (Input.GetKeyDown(KeyCode.Space)) RightClick();
        if (Input.GetKeyDown(KeyCode.M) && command == null) command = addShapes;
        if (Input.GetKeyDown(KeyCode.N) && command == null) command = removeShape;
        if (Input.GetKeyDown(KeyCode.Return)) OperateAlign();
        if (Input.GetKeyDown(KeyCode.T)) doToggleLimit3D();
        if (Input.GetKeyDown(KeyCode.Alpha1)) { update = true; opt.ov4.texture[1] = !opt.ov4.texture[1]; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { update = true; opt.ov4.texture[2] = !opt.ov4.texture[2]; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { update = true; opt.ov4.texture[3] = !opt.ov4.texture[3]; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { update = true; opt.ov4.texture[4] = !opt.ov4.texture[4]; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { update = true; opt.ov4.texture[5] = !opt.ov4.texture[5]; }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { update = true; opt.ov4.texture[6] = !opt.ov4.texture[6]; }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { update = true; opt.ov4.texture[7] = !opt.ov4.texture[7]; }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { update = true; opt.ov4.texture[8] = !opt.ov4.texture[8]; }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { update = true; opt.ov4.texture[9] = !opt.ov4.texture[9]; }
        if (Input.GetKeyDown(KeyCode.Alpha0)) { update = true; opt.ov4.texture[0] = !opt.ov4.texture[0]; }
        if (Input.GetKeyDown(KeyCode.X) && getSaveType() == IModel.SAVE_NONE) { update = true; opt.od.trainSpeed--; }
        if (Input.GetKeyDown(KeyCode.C) && getSaveType() == IModel.SAVE_NONE) { update = true; opt.od.trainSpeed = 0; }
        if (Input.GetKeyDown(KeyCode.V) && getSaveType() == IModel.SAVE_NONE) { update = true; opt.od.trainSpeed++; }
        if (Input.GetKeyDown(KeyCode.Q)) doToggleTrack();
        if (Input.GetKeyDown(KeyCode.Y)) engine.toggleFisheye();
        if (Input.GetKeyDown(KeyCode.P) && command == null) command = doPaint;
        if (Input.GetKeyDown(KeyCode.H) && opt.ov4.texture[0]) { update = true; opt.od.useEdgeColor = !opt.od.useEdgeColor; }
        if (Input.GetKeyDown(KeyCode.B) && (getSaveType() == IModel.SAVE_GEOM || getSaveType() == IModel.SAVE_NONE)) { update = true; opt.od.invertNormals = !opt.od.invertNormals; }
        if (Input.GetKeyDown(KeyCode.Comma) && target != engine) { update = true; opt.od.hidesel = !opt.od.hidesel; }
        if (Input.GetKeyDown(KeyCode.Period) && (getSaveType() == IModel.SAVE_GEOM || getSaveType() == IModel.SAVE_NONE)) { update = true; opt.od.separate = !opt.od.separate; overlayText.ShowText("Separation " + (opt.od.separate ? "on" : "off")); }
        if (update) updateOptions();
    }

    private void OperateAlign()
    {
        if (alignMode) { if (doToggleAlignMode()) overlayText.ShowText("Align mode off"); }
        else if (command == align || alignTime > 0 || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            { if (doToggleAlignMode()) overlayText.ShowText("Align mode on"); }
        else if (doAlign()) { overlayText.ShowText("Align"); alignTime = .2f; }
    }

    private void TouchInputFrame()
    {
        for (int i = 0; i < maxTouchCount; i++) operated[i] = false;
        for (int j = 0; j < Mathf.Min(maxTouchCount, Input.touchCount); j++)
        {
            Touch touch = Input.GetTouch(j);
            int id = touch.fingerId;
            int i = fingerIds.IndexOf(id);
            if (i < 0)
            {
                i = fingerIds.IndexOf(-1); if (i < 0) continue; else fingerIds[i] = id;
                fromTouchPos[i] = lastTouchPos[i] = touchPos[i] = touch.position / new Vector2(Screen.width, Screen.height);
                SetTypes(i);
                operated[i] = true;
            }
            else { touchPos[i] = touch.position / new Vector2(Screen.width, Screen.height); operated[i] = true; }
        }
        for (int i = 0; i < maxTouchCount; i++) if (operated[i] == false && fingerIds[i] != -1) { fingerIds[i] = -1; touchEnded[i] = true; }
        if (Input.touchCount == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                    fromTouchPos[0] = lastTouchPos[0] = touchPos[0] = Input.mousePosition / new Vector2(Screen.width, Screen.height);
                    SetTypes(0);
            }
            else if (Input.GetMouseButton(0)) touchPos[0] = Input.mousePosition / new Vector2(Screen.width, Screen.height);
            else if (Input.GetMouseButtonUp(0)) { touchEnded[0] = true; }
        }
    }

    private void SetTypes(int i) {
        if (menuCanvas.enabled) touchType[i] = TouchType.None;
        else if (fromTouchPos[i].y > 0.8f) {
            if (fromTouchPos[i].x < 0.09375f) touchType[i] = TouchType.Menu;
            else if (fromTouchPos[i].x > 0.896875f) Slice();
            else if (fromTouchPos[i].x > 0.79375f) doToggleLimit3D(); }
        else if (fromTouchPos[i].x < 0.09375f && fromTouchPos[i].y < 0.1875f && ButtonEnabled(TouchType.Align)) OperateAlign();
        else if (fromTouchPos[i].x < 0.09375f && fromTouchPos[i].y < 0.375f && ButtonEnabled(TouchType.Remove) && command == null) command = removeShape;
        else if (fromTouchPos[i].x < 0.3125f) {
            if (fromTouchPos[i].y < 0.375f) touchType[i] = !alt && rightTouchButton ? TouchType.MoveLateral1 : TouchType.MoveForward1;
            else if (fromTouchPos[i].y < 0.8f) touchType[i] = alt ? TouchType.MoveLateral1 : rightTouchButton ? TouchType.MoveLateral2 : TouchType.MoveForward2; }
        else if (fromTouchPos[i].x < 0.5f && fromTouchPos[i].y < 0.16667f) { touchType[i] = TouchType.LeftTouchButton; leftTouchButton = opt.oh.leftTouchToggleMode ? !leftTouchButton : true; }
        else if (fromTouchPos[i].x > 0.90625f && fromTouchPos[i].y < 0.1875f && ButtonEnabled(TouchType.Click)) RightClick();
        else if (fromTouchPos[i].x > 0.90625f && fromTouchPos[i].y < 0.375f && ButtonEnabled(TouchType.Add) && command == null) command = addShapes;
        else if (fromTouchPos[i].x > 0.6875f) {
            if (fromTouchPos[i].y < 0.375f) touchType[i] = !alt && leftTouchButton ? TouchType.Spin1 : TouchType.Turn1;
            else if (fromTouchPos[i].y < 0.8f) touchType[i] = alt || leftTouchButton ? TouchType.Spin2 : TouchType.Turn2; }
        else if (fromTouchPos[i].x > 0.5f && fromTouchPos[i].y < 0.16667f) { touchType[i] = TouchType.RightTouchButton; rightTouchButton = opt.oh.rightTouchToggleMode ? !rightTouchButton : true; }
        else if (fromTouchPos[i].y < 0.8f) touchType[i] = TouchType.CameraRotate;
    }

    float touchMoveSpeed = 1f;
    float touchRotateSpeed = 1f;
    float lerpc = 4f;
    private void calcInput()
    {
        relarot = Quaternion.Inverse(transform.rotation);
        lastPosLeft = posLeft; lastPosRight = posRight;
        lastRotLeft = rotLeft; lastRotRight = rotRight;
        posLeft = leftT.localPosition; posRight = rightT.localPosition;
        rotLeft = leftT.localRotation; rotRight = rightT.localRotation;
        dlPosLeft = relarot * (posLeft - lastPosLeft); dlPosRight = relarot * (posRight - lastPosRight);
        dfPosLeft = relarot * (posLeft - fromPosLeft); dfPosRight = relarot * (posRight - fromPosRight);
        if (!opt.oh.allowDiagonalMovement) { dlPosLeft = Vector3.zero; dfPosLeft = Vector3.zero; }
        Quaternion lRel = Quaternion.Inverse(fromRotLeft);
        dlRotLeft = lRel * rotLeft * Quaternion.Inverse(lRel * lastRotLeft);
        dlRotRight = relarot * rotRight * Quaternion.Inverse(relarot * lastRotRight);
        dfRotLeft = lRel * rotLeft * Quaternion.Inverse(lRel * fromRotLeft);
        dfRotRight = relarot * rotRight * Quaternion.Inverse(relarot * fromRotRight);

        leftMove = leftC.GetButton(WebXRController.ButtonTypes.ButtonA);
        rightMove = rightC.GetButton(WebXRController.ButtonTypes.ButtonA);
        reg1 = relarot * ((focusOnMap ? mapPos : cameraLookAt).position - head.position);
        for (int i = 0; i < 3; i++) eyeVector[i] = reg1[i];
        Vec.normalize(eyeVector, eyeVector);

        relarot = opt.oh.horizontalInputFollowing ? Quaternion.Euler(0, cameraRot.y, 0) : Quaternion.identity;
        for (int i = 0; i < maxTouchCount; i++)
        {
            reg0 = Vector3.zero;
            switch (touchType[i])
            {
                case TouchType.MoveForward1:
                    leftMove = true;
                    dlRotLeft = Quaternion.Euler(0, 0, -2 * (maxAng / limitLR) * (touchPos[i].y - lastTouchPos[i].y) * touchRotateSpeed) * dlRotLeft;
                    dfRotLeft = Quaternion.Euler(0, 0, -(limitAngForward / limit) * (touchPos[i].y - fromTouchPos[i].y) * touchRotateSpeed) * dfRotLeft;
                    if (opt.oh.allowDiagonalMovement) {
                        dlPosLeft += relarot * Vector3.right * (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height * touchMoveSpeed;
                        dfPosLeft += relarot * Vector3.right * (touchPos[i].x - fromTouchPos[i].x) * Screen.width / Screen.height * touchMoveSpeed; }
                    break;
                case TouchType.MoveForward2:
                case TouchType.MoveLateral2:
                    leftMove = true;
                    if (touchType[i] == TouchType.MoveLateral2 || opt.oh.allowDiagonalMovement) { 
                        reg0.x = (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height;
                        reg0.z = touchPos[i].y - lastTouchPos[i].y;
                        dlPosLeft += relarot * reg0 * touchMoveSpeed;
                        reg0.x = (touchPos[i].x - fromTouchPos[i].x) * Screen.width / Screen.height;
                        reg0.z = touchPos[i].y - fromTouchPos[i].y;
                        dfPosLeft += relarot * reg0 * touchMoveSpeed; }
                    break;
                case TouchType.MoveLateral1:
                    leftMove = true;
                    reg0.x = (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height;
                    reg0.y = touchPos[i].y - lastTouchPos[i].y;
                    dlPosLeft += relarot * reg0 * touchMoveSpeed;
                    reg0.x = (touchPos[i].x - fromTouchPos[i].x) * Screen.width / Screen.height;
                    reg0.y = touchPos[i].y - fromTouchPos[i].y;
                    dfPosLeft += relarot * reg0 * touchMoveSpeed;
                    break;
                case TouchType.Turn1:
                    rightMove = true;
                    reg0.x = (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height;
                    reg0.y = touchPos[i].y - lastTouchPos[i].y;
                    dlPosRight += relarot * reg0 * touchMoveSpeed;
                    reg0.x = (touchPos[i].x - fromTouchPos[i].x) * Screen.width / Screen.height;
                    reg0.y = touchPos[i].y - fromTouchPos[i].y;
                    dfPosRight += relarot * reg0 * touchMoveSpeed;
                    break;
                case TouchType.Turn2:
                    rightMove = true;
                    reg0.x = (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height;
                    reg0.z = touchPos[i].y - lastTouchPos[i].y;
                    dlPosRight += relarot * reg0 * touchMoveSpeed;
                    reg0.x = (touchPos[i].x - fromTouchPos[i].x) * Screen.width / Screen.height;
                    reg0.z = touchPos[i].y - fromTouchPos[i].y;
                    dfPosRight += relarot * reg0 * touchMoveSpeed;
                    break;
                case TouchType.Spin1:
                    rightMove = true;
                    reg0.x = touchPos[i].y - lastTouchPos[i].y;
                    reg0.y = (lastTouchPos[i].x - touchPos[i].x) * Screen.width / Screen.height;
                    dlRotRight = relarot * Quaternion.AngleAxis(360 * reg0.magnitude * touchRotateSpeed, reg0) * Quaternion.Inverse(relarot) * dlRotRight;
                    reg0.x = touchPos[i].y - fromTouchPos[i].y;
                    reg0.y = (fromTouchPos[i].x - touchPos[i].x) * Screen.width / Screen.height;
                    dfRotRight = relarot * Quaternion.AngleAxis(360 * reg0.magnitude * touchRotateSpeed, reg0) * Quaternion.Inverse(relarot) *dfRotRight;
                    break;
                case TouchType.Spin2:
                    rightMove = true;
                    reg0.x = touchPos[i].y - lastTouchPos[i].y;
                    reg0.z = (lastTouchPos[i].x - touchPos[i].x) * Screen.width / Screen.height;
                    dlRotRight = relarot * Quaternion.AngleAxis(360 * reg0.magnitude * touchRotateSpeed, reg0) * Quaternion.Inverse(relarot) * dlRotRight;
                    reg0.x = touchPos[i].y - fromTouchPos[i].y;
                    reg0.z = (fromTouchPos[i].x - touchPos[i].x) * Screen.width / Screen.height;
                    dfRotRight = relarot * Quaternion.AngleAxis(360 * reg0.magnitude * touchRotateSpeed, reg0) * Quaternion.Inverse(relarot) * dfRotRight;
                    break;
                case TouchType.LeftTouchButton:
                    if (touchEnded[i] && !opt.oh.leftTouchToggleMode) leftTouchButton = false;
                    break;
                case TouchType.RightTouchButton:
                    if (touchEnded[i] && !opt.oh.rightTouchToggleMode) rightTouchButton = false;
                    break;
                case TouchType.CameraRotate:
                    reg0.x = cameraRot.x + 180 * (lastTouchPos[i].y - touchPos[i].y);
                    if (Mathf.Abs(reg0.x) < 90) cameraRot.x = reg0.x;
                    cameraRot.y += 180 * (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height;
                    cameraRot.y = cameraRot.y % 360;
                    break;
                case TouchType.Menu:
                    if (touchEnded[i] && touchPos[i].y > 0.8f && touchPos[i].x < 0.09375f) menuCommand = ToggleMenu;
                    break;
                default: break;
            }
            lastTouchPos[i] = touchPos[i];
            if (touchEnded[i]) { touchEnded[i] = false; touchType[i] = TouchType.None; }
        }

        start = leftMove || rightMove;
    }

    private float ClampedLerp(float y) { return Mathf.Clamp01(((y - 0.5f) * lerpc + 1) * 0.5f); }

    private float tAlign = 0.5f; // threshold for align mode
    private float tAlignSpin = 0.8f;
    private float limitAng = 30;
    private float limitAngRoll = 30;
    private float limitAngForward = 30;
    private float maxAng = 60;
    private float limit = 0.1f; // controler Transform Unit
    private float limitLR = 0.3f; // LR Drag Unit
    private float max = 0.2f; // YP Drag Unit
    private const float epsilon = 0.000001f;
    private void control()
    {
        //nMove = (int)Math.Ceiling(fps * timeMove + epsilon);
        //nRotate = (int)Math.Ceiling(fps * timeRotate + epsilon);
        //nAlignMove = (int)Math.Ceiling(fps * timeAlignMove + epsilon);
        //nAlignRotate = (int)Math.Ceiling(fps * timeAlignRotate + epsilon);

        //dMove = 1 / (double)nMove;
        //dRotate = 90 / (double)nRotate;
        //dAlignMove = 1 / (double)nAlignMove;
        //dAlignRotate = 90 / (double)nAlignRotate;

        if (!isPlatformer()) {
            dMove = delta / timeMove;
            dRotate = 90 * delta / timeRotate;
        } else {
            dMove = delta * 2;
            dRotate = 90 * delta * 2;
        }
        dAlignMove = delta / timeAlignMove;
        dAlignRotate = 90 * delta / timeAlignRotate;

        IMove saveTarget = target;
        target.save(saveOrigin, saveAxis);
        if (command != null) command();
        else
        {
            // left hand
            if (!alignMode && isDrag(TYPE_LEFTANDRIGHT)) {
                for (int i = 0; i < 3; i++) reg2[i] = dlPosLeft[i];
                Vec.scale(reg2, reg2, 1.0 / limitLR / dMove);
            }
            else {
                for (int i = 0; i < 3; i++) reg2[i] = dfPosLeft[i];
                Vec.scale(reg2, reg2, 1.0 / Math.Max(limit, Vec.norm(reg2)));
            }
            Array.Copy(reg2, reg3, 3);
            if (!alignMode && isDrag(TYPE_FORWARD)) {
                relarot = dlRotLeft;
                reg3[3] = -Math.Asin(relarot.z) * Math.Sign(relarot.w);
                reg3[3] /= maxAng * Math.PI / 180 * dMove;
            }
            else {
                relarot = dfRotLeft;
                reg3[3] = -Math.Asin(relarot.z) * Math.Sign(relarot.w);
                reg3[3] /= Math.Max(limitAngForward * Math.PI / 180, Math.Abs(reg3[3]));
            }

            if (!leftMove) Vec.zero(reg3);
            keyControl(KEYMODE_SLIDE);
            if (opt.oo.limit3D || dim == 3) reg3[2] = 0;
            if (opt.oo.invertLeftAndRight) for (int i=0; i<reg3.Length-1; i++) reg3[i] = -reg3[i];
            if (opt.oo.invertForward) reg3[reg3.Length-1] = -reg3[reg3.Length-1];

            if (alignMode)
            {
                for (int i = 0; i < reg3.Length; i++)
                {
                    if (Math.Abs(reg3[i]) > tAlign)
                    {
                        tActive = timeMove;
                        ad0 = Dir.forAxis(Math.Min(i,dim-1), reg3[i] < 0);
                        if (target.canMove(Dir.getAxis(ad0), Dir.getSign(ad0))) {command = alignMove; break;}
                    }
                }
            }
            else
            {
                reg3[dim-1] = reg3[3];
                Vec.scale(reg7, reg3, dMove);
                target.move(reg7);
            }

            // right hand
            if (alignMode)
            {
                for (int i = 0; i < 3; i++) reg2[i] = dfPosRight[i];
                Vec.scale(reg2, reg2, 1.0 / Math.Max(limit, Vec.norm(reg2)));
                if (!rightMove) Vec.zero(reg2);
                keyControl(KEYMODE_TURN);
                if (opt.oo.limit3D || dim == 3) reg2[2] = 0;
                if (opt.oo.invertYawAndPitch) for (int i = 0; i < reg2.Length; i++) reg2[i] = -reg2[i];
                for (int i = 0; i < reg2.Length; i++)
                {
                    if (Math.Abs(reg2[i]) > tAlign)
                    {
                        tActive = timeRotate;
                        ad0 = Dir.forAxis(dim - 1);
                        ad1 = Dir.forAxis(i, reg2[i] < 0);
                        command = alignRotate;
                        break;
                    }
                }
                if (command == null)
                {
                    relarot = dfRotRight;
                    for (int i = 0; i < 3; i++) reg0[i] = Mathf.Asin(relarot[i]) * Mathf.Sign(relarot.w) / limitAng / Mathf.PI * 180;
                    if (!rightMove) reg0 = Vector3.zero;
                    keyControl(KEYMODE_SPIN);
                    if (opt.oo.limit3D || dim == 3) { reg0[0] = 0; reg0[1] = 0; }
                    if (isPlatformer()) { reg0[0] = 0; reg0[2] = 0; }
                    if (opt.oo.invertRoll) reg0 = -reg0;
                    for (int i = 0; i < 3; i++)
                    {
                        if (Mathf.Abs(reg0[i]) > tAlignSpin)
                        {
                            tActive = timeRotate;
                            ad0 = Dir.forAxis((i + 1) % 3);
                            ad1 = Dir.forAxis((i + 2) % 3, reg0[i] < 0);
                            command = alignRotate;
                            break;
                        }
                    }
                }
            }
            else
            {
                double t;
                if (isDrag(TYPE_YAWANDPITCH)) {
                    for (int i = 0; i < 3; i++) reg2[i] = dlPosRight[i];
                    t = Vec.norm(reg2);
                    if (t>0) Vec.scale(reg2, reg2, 90 / dRotate * Math.Min(max, t) / max / t);
                }
                else {
                    for (int i = 0; i < 3; i++) reg2[i] = dfPosRight[i];
                    t = Vec.norm(reg2);
                    if (t>0) Vec.scale(reg2, reg2, Math.Min(limit, t) / limit / t);
                }
                if (!rightMove) Vec.zero(reg2);
                keyControl(KEYMODE_TURN);
                if (opt.oo.limit3D || dim == 3) reg2[2] = 0;
                if (opt.oo.invertYawAndPitch) for (int i = 0; i < reg2.Length; i++) reg2[i] = -reg2[i];
                t = Vec.norm(reg2);
                if (t != 0)
                {
                    t *= dRotate * Math.PI / 180;
                    Vec.normalize(reg2, reg2);
                    for (int i = 0; i < dim-1; i++) reg7[i] = reg2[i] * Math.Sin(t);
                    reg7[dim-1] = Math.Cos(t);
                    Vec.unitVector(reg8, dim-1);
                    target.rotateAngle(reg8, reg7);
                }

                float f;
                if (isDrag(TYPE_ROLL)) {
                    relarot = dlRotRight;
                }
                else {
                    relarot = dfRotRight;
                    f = Mathf.Acos(relarot.w);
                    if (f>0) f = (dRotate / limitAngRoll * .5f) * Mathf.Min(limitAngRoll * Mathf.PI / 180, f) / f;
                    relarot = Quaternion.Slerp(Quaternion.identity, relarot, f);
                }
                if (!rightMove) relarot = Quaternion.identity;
                keyControl(KEYMODE_SPIN2);
                if (opt.oo.limit3D || dim == 3) { relarot[0] = 0; relarot[1] = 0; relarot[3] = Mathf.Sqrt(1 - relarot[2] * relarot[2]); }
                if (isPlatformer() || keepUpAndDown) { relarot[0] = 0; relarot[2] = 0; relarot[3] = Mathf.Sqrt(1 - relarot[1] * relarot[1]);}
                if (opt.oo.invertRoll) relarot = Quaternion.Inverse(relarot);
                if (relarot.w < 1f) {
                    relarot.ToAngleAxis(out f, out reg0);
                    reg1.Set(1, 0, 0);
                    Vector3.OrthoNormalize(ref reg0, ref reg1);
                    reg0 = relarot * reg1;
                    for (int i = 0; i < 3; i++) reg7[i] = reg0[i];
                    reg7[dim-1] = 0;
                    for (int i = 0; i < 3; i++) reg8[i] = reg1[i];
                    reg8[dim-1] = 0;
                    Vec.normalize(reg7, reg7);
                    Vec.normalize(reg8, reg8);
                    target.rotateAngle(reg8, reg7);
                }
            }

            //if (leftTrigger)
            //{
            //}
            //if (rightTrigger)
            //{

            //}
        }

        // update state

        // the click command is exclusive, so if the target changed,
        // no update needed.
        if (target == saveTarget
             && !target.update(saveOrigin, saveAxis, engine.getOrigin()))
        { // bonk

            target.restore(saveOrigin, saveAxis);

            //if (commandActive != null)
            //{
            //    commandActive = null;
            //    alignActive = null; // not a big deal but let's do it
            //}

            if (alignMode && !target.isAligned())
            {
                alignMode = false;
            }
        }
    }

    private const int TYPE_LEFTANDRIGHT = 0;
    private const int TYPE_FORWARD = 1;
    private const int TYPE_YAWANDPITCH = 2;
    private const int TYPE_ROLL = 3;
    private bool isPlatformer() { 
        return engine.getSaveType() == IModel.SAVE_ACTION
            || engine.getSaveType() == IModel.SAVE_BLOCK
            || engine.getSaveType() == IModel.SAVE_SHOOT; 
    }
    private bool isDrag(int type) {
        switch(type) {
            case TYPE_LEFTANDRIGHT:
                return !isPlatformer() && opt.oo.inputTypeLeftAndRight == OptionsControl.INPUTTYPE_DRAG;
            case TYPE_FORWARD:
                return !isPlatformer() && opt.oo.inputTypeForward == OptionsControl.INPUTTYPE_DRAG;
            case TYPE_YAWANDPITCH:
                return opt.oo.inputTypeYawAndPitch == OptionsControl.INPUTTYPE_DRAG;
            case TYPE_ROLL:
                return opt.oo.inputTypeRoll == OptionsControl.INPUTTYPE_DRAG;
        }
        return false;
    }

    private void alignMove()
    {
        Vec.unitVector(reg7, Dir.getAxis(ad0));
        double d;
        if ((d = tActive - delta) > 0) {
            tActive = d;
            Vec.scale(reg7, reg7, Dir.getSign(ad0) * dMove);
            target.move(reg7);
        }
        else {
            d = tActive / timeMove;
            Vec.scale(reg7, reg7, Dir.getSign(ad0) * d);
            target.move(reg7);
            target.align().snap();
            command = null;
        }
    }

    private void alignRotate()
    {
        Vec.unitVector(reg7, Dir.getAxis(ad0));
        Vec.scale(reg7, reg7, Dir.getSign(ad0));
        double d;
        if ((d = tActive - delta) > 0) {
            tActive = d;
            Vec.rotateAbsoluteAngleDir(reg8, reg7, ad0, ad1, dRotate);
            target.rotateAngle(reg7, reg8);
        }
        else {
            d = 90 * tActive / timeRotate;
            Vec.rotateAbsoluteAngleDir(reg8, reg7, ad0, ad1, d);
            target.rotateAngle(reg7, reg8);
            target.align().snap();
            command = null;
        }
    }

    public void align()
    {
        if (isPlatformer())
        {
            command = null;
            return;
        }
        if (alignActive == null) alignActive = target.align();
        if (alignActive.align(dAlignMove, dAlignRotate))
        {
            alignActive = null;
            command = null;
        }
    }

    public void click()
    {
        try {target = ((GeomModel)engine.retrieveModel()).click(engine.getOrigin(), engine.getViewAxis(), engine.getAxisArray());}
        catch (InvalidCastException){ return; }
        if (target != null)
        {
            engineAlignMode = alignMode; // save
            alignMode = target.isAligned(); // reasonable default
        }
        else
        {
            target = engine;
            alignMode = engineAlignMode; // restore
        }
        command = null;
    }

    public void jump() {
        engine.jump();
        command = null;
    }

    public void addShapes() {
        if (ot().paintWithAddButton) doPaint();
        else engine.addShapes(alignMode);
        command = null;
    }

    public void removeShape() {
        engine.removeShape();
        command = null;
    }

    public bool doAlign() {
        if (!isPlatformer() && !keepUpAndDown && command == null) { command = align; return true; }
        return false;
    }

    public bool doToggleAlignMode()
    {
        int n = getSaveType();
        if ((n == IModel.SAVE_MAZE || n == IModel.SAVE_GEOM || n == IModel.SAVE_NONE) && !keepUpAndDown)
        {
            alignMode = !alignMode;
            if (alignMode) command = align;
            return true;
        }
        return false;
    }

    public const KeyCode KEY_SLIDELEFT  = KeyCode.S;
    public const KeyCode KEY_SLIDERIGHT = KeyCode.F;
    public const KeyCode KEY_SLIDEUP    = KeyCode.A;
    public const KeyCode KEY_SLIDEDOWN  = KeyCode.Z;
    public const KeyCode KEY_SLIDEIN    = KeyCode.W;
    public const KeyCode KEY_SLIDEOUT   = KeyCode.R;
    public const KeyCode KEY_FORWARD    = KeyCode.E;
    public const KeyCode KEY_BACK       = KeyCode.D;
    public const KeyCode KEY_TURNLEFT   = KeyCode.J;
    public const KeyCode KEY_TURNRIGHT  = KeyCode.L;
    public const KeyCode KEY_TURNUP     = KeyCode.I;
    public const KeyCode KEY_TURNDOWN   = KeyCode.K;
    public const KeyCode KEY_TURNIN     = KeyCode.U;
    public const KeyCode KEY_TURNOUT    = KeyCode.O;
    public const KeyCode KEY_SPINLEFT   = KeyCode.K;
    public const KeyCode KEY_SPINRIGHT  = KeyCode.I;
    public const KeyCode KEY_SPINUP     = KeyCode.J;
    public const KeyCode KEY_SPINDOWN   = KeyCode.L;
    public const KeyCode KEY_SPININ     = KeyCode.U;
    public const KeyCode KEY_SPINOUT    = KeyCode.O;
    private const int KEYMODE_SLIDE = 0;
    private const int KEYMODE_TURN = 1;
    private const int KEYMODE_SPIN = 2;
    private const int KEYMODE_SPIN2 = 3;
    private void keyControl(int keyMode) {
        if (menuCanvas.enabled || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) return;
        switch (keyMode) {
            case (KEYMODE_SLIDE):
                if (Input.GetKey(KEY_SLIDELEFT )) { start = true; reg3[0] = -1; }
                if (Input.GetKey(KEY_SLIDERIGHT)) { start = true; reg3[0] =  1; }
                if (Input.GetKey(KEY_SLIDEUP   )) { start = true; reg3[1] =  1; }
                if (Input.GetKey(KEY_SLIDEDOWN )) { start = true; reg3[1] = -1; }
                if (Input.GetKey(KEY_SLIDEIN   )) { start = true; reg3[2] =  1; }
                if (Input.GetKey(KEY_SLIDEOUT  )) { start = true; reg3[2] = -1; }
                if (Input.GetKey(KEY_FORWARD   )) { start = true; reg3[3] =  1; }
                if (Input.GetKey(KEY_BACK      )) { start = true; reg3[3] = -1; }
                break;
            case (KEYMODE_TURN):
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) {
                    if (Input.GetKey(KEY_TURNLEFT )) { start = true; reg2[0] = -1; }
                    if (Input.GetKey(KEY_TURNRIGHT)) { start = true; reg2[0] =  1; }
                    if (Input.GetKey(KEY_TURNUP   )) { start = true; reg2[1] =  1; }
                    if (Input.GetKey(KEY_TURNDOWN )) { start = true; reg2[1] = -1; }
                    if (Input.GetKey(KEY_TURNIN   )) { start = true; reg2[2] =  1; }
                    if (Input.GetKey(KEY_TURNOUT  )) { start = true; reg2[2] = -1; }
                }
                break;
            case (KEYMODE_SPIN):
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || dim == 3) {
                    if (Input.GetKey(KEY_SPINLEFT )) { start = true; reg0[0] = -1; }
                    if (Input.GetKey(KEY_SPINRIGHT)) { start = true; reg0[0] =  1; }
                    if (Input.GetKey(KEY_SPINUP   )) { start = true; reg0[1] =  1; }
                    if (Input.GetKey(KEY_SPINDOWN )) { start = true; reg0[1] = -1; }
                    if (Input.GetKey(KEY_SPININ   )) { start = true; reg0[2] =  1; }
                    if (Input.GetKey(KEY_SPINOUT  )) { start = true; reg0[2] = -1; }
                }
                break;
            case (KEYMODE_SPIN2):
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || dim == 3) {
                    Quaternion q = Quaternion.identity;
                    if (Input.GetKey(KEY_SPINLEFT )) { start = true; q *= Quaternion.Euler(-dRotate,0,0); }
                    if (Input.GetKey(KEY_SPINRIGHT)) { start = true; q *= Quaternion.Euler( dRotate,0,0); }
                    if (Input.GetKey(KEY_SPINUP   )) { start = true; q *= Quaternion.Euler(0, dRotate,0); }
                    if (Input.GetKey(KEY_SPINDOWN )) { start = true; q *= Quaternion.Euler(0,-dRotate,0); }
                    if (Input.GetKey(KEY_SPININ   )) { start = true; q *= Quaternion.Euler(0,0, dRotate); }
                    if (Input.GetKey(KEY_SPINOUT  )) { start = true; q *= Quaternion.Euler(0,0,-dRotate); }
                    if (q != Quaternion.identity) relarot = q;
                }
                break;
        }
    }

    public OptionsAll getOptionsAll()
    {
        return oa;
    }

    public void setFrameRate(double frameRate)
    {
        interval = (int)Math.Ceiling(1000 / frameRate);
    }

    public void setOptionsMotion(/*OptionsKeysConfig okc,*/ OptionsMotion ot)
    {

        //for (int i = 0; i < 6; i++)
        //{
        //    param[i] = okc.param[i];
        //}

        // the frame rate and command times are all positive,
        // so the number of steps will always be at least 1 ...

        //if (engine.getSaveType() == IModel.SAVE_ACTION
        // || engine.getSaveType() == IModel.SAVE_BLOCK
        // || engine.getSaveType() == IModel.SAVE_SHOOT)
        //{
        //    nMove = (int)Math.Ceiling(ot.frameRate * 0.5);
        //    nRotate = (int)Math.Ceiling(ot.frameRate * 0.5);
        //    nAlignMove = (int)Math.Ceiling(ot.frameRate * 0.5);
        //    nAlignRotate = (int)Math.Ceiling(ot.frameRate * 0.5);
        //}
        //else
        //{
            //nMove = (int)Math.Ceiling(ot.frameRate * ot.timeMove);
            //nRotate = (int)Math.Ceiling(ot.frameRate * ot.timeRotate);
            //nAlignMove = (int)Math.Ceiling(ot.frameRate * ot.timeAlignMove);
            //nAlignRotate = (int)Math.Ceiling(ot.frameRate * ot.timeAlignRotate);
        //}

        // ... therefore, the distances will never exceed 1,
        // and the angles will never exceed 90 degrees

        //dMove = 1 / (double)nMove;
        //dRotate = 90 / (double)nRotate;
        //dAlignMove = 1 / (double)nAlignMove;
        //dAlignRotate = 90 / (double)nAlignRotate;

        timeMove =  ot.timeMove;
        timeRotate =  ot.timeRotate;
        timeAlignMove =  ot.timeAlignMove;
        timeAlignRotate =  ot.timeAlignRotate;
    }

    public void updateOptions()
    {
        engine.setOptions(oc(), ov(), oa.oeCurrent, ot(), oa.opt.od);
        IVLeft.ToggleLimit3D(opt.oo.limit3D);
        IVRight.ToggleLimit3D(opt.oo.limit3D);
    }

    public void setOptions()
    {
        engine.setOptions(oc(), ov(), oa.oeCurrent, ot(), oa.opt.od);
        setOptionsMotion(oa.opt.ot4);
        setFrameRate(oa.opt.ot4.frameRate);
    }

    private void setKeepUpAndDown() {
        keepUpAndDown = opt.oo.keepUpAndDown;
        if (keepUpAndDown) alignMode = false;
        engine.setKeepUpAndDown(keepUpAndDown);
    }

    public void doPaint()
    {
        var model = engine.retrieveModel() as GeomModel;
        if (model != null && model.canPaint()) model.paint(engine.getOrigin(), engine.getViewAxis());
        command = null;
    }

    public void doAddShapes()
    {
        var model = engine.retrieveModel() as GeomModel;
        if (model != null && model.canAddShapes()) model.addShapes(quantity, alignMode, engine.getOrigin(), engine.getViewAxis());
    }
    
    public void doScramble()
    {
        var model = engine.retrieveModel() as GeomModel;
        if (model != null) model.scramble(alignMode, engine.getOrigin());
    }

    public void doToggleTrack()
    {
        var model = engine.retrieveModel() as GeomModel;
        if (model != null) model.toggleTrack();
    }

    public void closeMenu()
    {
        // leftL.enabled = false;
        // rightL.enabled = false;
        // SteamVR_Actions.control.Activate(left);
        // SteamVR_Actions.control.Activate(right);
        menuCanvas.enabled = false;
        inputCanvas.enabled = xrState == WebXRState.NORMAL && opt.oh.showController;
        hint.SetActive(opt.oh.showHint);
    }

    public void ToggleSkyBox(bool b)
    {
        engine.objColor = b ? Color.white : Color.black;
        skyBox = b;
    }

    public void doQuit()
    {
        try {
            PropertyFile.save(FileItem.Combine(Application.persistentDataPath, fileCurrent),save);
        } catch (Exception e) {
            Debug.LogException(e);
        }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        Application.Quit();
#endif
    }

    public void doLoad()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private bool opened;
    private IStore store;
    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, opened ? null : "", "Load File", "Load");
        opened = true;

        Debug.Log("LoadFile " + (FileBrowser.Success ? "successful: " + Path.GetFileName(FileBrowser.Result[0]) : "failed"));

        if (FileBrowser.Success) {
            reloadFile = FileItem.Combine(Application.streamingAssetsPath, FileBrowser.Result[0]);
            Debug.Log("Load: " + Path.GetFileName(reloadFile));
            yield return LoadCoroutine();
        }
    }

    IEnumerator LoadCoroutine()
    {
        yield return PropertyFile.test(reloadFile);
        if (PropertyFile.isMaze) yield return doLoadMaze();
        else yield return doLoadGeom();
    }

    private IEnumerator doLoadMaze()
    {
        Debug.Log("Load: " + Path.GetFileName(reloadFile));
        yield return PropertyFile.load(reloadFile, loadMazeCommand);
    }

    private IEnumerator doLoadGeom()
    {
        // read file

        context = DefaultContext.create();
        Debug.Log("Load: " + Path.GetFileName(reloadFile));
        context.libDirs.Add("data" + Path.DirectorySeparatorChar + "lib");
        yield return Language.include(context, reloadFile);
        menuCommand = loadGeom;
    }
   private static readonly string VALUE_CHECK       = "Maze";

   private static readonly string KEY_CHECK         = "game";
   private static readonly string KEY_DIM           = "dim";
   private static readonly string KEY_OPTIONS_MAP   = "om";
   private static readonly string KEY_OPTIONS_COLOR = "oc";
   private static readonly string KEY_OPTIONS_VIEW  = "ov";
   private static readonly string KEY_OPTIONS_SEED  = "oe";
   private static readonly string KEY_OPTIONS_DISPLAY = "od";
   private static readonly string KEY_OPTIONS_CONTROL = "oo";
   private static readonly string KEY_ALIGN_MODE    = "align";

    public void loadMazeCommand(IStore store) { this.store = store; menuCommand = loadMaze; }
    private void loadMaze() { try { loadMaze(store); } catch (Exception e) { Debug.LogException(e); } store = null; }
    public void loadMaze(IStore store){
        if ( ! store.getString(KEY_CHECK).Equals(VALUE_CHECK) ) throw new Exception("getEmpty");

    // read file, but don't modify existing objects until we're sure of success

        int dimLoad = store.getInteger(KEY_DIM);
        if ( ! (dimLoad == 3 || dimLoad == 4) ) throw new Exception("dimError");

        OptionsMap omLoad = new OptionsMap(dimLoad);
        OptionsMap.copy(omLoad, om());
        OptionsColor ocLoad = new OptionsColor();
        OptionsColor.copy(ocLoad, oc());
        OptionsView ovLoad = new OptionsView();
        OptionsView.copy(ovLoad, ov());
        OptionsSeed oeLoad = new OptionsSeed();

        store.getObject(KEY_OPTIONS_MAP,omLoad);
        store.getObject(KEY_OPTIONS_COLOR,ocLoad);
        store.getObject(KEY_OPTIONS_VIEW,ovLoad);
        store.getObject(KEY_OPTIONS_SEED,oeLoad);
        if ( ! oeLoad.isSpecified() ) throw new Exception("seedError");
        bool alignModeLoad = store.getBool(KEY_ALIGN_MODE);

    // ok, we know enough ... even if the engine parameters turn out to be invalid,
    // we can still start a new game

        // and, we need to initialize the engine before it can validate its parameters

        dim = dimLoad;

        oa.omCurrent = omLoad; // may as well transfer as copy
        oa.ocCurrent = ocLoad;
        oa.ovCurrent = ovLoad;
        oa.oeCurrent = oeLoad;

        oa.opt.om4 = omLoad;
        oa.opt.oc4 = ocLoad;
        oa.opt.ov4 = ovLoad;
        oa.oeCurrent = oeLoad;
        // oeNext is not modified by loading a game

        IModel model = new MapModel(dim,oa.omCurrent,oc(),oa.oeCurrent,ov(),store);
        engine.newGame(dim,model,ov(),/*oa.opt.os,*/ot(),false);
        controllerReset();

        engine.load(store,alignModeLoad);
    }

    Context context;
    private void loadGeom()
    {
        try
        {
            loadGeom(context);
        }
        catch (Exception t)
        {
            string s = "";
            if (t is LanguageException)
            {
                LanguageException e = (LanguageException)t;
                //t = e.getCause();
                s = Path.GetFileName(e.getFile()) + "\n" + e.getDetail();
                Debug.LogException(new Exception(s));
            }
            else Debug.LogException(t);
            //t.printStackTrace();
            //JOptionPane.showMessageDialog(this, s + t.getClass().getName() + "\n" + t.getMessage(), App.getString("Maze.s25"), JOptionPane.ERROR_MESSAGE);
        }
        finally { context = null; }
    }
    public void loadGeom(Context c) //throws Exception
    {

        // build the model
        //Debug.Log("try");
        GeomModel model = buildModel(c);
        // run this before changing anything since it can fail
        //Debug.Log("complete");
        // switch to geom

        // if (model.getDimension() == 3) throw new Exception("The system does not support 3D scene");
        dim = model.getDimension();

        // no need to modify omCurrent, just leave it with previous maze values
        oa.ocCurrent = null;
        oa.ovCurrent = null;
        // no need to modify oeCurrent or oeNext

        bool[] texture = model.getDesiredTexture();
        if (texture != null)
        { // model -> ov
            OptionsView ovLoad = new OptionsView();
            OptionsView.copy(ovLoad, ov(), texture);
            oa.ovCurrent = ovLoad;
            // careful, if you set ovCurrent earlier
            // then ov() will return the wrong thing
        }
        else
        { // ov -> model
            texture = ov().texture;
        }
        model.setTexture(texture);

        // model already constructed
        engine.newGame(dim, model, ov(), /*oa.opt.os,*/ ot(), true);
        controllerReset();

        alignMode = model.getAlignMode(alignMode);
        menuPanel.Activate(oa, engine.retrieveModel() as ISelectShape);
    }

    public static GeomModel buildModel(Context c) //throws Exception
    {

        DimensionAccumulator da = new DimensionAccumulator();
        Track track = null;
        List<Train> tlist = new List<Train>();
        List<IScenery> scenery = new List<IScenery>();
        List<Geom.ShapeInterface> slist = new List<Geom.ShapeInterface>();
        List<Enemy> elist = new List<Enemy>();
        Struct.ViewInfo viewInfo = null;
        Struct.DrawInfo drawInfo = null;

        Struct.FinishInfo finishInfo = null;
        Struct.FootInfo footInfo = null;
        Struct.BlockInfo blockInfo = null;

        // scan for items
        foreach (object o in c.stack)
        {
            if (o is IDimension)
            {
                da.putDimension(((IDimension)o).getDimension());
            }
            else if (o is IDimensionMultiSrc)
            {
                ((IDimensionMultiSrc)o).getDimension(da);
            }
            // this is just a quick check to catch the most obvious mistakes.
            // I can't be bothered to add dimension accessors to the scenery.

            if (o == null)
            {
                throw new Exception("Unused null object on stack.");
            }
            else if (o is Track) {
                if (track != null) throw new Exception("Only one track object allowed (but it can have disjoint loops).");
                track = (Track)o;
            } else if (o is Train) {
                tlist.Add((Train)o);
            } 
            else if (o is IScenery)
            {
                scenery.Add((IScenery)o);
            }
            else if (o is Geom.ShapeInterface)
            { // Shape or CompositeShape
                ((Geom.ShapeInterface)o).unglue(slist);
            }
            else if (o is Struct.ViewInfo)
            {
                if (viewInfo != null) throw new Exception("Only one viewinfo command allowed.");
                viewInfo = (Struct.ViewInfo)o;
            }
            else if (o is Struct.DrawInfo)
            {
                if (drawInfo != null) throw new Exception("Only one drawinfo command allowed.");
                drawInfo = (Struct.DrawInfo)o;
            }
            else if (o is Struct.DimensionMarker)
            {
                // ignore, we're done with it
            }
            else if (o is Struct.FinishInfo)
            {
                if (finishInfo != null) throw new Exception("Only one finishInfo command allowed.");
                finishInfo = (Struct.FinishInfo)o;
            }
            else if (o is Struct.FootInfo)
            {
                footInfo = (Struct.FootInfo)o;
            }
            else if (o is Struct.BlockInfo)
            {
                blockInfo = (Struct.BlockInfo)o;
            }
            else if (o is Enemy)
            {
                elist.Add((Enemy)o);
            }
            else
            {
                throw new Exception("Unused object on stack (" + o.GetType().Name + ").");
            }
        }

        // use items to make model

        if (da.first) throw new Exception("Scene doesn't contain any objects.");
        if (da.error) throw new Exception("The number of dimensions is not consistent.");
        int dtemp = da.dim; // we shouldn't change the Core dim variable yet

        Geom.ShapeInterface[] list = slist.ToArray();

        //Geom.Shape[] shapes = (Geom.Shape[])slist.ToArray();
        Geom.Shape[] shapes = new Geom.Shape[slist.Count];
        for (int i = 0; i < slist.Count; i++) shapes[i] = (Geom.Shape)slist[i];
        //Train[] trains = (Train[])tlist.toArray(new Train[tlist.size()]);
        Train[] trains = new Train[tlist.Count];
        for (int i = 0; i < tlist.Count; i++) trains[i] = tlist[i];
        //Enemy[] enemies = (Enemy[])elist.toArray(new Enemy[elist.size()]);
        Enemy[] enemies = new Enemy[elist.Count];
        for (int i = 0; i < elist.Count; i++) enemies[i] = elist[i];

        if (track != null) TrainModel.init(track, trains); // kluge needed for track scale

        if (scenery.Count == 0) scenery.Add((dtemp == 3) ? new Mat.Mat3() : (IScenery)new Mat.Mat4());
        if (track != null) scenery.Add(track); // add last so it draws over other scenery

        GeomModel model;
        if (finishInfo != null) model = new ActionModel(dtemp, shapes, drawInfo, viewInfo, footInfo, finishInfo);
        else if (enemies.Length > 0) model = new ShootModel(dtemp, shapes, drawInfo, viewInfo, footInfo, enemies);
        else if (blockInfo != null) model = new BlockModel(dtemp, shapes, drawInfo, viewInfo, footInfo);
        else model = (track != null) ? new TrainModel(dtemp, shapes, drawInfo, viewInfo, track, trains)
        :
        model = new GeomModel(dtemp, shapes, drawInfo, viewInfo);
        model.addAllScenery(scenery);

        // gather dictionary info

        List<NamedObject<Color>> availableColors = new List<NamedObject<Color>>();
        List<NamedObject<Geom.Shape>> availableShapes = new List<NamedObject<Geom.Shape>>();
        Dictionary<string, Color> colorNames = new Dictionary<string, Color>();
        Dictionary<string, Geom.Shape> idealNames = new Dictionary<string, Geom.Shape>();

        foreach (KeyValuePair<string, object> entry in c.dict)
        {
            object o = entry.Value;
            if (o is Color)
            {
                Color color = (Color)o;
                string name = entry.Key;
                availableColors.Add(new NamedObject<Color>(name,color));
                if (!c.topLevelDef.Contains(name))
                {
                    colorNames.Add(name, color);
                }

            }
            else if (o is Geom.Shape)
            { // not ShapeInterface, at least for now
                Geom.Shape shape = (Geom.Shape)o;
                if (shape.getDimension() == dtemp)
                {
                    string name = entry.Key;
                    availableShapes.Add(new NamedObject<Geom.Shape>(name,shape));
                    if (!c.topLevelDef.Contains(name))
                    {
                        idealNames.Add(name, shape.ideal);
                    }
                }
            }
            // else it's not something we're interested in
        }

        availableColors.Sort();
        availableShapes.Sort();

        model.setAvailableColors(availableColors);
        model.setAvailableShapes(availableShapes);

        model.setSaveInfo(c.topLevelInclude, colorNames, idealNames);

        // done

        return model;
    }

    public void doReload(int delta) {
        if (reloadFile == null) return;

        if (delta != 0) {
            string file = FileItem.Retrieve(reloadFile);
            string[] f = Array.ConvertAll<FileItem, string>(FileItem.Find(FileItem.GetParent(file)).children.FindAll(x => !x.isDirectory).ToArray(), f => f.path);
            // Array.Sort(f);

            // results of listFiles have same parent directory so names are sufficient
            // (and probably faster for sorting)

            int i = Array.IndexOf(f,file);
            if (i != -1) {
                i += delta;
                if (i >= 0 && i < f.Length) file = f[i];
                else return; // we're at the end, don't do a reload
            }
            // else not found, fall through and report that error
            reloadFile = FileItem.Combine(Application.streamingAssetsPath, file);
        }
        StartCoroutine(LoadCoroutine());
    }

    private bool doInit() {
        try {
            PropertyFile.loadDefault(delegate(IStore store) { loadDefault(store); });
            if (File.Exists(FileItem.Combine(Application.persistentDataPath, fileCurrent))) PropertyFile.load(FileItem.Combine(Application.persistentDataPath, fileCurrent), load);
        } catch (Exception e) {
            Debug.LogException(e);
            return false;
        }
        return true;
    }

   private static string nameDefault = "default.properties";
   private static string fileCurrent = "current.properties";

   private static readonly String KEY_OPTIONS = "opt";
   private static readonly String KEY_BOUNDS  = "bounds";
   private static readonly String KEY_GAME_DIRECTORY  = "dir.game";
   private static readonly String KEY_IMAGE_DIRECTORY = "dir.image";
   private static readonly String KEY_VERSION = "version";
   private static readonly String KEY_FISHEYE = "opt.of"; // not part of opt (yet)

   // here we don't have to be careful about modifying an existing object,
   // because if any of the load process fails, the program will exit

    public void loadDefault(IStore store) {
        store.getObject(KEY_OPTIONS,optDefault);

        if (File.Exists(FileItem.Combine(Application.persistentDataPath, fileCurrent))) return;

        store.getObject(KEY_OPTIONS,opt);
        dim = 4;
        gameDirectory  = null;
    }

    public void load(IStore store) {

        store.getObject(KEY_OPTIONS,opt);
        dim = store.getInteger(KEY_DIM);
        //if ( ! (dim == 3 || dim == 4) ) throw App.getException("Maze.e1");
        //gameDirectory  = store.getString(KEY_GAME_DIRECTORY );

        //int? temp = store.getNullableInteger(KEY_VERSION);
        //int version = (temp == null) ? 1 : temp.Value;

        //if (version >= 2) {
            //store.getObject(KEY_FISHEYE,OptionsFisheye.of);
            //OptionsFisheye.recalculate();
        //}
    }
    private void doSave() {

        int saveType = getSaveType();
        if (!(saveType == IModel.SAVE_NONE || saveType == IModel.SAVE_GEOM)) { // train model
            return;
        }

        // JFileChooser chooser = new JFileChooser(getGameDirectory());
        // String title = App.getString("Maze.s16");
        // chooser.setDialogTitle(title);
        // int result = chooser.showSaveDialog(this);
        // if (result != JFileChooser.APPROVE_OPTION) return;

        // gameDirectory = chooser.getCurrentDirectory();
        // File file = chooser.getSelectedFile();
        // if (file.exists() && ! confirmOverwrite(file,title)) return;

        // try {
            // if (saveType == IModel.SAVE_MAZE) {
                // PropertyFile.save(file,core);
            // } else {
                // doSaveGeom(file);
                // // this handles exceptions internally, but no harm in having an extra layer here
            // }
        // } catch (Exception e) {
            // JOptionPane.showMessageDialog(this,e.getMessage(),App.getString("Maze.s17"),JOptionPane.ERROR_MESSAGE);
        // }
    }
    public void save(IStore store) {

        store.putObject(KEY_OPTIONS,oa.opt);
        store.putInteger(KEY_DIM,dim);
        //store.putString(KEY_GAME_DIRECTORY, gameDirectory);

        //store.putInteger(KEY_VERSION,2);

        //// version 2
        //store.putObject(KEY_FISHEYE,OptionsFisheye.of);
    }
    private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0f * near / (right - left);
        float y = 2.0f * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -2.0f * far * near / (far - near);
        float e = -1.0f;

        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x; m[0, 1] = 0; m[0, 2] = a; m[0, 3] = 0;
        m[1, 0] = 0; m[1, 1] = y; m[1, 2] = b; m[1, 3] = 0;
        m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = c; m[2, 3] = d;
        m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = e; m[3, 3] = 0;

        return m;
    }
}