/*
 * GeomModel.java
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//import java.awt.Color;
//import java.io.File;
//import java.io.FileWriter;
//import java.util.Collection;
//import java.util.HashMap;
//import java.util.LinkedList;
//import java.util.Vector;

/**
 * A model that lets the user move around geometric shapes.
 */

public class GeomModel : IModel, IMove, ISelectShape
{

    // --- fields ---

    protected int dim;
    public Geom.Shape[] shapes;
    private List<IScenery> scenery;
    private bool[] texture;
    protected PolygonBuffer buf;
    protected Clip.Draw[] clipUnits;
    private bool[][] inFront;
    private Geom.Separator[][] separators;
    protected bool usePolygon;
    protected bool useEdgeColor;
    protected Geom.Shape selectedShape;
    protected int hitShape, drawing;
    private int[] axisDirection; // direction of each axis, chosen when shape selected
    protected bool useSeparation;
    protected bool invertNormals;
    protected bool hideSel; // hide selection marks
    private Struct.DrawInfo drawInfo;
    private Struct.ViewInfo viewInfo;
    protected Clip.GJKTester gjk;
    //File file;
    //private static string ls = System.getProperty("line.separator");
    private Stopwatch sw = new Stopwatch();

    protected double[] origin;
    protected double[][] axis;
    protected double[] reg1;
    protected double[] reg2;
    protected Clip.Result clipResult;
    protected IDraw currentDraw;

    private List<NamedObject<Color>> availableColors;
    private List<NamedObject<Geom.Shape>> availableShapes;
    private Color addColor;
    protected Geom.Shape addShape;
    private Color paintColor;
    private int paintMode; // default 0, correct

    private List<string> topLevelInclude;
    private Dictionary<string, Color> colorNames;
    private Dictionary<string, Geom.Shape> idealNames;

    protected int faceNumber; // extra result from findShape
    protected int shapeNumber; // extra result from canMove

    private double transparency;
    // private double sceneryTransparency;
    protected double cameraDistance;

    protected bool glide;
    protected bool viewClip;

    // --- construction ---

    public GeomModel(int dim, Geom.Shape[] shapes, Struct.DrawInfo drawInfo, Struct.ViewInfo viewInfo) //throws Exception
    {

        this.dim = dim;
        this.shapes = shapes;
        scenery = new List<IScenery>();
        this.texture = new bool[10];
        // we'll receive a setTexture call later
        // buf is set shortly after construction
        clipUnits = new Clip.Draw[shapes.Length];
        for (int i = 0; i < shapes.Length; i++) clipUnits[i] = new Clip.Draw(dim);
        inFront = new bool[shapes.Length][];
        for (int i = 0; i < shapes.Length; i++) inFront[i] = new bool[shapes.Length];
        separators = new Geom.Separator[shapes.Length][];
        for (int i = 0; i < shapes.Length; i++) separators[i] = new Geom.Separator[shapes.Length];
        useEdgeColor = (drawInfo != null) ? drawInfo.useEdgeColor : true;
        selectedShape = null;
        hitShape = -1;
        axisDirection = new int[dim];
        useSeparation = true;
        invertNormals = false;
        hideSel = false;
        this.drawInfo = drawInfo;
        this.viewInfo = viewInfo;
        //gjk = new Clip.GJKTester(dim);
        availableColors = new List<NamedObject<Color>>();
        availableShapes = new List<NamedObject<Geom.Shape>>();
        //file = new File("../separateLog.txt");

        origin = new double[dim];
        axis = new double[dim][];
        for (int i = 0; i < dim; i++) axis[i] = new double[dim];
        reg1 = new double[dim];
        reg2 = new double[dim];
        clipResult = new Clip.Result();

        paintColor = Color.red * OptionsColor.fixer; // annoying to have to set up every time

        clip = new Clip.CustomBoundary[2 * (dim - 1)];
        axisClip = new Clip.CustomBoundary[2 * (dim - 1)];
        for (int i = 0; i < axisClip.Length; i++) axisClip[i] = new Clip.CustomBoundary(new double[dim], 0);
    }

    public int getDimension() { return dim; }

    public Geom.Shape getHitShape()
    {
        if (shapeNumber == -1) return null;
        return shapes[shapeNumber];
    }

    public void addScenery(IScenery o)
    {
        scenery.Add(o);
    }

    public void addAllScenery(IEnumerable<IScenery> c)
    {
        scenery.AddRange(c);
    }

    public void setSaveInfo(List<string> topLevelInclude, Dictionary<string, Color> colorNames, Dictionary<string, Geom.Shape> idealNames)
    {
        this.topLevelInclude = topLevelInclude;
        this.colorNames = colorNames;
        this.idealNames = idealNames;
    }

    public Geom.Shape[] retrieveShapes()
    {
        return shapes;
    }

    public bool[] retrieveTexture()
    {
        return texture;
    }

    public bool retrieveUseEdgeColor()
    {
        return useEdgeColor;
    }

    public List<string> retrieveTopLevelInclude()
    {
        return topLevelInclude;
    }

    public Dictionary<string, Color> retrieveColorNames()
    {
        return colorNames;
    }

    public Dictionary<string, Geom.Shape> retrieveIdealNames()
    {
        return idealNames;
    }

    private void clearAllSeparators()
    {
        // not worth checking for removed shapes
        for (int i = 0; i < shapes.Length - 1; i++)
        {
            for (int j = i + 1; j < shapes.Length; j++)
            {
                separators[i][j] = null;
            }
        }
    }

    protected void clearSeparators(int i)
    {
        // not worth checking for removed shapes
        for (int j = 0; j < i; j++) separators[j][i] = null;
        for (int j = i + 1; j < shapes.Length; j++) separators[i][j] = null;
    }

    private void calcInFront()
    {
        for (int i = 0; i < shapes.Length - 1; i++)
        {
            Geom.Shape s1 = shapes[i];
            if (s1 == null) continue;
            for (int j = i + 1; j < shapes.Length; j++)
            {
                Geom.Shape s2 = shapes[j];
                if (s2 == null) continue;

                // prefer dynamic separation even when we know a static
                // separator because dynamic ones are better at finding
                // the desired value NO_FRONT.
                int result = Clip.dynamicSeparate(s1, s2, origin, reg1, reg2);
                if (result == Geom.Separator.UNKNOWN)
                {
                    Geom.Separator sep;

                    if (isMobile(s1) || isMobile(s2))
                    {
                        //sep = separate(s1, s2, i, j);
                        sep = Clip.staticSeparate(s1,s2,/* any = */ false);
                        // don't remember the separator
                    }
                    else
                    {
                        sep = separators[i][j];
                        if (sep == null)
                        {
                            //sep = separate(s1, s2, i, j);
                            sep = Clip.staticSeparate(s1,s2,/* any = */ false);
                            separators[i][j] = sep;
                        }
                    }

                    result = sep.apply(origin);
                }

                inFront[i][j] = (result == Geom.Separator.S1_FRONT);
                inFront[j][i] = (result == Geom.Separator.S2_FRONT);
            }
        }
        // note that in general inFront is not transitive.  with long blocks
        // you can easily construct cycles where one is in front of the next
        // all the way around.
    }

    private Geom.Separator separate(Geom.Shape s1, Geom.Shape s2, int i1, int i2)
    {
        sw.Restart();
        Geom.Separator sep = gjk.separate(s1, s2);
        sw.Stop();
        int sec = (int)((double)sw.ElapsedTicks / (double)Stopwatch.Frequency * 1000000);
        //try
        //{
        //    FileWriter w = new FileWriter(file, true);
        //    w.write("shape " + i1 + " (" + s1.vertex.Length + " vertices)" + ls);
        //    w.write("shape " + i2 + " (" + s2.vertex.Length + " vertices)" + ls);
        //    w.write(time + ls + ls);
        //    w.close();
        //}
        //catch (Exception e) { System.out.println(e); }
        return sep;
    }

    private bool isMobile(Geom.Shape shape)
    {
        return (shape.systemMove || shape.isNoUserMove || shape == selectedShape);
    }

    protected int indexOf(Geom.Shape shape)
    {
        // not worth checking for removed shapes
        for (int i = 0; i < shapes.Length; i++)
        {
            if (shapes[i] == shape) return i;
        }
        throw new /*Runtime*/Exception("Shape not in table.");
    }

    private void mobilize(Geom.Shape shape)
    {
        // clear separators, we'll be computing every cycle for now
        clearSeparators(indexOf(shape));
    }

    private void demobilize(Geom.Shape shape)
    {
        // no action needed, separators can start to fill in again
    }

    // --- implementation of IKeysNew ---

    public virtual void adjustSpeed(int dv) { }
    public virtual void toggleTrack() { }

    public virtual void toggleEdgeColor()
    {
        useEdgeColor = !useEdgeColor;
    }

    public IMove click(double[] origin, double[] viewAxis, double[][] axisArray)
    {

        // always deselect if possible, otherwise it's too confusing

        if (selectedShape != null)
        {
            demobilize(selectedShape);
            selectedShape = null;
            return null;
        }

        // are we pointing at a shape?

        Geom.Shape shape = findShape(origin, viewAxis);
        if (shape == null)
        {
            clickNoShape(origin, viewAxis);
            return null;
        }

        // can we select it?

        if (shape.isNoUserMove)
        {
            clickNoUserMove(shape, origin, viewAxis);
            return null;
        }

        // yes, all good, select it

        // block for parallelism
        {
            mobilize(shape);
            Align.computeDirs(axisDirection, axisArray);
            setAxis();
            selectedShape = shape;
            return this;
        }
    }

    protected void setAxis()
    {
        for (int i = 0; i < dim; i++)
        {
            Vec.unitVector(axis[i], Dir.getAxis(axisDirection[i]));
            Vec.scale(axis[i], axis[i], Dir.getSign(axisDirection[i]));
        }
    }

    protected virtual void clickNoShape(double[] origin, double[] viewAxis)
    {
    }

    protected virtual void clickNoUserMove(Geom.Shape shape, double[] origin, double[] viewAxis)
    {
    }

    protected Geom.Shape findShape(double[] origin, double[] viewAxis)
    {

        Vec.addScaled(reg2, origin, viewAxis, 10000); // infinity
        double dMin = 1;
        Geom.Shape shapeMin = null;

        for (int i = 0; i < shapes.Length; i++)
        {
            Geom.Shape shape = shapes[i];
            if (shape == null) continue;
            if (Clip.closestApproach(shape.shapecenter, origin, viewAxis, reg1) <= shape.radius * shape.radius)
            { // could be a hit
                Clip.clip(origin, reg2, shape, clipResult);
                if (!invertNormals)
                {
                    if ((clipResult.clip & Clip.KEEP_A) != 0)
                    { // is a hit
                        if (clipResult.a < dMin)
                        {
                            dMin = clipResult.a;
                            shapeMin = shape;
                            faceNumber = clipResult.ia; // for paint
                        }
                    }
                }
                else
                {
                    if ((clipResult.clip & Clip.KEEP_B) != 0)
                    {
                        if (clipResult.b < dMin)
                        {
                            dMin = clipResult.b;
                            shapeMin = shape;
                            faceNumber = clipResult.ib;
                        }
                    }
                }
            }
        }

        return shapeMin;
    }

    /**
     * @param todo Can be null if you want everything in one list.
     */
    private void listShapes(List<Geom.Shape> todo, List<Geom.Shape> done)
    {
        for (int i = 0; i < shapes.Length; i++)
        {
            if (shapes[i] == null || shapes[i].systemMove) continue; // ignore trains

            if (todo == null || shapes[i].isNoUserMove)
            {
                done.Add(shapes[i]);
            }
            else
            {
                todo.Add(shapes[i]);
            }
        }
    }

    public virtual void scramble(bool alignMode, double[] origin)
    {

        if (selectedShape != null) return;
        //
        // it's perfectly fine to scramble while selected, I just don't like it.
        // scramble while motion in progress is already prevented in Controller.
        // actually that's not true now that we're using align mode.
        // we want to scramble according to the align mode of the user, not the
        // align mode of the selected shape.

        List<Geom.Shape> todo = new List<Geom.Shape>();
        List<Geom.Shape> done = new List<Geom.Shape>();
        listShapes(todo, done);
        Scramble.scramble(todo, done, alignMode, origin, gjk);

        clearAllSeparators();
    }

    public virtual void toggleSeparation()
    {
        useSeparation = !useSeparation;
    }

    protected int countSlots()
    {
        int count = 0;
        for (int i = 0; i < shapes.Length; i++)
        {
            if (shapes[i] == null) count++;
        }
        return count;
    }

    protected int findSlot(int i)
    {
        for (; i < shapes.Length; i++)
        {
            if (shapes[i] == null) return i;
        }
        return -1; // shouldn't happen
    }

    protected void reallocate(int len)
    {

        // if one insert is happening, there will probably
        // be more eventually, so allocate in blocks of 10
        int mod = len % 10;
        if (mod != 0) len += 10 - mod;

        Geom.Shape[] shapesNew = new Geom.Shape[len];
        Array.Copy(shapes, 0, shapesNew, 0, shapes.Length);
        // the rest start null

        Clip.Draw[] clipUnitsNew = new Clip.Draw[len];
        Array.Copy(clipUnits, 0, clipUnitsNew, 0, shapes.Length);
        for (int i = shapes.Length; i < len; i++) clipUnitsNew[i] = new Clip.Draw(dim);

        bool[][] inFrontNew = new bool[len][];
        for (int i = 0; i < len; i++) inFrontNew[i] = new bool[len];
        // just a temporary register, no need to copy anything

        Geom.Separator[][] separatorsNew = new Geom.Separator[len][];
        for (int i = 0; i < len; i++) separatorsNew[i] = new Geom.Separator[len];
        for (int i = 0; i < shapes.Length - 1; i++)
        {
            for (int j = i + 1; j < shapes.Length; j++)
            {
                separatorsNew[i][j] = separators[i][j];
            }
        }
        // the rest start null

        // no real need to wait until end, but it's good form
        shapes = shapesNew;
        clipUnits = clipUnitsNew;
        inFront = inFrontNew;
        separators = separatorsNew;
    }

    private static System.Random random = new System.Random();
    private static Color pickFrom(List<NamedObject<Color>> available)
    {
        int count = available.Count;
        int i = random.Next(count);
        return available[i].obj;
    }

    private static Geom.Shape pickFrom(List<NamedObject<Geom.Shape>> available)
    {
        int count = available.Count;
        int i = random.Next(count);
        return available[i].obj;
    }

    private Geom.Shape createShape()
    {

        Geom.Shape shape = addShape;
        if (shape == null) shape = pickFrom(availableShapes);

        shape = shape.copy(); // essentially pulling out of dictionary

        Color color = addColor;
        if (color != null)
        {
            if (color == ISelectShape.RANDOM_COLOR) color = pickFrom(availableColors);
            shape.setShapeColor(color);
        }

        return shape;
    }

    public virtual bool canAddShapes()
    {
        return (selectedShape == null && availableShapes.Count > 0 && availableColors.Count > 0);
        // no real reason for checking selectedShape,
        // except I want to see the align mode of the user, not the shape.
        // have to check available colors in case user picks random color.
    }

    public virtual void addShapes(int quantity, bool alignMode, double[] origin, double[] viewAxis)
    {
        // caller must check canAddShapes

        // viewAxis was for a feature I was thinking about where you
        // point at where you want the shape to go, but for now it's
        // not used.

        // reallocate arrays if necessary

        int alloc = quantity - countSlots();
        if (alloc > 0) reallocate(shapes.Length + alloc);

        // add shapes

        int index = 0;

        List<Geom.Shape> todo = new List<Geom.Shape>();
        List<Geom.Shape> done = new List<Geom.Shape>();
        listShapes(null, done);

        for (int i = 0; i < quantity; i++)
        {
            Geom.Shape shape = createShape();
            index = findSlot(index);
            shapes[index++] = shape; // increment to avoid re-scanning the used slot
            todo.Add(shape);
        }

        Scramble.scramble(todo, done, alignMode, origin, gjk);
    }

    public virtual void removeShape(double[] origin, double[] viewAxis)
    {

        // find the target shape (very similar to click)

        if (selectedShape != null) return; // remove selected would be extra hassle

        Geom.Shape shape = findShape(origin, viewAxis);
        if (shape == null) return;

        if (shape.isNoUserMove) return; // could allow deleting platforms but not trains

        // now delete it

        // do this by nulling everything out, not resizing arrays.
        // same idea as LineBuffer, which grows and never shrinks.
        // we do want to release some references though.

        int i = indexOf(shape);
        shapes[i] = null;
        clipUnits[i].setBoundaries(null);
        // inFront, no change
        clearSeparators(i);
    }

    public virtual void toggleNormals()
    {
        invertNormals = !invertNormals;
    }

    public virtual void toggleHideSel()
    {
        hideSel = !hideSel;
    }

    public virtual bool canPaint()
    {
        return (availableColors.Count > 0); // in case the color is random.
                                            // allow painting when a shape is selected, that's a natural action.
    }

    public virtual void paint(double[] origin, double[] viewAxis)
    {
        // caller must check canPaint

        bool paintShape = ((paintMode & 1) == 1);
        paintMode >>= 1;
        // even if color is "no effect", it still counts as a paint operation

        if (paintColor == ISelectShape.NO_EFFECT_COLOR) return; // no effect

        Geom.Shape shape = findShape(origin, viewAxis);
        if (shape == null) return; // no shape

        Color useColor;
        if (paintColor == ISelectShape.RANDOM_COLOR) useColor = pickFrom(availableColors);
        else if (paintColor == ISelectShape.REMOVE_COLOR) useColor = Color.clear;
        else useColor = paintColor;

        if (paintShape)
        {
            shape.setShapeColor(useColor);
        }
        else
        {
            shape.setFaceColor(faceNumber, useColor,/* xor = */ true);
        }
    }

    public virtual void jump()
    {
    }

    // --- implementation of ISelectShape ---

    // the first two aren't in the interface,
    // but they're closely related

    public void setAvailableColors(List<NamedObject<Color>> availableColors)
    {
        this.availableColors = new List<NamedObject<Color>>(availableColors);
    }

    public void setAvailableShapes(List<NamedObject<Geom.Shape>> availableShapes)
    {
        this.availableShapes = new List<NamedObject<Geom.Shape>>(availableShapes);
    }

    public List<NamedObject<Color>> getAvailableColors()
    {
        return availableColors;
    }

    public List<NamedObject<Geom.Shape>> getAvailableShapes()
    {
        return availableShapes;
    }

    public Color getSelectedColor()
    {
        return addColor;
    }

    public Geom.Shape getSelectedShape()
    {
        return addShape;
    }

    public void setSelectedColor(Color color)
    {
        addColor = color;
    }

    public void setSelectedShape(Geom.Shape shape)
    {
        addShape = shape;
    }

    // --- implementation of ISelectPaint (in ISelectShape) ---

    public Color getPaintColor()
    {
        return paintColor;
    }

    public void setPaintColor(Color color)
    {
        paintColor = color;
    }

    public int getPaintMode()
    {
        return paintMode;
    }

    public void setPaintMode(int mode)
    {
        paintMode = mode;
    }

    // --- implementation of IMove ---

    // this section is all for block motion, not player motion

    public bool canMove(int a, double d)
    {
        return true;
        // checking for collisions along whole path is too much for now
    }

    public void move(/*int a, double d*/double[] d)
    {
        Vec.fromAxisCoordinates(reg1, d, axis);
        selectedShape.translateFrame(reg1);
    }

    public void rotateAngle(/*int a1, int a2, double theta*/double[] from, double[] to)
    {
        // third-person view is just too weird.  the natural mapping is reversed.
        // and, actually the same intuition you have about the forward direction
        // applies in 4D to the outward direction, so we have to reverse z then too.
        // so, everything except xy rotations!
        //if (a1 >= 2 || a2 >= 2) theta = -theta;
        //selectedShape.rotateFrame(axisDirection[a1], axisDirection[a2], theta, null);

        if (from[dim-1]==0) Vec.swap(from, to, reg1);
        Vec.fromAxisCoordinates(reg1, from, axis);
        Vec.fromAxisCoordinates(reg2, to, axis);
        Vec.normalize(reg1, reg1);
        Vec.normalize(reg2, reg2);
        selectedShape.rotateFrame(reg2, reg1, null, from, to);
    }

    public Align align()
    {
        return new Align(Align.ROTATE_THEN_TRANSLATE, selectedShape.aligncenter, selectedShape.axis);
    }

    public bool isAligned()
    {
        return Align.isAligned(selectedShape.aligncenter, selectedShape.axis);
    }

    public bool update(double[] saveOrigin, double[][] saveAxis, double[] viewOrigin)
    {
        selectedShape.place();
        return useSeparation ? isSeparated(selectedShape, viewOrigin) : true;
    }

    public void save(double[] saveOrigin, double[][] saveAxis)
    {
        Vec.copy(saveOrigin, selectedShape.aligncenter);
        Vec.copyMatrix(saveAxis, selectedShape.axis);
    }

    public void restore(double[] saveOrigin, double[][] saveAxis)
    {
        Vec.copy(selectedShape.aligncenter, saveOrigin);
        Vec.copyMatrix(selectedShape.axis, saveAxis);
        selectedShape.place(); // since we updated and failed
    }

    public bool isSeparated(Geom.Shape shape, double[] viewOrigin)
    {

        // not perfect collision detection yet, but it's not bad
        // what can you collide with?

        // 1. the ground

        if (Clip.vmin(shape,/* axis = */ 1) < -Clip.overlap) return false;

        // 2. other blocks

        for (int i = 0; i < shapes.Length; i++)
        {
            if (shapes[i] == null || shapes[i] == shape || shapes[i].systemMove) continue;
            if (!Clip.isSeparated(shape, shapes[i], gjk))
            {
                hitShape = i;
                return false;
            }
        }
        hitShape = -1;
        // note, we don't handle the case where we're already collided.
        // that's part of why there's a command to turn off separation.

        // 3. user viewpoint (fast but rare, check last)

        if (Clip.clip(viewOrigin, viewOrigin, shape, clipResult) != Clip.KEEP_LINE) return false;

        // looks good!

        return true;
    }

    // --- implementation of IModel ---

    public override void initPlayer(double[] origin, double[][] axis)
    {
        if (viewInfo != null)
        {

            Vec.copy(origin, viewInfo.origin);
            Vec.copyMatrix(axis, viewInfo.axis);
            // note, this works even if the
            // axis vectors are 4D enumeration constants

        }
        else
        {

            // the prototype model is the tesseract [0,1]^4,
            // so start lines up with that.
            // we need half-integer coordinates in case we're in align mode.

            for (int i = 0; i < origin.Length; i++) origin[i] = 0.5;
            origin[origin.Length - 1] = -2.5;
            // this is the right thing in both 3D and 4D

            Vec.unitMatrix(axis);
        }
    }

    public virtual bool getAlignMode(bool defaultAlignMode)
    { // not in IModel, but like initPlayer
        if (viewInfo != null)
        {

            return defaultAlignMode && Align.isAligned(viewInfo.origin, viewInfo.axis);
            //
            // in the maze game load process, we load the align mode along with
            // the origin and axes, but that just doesn't make sense to me here.
            // I'm not even sure it makes sense there!

        }
        else
        {

            return defaultAlignMode;
        }
    }

    public override void testOrigin(double[] origin, int[] reg1, int[] reg2) //throws ValidationException
    {
    }

    public override void setColorMode(int colorMode)
    {
    }

    public override void setDepth(int depth)
    {
    }

    public bool[] getDesiredTexture()
    { // also not in IModel
        return (drawInfo != null) ? drawInfo.texture : null;
    }

    public override void setTexture(bool[] texture)
    {
        for (int i = 0; i < 10; i++)
        {
            this.texture[i] = texture[i];
        }
        // I forget why we copy rather than share, but that's what RenderAbsolute does
    }

    public override void setTransparency(double transparency)
    {
        this.transparency = transparency;
    }

    public override void setOptions(OptionsColor oc, int seed, int depth, bool[] texture, OptionsDisplay od)
    {
        setTexture(texture);
        setTransparency(od.transparency);
        usePolygon = od.usePolygon;
        useEdgeColor = od.useEdgeColor;
        hideSel = od.hidesel;
        invertNormals = od.invertNormals;
        useSeparation = od.separate;
        cameraDistance = od.cameraDistance;
        glide = od.glide;
    }

    private double retina;
    private Clip.CustomBoundary[] clip, axisClip;
    private double orthoRetina = 2;
    public override void setRetina(double retina) // almost same as RenderRelative.setRetina
    {
        this.retina = retina;

        int next = 0;
        double[] reg;
        if (retina > 0)
        for (int a = 0; a < dim - 1; a++)
        {

            reg = new double[dim];
            reg[a] = 1;
            reg[dim - 1] = retina;
            Vec.normalize(reg, reg); // for convert
            clip[next] = new Clip.CustomBoundary(reg, 0);
            next++;

            reg = new double[dim];
            reg[a] = -1;
            reg[dim - 1] = retina;
            clip[next] = new Clip.CustomBoundary(reg, 0);
            next++;

            // no need to zero other components,
            // they never become nonzero
        }
        else 
        for (int a = 0; a < dim - 1; a++)
        {

            reg = new double[dim];
            reg[a] = 1;
            clip[next] = new Clip.CustomBoundary(reg, -orthoRetina);
            next++;

            reg = new double[dim];
            reg[a] = -1;
            clip[next] = new Clip.CustomBoundary(reg, -orthoRetina);
            next++;

            // no need to zero other components,
            // they never become nonzero
        }
    }

    public override bool isAnimated()
    {
        return false;
    }

    public override int getSaveType()
    {
        return IModel.SAVE_GEOM;
    }

    const double epsilon = 0.00001;
    public override bool canMove(double[] p1, double[] p2, int[] reg1, double[] reg2, bool detectHits)
    {
        bool through = true;

        if (!useSeparation) return true;

        if (p2[1] < 0 && p2[1] < p1[1])
        {
            shapeNumber = -1;
            if (!glide || detectHits) return false; // solid floor
            else { through = false; p2[1] = epsilon; }
        }
        // I once got to negative y by aligning while near the floor
        List<double[]> normals = new List<double[]>();

        for (int i = 0; i < shapes.Length; i++)
        {
            Geom.Shape shape = shapes[i];
            if (shape == null) continue;

            if (shape.systemMove) continue;
            // what should happen if a train hits you?  there are three options.
            // 1. do some physics
            // 2. ignore collisions with train objects, which is what I'm doing
            // 3. treat trains just like anything else.  this is OK now that
            // it's possible to get out from inside shapes, but I don't like the
            // jerky motion you get if you try to follow a slow train.  so, nope

            // prefilter by checking distance to shape against radius
            // if (Clip.outsideRadius(p1, p2, shape)) continue;

            if ((Clip.clip(p1, p2, shape, clipResult) & (invertNormals ? Clip.KEEP_B : Clip.KEEP_A)) != 0)
            {
                shapeNumber = i;
                if (!glide || detectHits) return false;
                else
                {
                    through = false;
                    // projection on hitting cell
                    Vec.sub(reg2,p1,p2);
                    double[] normal = new double[dim];
                    Vec.copy(normal, shape.cell[invertNormals ? clipResult.ib : clipResult.ia].normal);
                    foreach (double[] n in normals)
                    {
                        Vec.addScaled(normal, normal, n, -Vec.dot(normal, n) / Vec.norm2(n));
                    }
                    double d = Math.Max((invertNormals ? clipResult.b : clipResult.a) - epsilon / Vec.norm(reg2), 0);
                    Vec.scale(reg2, normal, Vec.dot(reg2, normal) / Vec.norm2(normal));
                    Vec.addScaled(p2, p2, reg2, 1 - d);
                    Vec.addScaled(p1, p1, reg2,   - d);
                    normals.Add(normal);
                    i = -1;
                }
            }
            // it's possible to get inside a block by aligning
            // or by placing blocks carelessly, so only exclude motion that enters
            // a block from outside.  this is all with respect to a single shape,
            // so you can't navigate around inside a chain of blocks if you get in.
        }

        return through;
    }

    public override bool atFinish(double[] origin, int[] reg1, int[] reg2)
    {
        return false;
    }

    public override bool dead() { return false; }

    public override void setBuffer(PolygonBuffer buf)
    {
        this.buf = buf;
    }

    public override void animate(double delta)
    {
    }

    public override void render(double[] origin, double[][] axis, bool viewClip)
    {
        this.viewClip = viewClip;
        renderer(origin, axis);
    }

    protected void renderer(double[] origin, double[][] axis)
    {
        buf.clear();
        Vec.copy(this.origin, origin);
        viewBoundaryConvert(axis);

        double[] dist = new double[shapes.Length];
        for (int i = 0; i < shapes.Length; i++) if (shapes[i] != null) dist[i] = Vec.dist2(shapes[i].aligncenter, origin);
        int[] s= dist.Select((x, i) => new KeyValuePair<double,int>(x,i))
                         .OrderBy(x => -x.Key)
                         .Select(x => x.Value)
                         .ToArray();

        // calcViewBoundaries is expensive, but we always need the boundaries
        // to draw the scenery correctly
        currentDraw = buf;
        for (int i = 0; i < shapes.Length; i++)
        {
            if (shapes[s[i]] == null) continue;
            calcVisShape(shapes[s[i]]);
            clipUnits[s[i]].setBoundaries(Clip.calcViewBoundaries(origin, shapes[s[i]]));
            currentDraw = clipUnits[s[i]].chain(currentDraw); // set up for floor drawing
        }

        // currentDraw includes all objects, scenery must be distant
        for (int i = 0; i < scenery.Count; i++)
        {
            double[][] texture;
            Color[] textureColor;
            scenery[i].draw(out texture, out textureColor, origin);
            for (int j = 0; j < textureColor.Length; j++)
            {
                // textureColor[j].a *= (float)sceneryTransparency;
                drawLine(texture[j*2], texture[j*2+1], textureColor[j]);
            }
        }

        calcInFront();

        for (int i = 0; i < shapes.Length; i++)
        {
            drawing = i;
            if (shapes[i] == null) continue;
            currentDraw = buf;
            for (int h = 0; h < shapes.Length; h++)
            {
                if (shapes[s[h]] == null) continue;
                if (inFront[s[h]][i]) currentDraw = clipUnits[s[h]].chain(currentDraw);
            }
            int bn = buf.getSize();
            drawShape(shapes[i]);
            //if (bn == buf.getSize()) for (int j = 0; j < shapes.Length; j++) inFront[i][j] = inFront[j][i] = false;
        }
    }

    private void calcVisShape(Geom.Shape shape)
    {
        for (int i = 0; i < shape.face.Length; i++) shape.face[i].visible = false;
        for (int i = 0; i < shape.cell.Length; i++)
            if (calcVisCell(shape.cell[i]))
                for (int j = 0; j < shape.cell[i].ifa.Length; j++)
                    shape.face[shape.cell[i].ifa[j]].visible = true;
    }

    private bool calcVisCell(Geom.Cell cell)
    {
        if (cell.normal != null)
        {

            // late to be adding an epsilon here, but without this you can sometimes
            // see a flat face between two other faces that ought to be in contact.
            // the example is geom3-project4b (delete the front face and slide right).
            const double epsilon = -1e-9;

            Vec.sub(reg1, cell.center, origin); // vector from origin to face center
            cell.visible = (Vec.dot(reg1, cell.normal) < epsilon) ^ invertNormals;
        }
        else
        {
            cell.visible = true; // glass
        }
        return cell.visible;
    }

    private void drawShape(Geom.Shape shape)
    {
        for (int i = 0; i < shape.cell.Length; i++) drawCell(shape, shape.cell[i]);
    }

    private void drawCell(Geom.Shape shape, Geom.Cell cell)
    {

        if (!cell.visible) return;

        if (texture[0])
        {
            if (useEdgeColor) drawEdgeColor(shape, cell, 0.999999);
            else drawTexture(shape, cell, getColor(Color.white * OptionsColor.fixer), 0.999999);
        }

        bool selected = (shape == selectedShape) && !hideSel;
        Color cellColor = Geom.getColor(cell.color);

        if (cell.customTexture != null)
        {
            double[][] texture;
            Color[] textureColor;
            cell.customTexture.draw(out texture, out textureColor, cell, origin);
            for (int i = 0; i < textureColor.Length; i++) drawLine(texture[i*2], texture[i*2+1], textureColor[i]);
        }
        else
        {
            for (int i = 1; i < 10; i++)
            {
                if (i == 5 && selected) continue;
                if (texture[i]) drawTexture(shape, cell, getColor(cellColor), 0.1 * i);
            }
        }

        if (selected)
        {
            Color color = cellColor.Equals(COLOR_SELECTED) ? COLOR_SELECTED_ALTERNATE : COLOR_SELECTED;
            drawTexture(shape, cell, color, 0.5);
            // slightly different behavior than in RenderAbsolute:
            // change to alternate color even if texture 5 not on.
        }
    }

    private Color getColor(Color color)
    {
        return hitShape == drawing ? color.Equals(COLOR_SELECTED) ? COLOR_SELECTED_ALTERNATE : COLOR_SELECTED : color;
    }

    private static Color COLOR_SELECTED = Color.yellow * OptionsColor.fixer;
    private static Color COLOR_SELECTED_ALTERNATE = Color.red * OptionsColor.fixer;

    private void drawEdgeColor(Geom.Shape shape, Geom.Cell cell, double scale)
    {
        Polygon poly = new Polygon();
        if (usePolygon) for (int i = 0; i < cell.ifa.Length; i++)
        {
            Geom.Face face = shape.face[cell.ifa[i]];
            poly.vertex = new double[face.iv.Length][];
            for (int j = 0; j < face.iv.Length; j++)
            {
                poly.vertex[j] = new double[getDimension()];
                Vec.mid(poly.vertex[j], cell.center, shape.vertex[face.iv[j]], scale);
            }
            poly.color = getColor(Geom.getColor(/*edge.color, */cell.color));
            poly.color.a = (float)transparency;
            drawPolygon(poly);
        }
        for (int i = 0; i < cell.ie.Length; i++)
        {
            Geom.Edge edge = shape.edge[cell.ie[i]];
            Vec.mid(reg1, cell.center, shape.vertex[edge.iv1], scale);
            Vec.mid(reg2, cell.center, shape.vertex[edge.iv2], scale);
            drawLine(reg1, reg2, Geom.getColor(/*edge.color, */cell.color));
        }
    }

    private void drawTexture(Geom.Shape shape, Geom.Cell cell, Color color, double scale)
    {
        Polygon poly = new Polygon();
        if (usePolygon) for (int i = 0; i < cell.ifa.Length; i++)
        {
            Geom.Face face = shape.face[cell.ifa[i]];
            poly.vertex = new double[face.iv.Length][];
            for (int j = 0; j < face.iv.Length; j++)
            {
                poly.vertex[j] = new double[getDimension()];
                Vec.mid(poly.vertex[j], cell.center, shape.vertex[face.iv[j]], scale);
            }
            poly.color = color;
            poly.color.a = (float)transparency;
            drawPolygon(poly);
        }
        for (int i = 0; i < cell.ie.Length; i++)
        {
            Geom.Edge edge = shape.edge[cell.ie[i]];
            Vec.mid(reg1, cell.center, shape.vertex[edge.iv1], scale);
            Vec.mid(reg2, cell.center, shape.vertex[edge.iv2], scale);
            drawLine(reg1, reg2, color);
        }
    }

    private void drawPolygon(Polygon polygon)
    {
        for (int i = 0; i < clip.Length; i++)
        {
            if (Clip.clip(polygon, axisClip[i])) return;
        }
        currentDraw.drawPolygon(polygon, origin);
    }

    private void drawLine(double[] p1, double[] p2, Color color)
    {
        if (viewClip) for (int i = 0; i < clip.Length; i++)
        {
            if (Vec.clip(p1, p2, axisClip[i].n, axisClip[i].getThreshold(), 1)) return;
        }
        currentDraw.drawLine(p1, p2, color, origin);
    }

    protected void drawLine(double[] p1, double[] p2, Color color, double[] origin)
    {
        for (int i = 0; i < clip.Length; i++)
        {
            if (Vec.clip(p1, p2, axisClip[i].n, axisClip[i].getThreshold(), 1)) return;
        }
        currentDraw.drawLine(p1, p2, color, origin);
    }

    private void viewBoundaryConvert(double[][] axis) {
        for (int i = 0; i < clip.Length; i++) {
            Vec.fromAxisCoordinates(axisClip[i].n, clip[i].n, axis);
            axisClip[i].t = Vec.dot(origin, axisClip[i].n);
        }
    }
}

