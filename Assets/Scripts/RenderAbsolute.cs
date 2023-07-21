/*
 * RenderAbsolute.java
 */

using UnityEngine;

/**
 * An object that uses map and color information
 * to generate a set of lines oriented with respect to absolute coordinates.
 */

public class RenderAbsolute
{

    // --- fields ---

    private PolygonBuffer buf;
    private int dim;
    private Map map;
    private IColorize colorizer;
    public bool usePolygon;
    public bool useEdgeColor;

    private int depthMax;
    private bool[] texture;
    private double transparency;

    private int[] count; // direction use count
    private bool[] useClip;
    private Clip.CustomBoundary[] boundary;

    private double[] origin; // temporary registers
    private double[] reg1;
    private double[] reg2;
    private int[] reg3;
    private int[] reg4;
    private double[][] reg5;

    MapModel mapModel;

    // --- construction ---

    public RenderAbsolute(int dim, Map map, IColorize colorizer, OptionsView ov, MapModel mapModel)
    {

        // buf is set shortly after construction
        this.dim = dim;
        this.map = map;
        this.colorizer = colorizer;

        depthMax = ov.depth;
        texture = new bool[10];
        setTexture(ov.texture);

        count = new int[2 * dim]; // starts out zero
        useClip = new bool[OptionsView.DEPTH_MAX]; // starts out false
        boundary = new Clip.CustomBoundary[OptionsView.DEPTH_MAX];
        for (int i = 0; i < boundary.Length; i++) boundary[i] = new Clip.CustomBoundary(new double[dim], 0);

        origin = new double[dim];
        reg1 = new double[dim];
        reg2 = new double[dim];
        reg3 = new int[dim];
        reg4 = new int[dim];

        this.mapModel = mapModel;

    }

    public void setBuffer(PolygonBuffer buf)
    {
        this.buf = buf;
    }

    // --- options ---

    public void setDepth(int depth)
    {
        depthMax = depth;
    }

    public void setTexture(bool[] texture)
    {
        for (int i = 0; i < 10; i++)
        {
            this.texture[i] = texture[i];
        }
    }

    public void setTransparency(double transparency)
    {
        this.transparency = transparency;
    }

    // --- clipping ---

    /**
     * Clip at a corner, assuming we've moved in direction dir1,
     * then in direction dir2, and ended at point p.
     * The clip index i happens to be the depth, but that doesn't matter here.
     */
    private void clip(int i, int[] p, int dir1, int dir2)
    {
        useClip[i] = true;
        double[] dest = boundary[i].n;

        Grid.fromCell(dest, p);
        Dir.apply(dir1, dest, -0.5); // p is end point, back up along directions
        Dir.apply(dir2, dest, -0.5);
        Vec.sub(dest, dest, origin);

        // now we have a vector from the origin to the clipping point
        // project that vector into the plane defined by the two directions,
        // then rotate it to be a normal

        // since the clipping point has two integer coordinates,
        // it is not an open point, and therefore not equal to the origin.
        // so, the final clip vector won't be zero

        int a1 = Dir.getAxis(dir1);
        int a2 = Dir.getAxis(dir2);

        double d1 = dest[a1];
        double d2 = dest[a2];

        Vec.zero(dest);

        int s = Dir.getSign(dir1) * Dir.getSign(dir2);

        dest[a1] = s * d2;
        dest[a2] = -s * d1;

        // this choice of sign produces the correct result.
        // I don't know any easy way to see this, you just have to work through the cases.
        //
        // if, for example, dir1 and dir2 are both positive directions,
        // the vector (d1,d2) from the origin to the clipping point has positive coordinates
        // (because of the counting that prevents us from moving backward).
        // the clipping vector ought to point downward, so we should use (d2,-d1).
    }

    private void unclip(int i)
    {
        useClip[i] = false;
    }

    private void addPolygon(double[][] vertex, Color color)
    {
        if (usePolygon) {
            Polygon poly = buf.getNext();

            for (int i = 0; i < vertex.Length; i++) Vec.sub(vertex[i], vertex[i], origin);
            poly.vertex = vertex;
            poly.color = color;
            poly.color.a = (float)transparency;

            for (int i = 0; i < OptionsView.DEPTH_MAX; i++)
            {
                if (useClip[i] && Clip.clip(poly, boundary[i]))
                { // fully clipped?
                    buf.unget();
                    return;
                }
            }
        }
    }

    private void addLine(double[] p1, double[] p2, Color color)
    {
        Polygon poly = buf.getNext();

        poly.vertex = new double[2][];
        poly.vertex[0] = new double[dim];
        poly.vertex[1] = new double[dim];
        Vec.sub(poly.vertex[0], p1, origin);
        Vec.sub(poly.vertex[1], p2, origin);
        poly.color = color;

        for (int i = 0; i < OptionsView.DEPTH_MAX; i++)
        {
            if (useClip[i] && Vec.clip(poly.vertex[0], poly.vertex[1], boundary[i].n))
            { // fully clipped?
                buf.unget();
                return;
            }
        }
    }

    // --- faces ---

    private void addSquare(double[] p1, double[] p2, int a1, int a2, Color color, double edge)
    {
        // face

        if (dim == 4)
        {
            reg5 = new double[4][];
            for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
            Vec.copy(reg5[0], p1);
            p1[a1] += edge;

            Vec.copy(reg5[1], p1);
            p1[a2] += edge;

            Vec.copy(reg5[2], p1);
            p1[a1] -= edge;

            Vec.copy(reg5[3], p1);
            p1[a2] -= edge;
            addPolygon(reg5, color);
        }

        // edge

        p2[a1] += edge;
        addLine(p1, p2, color);
        p1[a1] += edge;

        p2[a2] += edge;
        addLine(p1, p2, color);
        p1[a2] += edge;

        p2[a1] -= edge;
        addLine(p1, p2, color);
        p1[a1] -= edge;

        p2[a2] -= edge;
        addLine(p1, p2, color);
        p1[a2] -= edge;
    }

    private void addLines(double[] p1, double[] p2, int a1, int a2, Color color, double edge)
    {
        // face

        reg5 = new double[4][];
        for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
        Vec.copy(reg5[0], p1);
        Vec.copy(reg5[1], p2);
        p2[a1] += edge;
        p1[a1] += edge;
        Vec.copy(reg5[2], p2);
        Vec.copy(reg5[3], p1);
        addPolygon(reg5, color);

        reg5 = new double[4][];
        for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
        Vec.copy(reg5[0], p1);
        Vec.copy(reg5[1], p2);
        p2[a2] += edge;
        p1[a2] += edge;
        Vec.copy(reg5[2], p2);
        Vec.copy(reg5[3], p1);
        addPolygon(reg5, color);

        reg5 = new double[4][];
        for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
        Vec.copy(reg5[0], p1);
        Vec.copy(reg5[1], p2);
        p2[a1] -= edge;
        p1[a1] -= edge;
        Vec.copy(reg5[2], p2);
        Vec.copy(reg5[3], p1);
        addPolygon(reg5, color);

        reg5 = new double[4][];
        for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
        Vec.copy(reg5[0], p1);
        Vec.copy(reg5[1], p2);
        p2[a2] -= edge;
        p1[a2] -= edge;
        Vec.copy(reg5[2], p2);
        Vec.copy(reg5[3], p1);
        addPolygon(reg5, color);

        // edge

        p2[a1] += edge;
        p1[a1] += edge;
        addLine(p1, p2, color);

        p2[a2] += edge;
        p1[a2] += edge;
        addLine(p1, p2, color);

        p2[a1] -= edge;
        p1[a1] -= edge;
        addLine(p1, p2, color);

        p2[a2] -= edge;
        p1[a2] -= edge;
        addLine(p1, p2, color);
    }

    private void addVector(int[] p, int dir1, int dir2, Color color)
    {
        int a1 = Dir.getAxis(dir1);
        int a2 = Dir.getAxis(dir2);

        Grid.fromCell(reg1, p);
        Dir.apply(dir1, reg1, 0.5);
        if (dir1 != dir2) Dir.apply(dir2, reg1, 0.25);

        int a3 = -1;
        int a4 = -1;
        int a5 = -1;
        for (int i = 0; i < dim; i++)
        {
            if (i == a1 || i == a2) continue;
            if (a3 == -1) { a3 = i; continue; }
            if (a4 == -1) { a4 = i; continue; }
            a5 = i;
        }

        // face

        if (dim == 4)
        {
            Vec.copy(reg2, reg1);

            if (dir1 != dir2)
            {
                reg5 = new double[3][];
                for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
                Vec.copy(reg5[0], reg1);
                Dir.apply(dir2, reg2, -0.5);
                reg2[a3] += 0.25;
                reg2[a4] += 0.25;
                Vec.copy(reg5[1], reg2);
                reg2[a3] -= 0.5;
                Vec.copy(reg5[2], reg2);
                addPolygon(reg5, color);

                reg5 = new double[3][];
                for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
                Vec.copy(reg5[0], reg1);
                Vec.copy(reg5[1], reg2);
                reg2[a4] -= 0.5;
                Vec.copy(reg5[2], reg2);
                addPolygon(reg5, color);

                reg5 = new double[3][];
                for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
                Vec.copy(reg5[0], reg1);
                Vec.copy(reg5[1], reg2);
                reg2[a3] += 0.5;
                Vec.copy(reg5[2], reg2);
                addPolygon(reg5, color);

                reg5 = new double[3][];
                for (int i = 0; i < reg5.Length; i++) reg5[i] = new double[dim];
                Vec.copy(reg5[0], reg1);
                Vec.copy(reg5[1], reg2);
                reg2[a4] += 0.5;
                Vec.copy(reg5[2], reg2);
                addPolygon(reg5, color);
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    reg5 = new double[3][];
                    for (int j = 0; j < reg5.Length; j++) reg5[j] = new double[dim];
                    reg2[a3] += 0.25 * (i % 2 * 2 - 1);
                    Vec.copy(reg5[0], reg2);
                    reg2[a3] -= 0.25 * (i % 2 * 2 - 1);
                    reg2[a4] += 0.25 * (i / 2 % 2 * 2 - 1);
                    Vec.copy(reg5[1], reg2);
                    reg2[a4] -= 0.25 * (i / 2 % 2 * 2 - 1);
                    reg2[a5] += 0.25 * (i / 4 % 2 * 2 - 1);
                    Vec.copy(reg5[2], reg2);
                    addPolygon(reg5, color);
                    reg2[a5] -= 0.25 * (i / 4 % 2 * 2 - 1);
                }
            }
        }

        // edge

        Vec.copy(reg2, reg1);

        if (dir1 != dir2) Dir.apply(dir2, reg2, -0.5);
        reg2[a3] += 0.25;
        addLine(reg1, reg2, color);

        reg2[a3] -= 0.5;
        addLine(reg1, reg2, color);
        if (a4 >= 0)
        {

            reg2[a3] += 0.25;
            reg2[a4] += 0.25;
            addLine(reg1, reg2, color);

            reg2[a4] -= 0.5;
            addLine(reg1, reg2, color);
        }
        if (a5 >= 0)
        {

            reg2[a4] += 0.25;
            reg2[a5] += 0.25;
            addLine(reg1, reg2, color);

            reg2[a5] -= 0.5;
            addLine(reg1, reg2, color);
        }
    }

    private void addTexture(int[] p, int dir, Color color, double edge)
    {
        int a = Dir.getAxis(dir);

        Grid.fromCell(reg1, p);
        Dir.apply(dir, reg1, 0.5);

        for (int i = 0; i < dim; i++)
        {
            if (i == a) continue;
            reg1[i] -= edge / 2;
        }

        Vec.copy(reg2, reg1);

        if (dim == 3)
        {

            int a1 = (a + 1) % dim;
            int a2 = (a + 2) % dim;

            addSquare(reg1, reg2, a1, a2, color, edge);

        }
        else
        {

            int a1 = (a + 1) % dim;
            int a2 = (a + 2) % dim;
            int a3 = (a + 3) % dim;

            addSquare(reg1, reg2, a1, a2, color, edge);
            reg2[a3] += edge;
            addLines(reg1, reg2, a1, a2, color, edge);
            reg1[a3] += edge;
            addSquare(reg1, reg2, a1, a2, color, edge);
        }
    }

    public static Color COLOR_START = Color.gray;
    private static Color COLOR_START_ALTERNATE = OptionsColor.RIGHTGRAY;

    public static Color COLOR_FINISH = Color.yellow * OptionsColor.fixer; // the idea is, gold
    private static Color COLOR_FINISH_ALTERNATE = OptionsColor.ORANGE * OptionsColor.fixer;

    // alternate colors are used when a start or finish mark
    // would be indistinguishable from a normal texture
    //
    // there are still a few cases where the start and finish aren't recognizable
    //
    // (1) when the walls are all the same color,
    //     and no texture except 5 is turned on
    //
    // (2) at a 2*D-way intersection (where there are no walls)

    private void addFace(int[] p, int dir)
    {
        Color color = colorizer.getColor(p, dir);
        // Color color5 = Color.clear;

        bool force = false;
        if (Grid.equals(p, map.getStart()))
        {
            force = true;
            color/*5*/ =/* (texture[5] && color.Equals(COLOR_START)) ? COLOR_START_ALTERNATE :*/ COLOR_START;
        }
        else if (Grid.equals(p, map.getFinish()))
        {
            force = true;
            color/*5*/ =/* (texture[5] && color.Equals(COLOR_FINISH)) ? COLOR_FINISH_ALTERNATE :*/ COLOR_FINISH;
        }

        if (texture[0]) addTexture(p, dir, useEdgeColor || force ? color : Color.white * OptionsColor.fixer, 0.999999);

        int d = colorizer.getTrace(p);
        if (texture[1] && d >= 0) addVector(p, dir, d, color);
        for (int i = 2; i < 10; i++)
        {

            Color c = color;
            bool draw = texture[i];

            // if (i == 5 && color5 != Color.clear)
            // {
                // c = color5;
                // draw = true;
            // }

            if (draw) addTexture(p, dir, c, 0.1 * i);
        }
    }

    // --- processing ---

    private void build(int[] p, int depth, int dirPrev)
    {
        for (int dir = 0; dir < 2 * dim; dir++)
        {

            // the count array keeps track of the opposites of the directions we've gone;
            // we no longer need to consider those directions.
            // among other things, counting prevents looping back to the same cell.
            //
            if (count[dir] > 0) continue;

            Dir.apply(dir, p, 1);

            if (!map.isOpen(p))
            { // there is a wall

                Dir.apply(dir, p, -1);
                addFace(p, dir);

            }
            else
            {                 // there is no wall
                if (depth < depthMax)
                {

                    // clip when we're going around a corner
                    // note dir can't be opposite of dirPrev because of counting
                    //
                    if (dirPrev != Dir.DIR_NONE && dir != dirPrev)
                    {
                        clip(depth, p, dirPrev, dir);
                    }

                    count[Dir.getOpposite(dir)]++; // exclude backward direction
                    build(p, depth + 1, dir);
                    count[Dir.getOpposite(dir)]--;

                    unclip(depth); // fast, just do in every case
                }
                Dir.apply(dir, p, -1);
            }
        }
    }

    public void run(double[] origin, double[][] axis)
    {
        buf.clear();
        Vec.copy(this.origin, origin);

        int dir = Grid.toCell(reg3, reg4, origin);
        if (colorizer.getTrace(reg3) == -1) mapModel.addShape(reg3);
        colorizer.setTrace(reg3);
        if (dir == Dir.DIR_NONE)
        {

            build(reg3, 0, Dir.DIR_NONE);

        }
        else
        {

            count[dir]++; // dir points from reg3 to reg4, and that's not allowed
            build(reg3, 0, Dir.DIR_NONE); // all dirPrev does is produce clipping, and that's not needed
            count[dir]--;

            count[Dir.getOpposite(dir)]++; // now, from reg4, opposite isn't allowed
            build(reg4, 0, Dir.DIR_NONE);
            count[Dir.getOpposite(dir)]--;
        }
    }
}