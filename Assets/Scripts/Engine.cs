using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * The game graphics engine.
 */

public class Engine : IMove
{

    // --- fields ---

    //private IDisplay displayInterface;
    private IModel model;

    private double[] origin;
    private double[][] axis;
    private bool win;
    private bool keepUpAndDown;

    private Clip.Result clipResult;
    private double fall;
    //private double height;
    //private double gravity;
    private const double gdef = 18;
    private const double hdef = 7.5;

    private double[][] sraxis;
    private double nonFisheyeRetina; // have to cache this up here for fisheye

    private PolygonBuffer bufAbsolute;
    private PolygonBuffer bufRelative;
    //private PolygonBuffer[] bufDisplay;

    private RenderRelative renderRelative;

    private double[][] objRetina, objCross, objWin, objDead;

    //private Display[] display;

    private int dimSpaceCache; // display-related cache of engine-level fields
    private bool enableCache;
    private int edgeCache;

    private int[] reg1; // temporary registers
    private int[] reg2;
    private double[] reg3;
    private double[] reg4;
    private double[] reg9;
    private double[] regA;
    private double[] regB;

    private MainTransform mt;
    private SideTransform st;
    private CrossTransform ct;

    private double[] reg5, reg6, reg7, reg8;
    private List<Vector3> verts;
    private List<int> tris;
    private List<Color> cols;
    private Mesh mesh;
    //private double border;

    // --- construction ---

    /**
     * Construct a new engine object.
     * After construction, you must call newGame before anything else.
     */
    public Engine(/*IDisplay displayInterface*/ Mesh mesh)
    {
        //this.displayInterface = displayInterface;

        // dimSpaceCache starts at zero, that will force rebuild ...
        //  ... so enableCache is irrelevant
        // edgeCache starts at zero
        this.mesh = mesh;
    }

    // --- helpers ---

    // caller must not change these!

    public double[] getOrigin()
    {
        return origin;
    }

    public double[] getViewAxis()
    {
        return axis[axis.Length - 1];
    }

    public double[][] getAxisArray()
    {
        return axis;
    }

    public IModel retrieveModel()
    {
        return model;
    }

    // --- games ---

    public void newGame(int dimSpace, IModel model, OptionsView ov, /*OptionsStereo os,*/ OptionsMotion ot, bool render)
    {

        this.model = model;

        origin = new double[dimSpace];
        axis = new double[dimSpace][];
        for (int i = 0; i < axis.Length; i++) axis[i] = new double[dimSpace];
        initPlayer();

        if (isPlatformer()) ((ActionModel)model).setEngine(this);

        sraxis = new double[dimSpace][];
        for (int i = 0; i < sraxis.Length; i++) sraxis[i] = new double[dimSpace];
        nonFisheyeRetina = ov.retina;

        bufAbsolute = new PolygonBuffer(dimSpace);
        bufRelative = new PolygonBuffer(dimSpace - 1);
        //bufDisplay = new PolygonBuffer[2];
        //bufDisplay[0] = new PolygonBuffer(2);
        //bufDisplay[1] = new PolygonBuffer(2); // may not be used

        model.setBuffer(bufAbsolute);
        renderRelative = new RenderRelative(bufAbsolute, bufRelative, dimSpace, getRetina());

        //if (dimSpace == 3)
        //{
        //    objRetina = objRetina2;
        //    objCross = objCross2;
        //    objWin = objWin2;
        //    objDead = objDead2;
        //}
        //else
        {
            objRetina = objRetina3;
            objCross = objCross3;
            objWin = objWin3;
            objDead = objDead3;
        }

        //setDisplay(dimSpace, ov.scale, /*os,*/ true);

        reg1 = new int[dimSpace];
        reg2 = new int[dimSpace];
        reg3 = new double[dimSpace];
        reg4 = new double[dimSpace];
        reg9 = new double[dimSpace];
        regA = new double[dimSpace];
        regB = new double[dimSpace];

        mt = new MainTransform(reg3);
        st = new SideTransform(reg3);
        ct = new CrossTransform(reg3);

        reg5 = new double[3];
        reg6 = new double[3];
        reg7 = new double[3];
        reg8 = new double[3];
        verts = new List<Vector3>();
        tris = new List<int>();
        cols = new List<Color>();

        fall = 0;
        //gravity = gdef / ot.frameRate / ot.frameRate;
        //height = hdef / ot.frameRate;

        //if (render) renderAbsolute();
        // else we are loading a saved game, and will render later
    }

    private void initPlayer()
    {
        model.initPlayer(origin, axis);
        win = false;
    }

    public void resetWin()
    {
        if (win && !atFinish())
        {
            win = false;
            //RenderRelative();
        }
    }

    public void restartGame()
    {
        initPlayer();
        //renderAbsolute();
    }

    public void toggleFisheye()
    {
        OptionsFisheye.of.fisheye = !OptionsFisheye.of.fisheye;
        updateRetina();
    }

    private double getRetina()
    {
        return OptionsFisheye.of.fisheye ? 1 : nonFisheyeRetina;
    }

    private void updateRetina()
    {
        renderRelative.setRetina(getRetina());
        model.setRetina(getRetina());
    }

    // --- implementation of IStorable ---

    private const string KEY_ORIGIN = "origin";
    private const string KEY_AXIS = "axis";
    private const string KEY_WIN = "win";

    public void load(IStore store, bool alignMode)
    {
        try
        {

            store.getObject(KEY_ORIGIN, origin);
            store.getObject(KEY_AXIS, axis);
            win = store.getBool(KEY_WIN);

            model.testOrigin(origin, reg1, reg2);

            // check that axes are orthonormal, more or less
            const double EPSILON = 0.001;
            for (int i = 0; i < axis.Length; i++)
            {
                for (int j = 0; j < axis.Length; j++)
                {
                    double dotExpected = (i == j) ? 1 : 0; // delta_ij
                    double dot = Vec.dot(axis[i], axis[j]);
                    if (Math.Abs(dot - dotExpected) > EPSILON) throw new Exception("axis vector is zero.");//App.getEmptyException();
                }
            }

            if (alignMode) align().snap();
            //
            // pseudo-validation to prevent being in align mode without being aligned.
            // this can only happen if someone modifies a file by hand
            //
            // a real validation would compare the current position to the align goal,
            // and if they were different, would snap to the goal and throw an exception
            // carrying a message similar to Engine.e1

        }
        catch (Exception)
        {
            initPlayer();
            throw new Exception("Unable to set position, restarting saved game.");
        }
        finally
        {
            //renderAbsolute();
        }
    }

    public int getSaveType()
    {
        return model.getSaveType();
    }

    public void save(IStore store, OptionsMap om) {
        store.putObject(KEY_ORIGIN,origin);
        store.putObject(KEY_AXIS,axis);
        store.putBool(KEY_WIN,win);
        if (getSaveType() == IModel.SAVE_MAZE) ((MapModel)model).save(store, om);
    }

    // --- options ---

    public void setColorMode(int colorMode)
    {
        model.setColorMode(colorMode);
    }

    public void setDepth(int depth)
    {
        model.setDepth(depth);
    }

    public void setTexture(bool[] texture)
    {
        model.setTexture(texture);
    }

    public void setRetina(double retina)
    {
        nonFisheyeRetina = retina;
        updateRetina();
    }

    //public void setScale(double scale)
    //{
    //    for (int i = 0; i < display.Length; i++) display[i].setScale(scale);
    //}

    //public void setScreenWidth(double screenWidth)
    //{
    //    for (int i = 0; i < display.Length; i++) display[i].setScreenWidth(screenWidth);
    //}

    //public void setScreenDistance(double screenDistance)
    //{
    //    for (int i = 0; i < display.Length; i++) display[i].setScreenDistance(screenDistance);
    //}

    //public void setEyeSpacing(double eyeSpacing)
    //{
    //    for (int i = 0; i < display.Length; i++) display[i].setEyeSpacing(eyeSpacing);
    //}

    //public void setTiltVertical(double tiltVertical)
    //{
    //    for (int i = 0; i < display.Length; i++) display[i].setTiltVertical(tiltVertical);
    //}

    //public void setTiltHorizontal(double tiltHorizontal)
    //{
    //    for (int i = 0; i < display.Length; i++) display[i].setTiltHorizontal(tiltHorizontal);
    //}

    // these last two option-setting functions are not invoked via the controller,
    // so they need to end with an explicit re-render call

    public void setOptions(OptionsColor oc, OptionsView ov, /*OptionsStereo os,*/ OptionsSeed oe, OptionsMotion ot, OptionsDisplay od)
    {

        model.setOptions(oc, oe.colorSeed, ov.depth, ov.texture, od);

        setRetina(ov.retina);
        //border = od.border;

        //setDisplay(dimSpaceCache, ov.scale, /*os,*/ false);

        //gravity = gdef / ot.frameRate / ot.frameRate;
        //height = hdef / ot.frameRate;

        //renderAbsolute(); // not always necessary, but who cares, it's fast enough
        width = od.lineThickness;
    }

    public void setKeepUpAndDown(bool b) {
        keepUpAndDown = b;
        if (keepUpAndDown) {
            if (axis[1][1] < 0) {
                for (int i = 0; i < axis.Length; i++) Vec.scale(axis[i],axis[i],-1);
            }
            else if (!(axis[1][1] > 0)) {
                Vec.copy(reg3,axis[1]);
                Vec.unitVector(reg4,1);
                for (int i = 0; i < axis.Length; i++) Vec.rotate(axis[i],axis[i],reg3,reg4,regA,regB);
            }
        }
    }

    //public void setEdge(int edge)
    //{
    //    edgeCache = edge;
    //    for (int i = 0; i < display.Length; i++) display[i].setEdge(edge);
    //    renderDisplay();
    //}

    // --- display ---

    private const int DISPLAY_MODE_NONE = 0;
    private const int DISPLAY_MODE_3D = 1;
    private const int DISPLAY_MODE_4D_MONO = 2;
    private const int DISPLAY_MODE_4D_STEREO = 3;

    private int getDisplayMode(int dimSpace, bool enable)
    {
        switch (dimSpace)
        {
            case 3: return DISPLAY_MODE_3D;
            case 4: return enable ? DISPLAY_MODE_4D_STEREO : DISPLAY_MODE_4D_MONO;
            default: return DISPLAY_MODE_NONE;
        }
    }

    private int getPanels(int mode)
    {
        return (mode == DISPLAY_MODE_4D_STEREO) ? 2 : 1;
    }

    //private void setDisplay(int dimSpace, double scale, /*OptionsStereo os,*/ bool force)
    //{

    //    int modeNew = getDisplayMode(dimSpace, os.enable);
    //    int modeOld = getDisplayMode(dimSpaceCache, enableCache);

    //    // here we are embedding the knowledge that the edge changes (and setEdge gets called)
    //    // if and only if the number of visible panels changes.
    //    // so, that's the condition we should use to decide when to clear the cache.
    //    // the goal is not to draw stereo displays until after re-layout occurs
    //    if (getPanels(modeNew) != getPanels(modeOld)) edgeCache = 0;

    //    if (modeNew != modeOld || force)
    //    { // must rebuild on new game, because buffers are changing
    //        dimSpaceCache = dimSpace;
    //        enableCache = os.enable;
    //        rebuildDisplay(scale, os);
    //    }
    //    else
    //    {
    //        for (int i = 0; i < display.Length; i++) display[i].setOptions(scale, os);
    //    }
    //}

    /**
     * A function that rebuilds the display objects, for when the stereo mode has changed.
     */
    //private void rebuildDisplay(double scale, OptionsStereo os)
    //{
    //    switch (getDisplayMode(dimSpaceCache, os.enable))
    //    {

    //        case DISPLAY_MODE_3D:

    //            display = new Display[1];
    //            display[0] = new DisplayScaled(bufRelative, bufDisplay[0], scale);

    //            displayInterface.setMode3D(bufDisplay[0]);
    //            break;

    //        case DISPLAY_MODE_4D_MONO:

    //            display = new Display[1];
    //            display[0] = new DisplayStereo(bufRelative, bufDisplay[0], 0, scale, os, edgeCache);

    //            displayInterface.setMode4DMono(bufDisplay[0]);
    //            break;

    //        case DISPLAY_MODE_4D_STEREO:

    //            display = new Display[2];
    //            display[0] = new DisplayStereo(bufRelative, bufDisplay[0], -1, scale, os, edgeCache);
    //            display[1] = new DisplayStereo(bufRelative, bufDisplay[1], +1, scale, os, edgeCache);

    //            displayInterface.setMode4DStereo(bufDisplay[0], bufDisplay[1]);
    //            break;
    //    }
    //}

    // --- motion ---

    public bool canMove(int a, double d)
    {

        if (!isPlatformer())
        {
            Vec.addScaled(reg3, origin, axis[a], d);
            if (!model.canMove(origin, reg3, reg1, reg4, true)) return false;
        }

        return true;
    }

    private bool atFinish()
    {
        return model.atFinish(origin, reg1, reg2);
    }

    private bool isPlatformer() { 
        return getSaveType() == IModel.SAVE_ACTION
            || getSaveType() == IModel.SAVE_BLOCK
            || getSaveType() == IModel.SAVE_SHOOT; 
    }
    const double epsilon = 0.00001;
    public void move(double[] d)
    {
        if (!isPlatformer()) {
            Vec.fromAxisCoordinates(reg3, d, axis);
            Vec.add(origin, origin, reg3);
        }
        else {
            d[1] = 0;
            Vec.fromAxisCoordinates(reg3, d, axis);
            Vec.unitVector(reg4, 1);
            Vec.rotate(d,reg3,axis[1],reg4,regA,regB);
            Vec.add(reg3, origin, d);
            if (model.canMove(origin, reg3, reg1, reg4, false))
            {
                Vec.copy(origin, reg3);
            }
            else
            { // not functioning (climing)
                Clip.Result clipResult = ((ActionModel)model).getResult();
                int ib = clipResult.ib;
                Vec.unitVector(reg3, 1);
                Vec.addScaled(reg3, origin, reg3, Vec.norm(d));
                if ((Clip.clip(origin, reg3, ((GeomModel)model).retrieveShapes()[ib], clipResult) & Clip.KEEP_B) != 0)
                {
                    Vec.unitVector(reg3, 1);
                    Vec.addScaled(origin, origin, reg3, fall * clipResult.b + epsilon);
                }
            }
        }
    }

    public void rotateAngle(double[] from, double[] to)
    {
        if (!isPlatformer() && !keepUpAndDown) {
            Vec.fromAxisCoordinates(reg3, from, axis);
            Vec.fromAxisCoordinates(reg4, to, axis);
            Vec.normalize(reg3, reg3);
            Vec.normalize(reg4, reg4);
            for (int i = 0; i < axis.Length; i++) Vec.rotate(axis[i], axis[i], reg3, reg4, from, to);
        }
        else
        {
            const double epsilon = 0.000001;
            if (from[3] > 0)
            {
                // w-y rotation
                Vec.zero(reg3);
                reg3[1] = to[1];
                reg3[3] = Math.Sqrt(1-reg3[1]*reg3[1]);
                Vec.fromAxisCoordinates(reg4, reg3, axis);
                Vec.copy(reg3,axis[1]);
                Vec.normalize(reg4, reg4);
                Vec.rotate(axis[1],axis[1],axis[3],reg4,regA,regB);
                if (axis[1][1] < epsilon) Vec.copy(axis[1],reg3);
                else {
                    Vec.rotate(axis[3],axis[3],axis[3],reg4,regA,regB);
                    Vec.normalize(axis[1], axis[1]);
                    Vec.normalize(axis[3], axis[3]);
                }
                // w-x&z rotation
                Vec.copy(reg3,to);
                reg3[1] = 0;
                reg3[3] = Math.Sqrt(1-reg3[0]*reg3[0]-reg3[2]*reg3[2]);
                Vec.fromAxisCoordinates(reg4, reg3, axis);
                Vec.unitVector(reg3, 1);
                Vec.normalize(reg3, reg3);
                Vec.rotate(reg4,reg4,axis[1],reg3,regA,regB); // to
                Vec.rotate(reg9,axis[3],axis[1],reg3,regA,regB); // from
                Vec.normalize(reg4, reg4);
                Vec.normalize(reg9, reg9);
                for (int i = 0; i < axis.Length; i++) Vec.rotate(axis[i],axis[i],reg9,reg4,regA,regB);
            }
            else
            {
                // x-z rotation (already restricted by control())
                Vec.fromAxisCoordinates(reg3, from, axis);
                Vec.fromAxisCoordinates(reg4, to, axis);
                Vec.unitVector(reg9, 1);
                Vec.rotate(reg3,reg3,axis[1],reg9,regA,regB);
                Vec.rotate(reg4,reg4,axis[1],reg9,regA,regB);
                Vec.normalize(reg3, reg3);
                Vec.normalize(reg4, reg4);
                for (int i = 0; i < axis.Length; i++) Vec.rotate(axis[i],axis[i],reg3,reg4,regA,regB);
            }
        }
    }

    public Align align()
    {
        if (getSaveType() != IModel.SAVE_ACTION
         && getSaveType() != IModel.SAVE_BLOCK
         && getSaveType() != IModel.SAVE_SHOOT)
        {
            return new Align(origin, axis);
        }
        return null;
    }

    public bool isAligned()
    {
        return Align.isAligned(origin, axis);
    }

    public bool update(double[] saveOrigin, double[][] saveAxis, double[] viewOrigin)
    {
        if (model.canMove(saveOrigin, origin, reg1, reg4, false))
        {
            if (atFinish()) win = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void save(double[] saveOrigin, double[][] saveAxis)
    {
        Vec.copy(saveOrigin, origin);
        Vec.copyMatrix(saveAxis, axis);
    }

    public void restore(double[] saveOrigin, double[][] saveAxis)
    {
        Vec.copy(origin, saveOrigin);
        Vec.copyMatrix(axis, saveAxis);
    }

    public void jump()
    {
        const double epsilon = 0.001;
        Vec.unitVector(reg3, 1);
        Vec.addScaled(reg3, origin, reg3, -epsilon);
        if (!model.canMove(origin, reg3, reg1, reg4, true) || reg3[1] < 0) fall = hdef;
    }

    public void Fall(double delta)
    {
        const double epsilon = 0.00001;
        Vec.unitVector(reg3, 1);
        double d = fall*delta - gdef*delta*delta/2;
        Vec.addScaled(reg3, origin, reg3, d);
        fall -= gdef*delta;
        if (reg3[1] < epsilon)
        {
            fall = 0;
            origin[1] = epsilon;
        }
        else if (model.canMove(origin, reg3, reg1, reg4, true))
        {
            Vec.copy(origin, reg3);
        }
        else
        {
            clipResult = ((ActionModel)model).getResult();
            Vec.unitVector(reg3, 1);
            Vec.addScaled(origin, origin, reg3, d * clipResult.a + ((fall > 0) ? -epsilon : epsilon));
            fall = 0;
        }
        if (atFinish()) win = true;
    }

    public void addShapes(bool alignMode) {
        if (getSaveType() != IModel.SAVE_MAZE) {
            GeomModel m = (GeomModel)model;
            if (m.canAddShapes()) m.addShapes(1, alignMode, origin, axis[axis.Length-1]);
        }
    }

    public void removeShape() {
        if (getSaveType() != IModel.SAVE_MAZE) ((GeomModel)model).removeShape(origin,axis[axis.Length-1]);
    }

    // --- rendering ---

    public void renderAbsolute(double[] eyeVector, OptionsControl oo, double delta)
    {
        try {
            model.animate(delta);
            model.render(origin, axis);
            RenderRelative(eyeVector, oo);
        }catch(Exception e) {Debug.LogException(e);};
    }

    private readonly Color white = new Color(0, 0, 0);
    private void renderObject(PolygonBuffer buf, double[][] obj)
    {
        renderObject(buf, obj, white);
    }

    private void renderObject(PolygonBuffer buf, double[][] obj, Color color)
    {
        //for (int i = 0; i < obj.Length; i += 3)
        //{
        //    Polygon poly = buf.getNext();

        //    poly.vertex = new double[3][];
        //    for (int j = 0; j < 3; j++)
        //    {
        //        poly.vertex[j] = new double[3];
        //        Vec.copy(poly.vertex[j], obj[i + j]);
        //    }
        //    poly.color = color;
        //}
        for (int i = 0; i < obj.Length; i += 2)
        {
            buf.add(obj[i], obj[i + 1], color);
        }
    }

    private void renderPolygon(PolygonBuffer buf, double[][] obj, int n) // for sliceMode
    {
        renderPolygon(buf, obj, n, Color.clear);
    }
    private void renderPolygon(PolygonBuffer buf, double[][] obj, int n, Color color)
    {
        Polygon poly = new Polygon();
        poly.vertex = new double[n][];
        for (int i = 0; i < obj.Length; i += n)
        {
            for (int j = 0; j < n; j++) poly.vertex[j] = obj[i + j];
            poly.color = color;
            buf.add(poly);
        }
    }
    private void renderPolygon(PolygonBuffer buf, double[][] obj, int n, int dir)
    {
        Polygon poly = new Polygon();
        poly.vertex = new double[n][];
        for (int i = 0; i < obj.Length; i += n)
        {
            for (int j = 0; j < n; j++) {
                switch (dir) {
                    case 2:
                        reg3[0] = obj[i + j][2];
                        reg3[1] = obj[i + j][1];
                        reg3[2] =-obj[i + j][0];
                        break;
                    case 3:
                        reg3[0] = obj[i + j][0];
                        reg3[1] =-obj[i + j][2];
                        reg3[2] = obj[i + j][1];
                        break;
                    default:
                        reg3[0] = obj[i + j][0];
                        reg3[1] = obj[i + j][1];
                        reg3[2] = obj[i + j][2];
                        break;
                }
                poly.vertex[j] = new double[3]; 
                Vec.copy(poly.vertex[j], reg3);
            }
            poly.color = Color.clear;
            buf.add(poly);
        }
    }

    private void RenderRelative(double[] eyeVector, OptionsControl oo)
    {
        if (OptionsFisheye.of.fisheye)
        {
            renderPrepare();
            if (OptionsFisheye.of.rainbow/* && dimSpaceCache == 4*/)
            {
                renderRainbow();
            }
            else
            {
                renderFisheye();
            }
        }
        else
        {
            renderRelative.run(axis, model.getSaveType()==IModel.SAVE_MAZE);
            renderObject(bufRelative, objRetina);
            renderPolygon(bufRelative, objRetinaPoly, 3, oo.sliceDir);
            renderObject(bufRelative, objCross);
            renderPolygon(bufRelative, objCrossPoly, 4, oo.sliceDir);
        }

        if (win)
        {
            renderPolygon(bufRelative, objWinSlice, 4, oo.sliceDir);
            renderObject(bufRelative, objWin);
        }
        if (model.dead()) renderObject(bufRelative, objDead, Color.red);

        if (getSaveType() == IModel.SAVE_MAZE  && ((MapModel)model).showMap)
        {
            bufRelative.add(((MapModel)model).bufRelative);
            renderObject(bufRelative, objCrossMap);
            renderPolygon(bufRelative, objCrossMapPoly, 4, oo.sliceDir);
        }

        //renderDisplay();
        bufRelative.sort(eyeVector);
        convert(eyeVector, oo);
    }

    private void renderPrepare()
    {
        int f = sraxis.Length - 1; // forward

        Vec.zero(reg3);

        // no need to set first or last now,
        // they'll get set in the next loop
        for (int i = 1; i < f; i++)
        {
            Vec.copy(sraxis[i], axis[i]);
        }
    }

    private void renderFisheye()
    {
        int f = sraxis.Length - 1;

        renderRelative.run(axis, true, mt, model.getSaveType()==IModel.SAVE_MAZE);
        renderRelative.runObject(objCross, -1, ct);

        for (int i = 0; i < f; i++)
        {
            renderPair(f, i, i);
        }
    }

    private void renderRainbow()
    {
        int f = sraxis.Length - 1;

        reg3[1] = -OptionsFisheye.rdist;
        renderRelative.run(axis, true, mt, model.getSaveType()==IModel.SAVE_MAZE);
        renderRelative.runObject(objRetina, r, mt);
        renderRelative.runObject(objCross, -1, ct);
        renderPair(f, 0, 0); // x pair offset in x

        Vec.copy(sraxis[3], axis[3]); // renderPair doesn't really put everything back

        // for the rest of them we have to rotate everything.
        // positive z goes to positive x, that's the
        // natural way because of the retina horizontal tilt.
        //
        Vec.scale(sraxis[2], axis[0], -1);
        Vec.copy(sraxis[0], axis[2]);

        reg3[1] = OptionsFisheye.rdist;
        renderRelative.run(sraxis, false, mt, model.getSaveType()==IModel.SAVE_MAZE);
        renderRelative.runObject(objRetina, r, mt);
        renderRelative.runObject(objCross, -1, ct);
        renderPair(f, 2, 0); // z pair offset in x

        // no need to put back, we're done
    }

    private static int[] rmask = new int[] { 0x7, 0xD, 0xE, 0xB, 0x677, 0x9DD, 0xCEE, 0x3BB, 0xFF0, 0xF0F };
    private const int r = 0x055; // rainbow mode main retina mask

    private void renderPair(int f, int i, int j)
    {
        int n = ((dimSpaceCache == 4) ? 4 : 0) + 2 * j;

        reg3[j] = OptionsFisheye.offset;
        Vec.scale(sraxis[j], axis[f], -1);
        Vec.copy(sraxis[f], axis[i]);
        st.configure(j, 1);
        renderRelative.run(sraxis, false, st, model.getSaveType()==IModel.SAVE_MAZE);
        renderRelative.runObject(objRetina, rmask[n], st);

        reg3[j] = -OptionsFisheye.offset;
        Vec.copy(sraxis[j], axis[f]);
        Vec.scale(sraxis[f], axis[i], -1);
        st.configure(j, -1);
        renderRelative.run(sraxis, false, st, model.getSaveType()==IModel.SAVE_MAZE);
        renderRelative.runObject(objRetina, rmask[n + 1], st);

        // now put everything back
        reg3[j] = 0;
        Vec.copy(sraxis[j], axis[i]); // incorrect in j != i case but it's the last thing we do
    }

    //private void renderDisplay()
    //{
    //    for (int i = 0; i < display.Length; i++)
    //    {
    //        display[i].run();
    //    }
    //    displayInterface.nextFrame();
    //}

    private double width = 0.005;
    private float t2 = 1f;
    private void convert(double[] eyeVector, OptionsControl oo)
    {
        int count = 0;
        Polygon p;
        verts.Clear();
        tris.Clear();
        cols.Clear();
        for (int i = 0; i < bufRelative.getSize(); i++)
        {
            p = bufRelative.get(i);
            int v = p.vertex.Length;
            if (oo.sliceDir > 0) p.color.a *= oo.baseTransparency;
            if (v == 2)
            {
                v = 4;
                Vec.sub(reg5, p.vertex[1], p.vertex[0]);
                Vec.cross(reg6, reg5, eyeVector);
                Vec.normalize(reg6, reg6);
                Vec.scale(reg6, reg6, width);

                Vec.add(reg5, p.vertex[0], reg6);
                verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                cols.Add(p.color);

                Vec.addScaled(reg5, p.vertex[0], reg6, -1);
                verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                cols.Add(p.color);

                Vec.add(reg5, p.vertex[1], reg6);
                verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                cols.Add(p.color);

                Vec.addScaled(reg5, p.vertex[1], reg6, -1);
                verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                cols.Add(p.color);

                tris.Add(count);
                tris.Add(count + 1);
                tris.Add(count + 2);
                tris.Add(count + 2);
                tris.Add(count + 1);
                tris.Add(count + 3);
            }
            else
            {
                for (int j = 0; j < v; j++)
                {
                    reg5 = p.vertex[j];
                    verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                    cols.Add(p.color);
                }
                for (int j = 0; j < v - 2; j++)
                {
                    tris.Add(count);
                    tris.Add(count + j + 1);
                    tris.Add(count + j + 2);
                }
                if (oo.sliceDir > 0)
                {
                    int k = 0;
                    int x =  oo.sliceDir - 1;
                    int y =  oo.sliceDir      % 3;
                    int z = (oo.sliceDir + 1) % 3;
                    for (int j = 0; j < v - 1; j++)
                    {
                        if (p.vertex[j][z] * p.vertex[j + 1][z] < 0)
                        {
                            if (k == 0)
                            {
                                reg7[x] = (p.vertex[j][x] * Math.Abs(p.vertex[j + 1][z]) + p.vertex[j + 1][x] * Math.Abs(p.vertex[j][z])) / (Math.Abs(p.vertex[j][z]) + Math.Abs(p.vertex[j + 1][z]));
                                reg7[y] = (p.vertex[j][y] * Math.Abs(p.vertex[j + 1][z]) + p.vertex[j + 1][y] * Math.Abs(p.vertex[j][z])) / (Math.Abs(p.vertex[j][z]) + Math.Abs(p.vertex[j + 1][z]));
                                reg7[z] = 0;
                            }
                            else
                            {
                                reg8[x] = (p.vertex[j][x] * Math.Abs(p.vertex[j + 1][z]) + p.vertex[j + 1][x] * Math.Abs(p.vertex[j][z])) / (Math.Abs(p.vertex[j][z]) + Math.Abs(p.vertex[j + 1][z]));
                                reg8[y] = (p.vertex[j][y] * Math.Abs(p.vertex[j + 1][z]) + p.vertex[j + 1][y] * Math.Abs(p.vertex[j][z])) / (Math.Abs(p.vertex[j][z]) + Math.Abs(p.vertex[j + 1][z]));
                                reg8[z] = 0;
                            }
                            k += 1;
                        }
                    }
                    if (k == 1)
                    {
                        reg8[x] = (p.vertex[0][x] * Math.Abs(p.vertex[v - 1][z]) + p.vertex[v - 1][x] * Math.Abs(p.vertex[0][z])) / (Math.Abs(p.vertex[0][z]) + Math.Abs(p.vertex[v - 1][z]));
                        reg8[y] = (p.vertex[0][y] * Math.Abs(p.vertex[v - 1][z]) + p.vertex[v - 1][y] * Math.Abs(p.vertex[0][z])) / (Math.Abs(p.vertex[0][z]) + Math.Abs(p.vertex[v - 1][z]));
                        reg8[z] = 0;
                    }
                    if (k > 0)
                    {
                        count += v;
                        p.color.a = oo.sliceTransparency;
                        v = 4;
                        Vec.sub(reg5, reg8, reg7);
                        Vec.cross(reg6, reg5, eyeVector);
                        Vec.normalize(reg6, reg6);
                        Vec.scale(reg6, reg6, width * 2);

                        Vec.add(reg5, reg7, reg6);
                        verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                        cols.Add(p.color);

                        Vec.addScaled(reg5, reg7, reg6, -1);
                        verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                        cols.Add(p.color);

                        Vec.add(reg5, reg8, reg6);
                        verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                        cols.Add(p.color);

                        Vec.addScaled(reg5, reg8, reg6, -1);
                        verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                        cols.Add(p.color);

                        tris.Add(count);
                        tris.Add(count + 1);
                        tris.Add(count + 2);
                        tris.Add(count + 2);
                        tris.Add(count + 1);
                        tris.Add(count + 3);
                    }
                }
            }
            count += v;
        }
    }

    public void ApplyMesh() {
        if (verts.Count < mesh.vertices.Length) // triangles の参照する項が vertices から消えるとエラーを吐くため注意する
        {
            mesh.triangles = tris.ToArray();
            mesh.vertices = verts.ToArray();
        }
        else
        {
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
        }
        mesh.colors = cols.ToArray();
    }

    // --- fixed objects ---

    // private static readonly double[][] objRetina2 = new double[][] {
    //   new double[] {-1,-1}, new double[] { 1,-1},
    //      new double[] { 1,-1}, new double[] { 1, 1},
    //      new double[] { 1, 1}, new double[] {-1, 1},
    //      new double[] {-1, 1}, new double[] {-1,-1}
    //};

    private static readonly double[][] objRetinaPoly = new double[][] {
        //new double[] {-1,-1,-1}, new double[] { 1,-1,-1}, new double[] {-1, 1,-1},
        //new double[] {-1, 1,-1}, new double[] { 1,-1,-1}, new double[] { 1, 1,-1},

        new double[] { 1,-1,-1}, new double[] { 1,-1, 1}, new double[] { 1, 1,-1},
        new double[] { 1, 1,-1}, new double[] { 1,-1, 1}, new double[] { 1, 1, 1},

        new double[] { 1, 1,-1}, new double[] { 1, 1, 1}, new double[] {-1, 1,-1},
        new double[] {-1, 1,-1}, new double[] { 1, 1, 1}, new double[] {-1, 1, 1},

        new double[] {-1, 1,-1}, new double[] {-1, 1, 1}, new double[] {-1,-1,-1},
        new double[] {-1,-1,-1}, new double[] {-1, 1, 1}, new double[] {-1,-1, 1},

        new double[] {-1,-1,-1}, new double[] {-1,-1, 1}, new double[] { 1,-1,-1},
        new double[] { 1,-1,-1}, new double[] {-1,-1, 1}, new double[] { 1,-1, 1},

        //new double[] { 1,-1, 1}, new double[] {-1,-1, 1}, new double[] { 1, 1, 1},
        //new double[] { 1, 1, 1}, new double[] {-1,-1, 1}, new double[] {-1, 1, 1}
    };
    private static readonly double[][] objRetina3 = new double[][] {
      new double[] {-1,-1,-1}, new double[] { 1,-1,-1},
         new double[] { 1,-1,-1}, new double[] { 1, 1,-1},
         new double[] { 1, 1,-1}, new double[] {-1, 1,-1},
         new double[] {-1, 1,-1}, new double[] {-1,-1,-1},

         new double[] {-1,-1, 1}, new double[] { 1,-1, 1},
         new double[] { 1,-1, 1}, new double[] { 1, 1, 1},
         new double[] { 1, 1, 1}, new double[] {-1, 1, 1},
         new double[] {-1, 1, 1}, new double[] {-1,-1, 1},

         new double[] {-1,-1,-1}, new double[] {-1,-1, 1},
         new double[] { 1,-1,-1}, new double[] { 1,-1, 1},
         new double[] { 1, 1,-1}, new double[] { 1, 1, 1},
         new double[] {-1, 1,-1}, new double[] {-1, 1, 1}
   };

    private const double B = 0.04;
    // private static readonly double[][] objCross2 = new double[][] {
    //   new double[] {-B, 0}, new double[] { B, 0},
    //      new double[] { 0,-B}, new double[] { 0, B}
    //};

    private const double C = 0.1;
    //private static readonly double[][] objCross3 = new double[][] {
    //    new double[] {-C, 0, 0 }, new double[] { 0, B, 0 }, new double[] { 0, 0, 0 },
    //    new double[] {-C, 0, 0 }, new double[] { 0,-B, 0 }, new double[] { 0, 0, 0 },
    //    new double[] { C, 0, 0 }, new double[] { 0, B, 0 }, new double[] { 0, 0, 0 },
    //    new double[] { C, 0, 0 }, new double[] { 0,-B, 0 }, new double[] { 0, 0, 0 },
    //    new double[] { 0,-C, 0 }, new double[] { 0, 0, B }, new double[] { 0, 0, 0 },
    //    new double[] { 0,-C, 0 }, new double[] { 0, 0,-B }, new double[] { 0, 0, 0 },
    //    new double[] { 0, C, 0 }, new double[] { 0, 0, B }, new double[] { 0, 0, 0 },
    //    new double[] { 0, C, 0 }, new double[] { 0, 0,-B }, new double[] { 0, 0, 0 },
    //    new double[] { 0, 0,-C }, new double[] { B, 0, 0 }, new double[] { 0, 0, 0 },
    //    new double[] { 0, 0,-C }, new double[] {-B, 0, 0 }, new double[] { 0, 0, 0 },
    //    new double[] { 0, 0, C }, new double[] { B, 0, 0 }, new double[] { 0, 0, 0 },
    //    new double[] { 0, 0, C }, new double[] {-B, 0, 0 }, new double[] { 0, 0, 0 },
    //};
    private static readonly double[][] objCross3 = new double[][] {
      new double[] {-C, 0, 0}, new double[] { C, 0, 0},
         new double[] { 0,-C, 0}, new double[] { 0, C, 0},
         new double[] { 0, 0,-C}, new double[] { 0, 0, C}
   };

    private static readonly double[][] objCrossPoly = new double[][] {
      new double[] {-C, 0, -C}, new double[] { C, 0, -C},
      new double[] { C, 0, C}, new double[] { -C, 0, C},

         new double[] { 0,-C, -C}, new double[] { 0, C, -C},
         new double[] { 0, C, C}, new double[] { 0, -C, C}
   };

    private static readonly double[][] objCrossMap = new double[][] {
      new double[] {-C-3, 0, 0}, new double[] { C-3, 0, 0},
         new double[] { -3,-C, 0}, new double[] { -3, C, 0},
         new double[] { -3, 0,-C}, new double[] { -3, 0, C}
   };

    private static readonly double[][] objCrossMapPoly = new double[][] {
      new double[] {-C-3, 0, -C}, new double[] { C-3, 0, -C},
      new double[] { C-3, 0, C}, new double[] { -C-3, 0, C},

         new double[] { 0-3,-C, -C}, new double[] { 0-3, C, -C},
         new double[] { 0-3, C, C}, new double[] { 0-3, -C, C}
   };

    // private static readonly double[][] objWin2 = new double[][] {
    //   new double[] {-0.8, 0.4}, new double[] {-0.8,-0.4},
    //      new double[] {-0.8,-0.4}, new double[] {-0.6, 0  },
    //      new double[] {-0.6, 0  }, new double[] {-0.4,-0.4},
    //      new double[] {-0.4,-0.4}, new double[] {-0.4, 0.4},

    //      new double[] {-0.1, 0.4}, new double[] { 0.1, 0.4},
    //      new double[] { 0,   0.4}, new double[] { 0,  -0.4},
    //      new double[] {-0.1,-0.4}, new double[] { 0.1,-0.4},

    //      new double[] { 0.4,-0.4}, new double[] { 0.4, 0.4},
    //      new double[] { 0.4, 0.4}, new double[] { 0.8,-0.4},
    //      new double[] { 0.8,-0.4}, new double[] { 0.8, 0.4}
    //};

    // private static readonly double[][] objWin3 = new double[][] {
    //     new double[] {-0.8, 0.4,-1}, new double[] {-0.7,-0.4,-1}, new double[] {-0.9, 0.4,-1},
    //     new double[] {-0.7,-0.4,-1}, new double[] {-0.6, 0.1,-1}, new double[] {-0.6, 0.2,-1},
    //     new double[] {-0.6, 0.1,-1}, new double[] {-0.5,-0.4,-1}, new double[] {-0.6, 0.2,-1},
    //     new double[] {-0.5,-0.4,-1}, new double[] {-0.4, 0.4,-1}, new double[] {-0.3, 0.4,-1},

    //     new double[] {-0.05, 0.4,-1}, new double[] { 0.05, 0.4,-1}, new double[] { 0,   0,  -1},
    //     new double[] {-0.05,-0.4,-1}, new double[] { 0.05,-0.4,-1}, new double[] { 0,   0,  -1},

    //     new double[] { 0.4,-0.4,-1}, new double[] { 0.4, 0.4,-1}, new double[] { 0.5,-0.4,-1},
    //     new double[] { 0.4, 0.4,-1}, new double[] { 0.8,-0.4,-1}, new double[] { 0.8,-0.3,-0.999},
    //     new double[] { 0.8,-0.4,-1}, new double[] { 0.8, 0.4,-1}, new double[] { 0.7, 0.4,-1}
    //};

    private static readonly double[][] objWin3 = new double[][] {
         new double[] {-0.8, 0.4,-1}, new double[] {-0.8,-0.4,-1},
         new double[] {-0.8,-0.4,-1}, new double[] {-0.6, 0,  -1},
         new double[] {-0.6, 0,  -1}, new double[] {-0.4,-0.4,-1},
         new double[] {-0.4,-0.4,-1}, new double[] {-0.4, 0.4,-1},
 
         new double[] {-0.1, 0.4,-1}, new double[] { 0.1, 0.4,-1},
         new double[] { 0,   0.4,-1}, new double[] { 0,  -0.4,-1},
         new double[] {-0.1,-0.4,-1}, new double[] { 0.1,-0.4,-1},

         new double[] { 0.4,-0.4,-1}, new double[] { 0.4, 0.4,-1},
         new double[] { 0.4, 0.4,-1}, new double[] { 0.8,-0.4,-1},
         new double[] { 0.8,-0.4,-1}, new double[] { 0.8, 0.4,-1},


         new double[] {-1, 0.4, 0.8}, new double[] {-1,-0.4, 0.8},
         new double[] {-1,-0.4, 0.8}, new double[] {-1, 0,   0.6},
         new double[] {-1, 0,   0.6}, new double[] {-1,-0.4, 0.4},
         new double[] {-1,-0.4, 0.4}, new double[] {-1, 0.4, 0.4},

         new double[] {-1, 0.4, 0.1}, new double[] {-1, 0.4,-0.1},
         new double[] {-1, 0.4, 0  }, new double[] {-1,-0.4, 0  },
         new double[] {-1,-0.4, 0.1}, new double[] {-1,-0.4,-0.1},

         new double[] {-1,-0.4,-0.4}, new double[] {-1, 0.4,-0.4},
         new double[] {-1, 0.4,-0.4}, new double[] {-1,-0.4,-0.8},
         new double[] {-1,-0.4,-0.8}, new double[] {-1, 0.4,-0.8},


         new double[] { 0.8, 0.4, 1}, new double[] { 0.8,-0.4, 1},
         new double[] { 0.8,-0.4, 1}, new double[] { 0.6, 0,   1},
         new double[] { 0.6, 0,   1}, new double[] { 0.4,-0.4, 1},
         new double[] { 0.4,-0.4, 1}, new double[] { 0.4, 0.4, 1},
 
         new double[] { 0.1, 0.4, 1}, new double[] {-0.1, 0.4, 1},
         new double[] { 0,   0.4, 1}, new double[] { 0,  -0.4, 1},
         new double[] { 0.1,-0.4, 1}, new double[] {-0.1,-0.4, 1},

         new double[] {-0.4,-0.4, 1}, new double[] {-0.4, 0.4, 1},
         new double[] {-0.4, 0.4, 1}, new double[] {-0.8,-0.4, 1},
         new double[] {-0.8,-0.4, 1}, new double[] {-0.8, 0.4, 1},


         new double[] { 1, 0.4,-0.8}, new double[] { 1,-0.4,-0.8},
         new double[] { 1,-0.4,-0.8}, new double[] { 1, 0,  -0.6},
         new double[] { 1, 0,  -0.6}, new double[] { 1,-0.4,-0.4},
         new double[] { 1,-0.4,-0.4}, new double[] { 1, 0.4,-0.4},

         new double[] { 1, 0.4,-0.1}, new double[] { 1, 0.4, 0.1},
         new double[] { 1, 0.4, 0  }, new double[] { 1,-0.4, 0  },
         new double[] { 1,-0.4,-0.1}, new double[] { 1,-0.4, 0.1},

         new double[] { 1,-0.4, 0.4}, new double[] { 1, 0.4, 0.4},
         new double[] { 1, 0.4, 0.4}, new double[] { 1,-0.4, 0.8},
         new double[] { 1,-0.4, 0.8}, new double[] { 1, 0.4, 0.8},
   };

    private static readonly double[][] objWinSlice = new double[][] {
         new double[] {-0.8, 0.4,-0.1}, new double[] {-0.8,-0.4,-0.1}, new double[] {-0.8,-0.4,0.1}, new double[] {-0.8, 0.4,0.1},
         new double[] {-0.8,-0.4,-0.1}, new double[] {-0.6, 0,  -0.1}, new double[] {-0.6, 0,  0.1}, new double[] {-0.8,-0.4,0.1},
         new double[] {-0.6, 0,  -0.1}, new double[] {-0.4,-0.4,-0.1}, new double[] {-0.4,-0.4,0.1}, new double[] {-0.6, 0,  0.1},
         new double[] {-0.4,-0.4,-0.1}, new double[] {-0.4, 0.4,-0.1}, new double[] {-0.4, 0.4,0.1}, new double[] {-0.4,-0.4,0.1},
 
         new double[] {-0.1, 0.4,-0.1}, new double[] { 0.1, 0.4,-0.1}, new double[] { 0.1, 0.4,0.1},new double[] {-0.1, 0.4,0.1},
         new double[] { 0,   0.4,-0.1}, new double[] { 0,  -0.4,-0.1}, new double[] { 0,  -0.4,0.1},new double[] { 0,   0.4,0.1},
         new double[] {-0.1,-0.4,-0.1}, new double[] { 0.1,-0.4,-0.1}, new double[] { 0.1,-0.4,0.1},new double[] {-0.1,-0.4,0.1},
                                                                      
         new double[] { 0.4,-0.4,-0.1}, new double[] { 0.4, 0.4,-0.1}, new double[] { 0.4, 0.4,0.1},new double[] { 0.4,-0.4,0.1},
         new double[] { 0.4, 0.4,-0.1}, new double[] { 0.8,-0.4,-0.1}, new double[] { 0.8,-0.4,0.1},new double[] { 0.4, 0.4,0.1},
         new double[] { 0.8,-0.4,-0.1}, new double[] { 0.8, 0.4,-0.1}, new double[] { 0.8, 0.4,0.1},new double[] { 0.8,-0.4,0.1},
   };

    // private static readonly double[][] objDead2 = new double[][] {
    //   new double[] {-1,-1}, new double[] { 1, 1}, new double[] { 1,-1}, new double[] {-1, 1}
    //};

     private static readonly double[][] objDead3 = new double[][] {
       new double[] {-1,-1,-1}, new double[] { 1, 1, 1}, new double[] {-1,-1, 1}, new double[] { 1, 1,-1},
         new double[] {-1, 1,-1}, new double[] { 1,-1, 1}, new double[] { 1,-1,-1}, new double[] {-1, 1, 1}
    };

}

