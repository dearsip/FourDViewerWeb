/*
 * MapModel.java
 */

using System;
using System.Collections.Generic;

/**
 * A model that lets the user move through a maze.
 */

public class MapModel : IModel
{

    // --- fields ---

    private Map map;
    private DynamicArray.OfInt cubeNum;
    private int[] limits;
    private Colorizer colorizer;
    private int colorMode;
    private RenderAbsolute renderAbsolute;
    private GeomModel geomModel;
    private PolygonBuffer bufAbsolute;
    public  PolygonBuffer bufRelative;
    private RenderRelative geomRelative;

    private readonly double retina = 0.8;
    private readonly double distance = 7;
    private int count;
    private double[] reg;
    private int[] reg7;
    public bool showMap;
    private List<double[][]> normals = new List<double[][]>();
    private bool glide, glass, mark;
    private float mapDistance;

    // --- construction ---

    public MapModel(int dimSpace, OptionsMap om, OptionsColor oc, OptionsSeed oe, OptionsView ov, IStore store)
    {
        om.dimMap = Math.Min(dimSpace, om.dimMap);
        try { map = new Map(dimSpace, om, store); }
        catch (Exception) { UnityEngine.Debug.LogError("failed to load raw map data."); map = new Map(dimSpace, om, oe.mapSeed); }
        limits = DynamicArray.makeLimits(om.size, dimSpace);
        cubeNum = new DynamicArray.OfInt(dimSpace, limits);
        colorizer = new Colorizer(dimSpace, om.dimMap, om.size, oc, oe.colorSeed);
        renderAbsolute = new RenderAbsolute(dimSpace, map, colorizer, ov, this);
        bufAbsolute = new PolygonBuffer(dimSpace);
        bufRelative = new PolygonBuffer(dimSpace - 1);

        reg = new double[dimSpace];
        reg7 = new int[dimSpace];
        normals.Clear();

        geomModel = new GeomModel(dimSpace, new Geom.Shape[om.size[0] * om.size[1] * om.size[2] * om.size[3]], null, null);
        geomModel.setBuffer(bufAbsolute);
        geomRelative = new RenderRelative(bufAbsolute, bufRelative, dimSpace, retina);
        cube = dimSpace == 3 ? cube3 : cube4;
        count = 0;
    }

    // --- implementation of IModel ---

    public override void initPlayer(double[] origin, double[][] axis)
    {

        Grid.fromCell(origin, map.getStart());

        // cycle the axes so that we're correctly oriented when dimMap < dimSpace
        // axis[dimSpace-1] points in the forward direction, which should be unitVector(0) ... etc.
        // everything else is random, so it's OK for the axes to be deterministic
        //
        for (int a = 0; a < axis.Length; a++) Vec.unitVector(axis[a], (a + 1) % axis.Length);
    }

    public override void testOrigin(double[] origin, int[] reg1, int[] reg2)
    {

        // check that origin is in bounds and open
        // this is clumsy, because normally the walls keep us in bounds.

        // we might be on multiple boundaries, in which case toCell can't return all cells,
        // but that doesn't matter here.
        // the cells are all adjacent, so if any one is strictly in bounds,
        // the rest are enough in bounds not to cause an array fault in isOpen.

        Grid.toCell(reg1, reg2, origin); // ignore result
        if (!map.inBounds(reg1)) throw new Exception();

        if (!Grid.isOpen(origin, map, reg1)) throw new Exception();
    }

    public override void setTexture(bool[] texture)
    {
        renderAbsolute.setTexture(texture);
    }

    public override void setTransparency(float transparency)
    {
        renderAbsolute.setTransparency(transparency);
    }

    public override void setOptions(OptionsColor oc, int seed, OptionsView ov, OptionsDisplay od)
    {
        colorizer.setOptions(oc, seed);
        if (colorMode != oc.colorMode || mark != ov.mark) SetMapColor();
        colorMode = oc.colorMode;
        renderAbsolute.setOptions(oc, seed, ov, od);
        geomModel.setOptions(oc, seed, ov, od);
        showMap = od.map;
        glide = od.glide;
        glass = od.glass;
        mark = ov.mark;
        ToggleGlass();
        mapDistance = od.mapDistance;
    }

    public override void setRetina(double retina) {
        geomModel.setRetina(retina);
    }

    public override bool isAnimated()
    {
        return false;
    }

    public override int getSaveType()
    {
        return IModel.SAVE_MAZE;
    }

    public override bool canMove(double[] p1, double[] p2, int[] reg1, double[] reg2, bool detectHits)
    {
        return Grid.isOpenMove(p1, p2, map, reg1, reg2, detectHits, glide);
    }

    public override bool atFinish(double[] origin, int[] reg1, int[] reg2)
    {
        int dir = Grid.toCell(reg1, reg2, origin);
        return (Grid.equals(reg1, map.getFinish())
                 || (dir != Dir.DIR_NONE && Grid.equals(reg2, map.getFinish())));
    }

    public override bool dead() { return false; }

    public override void setBuffer(PolygonBuffer buf)
    {
        renderAbsolute.setBuffer(buf);
    }

    public override void animate(double delta)
    {
    }

    private Polygon p;
    public override void render(double[] origin, double[][] axis, bool viewClip)
    {
        renderAbsolute.run(origin, axis);
        if (showMap) {
            Vec.addScaled(reg, origin, axis[axis.Length-1], -distance);
            geomModel.render(reg, axis, false);
            geomRelative.run(axis, false);
            for (int i = 0; i < bufRelative.getSize(); i++)
            {
                p = bufRelative.get(i);
                foreach (double[] v in p.vertex)
                {
                    Vec.scale(v, v, 2);
                    v[0] -= mapDistance;
                }
            }
        }
    }

    private Geom.Shape cube;
    private static readonly Geom.Shape cube3 = GeomUtil.rect(new double[][] { new double[] { 0, 1 },
                                                             new double[] { 0, 1 },
                                                             new double[] { 0, 1 } });
    private static readonly Geom.Shape cube4 = GeomUtil.rect(new double[][] { new double[] { 0, 1 },
                                                             new double[] { 0, 1 },
                                                             new double[] { 0, 1 },
                                                             new double[] { 0, 1 } });
    private static readonly Geom.Texture gtex = new Geom.Texture(new Geom.Edge[0], new double[][] { new double[] {} });
    public void addShape(int[] pos)
    {
        Geom.Shape s = cube.copy();
        for (int i = 0; i < pos.Length * 2; i++)
        {
            int j = Dir.getOpposite(i); // cube: negative -> positive, dir: positive -> negative
            Dir.apply(j, pos, 1);
            bool open = false;
            if (map.isOpen(pos))
            {
                open = true;
                if (colorizer.getTrace(pos) != -1)
                {
                    s.setFaceTexture(i, gtex, Vec.PROJ_NORMAL, null);
                    geomModel.shapes[cubeNum.get(pos)].setFaceTexture(j, gtex, Vec.PROJ_NORMAL, null);
                }
            }
            Dir.apply(i, pos, 1);
            s.cell[i].color = open ? UnityEngine.Color.white : mark ? colorizer.getColor(pos, j) : Grid.equals(pos, map.getStart()) ? RenderAbsolute.COLOR_START : Grid.equals(pos, map.getFinish()) ? RenderAbsolute.COLOR_FINISH : colorizer.getColor(pos, j);
        }
        for (int i = 0; i < pos.Length; i++) reg[i] = 0.999;
        s.scale(reg);
        for (int i = 0; i < pos.Length; i++) reg[i] = pos[i];
        s.translate(reg);
        double[][] n = new double[pos.Length * 2][];
        for (int i = 0; i < pos.Length * 2; i++)
        {
            n[i] = new double[pos.Length];
            Vec.copy(n[i], s.cell[i].normal);
        }
        normals.Add(n);
        if (glass) s.glass();
        cubeNum.set(pos, count);
        geomModel.shapes[count++] = s;
    }

    private void ToggleGlass()
    {
        if (glass)
        {
            for (int i = 0; i < count; i++)
            {
                geomModel.shapes[i].glass();
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < geomModel.shapes[i].cell.Length; j++)
                {
                    geomModel.shapes[i].cell[j].normal = normals[i][j];
                }
            }
        }
    }

    private void SetMapColor()
    {
        for (int i = 0; i < count; i++)
        {
            Grid.toCell(reg7, reg7, geomModel.shapes[i].getAlignCenter());
            for (int j = 0; j < geomModel.shapes[i].cell.Length; j++)
            {
                int dir = Dir.getOpposite(j);
                Dir.apply(dir, reg7, 1);
                bool open = false;
                if (map.isOpen(reg7))
                {
                    open = true;
                }
                Dir.apply(j, reg7, 1);
                geomModel.shapes[i].cell[j].color = open ? UnityEngine.Color.white : !mark/*not updated yet*/ ? colorizer.getColor(reg7, dir) : Grid.equals(reg7, map.getStart()) ? RenderAbsolute.COLOR_START : Grid.equals(reg7, map.getFinish()) ? RenderAbsolute.COLOR_FINISH : colorizer.getColor(reg7, dir);
            }
        }
    }

    public void save(IStore store, OptionsMap om) {
        map.save(store, om, reg.Length);
    }

    public override void ResetTrace()
    {
        renderAbsolute.ResetTrace(); 
        for (int i = 0; i < count; i++)
        {
            geomModel.shapes[i] = null;
            geomModel.clipUnits[i].setBoundaries(null);
            geomModel.clearSeparators(i);
        }
        count = 0;
        cubeNum = new DynamicArray.OfInt(reg.Length, limits);
    }
}

