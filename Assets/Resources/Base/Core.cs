using System.Collections;
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
    private int dim;
    private string gameDirectory;
    private string reloadFile;

    private OptionsAll oa;
    private Engine engine;

    private Mesh mesh;

    private bool engineAlignMode;
    private bool active, excluded;
    private int[] param;
    private double delta;
    private double timeMove, timeRotate, timeAlignMove, timeAlignRotate;
    private int nMove, nRotate, nAlignMove, nAlignRotate;
    private double dMove, dRotate, dAlignMove, dAlignRotate;
    private bool alwaysRun;
    private IMove target;
    private double[] saveOrigin;
    private double[][] saveAxis;
    public bool alignMode;
    private int ad0, ad1;
    private double tActive;
    private Align alignActive;
    public bool keepUpAndDown;
    private bool disableLeftAndRight;

    private int interval;

    public Command command;
    public Command menuCommand;
    private Vector3 posLeft, lastPosLeft, fromPosLeft, posRight, lastPosRight, fromPosRight, dlPosLeft, dfPosLeft, dlPosRight, dfPosRight;
    private Quaternion rotLeft, lastRotLeft, fromRotLeft, rotRight, lastRotRight, fromRotRight, dlRotLeft, dfRotLeft, dlRotRight, dfRotRight, relarot;
    private bool leftTrigger, rightTrigger, lastLeftTrigger, lastRightTrigger, leftTriggerPressed, rightTriggerPressed,
        leftMove, rightMove, leftGrip, rightGrip, lastLeftGrip, lastRightGrip;
    private int maxTouchCount = 4;
    private bool[] operated;
    private List<int> fingerIds;
    private bool leftTouchButton, rightTouchButton;
    public bool leftTouchToggleMode = false;
    public bool rightTouchToggleMode = false;
    public bool hideController = false;
    public bool allowDiagonalMovement = true;
    public bool horizontalInputFollowing = true;
    public bool stereo = false;
    public float iPD = 0.064f;
    private enum TouchType { MoveForward1, MoveForward2, MoveLateral1, MoveLateral2, Turn1, Turn2, Spin1, Spin2, LeftTouchButton, RightTouchButton, CameraRotate, Align, Click, Remove, Add, Menu, None }
    private TouchType[] touchType;
    private bool[] touchEnded;
    private Vector2[] fromTouchPos, lastTouchPos, touchPos;
    public Image alignButton, clickButton, removeShapeButton, addShapesButton;
    public Menu menuPanel;
    public Canvas menuCanvas, inputCanvas;
    private WebXRState xrState = WebXRState.NORMAL;
    public Camera fixedCamera;
    public Transform cameraLookAt;
    private float cameraDistance;
    private readonly float cameraDistanceDefault = 0.54f;
    private Vector2 cameraRot;
    private readonly Vector2 cameraRotDefault = new Vector2(26f, 0f);

    public Transform leftT, rightT;
    public Transform head;
    [SerializeField] WebXRController leftC;
    [SerializeField] WebXRController rightC;

    private Vector3 reg0, reg1;
    private double[] reg2, reg3, reg4, reg5, reg6;
    private double[] eyeVector;
    private double[] cursor;
    private double[][] cursorAxis;

    public OverlayText overlayText;
    public InputViewer IVLeft, IVRight;
    private bool skyBox = false;
    public GameObject environment;

    // --- option accessors ---

    // some of these also implement IOptions

    private OptionsMap om()
    {
        // omCurrent is always non-null, so can be used directly
        return /*(dim == 3) ? oa.opt.om3 :*/ oa.opt.om4;
    }

    public OptionsColor oc()
    {
        if (oa.ocCurrent != null) return oa.ocCurrent;
        return /*(dim == 3) ? oa.opt.oc3 :*/ oa.opt.oc4;
    }

    public OptionsView ov()
    {
        if (oa.ovCurrent != null) return oa.ovCurrent;
        return /*(dim == 3) ? oa.opt.ov3 :*/ oa.opt.ov4;
    }

    //public OptionsStereo os()
    //{
    //    return oa.opt.os;
    //}

    //private OptionsKeys ok()
    //{
    //    return (dim == 3) ? oa.opt.ok3 : oa.opt.ok4;
    //}

    private OptionsMotion ot()
    {
        return /*(dim == 3) ? oa.opt.ot3 :*/ oa.opt.ot4;
    }

    //public OptionsImage oi()
    //{
    //    return oa.opt.oi;
    //}

    //private KeyMapper keyMapper()
    //{
    //    return (dim == 3) ? keyMapper3 : keyMapper4;
    //}

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
    void Start() // ルーチン開始
    {
        posLeft = leftT.localPosition; rotLeft = leftT.localRotation;
        posRight = rightT.localPosition; rotRight = rightT.localRotation;

        optDefault = ScriptableObject.CreateInstance<Options>();
        opt = ScriptableObject.CreateInstance<Options>();
        // ロード
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
        active = true;

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
        }
        else menuCanvas.enabled = true;

        environment.SetActive(xrState != WebXRState.AR && !skyBox);
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

    private void LeftGrip() {
        opt.oo.limit3D = !opt.oo.limit3D;
        overlayText.ShowText(opt.oo.limit3D ? "Restrict\noperation to 3D" : "Remove\n3D restriction");
        IVLeft.ToggleLimit3D(opt.oo.limit3D);
        IVRight.ToggleLimit3D(opt.oo.limit3D);
    }

    private void openMenu()
    {
        if (xrState != WebXRState.NORMAL) return;
        menuCanvas.enabled = true;
        inputCanvas.enabled = false;
        menuPanel.Activate(oa);
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

        alignButton.color = ButtonEnabled(TouchType.Align) ? enabledColor : disabledColor;
        clickButton.color = ButtonEnabled(TouchType.Click) ? enabledColor : disabledColor;
        removeShapeButton.color = ButtonEnabled(TouchType.Remove) ? enabledColor : disabledColor;
        addShapesButton.color = ButtonEnabled(TouchType.Add) ? enabledColor : disabledColor;
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
        cameraDistance = cameraDistanceDefault;
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
        engine.renderAbsolute(eyeVector, opt.oo, delta);

        if (xrState == WebXRState.NORMAL) {
            fixedCamera.transform.rotation = Quaternion.Euler(cameraRot) * cameraLookAt.rotation;
            fixedCamera.transform.position = cameraLookAt.position + fixedCamera.transform.rotation * Vector3.back * cameraDistance;
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
            openMenu();
        if (rightC.GetButtonDown(WebXRController.ButtonTypes.Trigger))
            RightClick();
        if (leftC.GetButtonDown(WebXRController.ButtonTypes.Grip))
            LeftGrip();
        if (rightC.GetButtonDown(WebXRController.ButtonTypes.ButtonB))
            OperateAlign();
        if (alignTime > 0) alignTime -= Time.deltaTime;

        Vector2 v = rightC.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
        if (Mathf.Abs(v.x) <= tSwipe) swipeDir = 0;
        if  (swipeDir >= 0 && v.x < -tSwipe && command == null) { command = removeShape; swipeDir = -1; }
        else if (swipeDir <= 0 && v.x > tSwipe && command == null) { command = addShapes; swipeDir = 1; }

        if ((leftC.GetButtonDown(WebXRController.ButtonTypes.Trigger)) || Input.GetKeyDown(KeyCode.Q)) 
            Slice();
        if (Input.GetKeyDown(KeyCode.Space))
            RightClick();
        if (Input.GetKeyDown(KeyCode.H) && command == null)
            command = addShapes;
        if (Input.GetKeyDown(KeyCode.Y) && command == null)
            command = removeShape;
    }

    private void OperateAlign()
    {
        if (alignMode) { if (doToggleAlignMode()) overlayText.ShowText("Align mode off"); }
        else if (command == align || alignTime > 0) { if (doToggleAlignMode()) overlayText.ShowText("Align mode on"); }
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
            else if (fromTouchPos[i].x > 0.79375f) LeftGrip(); }
        if (fromTouchPos[i].x < 0.09375f && fromTouchPos[i].y < 0.375f) {
            if (fromTouchPos[i].y < 0.1875f) {
                if (ButtonEnabled(TouchType.Align)) OperateAlign();
                else touchType[i] = rightTouchButton ? TouchType.MoveLateral1 : TouchType.MoveForward1; }
            else if (ButtonEnabled(TouchType.Remove)) { if (command == null) command = removeShape; }
            else touchType[i] = rightTouchButton ? TouchType.MoveLateral1 : TouchType.MoveForward1; }
        else if (fromTouchPos[i].x < 0.3125f) {
            if (fromTouchPos[i].y < 0.375f) touchType[i] = rightTouchButton ? TouchType.MoveLateral1 : TouchType.MoveForward1;
            else if (fromTouchPos[i].y < 0.8f) touchType[i] = rightTouchButton ? TouchType.MoveLateral2 : TouchType.MoveForward2; }
        else if (fromTouchPos[i].x < 0.40625f && fromTouchPos[i].y < 0.16667f) { touchType[i] = TouchType.LeftTouchButton; leftTouchButton = leftTouchToggleMode ? !leftTouchButton : true; }
        else if (fromTouchPos[i].x > 0.90625f && fromTouchPos[i].y < 0.375f) {
            if (fromTouchPos[i].y < 0.1875f) {
                if (ButtonEnabled(TouchType.Click)) RightClick();
                else touchType[i] = leftTouchButton ? TouchType.Spin1 : TouchType.Turn1; }
            else if (ButtonEnabled(TouchType.Add)) { if (command == null) command = addShapes; }
            else touchType[i] = leftTouchButton ? TouchType.Spin1 : TouchType.Turn1; }
        else if (fromTouchPos[i].x > 0.6875f) {
            if (fromTouchPos[i].y < 0.375f) touchType[i] = leftTouchButton ? TouchType.Spin1 : TouchType.Turn1;
            else if (fromTouchPos[i].y < 0.8f) touchType[i] = leftTouchButton ? TouchType.Spin2 : TouchType.Turn2; }
        else if (fromTouchPos[i].x > 0.59375f && fromTouchPos[i].y < 0.16667f) { touchType[i] = TouchType.RightTouchButton; rightTouchButton = rightTouchToggleMode ? !rightTouchButton : true; }
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
        // posLeft = pose.GetLocalPosition(left); posRight = pose.GetLocalPosition(right);
        // rotLeft = pose.GetLocalRotation(left); rotRight = pose.GetLocalRotation(right);
        posLeft = leftT.localPosition; posRight = rightT.localPosition;
        rotLeft = leftT.localRotation; rotRight = rightT.localRotation;
        dlPosLeft = relarot * (posLeft - lastPosLeft); dlPosRight = relarot * (posRight - lastPosRight);
        dfPosLeft = relarot * (posLeft - fromPosLeft); dfPosRight = relarot * (posRight - fromPosRight);
        Quaternion lRel = Quaternion.Inverse(fromRotLeft);
        dlRotLeft = lRel * rotLeft * Quaternion.Inverse(lRel * lastRotLeft);
        dlRotRight = relarot * rotRight * Quaternion.Inverse(relarot * lastRotRight);
        dfRotLeft = lRel * rotLeft * Quaternion.Inverse(lRel * fromRotLeft);
        dfRotRight = relarot * rotRight * Quaternion.Inverse(relarot * fromRotRight);

        leftMove = leftC.GetButton(WebXRController.ButtonTypes.ButtonA);
        rightMove = rightC.GetButton(WebXRController.ButtonTypes.ButtonA);
        reg1 = relarot * 
               (transform.position - head.position);
               //(transform.position - ((headsetOnHead.GetState(SteamVR_Input_Sources.Head)) ?
                //player.hmdTransform.position : fixedCamera.transform.position));
        for (int i = 0; i < 3; i++) eyeVector[i] = reg1[i];
        Vec.normalize(eyeVector, eyeVector);

        relarot = horizontalInputFollowing ? Quaternion.Euler(0, cameraRot.y, 0) : Quaternion.identity;
        for (int i = 0; i < maxTouchCount; i++)
        {
            reg0 = Vector3.zero;
            switch (touchType[i])
            {
                case TouchType.MoveForward1:
                    leftMove = true;
                    dlRotLeft = Quaternion.Euler(0, 0, -2 * (float)(maxAng / limitLR) * (touchPos[i].y - lastTouchPos[i].y) * touchRotateSpeed) * dlRotLeft;
                    dfRotLeft = Quaternion.Euler(0, 0, -(float)(limitAngForward / limit) * (touchPos[i].y - fromTouchPos[i].y) * touchRotateSpeed) * dfRotLeft;
                    if (allowDiagonalMovement) {
                        dlPosLeft += relarot * Vector3.right * (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height * touchMoveSpeed;
                        dfPosLeft += relarot * Vector3.right * (touchPos[i].x - fromTouchPos[i].x) * Screen.width / Screen.height * touchMoveSpeed; }
                    break;
                case TouchType.MoveForward2:
                case TouchType.MoveLateral2:
                    leftMove = true;
                    if (allowDiagonalMovement) { 
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
                    reg0.x = lastTouchPos[i].y - touchPos[i].y;
                    reg0.y = (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height;
                    dlRotRight = relarot * Quaternion.AngleAxis(360 * reg0.magnitude * touchRotateSpeed, reg0) * Quaternion.Inverse(relarot) * dlRotRight;
                    reg0.x = fromTouchPos[i].y - touchPos[i].y;
                    reg0.y = (touchPos[i].x - fromTouchPos[i].x) * Screen.width / Screen.height;
                    dfRotRight = relarot * Quaternion.AngleAxis(360 * reg0.magnitude * touchRotateSpeed, reg0) * Quaternion.Inverse(relarot) *dfRotRight;
                    break;
                case TouchType.Spin2:
                    rightMove = true;
                    reg0.x = lastTouchPos[i].y - touchPos[i].y;
                    reg0.z = (touchPos[i].x - lastTouchPos[i].x) * Screen.width / Screen.height;
                    dlRotRight = relarot * Quaternion.AngleAxis(360 * reg0.magnitude * touchRotateSpeed, reg0) * Quaternion.Inverse(relarot) * dlRotRight;
                    reg0.x = fromTouchPos[i].y - touchPos[i].y;
                    reg0.z = (touchPos[i].x - fromTouchPos[i].x) * Screen.width / Screen.height;
                    dfRotRight = relarot * Quaternion.AngleAxis(360 * reg0.magnitude * touchRotateSpeed, reg0) * Quaternion.Inverse(relarot) * dfRotRight;
                    break;
                case TouchType.LeftTouchButton:
                    if (touchEnded[i] && !leftTouchToggleMode) leftTouchButton = false;
                    break;
                case TouchType.RightTouchButton:
                    if (touchEnded[i] && !rightTouchToggleMode) rightTouchButton = false;
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
    }

    private float ClampedLerp(float y) { return Mathf.Clamp01(((y - 0.5f) * lerpc + 1) * 0.5f); }

    private double tAlign = 0.5; // threshold for align mode
    private double tAlignSpin = 0.8;
    private double limitAng = 30;
    private double limitAngRoll = 30;
    private double limitAngForward = 30;
    private double maxAng = 60;
    private double limit = 0.1; // controler Transform Unit
    private double limitLR = 0.3; // LR Drag Unit
    private double max = 0.2; // YP Drag Unit
    private const double epsilon = 0.000001;
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

            if (opt.oo.limit3D) reg3[2] = 0;
            if (disableLeftAndRight) for (int i=0; i<reg3.Length-1; i++) reg3[i] = 0;
            if (opt.oo.invertLeftAndRight) for (int i=0; i<reg3.Length-1; i++) reg3[i] = -reg3[i];
            if (opt.oo.invertForward) reg3[reg3.Length-1] = -reg3[reg3.Length-1];
            if (!leftMove) Vec.zero(reg3);
            keyControl(KEYMODE_SLIDE);

            if (alignMode)
            {
                for (int i = 0; i < reg3.Length; i++)
                {
                    if (Math.Abs(reg3[i]) > tAlign)
                    {
                        tActive = timeMove;
                        ad0 = Dir.forAxis(i, reg3[i] < 0);
                        if (target.canMove(Dir.getAxis(ad0), Dir.getSign(ad0))) {command = alignMove; break;}
                    }
                }
            }
            else
            {
                Vec.scale(reg3, reg3, dMove);
                target.move(reg3);
            }

            // right hand
            if (alignMode)
            {
                for (int i = 0; i < 3; i++) reg2[i] = dfPosRight[i];
                Vec.scale(reg2, reg2, 1.0 / Math.Max(limit, Vec.norm(reg2)));
                if (opt.oo.limit3D) reg2[2] = 0;
                if (opt.oo.invertYawAndPitch) for (int i = 0; i < reg2.Length; i++) reg2[i] = -reg2[i];
                if (!rightMove) Vec.zero(reg2);
                keyControl(KEYMODE_TURN);
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
                    for (int i = 0; i < 3; i++) reg0[i] = Mathf.Asin(relarot[i]) * Mathf.Sign(relarot.w) / (float)limitAng / Mathf.PI * 180;
                    if (opt.oo.limit3D) { reg0[0] = 0; reg0[1] = 0; }
                    if (isPlatformer()) { reg0[0] = 0; reg0[2] = 0; }
                    if (opt.oo.invertRoll) reg0 = -reg0;
                    if (!rightMove) reg0 = Vector3.zero;
                    keyControl(KEYMODE_SPIN);
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
                Vec.unitVector(reg3, 3);
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
                if (opt.oo.limit3D) reg2[2] = 0;
                if (opt.oo.invertYawAndPitch) for (int i = 0; i < reg2.Length; i++) reg2[i] = -reg2[i];
                if (!rightMove) Vec.zero(reg2);
                keyControl(KEYMODE_TURN);
                t = Vec.norm(reg2);
                if (t != 0)
                {
                    t *= dRotate * Math.PI / 180;
                    Vec.normalize(reg2, reg2);
                    for (int i = 0; i < 3; i++) reg4[i] = reg2[i] * Math.Sin(t);
                    reg4[3] = Math.Cos(t);
                    target.rotateAngle(reg3, reg4);
                }

                float f;
                if (isDrag(TYPE_ROLL)) {
                    relarot = dlRotRight;
                }
                else {
                    relarot = dfRotRight;
                    f = Mathf.Acos(relarot.w);
                    if (f>0) f = (float)(dRotate / limitAngRoll) * Mathf.Min((float)limitAngRoll * Mathf.PI / 180, f) / f;
                    relarot = Quaternion.Slerp(Quaternion.identity, relarot, f);
                }
                if (isPlatformer() || keepUpAndDown) { relarot[0] = 0; relarot[2] = 0; }
                if (opt.oo.invertRoll) relarot = Quaternion.Inverse(relarot);
                if (!rightMove) relarot = Quaternion.identity;
                if (opt.oo.limit3D) { relarot[0] = 0; relarot[1] = 0; }
                keyControl(KEYMODE_SPIN2);
                if (relarot.w < 1f) {
                    relarot.ToAngleAxis(out f, out reg0);
                    //f = Math.PI / 180 * (float)dRotate * f / Mathf.Max((float)limitAng, f);
                    reg1.Set(1, 0, 0);
                    Vector3.OrthoNormalize(ref reg0, ref reg1);
                    //for (int i = 0; i < 3; i++) relarot[i] = reg0[i] * Mathf.Sin(f);
                    //relarot[3] = Mathf.Cos(f);
                    reg0 = relarot * reg1;
                    for (int i = 0; i < 3; i++) reg3[i] = reg0[i];
                    reg3[3] = 0;
                    for (int i = 0; i < 3; i++) reg4[i] = reg1[i];
                    reg4[3] = 0;
                    Vec.normalize(reg3, reg3);
                    Vec.normalize(reg4, reg4);
                    target.rotateAngle(reg4, reg3);
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
        Vec.unitVector(reg3, Dir.getAxis(ad0));
        double d;
        if ((d = tActive - delta) > 0) {
            tActive = d;
            Vec.scale(reg3, reg3, Dir.getSign(ad0) * dMove);
            target.move(reg3);
        }
        else {
            d = tActive / timeMove;
            Vec.scale(reg3, reg3, Dir.getSign(ad0) * d);
            target.move(reg3);
            target.align().snap();
            command = null;
        }
    }

    private void alignRotate()
    {
        Vec.unitVector(reg3, Dir.getAxis(ad0));
        Vec.scale(reg3, reg3, Dir.getSign(ad0));
        double d;
        if ((d = tActive - delta) > 0) {
            tActive = d;
            Vec.rotateAbsoluteAngleDir(reg4, reg3, ad0, ad1, dRotate);
            target.rotateAngle(reg3, reg4);
        }
        else {
            d = 90 * tActive / timeRotate;
            Vec.rotateAbsoluteAngleDir(reg4, reg3, ad0, ad1, d);
            target.rotateAngle(reg3, reg4);
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
        engine.addShapes(alignMode);
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
    public const KeyCode KEY_SPINLEFT   = KeyCode.J;
    public const KeyCode KEY_SPINRIGHT  = KeyCode.L;
    public const KeyCode KEY_SPINUP     = KeyCode.I;
    public const KeyCode KEY_SPINDOWN   = KeyCode.K;
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
                if (Input.GetKey(KEY_SLIDELEFT )) reg3[0] = -1;
                if (Input.GetKey(KEY_SLIDERIGHT)) reg3[0] =  1;
                if (Input.GetKey(KEY_SLIDEUP   )) reg3[1] =  1;
                if (Input.GetKey(KEY_SLIDEDOWN )) reg3[1] = -1;
                if (Input.GetKey(KEY_SLIDEIN   )) reg3[2] =  1;
                if (Input.GetKey(KEY_SLIDEOUT  )) reg3[2] = -1;
                if (Input.GetKey(KEY_FORWARD   )) reg3[3] =  1;
                if (Input.GetKey(KEY_BACK      )) reg3[3] = -1;
                break;
            case (KEYMODE_TURN):
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) {
                    if (Input.GetKey(KEY_TURNLEFT )) reg2[0] = -1;
                    if (Input.GetKey(KEY_TURNRIGHT)) reg2[0] =  1;
                    if (Input.GetKey(KEY_TURNUP   )) reg2[1] =  1;
                    if (Input.GetKey(KEY_TURNDOWN )) reg2[1] = -1;
                    if (Input.GetKey(KEY_TURNIN   )) reg2[2] =  1;
                    if (Input.GetKey(KEY_TURNOUT  )) reg2[2] = -1;
                }
                break;
            case (KEYMODE_SPIN):
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    if (Input.GetKey(KEY_SPINLEFT )) reg0[0] = -1;
                    if (Input.GetKey(KEY_SPINRIGHT)) reg0[0] =  1;
                    if (Input.GetKey(KEY_SPINUP   )) reg0[1] =  1;
                    if (Input.GetKey(KEY_SPINDOWN )) reg0[1] = -1;
                    if (Input.GetKey(KEY_SPININ   )) reg0[2] =  1;
                    if (Input.GetKey(KEY_SPINOUT  )) reg0[2] = -1;
                }
                break;
            case (KEYMODE_SPIN2):
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    Quaternion q = Quaternion.identity;
                    if (Input.GetKey(KEY_SPINLEFT )) q *= Quaternion.Euler(-(float)dRotate,0,0);
                    if (Input.GetKey(KEY_SPINRIGHT)) q *= Quaternion.Euler( (float)dRotate,0,0);
                    if (Input.GetKey(KEY_SPINUP   )) q *= Quaternion.Euler(0, (float)dRotate,0);
                    if (Input.GetKey(KEY_SPINDOWN )) q *= Quaternion.Euler(0,-(float)dRotate,0);
                    if (Input.GetKey(KEY_SPININ   )) q *= Quaternion.Euler(0,0, (float)dRotate);
                    if (Input.GetKey(KEY_SPINOUT  )) q *= Quaternion.Euler(0,0,-(float)dRotate);
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

    public void closeMenu()
    {
        // leftL.enabled = false;
        // rightL.enabled = false;
        // SteamVR_Actions.control.Activate(left);
        // SteamVR_Actions.control.Activate(right);
        menuCanvas.enabled = false;
        inputCanvas.enabled = xrState == WebXRState.NORMAL && !hideController;
    }

    public void ToggleSkyBox(bool b)
    {
        engine.objColor = b ? Color.white * OptionsColor.fixer : Color.black;
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
    private void loadMaze() { loadMaze(store); store = null; }
    public void loadMaze(IStore store){
        try {
            if ( ! store.getString(KEY_CHECK).Equals(VALUE_CHECK) ) throw new Exception("getEmpty");//App.getEmptyException();
        } catch (Exception e) {
            throw e;//App.getException("Core.e1");
        }

    // read file, but don't modify existing objects until we're sure of success

        int dimLoad = store.getInteger(KEY_DIM);
        if ( ! (dimLoad == 3 || dimLoad == 4) ) throw new Exception("dimError");//App.getException("Core.e2");

        OptionsMap omLoad = new OptionsMap(dimLoad);
        OptionsColor ocLoad = new OptionsColor();
        OptionsView ovLoad = new OptionsView();
        OptionsSeed oeLoad = new OptionsSeed();

        store.getObject(KEY_OPTIONS_MAP,omLoad);
        store.getObject(KEY_OPTIONS_COLOR,ocLoad);
        store.getObject(KEY_OPTIONS_VIEW,ovLoad);
        store.getObject(KEY_OPTIONS_SEED,oeLoad);
        if ( ! oeLoad.isSpecified() ) throw new Exception("seedError");//App.getException("Core.e3");
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

        if (model.getDimension() == 3) throw new Exception("The system does not support 3D scene");

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

}