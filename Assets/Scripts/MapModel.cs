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
    private double[] reg, reg3, reg4, reg5, reg6;
    private int[] reg7, reg8, reg9;
    private double[] origin;
    private double[][] axis;
    public bool showMap;
    private List<double[][]> normals = new List<double[][]>();
    private bool glide, glass;
    private float mapDistance;

    // --- construction ---

    public MapModel(int dimSpace, OptionsMap om, OptionsColor oc, OptionsSeed oe, OptionsView ov, IStore store)
    {
        if (store != null) map = new Map(dimSpace, om, store);
        else map = new Map(dimSpace, om, oe.mapSeed);
        colorizer = new Colorizer(dimSpace, om.dimMap, om.size, oc, oe.colorSeed);
        renderAbsolute = new RenderAbsolute(dimSpace, map, colorizer, ov, this);
        bufAbsolute = new PolygonBuffer(dimSpace);
        bufRelative = new PolygonBuffer(dimSpace - 1);

        reg = new double[dimSpace];
        reg3 = new double[dimSpace];
        reg4 = new double[dimSpace];
        reg5 = new double[dimSpace];
        reg6 = new double[dimSpace];
        reg7 = new int[dimSpace];
        reg8 = new int[dimSpace];
        reg9 = new int[dimSpace];
        normals.Clear();

        geomModel = new GeomModel(dimSpace, new Geom.Shape[om.size[0] * om.size[1] * om.size[2] * om.size[3]], null, null);
        geomModel.setBuffer(bufAbsolute);
        geomRelative = new RenderRelative(bufAbsolute, bufRelative, dimSpace, retina);
        cube = dimSpace == 3 ? cube3 : cube4;
        if (dimSpace == 3)
        {
            tex = new Geom.Texture[3];
            for (int k = 0; k < reg3.Length; k++) reg3[k] = 0.5;
            tex[0] = tex3.copy();
            tex[1] = tex3.copy();
            tex[1].rotate(0, 2, 90, reg3);
            tex[2] = tex3.copy();
            tex[2].rotate(0, 4, 90, reg3);
        }
        else
        {
            tex = new Geom.Texture[4];
            for (int k = 0; k < reg3.Length; k++) reg3[k] = 0.5;
            tex[0] = tex4.copy();
            tex[1] = tex4.copy();
            tex[1].rotate(0, 2, 90, reg3);
            tex[2] = tex4.copy();
            tex[2].rotate(0, 4, 90, reg3);
            tex[3] = tex4.copy();
            tex[3].rotate(0, 6, 90, reg3);
        }
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

        this.axis = axis;
        this.origin = origin;
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

    public override void setColorMode(int colorMode)
    {
        colorizer.setColorMode(colorMode);
    }

    public override void setDepth(int depth)
    {
        renderAbsolute.setDepth(depth);
    }

    public override void setArrow(bool arrow)
    {
        renderAbsolute.setArrow(arrow);
    }

    public override void setTexture(bool[] texture)
    {
        renderAbsolute.setTexture(texture);
    }

    public override void setTransparency(double transparency)
    {
        renderAbsolute.setTransparency(transparency);
    }

    public override void setOptions(OptionsColor oc, int seed, int depth, bool arrow, bool[] texture, OptionsDisplay od)
    {
        colorizer.setOptions(oc, seed);
        if (colorMode != oc.colorMode) SetMapColor();
        colorMode = oc.colorMode;
        renderAbsolute.setDepth(depth);
        renderAbsolute.setArrow(arrow);
        renderAbsolute.setTexture(texture);
        renderAbsolute.setTransparency(od.transparency);
        renderAbsolute.setTransparency(od.transparency);
        renderAbsolute.useEdgeColor = od.useEdgeColor;
        renderAbsolute.usePolygon = od.usePolygon;
        geomModel.setOptions(oc, seed, depth, arrow, texture, od);
        showMap = od.map;
        glide = od.glide;
        glass = od.glass;
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
            Vec.addScaled(reg, origin, this.axis[axis.Length-1], -distance);
            geomModel.render(reg, axis, false);
            geomRelative.run(this.axis, false);
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
    private Geom.Texture[] tex;
    private static readonly Geom.Texture gtex = new Geom.Texture(new Geom.Edge[0], new double[][] { new double[] {} });
    private const double l = 0.1;
    private const double h = 0.9;
    private static readonly UnityEngine.Color w = UnityEngine.Color.white;
    private static readonly Geom.Texture tex3 = new Geom.Texture(new Geom.Edge[] {
        new Geom.Edge(0,1,w), new Geom.Edge(1,2,w), new Geom.Edge(2,3,w), new Geom.Edge(3,0,w) }, new double[][] { 
        new double[] {l,l,l}, new double[] {l,h,l}, new double[] {l,h,h}, new double[] {l,l,h} });
    private static readonly Geom.Texture tex4 = new Geom.Texture(new Geom.Edge[] {
        new Geom.Edge(0,1,w), new Geom.Edge(1,2,w), new Geom.Edge(2,3,w), new Geom.Edge(3,0,w),
        new Geom.Edge(0,4,w), new Geom.Edge(1,5,w), new Geom.Edge(2,6,w), new Geom.Edge(3,7,w),
        new Geom.Edge(4,5,w), new Geom.Edge(5,6,w), new Geom.Edge(6,7,w), new Geom.Edge(7,4,w), }, new double[][] { 
        new double[] {l,l,l,l}, new double[] {l,h,l,l}, new double[] {l,h,h,l}, new double[] {l,l,h,l},
        new double[] {l,l,l,h}, new double[] {l,h,l,h}, new double[] {l,h,h,h}, new double[] {l,l,h,h} });
    public void addShape(int[] pos)
    {
        Geom.Shape s = cube.copy();
        for (int i = 0; i < pos.Length * 2; i++)
        {
            int j = Dir.getOpposite(i);
            Dir.apply(j, pos, 1);
            if (map.isOpen(pos))
            {
                s.setFaceTexture(i, glass ? gtex : tex[Dir.getAxis(j)].copy(), Vec.PROJ_NORMAL, null);
            }
            Dir.apply(i, pos, 1);
            s.cell[i].color = Grid.equals(pos, map.getStart()) ? RenderAbsolute.COLOR_START : Grid.equals(pos, map.getFinish()) ? RenderAbsolute.COLOR_FINISH : colorizer.getColor(pos, j);
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
        geomModel.shapes[count++] = s;
    }

    private void ToggleGlass()
    {
        if (glass)
        {
            for (int i = 0; i < count; i++)
            {
                geomModel.shapes[i].glass();
                for (int j = 0; j < geomModel.shapes[i].cell.Length; j++)
                {
                    if (geomModel.shapes[i].cell[j].customTexture != null) geomModel.shapes[i].cell[j].customTexture = gtex;
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < geomModel.shapes[i].cell.Length; j++)
                {
                    geomModel.shapes[i].cell[j].normal = normals[i][j];
                    if (geomModel.shapes[i].cell[j].customTexture != null)
                    {
                        Geom.Texture t = tex[Dir.getAxis(j)].copy();
                        t.translate(geomModel.shapes[i].vertex[0]);
                        geomModel.shapes[i].setFaceTexture(j, t, Vec.PROJ_NORMAL, null);
                    }
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
                geomModel.shapes[i].cell[j].color = Grid.equals(reg7, map.getStart()) ? RenderAbsolute.COLOR_START : Grid.equals(reg7, map.getFinish()) ? RenderAbsolute.COLOR_FINISH : colorizer.getColor(reg7, dir);
            }
        }
    }

    public void save(IStore store, OptionsMap om) {
        map.save(store, om);
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
    }
}

