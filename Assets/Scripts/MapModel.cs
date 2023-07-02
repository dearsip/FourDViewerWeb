/*
 * MapModel.java
 */

using System;

/**
 * A model that lets the user move through a maze.
 */

public class MapModel : IModel
{

    // --- fields ---

    private Map map;
    private Colorizer colorizer;
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
    private bool glide;

    // --- construction ---

    public MapModel(int dimSpace, OptionsMap om, OptionsColor oc, OptionsSeed oe, OptionsView ov, IStore store)
    {
        if (store != null) map = new Map(dimSpace, om, store);
        else map = new Map(dimSpace, om, oe.mapSeed);
        colorizer = new Colorizer(dimSpace, om.dimMap, om.size, oc, oe.colorSeed);
        renderAbsolute = new RenderAbsolute(dimSpace, map, colorizer, ov, this);
        bufAbsolute = new PolygonBuffer(dimSpace);
        bufRelative = new PolygonBuffer(dimSpace - 1);

        geomModel = new GeomModel(dimSpace, new Geom.Shape[om.size[0] * om.size[1] * om.size[2] * om.size[3]], null, null);
        geomModel.setBuffer(bufAbsolute);
        geomRelative = new RenderRelative(bufAbsolute, bufRelative, dimSpace, retina);
        count = 0;
        reg = new double[dimSpace];
        reg3 = new double[dimSpace];
        reg4 = new double[dimSpace];
        reg5 = new double[dimSpace];
        reg6 = new double[dimSpace];
        reg7 = new int[dimSpace];
        reg8 = new int[dimSpace];
        reg9 = new int[dimSpace];
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

    public override void setTexture(bool[] texture)
    {
        renderAbsolute.setTexture(texture);
    }

    public override void setTransparency(double transparency)
    {
        renderAbsolute.setTransparency(transparency);
    }

    public override void setOptions(OptionsColor oc, int seed, int depth, bool[] texture, OptionsDisplay od)
    {
        colorizer.setOptions(oc, seed);
        renderAbsolute.setDepth(depth);
        renderAbsolute.setTexture(texture);
        renderAbsolute.setTransparency(od.transparency);
        renderAbsolute.setTransparency(od.transparency);
        renderAbsolute.useEdgeColor = od.useEdgeColor;
        renderAbsolute.usePolygon = od.usePolygon;
        geomModel.setOptions(oc, seed, depth, texture, od);
        showMap = od.map;
        glide = od.glide;
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

    public override double touch(double[] vector)
    {
        Vec.addScaled(reg3, axis[3], vector, retina);
        Vec.addScaled(reg4, origin, reg3, 10000); // infinity
        double d = 0;
        for (int i = 0; i < 4; i++) reg7[i] = Math.Sign(reg4[i] - origin[i]);
        int a = Grid.toCell(reg8, reg9, origin);
        if (a != Dir.DIR_NONE) reg8 = (reg7[Dir.getAxis(a)] == 1) ? reg9 : reg8;
        for (int i = 0; i < 4; i++)
        {
            if (reg7[i] == 0) { reg6[i] = 1; continue; }
            reg5[i] = (reg7[i] == 1) ? Math.Ceiling(origin[i]) : Math.Floor(origin[i]);
            reg6[i] = (reg5[i] - origin[i]) / (reg4[i] - origin[i]);
        }
        d = reg6[0]; a = 0;
        while (d < 1)
        {
            for (int i = 0; i < 4; i++) if (d > reg6[i]) { d = reg6[i]; a = i; }
            reg8[a] += reg7[a];
            if (!map.isOpen(reg8)) return d;
            reg5[a] += reg7[a];
            reg6[a] = (reg5[a] - origin[a]) / (reg4[a] - origin[a]);
            d = reg6[a];
        }
        return 1;
    }

    public override void setBuffer(PolygonBuffer buf)
    {
        renderAbsolute.setBuffer(buf);
    }

    public override void animate(double delta)
    {
    }

    private Polygon p;
    public override void render(double[] origin, double[][] axis)
    {
        renderAbsolute.run(origin, axis);
        if (showMap) {
            Vec.addScaled(reg, origin, this.axis[3], -distance);
            geomModel.render(reg, axis);
            geomRelative.run(this.axis, false);
            for (int i = 0; i < bufRelative.getSize(); i++)
            {
                p = bufRelative.get(i);
                foreach (double[] v in p.vertex)
                {
                    Vec.scale(v, v, 2);
                    v[0] -= 3;
                }
            }
        }
    }

    private Geom.Shape cube = GeomUtil.rect(new double[][] { new double[] { 0, 1 },
                                                             new double[] { 0, 1 },
                                                             new double[] { 0, 1 },
                                                             new double[] { 0, 1 } });
    public void addShape(int[] pos)
    {
        Geom.Shape s = cube.copy();
        s.scale(new double[] { 0.999, 0.999, 0.999, 0.999 } );
        s.translate(new double[] { pos[0], pos[1], pos[2], pos[3] });
        for (int i = 0; i < pos.Length * 2; i++)
        {
            s.cell[i].color = colorizer.getColor(pos, i);
        }
        s.glass();
        geomModel.shapes[count++] = s;
    }

   public void save(IStore store, OptionsMap om) {
      map.save(store, om);
   }
}

