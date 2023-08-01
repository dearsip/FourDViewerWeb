using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Valve.VR;
// using Valve.VR.InteractionSystem;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WebXR;

public class Menu : MonoBehaviour
{
    public Core core;
    public OptionsAll oa;
    public ISelectShape iss;
    public Options optDefault;
    private bool isActivating;
    public Canvas canvas;
    public int tab { get; set; } = 7;
    public static readonly int NTAB = 8;

    [SerializeField] WebXRController leftC;
    private bool leftMButton, rightMButton, lastLeftMButton, lastRightMButton;


    public Slider dimSlider, sizeSlider, densitySlider, twistProbabilitySlider, branchProbabilitySlider, loopCrossProbabilitySlider,
        dimSameParallelSlider, dimSamePerpendicularSlider, depthSlider, retinaSlider, scaleSlider, trainSpeedSlider, mapDistanceSlider, cameraDistanceSlider,
        transparencySlider, lineThicknessSlider, retinaSizeSlider, baseTransparencySlider, sliceTransparencySlider, 
        timeMoveSlider, timeRotateSlider, timeAlignMoveSlider, timeAlignRotateSlider, widthSlider, flareSlider, rainbowGapSlider,iPDSlider, fovscaleSlider, distanceSlider;
    public InputField dimCurrent, dimNext, sizeCurrent, sizeNext, densityCurrent, densityNext, twistPobabilityCurrent, twistProbabilityNext,
        branchProbabilityCurrent, branchProbabilityNext, loopCrossProbabilityCurrent, loopCrossProbabilityNext, dimSameParallelField,
        dimSamePerpendicularField, mazeCurrent, mazeNext, colorCurrent, colorNext, depthField, retinaField, scaleField, trainSpeedField, cameraDistanceField,
        transparencyField, lineThicknessField, retinaSizeField, baseTransparencyField, sliceTransparencyField, 
        timeMoveField, timeRotateField, timeAlignMoveField, timeAlignRotateField, width, flare, rainbowGap, iPDField, fovscaleField, distanceField, paintColorField, quantityField;
    public Toggle allowLoopsCurrent, allowLoopsNext, allowReservedPathsCurrent, allowReservedPathsNext, arrow, usePolygon, useEdgeColor, hideSel, invertNormals, separate, map, focus, glass, invertLeftAndRight, invertForward,
        invertYawAndPitch, invertRoll, sliceMode, limit3D, showInput, keepUpAndDown, fisheye, custom, rainbow, glide, allowDiagonalMovement, buttonToggleModeLeft, buttonToggleModeRight, showController, showHint,horizontalInputFollowing, stereo, cross, invertX, invertY, alternativeControlIn3D, threeDMazeIn3DScene, paintWithAddButton;
    public Toggle[] enable, texture;
    public Dropdown colorMode, inputTypeLeftAndRight, inputTypeForward, inputTypeYawAndPitch, inputTypeRoll, paintColor, addShapes, paintMode;
    public Toggle[] keyShift;
    public Dropdown[] key;
    public Material defaultMat, alternativeMat;
    public GameObject environment;
    public Toggle skyboxToggle;
    public GameObject[] panels;

    private void Start()
    {
        canvas.enabled = false;
    }

    private void CloseMenu() {
        if (canvas.enabled)
        {
            doOK();
        }
    }

    void Update() {
        if (leftC.GetButtonUp(WebXRController.ButtonTypes.ButtonB))
            CloseMenu();
        lastLeftMButton = leftMButton;
        lastRightMButton = rightMButton;
    }
    public void Activate(ISelectShape iss)
    {
        isActivating = true;
        for (int i = 0; i < NTAB; i++) panels[i].SetActive(i == tab);

        put(dimCurrent, oa.omCurrent.dimMap);
        put(dimNext, dimSlider, oa.opt.om4.dimMap);
        put(sizeCurrent, oa.omCurrent.size);
        put(sizeNext, sizeSlider, oa.opt.om4.size);
        put(densityCurrent, oa.omCurrent.density);
        put(densityNext, densitySlider, oa.opt.om4.density);
        put(twistPobabilityCurrent, oa.omCurrent.twistProbability);
        put(twistProbabilityNext, twistProbabilitySlider, oa.opt.om4.twistProbability);
        put(branchProbabilityCurrent, oa.omCurrent.branchProbability);
        put(branchProbabilityNext, branchProbabilitySlider, oa.opt.om4.branchProbability);
        put(allowLoopsCurrent, oa.omCurrent.allowLoops);
        put(allowLoopsNext, oa.opt.om4.allowLoops);
        put(loopCrossProbabilityCurrent, oa.omCurrent.loopCrossProbability);
        put(loopCrossProbabilityNext, loopCrossProbabilitySlider, oa.opt.om4.loopCrossProbability);
        put(allowReservedPathsCurrent, oa.omCurrent.allowReservedPaths);
        put(allowReservedPathsNext, oa.opt.om4.allowReservedPaths);

        put(colorMode, oa.opt.oc4.colorMode);
        put(dimSameParallelField, dimSameParallelSlider, oa.opt.oc4.dimSameParallel);

        put(dimSamePerpendicularField, dimSamePerpendicularSlider, oa.opt.oc4.dimSamePerpendicular);
        put(enable, oa.opt.oc4.enable);

        put(mazeCurrent, oa.oeCurrent.mapSeed);
        put(colorCurrent, oa.oeCurrent.colorSeed);

        put(depthField, depthSlider, oa.opt.ov4.depth);
        put(arrow, oa.opt.ov4.arrow);
        put(texture, oa.opt.ov4.texture);
        put(retinaField, retinaSlider, oa.opt.ov4.retina);
        put(scaleField, scaleSlider, oa.opt.ov4.scale);

        put(transparencyField, transparencySlider, oa.opt.od.transparency);
        put(lineThicknessField, lineThicknessSlider, oa.opt.od.lineThickness);
        put(retinaSizeField, retinaSizeSlider, oa.opt.od.size);
        put(usePolygon, oa.opt.od.usePolygon);
        put(useEdgeColor, oa.opt.od.useEdgeColor);
        put(hideSel, oa.opt.od.hidesel);
        put(invertNormals, oa.opt.od.invertNormals);
        put(skyboxToggle, oa.opt.od.toggleSkyBox);
        put(separate, oa.opt.od.separate);
        put(map, oa.opt.od.map); focus.interactable = oa.opt.od.map; glass.interactable = oa.opt.od.map;
        put(focus, oa.opt.od.focus);
        put(glass, oa.opt.od.glass);
        put(mapDistanceSlider, oa.opt.od.mapDistance);
        put(cameraDistanceField, cameraDistanceSlider, oa.opt.od.cameraDistance);
        put(trainSpeedField, trainSpeedSlider, oa.opt.od.trainSpeed);
        put(glide, oa.opt.od.glide);

        put(inputTypeLeftAndRight, oa.opt.oo.inputTypeLeftAndRight);
        put(inputTypeForward, oa.opt.oo.inputTypeForward);
        put(inputTypeYawAndPitch, oa.opt.oo.inputTypeYawAndPitch);
        put(inputTypeRoll, oa.opt.oo.inputTypeRoll);
        put(invertLeftAndRight, oa.opt.oo.invertLeftAndRight);
        put(invertForward, oa.opt.oo.invertForward);
        put(invertYawAndPitch, oa.opt.oo.invertYawAndPitch);
        put(invertRoll, oa.opt.oo.invertRoll);
        put(sliceMode, oa.opt.oo.sliceMode);
        put(baseTransparencyField, baseTransparencySlider, oa.opt.oo.baseTransparency);
        put(sliceTransparencyField, sliceTransparencySlider, oa.opt.oo.sliceTransparency);
        put(limit3D, oa.opt.oo.limit3D);
        put(showInput, oa.opt.oo.showInput);
        put(keepUpAndDown, oa.opt.oo.keepUpAndDown);
        for (int i = 0; i < OptionsControl.NKEY; i++)
        {
            put(key[i], oa.opt.oo.key[i]);
            put(keyShift[i], oa.opt.oo.keyShift[i]);
        }

        put(timeMoveField, timeMoveSlider, oa.opt.ot4.timeMove);
        put(timeRotateField, timeRotateSlider, oa.opt.ot4.timeRotate);
        put(timeAlignMoveField, timeAlignMoveSlider, oa.opt.ot4.timeAlignMove);
        put(timeAlignRotateField, timeAlignRotateSlider, oa.opt.ot4.timeAlignRotate);
        put(paintWithAddButton, oa.opt.ot4.paintWithAddButton);

        put(fisheye, oa.opt.of.fisheye);
        put(custom, oa.opt.of.adjust);
        put(rainbow, oa.opt.of.rainbow);
        put(width, widthSlider, oa.opt.of.width);
        put(flare, flareSlider, oa.opt.of.flare);
        put(rainbowGap, rainbowGapSlider, oa.opt.of.rainbowGap);
        put(threeDMazeIn3DScene, oa.opt.of.threeDMazeIn3DScene);

        put(allowDiagonalMovement, oa.opt.oh.allowDiagonalMovement);
        put(buttonToggleModeLeft, oa.opt.oh.leftTouchToggleMode);
        put(buttonToggleModeRight, oa.opt.oh.rightTouchToggleMode);
        put(showController, oa.opt.oh.showController);
        put(showHint, oa.opt.oh.showHint); showHint.interactable = oa.opt.oh.showController;
        put(horizontalInputFollowing, oa.opt.oh.horizontalInputFollowing);
        put(stereo, oa.opt.oh.stereo);
        put(cross, oa.opt.oh.cross);
        put(invertX, oa.opt.oh.invertX);
        put(invertY, oa.opt.oh.invertY);
        put(iPDField, iPDSlider, oa.opt.oh.iPD);
        put(fovscaleField, fovscaleSlider, oa.opt.oh.fovscale);
        put(distanceField, distanceSlider, oa.opt.oh.cameraDistanceScale);
        put(alternativeControlIn3D, oa.opt.oh.alternativeControlIn3D);

        if (iss != null)
        {
            this.iss = iss;
            paintColor.options.Clear();
            paintColor.options.Add(new Dropdown.OptionData("(no effect)"));
            paintColor.options.Add(new Dropdown.OptionData("(paint remover)"));
            paintColor.options.Add(new Dropdown.OptionData("(random color)"));
            paintColor.options.Add(new Dropdown.OptionData("(in order)"));
            foreach (NamedObject<Color> c in iss.getAvailableColors())
            {
                paintColor.options.Add(new Dropdown.OptionData(c.name));
            }
            Color c_ = iss.getPaintColor();
            paintColor.value = iss.getAvailableColors().FindIndex(o => o.obj.Equals(c_)) + 4;
            if (paintColor.value == 3) { paintColor.value = 0;
                if (c_ == ISelectShape.REMOVE_COLOR) paintColor.value = 1;
                else if (c_ == ISelectShape.RANDOM_COLOR) paintColor.value = 2;
                else if (c_ == ISelectShape.INORDER_COLOR) paintColor.value = 3;
            }
            paintColor.RefreshShownValue();

            addShapes.options.Clear();
            addShapes.options.Add(new Dropdown.OptionData("(random block)"));
            foreach (NamedObject<Geom.Shape> s in iss.getAvailableShapes())
            {
                addShapes.options.Add(new Dropdown.OptionData(s.name));
            }
            Geom.Shape s_ = iss.getSelectedShape();
            addShapes.value = iss.getAvailableShapes().FindIndex(o => o.obj.Equals(s_)) + 1;
            addShapes.RefreshShownValue();
        }

        isActivating = false;
    }

    public void doUpdate()
    {
        if (isActivating) return;
        oa.opt.oc4.colorMode = getInt(colorMode);
        getInt(ref oa.opt.oc4.dimSameParallel, dimSameParallelField, OptionsColor.DIM_SAME_MIN, OptionsColor.DIM_SAME_MAX);
        getInt(ref oa.opt.oc4.dimSamePerpendicular, dimSamePerpendicularField, OptionsColor.DIM_SAME_MIN, OptionsColor.DIM_SAME_MAX);
        getBool(enable, oa.opt.oc4.enable);

        getInt(ref oa.opt.ov4.depth, depthField, OptionsView.DEPTH_MIN, OptionsView.DEPTH_MAX);
        oa.opt.ov4.arrow = getBool(arrow);
        getBool(texture, oa.opt.ov4.texture);
        getFloat(ref oa.opt.ov4.retina, retinaField, false);
        getFloat(ref oa.opt.ov4.scale, scaleField, OptionsView.SCALE_MIN, OptionsView.SCALE_MAX, false);

        getFloat(ref oa.opt.od.transparency, transparencyField, OptionsDisplay.TRANSPARENCY_MIN, OptionsDisplay.TRANSPARENCY_MAX, true);
        getFloat(ref oa.opt.od.lineThickness, lineThicknessField, false);
        getFloat(ref oa.opt.od.size, retinaSizeField, false);
        oa.opt.od.usePolygon = getBool(usePolygon);
        getFloat(ref oa.opt.od.size, retinaSizeField, false);
        oa.opt.od.useEdgeColor = getBool(useEdgeColor);
        oa.opt.od.hidesel = getBool(hideSel);
        oa.opt.od.invertNormals = getBool(invertNormals);
        oa.opt.od.toggleSkyBox = getBool(skyboxToggle);
        oa.opt.od.map = getBool(map); focus.interactable = oa.opt.od.map; glass.interactable = oa.opt.od.map;
        oa.opt.od.focus = getBool(focus); focus.isOn = oa.opt.od.focus;
        oa.opt.od.glass = getBool(glass); glass.isOn = oa.opt.od.glass;
        oa.opt.od.mapDistance = mapDistanceSlider.value;
        getFloat(ref oa.opt.od.cameraDistance, cameraDistanceField, true);
        getInt(ref oa.opt.od.trainSpeed, trainSpeedField);

        getFloat(ref oa.opt.oo.baseTransparency, baseTransparencyField, true);
        getFloat(ref oa.opt.oo.sliceTransparency, sliceTransparencyField, true);
        oa.opt.oo.showInput = getBool(showInput);

        oa.opt.of.fisheye = getBool(fisheye);
        oa.opt.of.adjust = getBool(custom);
        oa.opt.of.rainbow = getBool(rainbow);
        getFloat(ref oa.opt.of.width, width, 0, 1, false);
        getFloat(ref oa.opt.of.flare, flare, 0, 1, true);
        getFloat(ref oa.opt.of.rainbowGap, rainbowGap, 0, 1, true);
        oa.opt.of.threeDMazeIn3DScene = getBool(threeDMazeIn3DScene);
        oa.opt.of.recalculate();

        oa.opt.oh.invertX = getBool(invertX);
        oa.opt.oh.invertY = getBool(invertY);
        oa.opt.oh.stereo = getBool(stereo);
        oa.opt.oh.cross = getBool(cross);
        getFloat(ref oa.opt.oh.iPD, iPDField, true);
        getFloat(ref oa.opt.oh.fovscale, fovscaleField, false);
        getFloat(ref oa.opt.oh.cameraDistanceScale, distanceField, false);

        core.menuCommand = core.updateOptions;
    }

    public void doOK()
    {
        getInt(ref oa.opt.om4.dimMap, dimNext, OptionsMap.DIM_MAP_MIN, OptionsMap.DIM_MAP_MAX);
        getDimMap(oa.opt.om4.size, sizeNext);
        getFloat(ref oa.opt.om4.density, densityNext, OptionsMap.DENSITY_MIN, OptionsMap.DIM_MAP_MAX, true);
        getFloat(ref oa.opt.om4.twistProbability, twistProbabilityNext, OptionsMap.PROBABILITY_MIN, OptionsMap.PROBABILITY_MAX, true);
        getFloat(ref oa.opt.om4.branchProbability, branchProbabilityNext, OptionsMap.PROBABILITY_MIN, OptionsMap.PROBABILITY_MAX, true);
        oa.opt.om4.allowLoops = getBool(allowLoopsNext);
        getFloat(ref oa.opt.om4.loopCrossProbability, loopCrossProbabilityNext, OptionsMap.PROBABILITY_MIN, OptionsMap.PROBABILITY_MAX, true);
        oa.opt.om4.allowReservedPaths = getBool(allowReservedPathsNext);

        if (mazeNext.text.Length > 0)
        {
            oa.oeNext.mapSeedSpecified = true;
            getInt(ref oa.oeNext.mapSeed, mazeNext);
        }
        else oa.oeNext.mapSeedSpecified = false;
        if(colorCurrent.text.Length > 0)
        {
            getInt(ref oa.oeCurrent.colorSeed, colorCurrent);
            oa.oeCurrent.forceSpecified();
        }
        if(colorNext.text.Length > 0)
        {
            oa.oeNext.colorSeedSpecified = true;
            getInt(ref oa.oeNext.colorSeed, colorNext);
        }
        else oa.oeNext.colorSeedSpecified = false;

        oa.opt.od.separate = getBool(separate);
        oa.opt.od.glide = getBool(glide);

        oa.opt.oo.inputTypeLeftAndRight = getInt(inputTypeLeftAndRight);
        oa.opt.oo.inputTypeForward = getInt(inputTypeForward);
        oa.opt.oo.inputTypeYawAndPitch = getInt(inputTypeYawAndPitch);
        oa.opt.oo.inputTypeRoll = getInt(inputTypeRoll);
        oa.opt.oo.invertLeftAndRight = getBool(invertLeftAndRight);
        oa.opt.oo.invertForward = getBool(invertForward);
        oa.opt.oo.invertYawAndPitch = getBool(invertYawAndPitch);
        oa.opt.oo.invertRoll = getBool(invertRoll);
        oa.opt.oo.limit3D = getBool(limit3D);
        oa.opt.oo.keepUpAndDown = getBool(keepUpAndDown);
        oa.opt.oo.sliceMode = getBool(sliceMode);
        for (int i = 0; i < OptionsControl.NKEY; i++)
        {
            oa.opt.oo.key[i] = getInt(key[i]);
            oa.opt.oo.keyShift[i] = getBool(keyShift[i]);
        }

        getFloat(ref oa.opt.ot4.timeMove, timeMoveField, false);
        getFloat(ref oa.opt.ot4.timeRotate, timeRotateField, false);
        getFloat(ref oa.opt.ot4.timeAlignMove, timeAlignMoveField, false);
        getFloat(ref oa.opt.ot4.timeAlignRotate, timeAlignRotateField, false);
        oa.opt.ot4.paintWithAddButton = getBool(paintWithAddButton);

        oa.opt.oh.allowDiagonalMovement = getBool(allowDiagonalMovement);
        oa.opt.oh.leftTouchToggleMode = getBool(buttonToggleModeLeft);
        oa.opt.oh.rightTouchToggleMode = getBool(buttonToggleModeRight);
        oa.opt.oh.showController = getBool(showController);
        oa.opt.oh.showHint = getBool(showHint);
        oa.opt.oh.horizontalInputFollowing = getBool(horizontalInputFollowing);
        oa.opt.oh.alternativeControlIn3D = getBool(alternativeControlIn3D);

        PropertyFile.save(core.save, PropertyFile.SaveType.SAVE_PROPERTIES);

        // command
        core.menuCommand = core.setOptions;
        doCancel();
    }

    public void doCancel()
    {
        if (iss != null)
        {
            iss.setPaintMode(-paintMode.value);
            SelectPaintColor();
            SelectAddShapes();
        }
        core.closeMenu();
    }

    public void doAlign()
    {
        core.doAlign();
    }

    public void doNewGame(int dim)
    {
        doOK();
        core.dimNext = dim;
        core.menuCommand = core.newGame;
    }

    public void doRestart() {
        core.restartGame();
    }

    public void doResetWin() {
        core.resetWin();
    }

    public void doToggleSkybox()
    {
        if (oa.opt.od.toggleSkyBox) {
            environment.SetActive(false);
            RenderSettings.skybox = alternativeMat;
        }

        else {
            environment.SetActive(true);
            RenderSettings.skybox = defaultMat;
        }
        core.ToggleSkyBox();
    }

    private bool SelectPaintColor()
    {
        Color c;
        if (!ColorUtility.TryParseHtmlString(paintColorField.text, out c))
        {
            string s = paintColor.options[paintColor.value].text;
            if (s == "(no effect)") { c = ISelectShape.NO_EFFECT_COLOR; }
            else if (s == "(paint remover)") { c = ISelectShape.REMOVE_COLOR; }
            else if (s == "(random color)") { c = ISelectShape.RANDOM_COLOR; }
            else if (s == "(in order)") { c = ISelectShape.INORDER_COLOR; }
            else foreach (NamedObject<Color> c_ in iss.getAvailableColors())
                if (c_.name == s) c = c_.obj;
        }
        if (c == Color.clear) return false;
        iss.setSelectedColor(c);
        iss.setPaintColor(c);
        return true;
    }

    private void SelectAddShapes()
    {
        string s = addShapes.options[addShapes.value].text;
        if (s == "(random block)") iss.setSelectedShape(null);
        else
        {
            foreach (NamedObject<Geom.Shape> shape in iss.getAvailableShapes())
            {
                if (shape.name == s)
                {
                    iss.setSelectedShape(shape.obj);
                    break;
                }
            }
        }
    }

    public void doPaint()
    {
        if (iss == null) return;
        iss.setPaintMode(-paintMode.value);
        if (SelectPaintColor() && core.menuCommand == null) core.menuCommand = core.doPaint;
    }

    public void doAddShapes()
    {
        if (iss == null) return;
        SelectPaintColor();
        SelectAddShapes();
        try { core.quantity = int.Parse(quantityField.text); } catch (Exception) { core.quantity = 1; }
        if (core.menuCommand == null) core.menuCommand = core.doAddShapes;
    }

    public void doChangeScene() { SceneManager.LoadScene("New Scene"); }

    public void doReset(bool all)
    {
        if (all)
        {
            for (int i = 0; i < NTAB; i++) doReset(i);
        }
        else doReset(tab);
        Activate(null);
        core.menuCommand = core.updateOptions;
    }

    private void doReset(int tab)
    {
        switch (tab)
        {
            case 0: OptionsMap.copy(oa.opt.om4, optDefault.om4); break;
            case 1: OptionsColor.copy(oa.opt.oc4, optDefault.oc4); break;
            case 2: OptionsView.copy(oa.opt.ov4, optDefault.ov4); break;
            case 3: OptionsDisplay.copy(oa.opt.od, optDefault.od); break;
            case 4: OptionsControl.copy(oa.opt.oo, optDefault.oo); break;
            case 5: OptionsMotion.copy(oa.opt.ot4, optDefault.ot4); break;
            case 6: OptionsFisheye.copy(oa.opt.of, optDefault.of); break;
            case 7: OptionsTouch.copy(oa.opt.oh, optDefault.oh); break;
        }
    }

    private void put(InputField inputField, Slider slider, float value)
    {
        slider.value = value;
        inputField.text = value.ToString();
    }

    private void put(InputField inputField, Slider slider, double value)
    {
        slider.value = (float)value;
        inputField.text = value.ToString();
    }

    private void put(InputField inputField, Slider slider, int[] value)
    {
        slider.value = value[0];
        inputField.text = Vec.ToString(value);
    }

    private void put(InputField inputField, int value)
    {
        inputField.text = value.ToString();
    }

    private void put(InputField inputField, int[] value)
    {
        inputField.text = Vec.ToString(value);
    }

    private void put(InputField inputField, float value)
    {
        inputField.text = value.ToString();
    }

    private void put(InputField inputField, double value)
    {
        inputField.text = value.ToString();
    }

    private void put(Slider slider, float value)
    {
        slider.value = (float)value;
    }

    private void put(Slider slider, double value)
    {
        slider.value = (float)value;
    }

    private void put(InputField inputField, Slider slider, int value)
    {
        slider.value = value;
        inputField.text = value.ToString();
    }

    private void put(Toggle toggle, bool value)
    {
        toggle.isOn = value;
    }

    private void put(Toggle[] toggle, bool[] value)
    {
        for (int i = 0; i < toggle.Length; i++) toggle[i].isOn = value[i];
    }

    private void put(Dropdown dropdown, int value)
    {
        dropdown.value = value;
    }

    private void getInt(ref int i, InputField inputField)
    {
        try {
            i = int.Parse(inputField.text);
        } catch (Exception e) { Debug.LogException(e); }
    }

    private void getInt(ref int i, InputField inputField, bool allowZero)
    {
        try {
            i = int.Parse(inputField.text);
        if (i < 0 || (!allowZero && i == 0)) throw new Exception();
        } catch (Exception e) { Debug.LogException(e); }
    }

    private void getInt(ref int i, InputField inputField, int min, int max)
    {
        try {
            int i_ = int.Parse(inputField.text);
            if (i_ < min || i_ > max) throw new Exception();
            i = i_;
        } catch (Exception e) { Debug.LogException(e); }
    }

    private void getFloat(ref float f, InputField inputField, bool allowZero)
    {
        try {
            float f_ = float.Parse(inputField.text);
            if (f_ < 0 || (!allowZero && f_ == 0)) throw new Exception();
            f = f_;
        } catch (Exception e) { Debug.LogException(e); }
    }

    private void getFloat(ref float f, InputField inputField, float min, float max, bool allowMin)
    {
        try {
            float f_ = float.Parse(inputField.text);
            if (f_ > max || f_ < min || (!allowMin && f_ == min)) throw new Exception();
            f = f_;
        } catch (Exception e) { Debug.LogException(e); }
    }

    private void getDouble(ref double d, InputField inputField, bool allowZero)
    {
        try {
            double d_ = float.Parse(inputField.text);
            if (d_ < 0 || (!allowZero && d_ == 0)) throw new Exception();
            d = d_;
        } catch (Exception e) { Debug.LogException(e); }
    }

    private void getDouble(ref double d, InputField inputField, double min, double max, bool allowMin)
    {
        try {
            double d_ = float.Parse(inputField.text);
            if (d_ > max || d_ < min || (!allowMin && d_ == min)) throw new Exception();
            d = d_;
        } catch (Exception e) { Debug.LogException(e); }
    }

    readonly char[] separator = new char[] { ',' };
    private void getDimMap(int[] dest, InputField inputField)
    {
        try {
            string[] reg = inputField.text.Split(separator);
            if (reg.Length == 1)
            {
                int j = int.Parse(reg[0].Trim());
                for (int i = 0; i < dest.Length; i++) dest[i] = j;
                return;
            }
            for (int i = 0; i < dest.Length; i++) dest[i] = int.Parse(reg[i].Trim());
        } catch ( Exception e) { Debug.LogException(e); }
    }

    private int getInt(Dropdown dropdown) { return dropdown.value; }

    private bool getBool(Toggle toggle) { return toggle.isOn; }

    private void getBool(Toggle[] toggle, bool[] bools) { for (int i = 0; i < toggle.Length; i++) bools[i] = getBool(toggle[i]); }
}