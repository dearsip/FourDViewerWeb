/*
 * Scenery.java
 */

using System;
using UnityEngine;
//import java.awt.Color;

/**
 * Some view-centered scenery objects.
 */

public class Scenery
{

    // --- mesh functions ---

    // scenery meshes should have y=0 and norm 1.
    // being normalized doesn't matter for meshes
    // at y=0, but when there's a nonzero height,
    // that breaks the scaling symmetry.

    // exception: certain ground meshes are not normalized.

    // you might think the square meshes should use
    // lines that go all across the faces, but no,
    // remember this is for drawing suns and rays and terrain.
    // we need individual segments all the way across.

    // it might be slightly more efficient to rework the union and merge calls
    // to use Geom.Builder, but I figure it's not worth the effort

    /**
     * General ring generator, almost the same thing as GeomUtil.polygon.
     * @param a3 Axis to load with value d3 or -1 if you don't need that.
     */
    private static Geom.Texture ring(int dim, double r, int n, double offset, int a1, int a2, int a3, double d3)
    {
        double theta = 2 * Math.PI / n;

        double[][] vertex = new double[n][];
        for (int i = 0; i < n; i++)
        {
            double[] v = new double[dim];
            v[a1] = r * Math.Cos(theta * (i + offset));
            v[a2] = r * Math.Sin(theta * (i + offset));
            if (a3 != -1) v[a3] = d3;
            vertex[i] = v;
        }

        return new Geom.Texture(GeomUtil.edgeRing(n), vertex);
    }

    public static Geom.Texture ring3(int n, double offset)
    {
        return ring(3, 1, n, offset, 0, 2, -1, 0);
    }

    private static Geom.Texture ring4int(int n, double offset)
    {
        Geom.Texture m1 = ring(4, 1, n, offset, 0, 2, -1, 0);
        Geom.Texture m2 = ring(4, 1, n, offset, 2, 3, -1, 0);
        Geom.Texture m3 = ring(4, 1, n, offset, 3, 0, -1, 0); // axis order does matter for symmetry if n is odd
        return m1.union(m2).union(m3);
    }

    public static Geom.Texture ring4(int n, double offset)
    {
        return ring4int(n, offset).merge();
    }

    // I'm not sure what the best latitude levels are,
    // and also not even sure I'll be using this,
    // so let's not worry too much about it right now,
    // just pass in the whole array.
    /**
     * @param lat An array conceptually but not actually starting with -1 and ending with 1.
     */
    public static Geom.Texture sphere4(double[] lat, int n, double offset)
    {
        Geom.Texture accum = null;

        for (int i = 0; i < lat.Length; i++)
        {
            double r = Math.Sqrt(1 - lat[i] * lat[i]); // lat values in (-1,1)
            Geom.Texture mesh = ring(4, r, n, offset, 0, 2, 3, lat[i]);
            accum = (accum == null) ? mesh : accum.union(mesh);
        }

        // now add polar vertices and lines of longitude

        double[][] vertex = new double[2][];
        vertex[0] = new double[] { 0, 0, 0, -1 };
        vertex[1] = new double[] { 0, 0, 0, 1 };

        int south = n * lat.Length;
        int north = south + 1;

        Geom.Edge[] edge = new Geom.Edge[n * (lat.Length + 1)];
        int ie = 0;
        int max = n * (lat.Length - 1);

        for (int i = 0; i < n; i++) edge[ie++] = new Geom.Edge(south, i);
        for (int i = 0; i < max; i++) edge[ie++] = new Geom.Edge(i, i + n);
        for (int i = 0; i < n; i++) edge[ie++] = new Geom.Edge(max + i, north);

        return accum.union(new Geom.Texture(edge, vertex),/* preadded = */ true);
    }

    /**
     * @param d An array starting with -1 and ending with 1.
     */
    private static Geom.Texture frame(int dim, double[] d, int a1, int a2, int a3, double d3)
    {
        int n = d.Length;
        int total = 4 * (n - 1);

        double[][] vertex = new double[total][];
        int iv = 0;
        for (int i = 0; i < n - 1; i++)
        { // (-1,-1) almost to ( 1,-1)
            double[] v = new double[dim];
            v[a1] = d[i];
            v[a2] = -1;
            if (a3 != -1) v[a3] = d3;
            vertex[iv++] = v;
        }
        for (int i = 0; i < n - 1; i++)
        { // ( 1,-1) almost to ( 1, 1)
            double[] v = new double[dim];
            v[a1] = 1;
            v[a2] = d[i];
            if (a3 != -1) v[a3] = d3;
            vertex[iv++] = v;
        }
        for (int i = 0; i < n - 1; i++)
        { // ( 1, 1) almost to (-1, 1)
            double[] v = new double[dim];
            v[a1] = d[n - 1 - i];
            v[a2] = 1;
            if (a3 != -1) v[a3] = d3;
            vertex[iv++] = v;
        }
        for (int i = 0; i < n - 1; i++)
        { // (-1, 1) almost to (-1,-1)
            double[] v = new double[dim];
            v[a1] = -1;
            v[a2] = d[n - 1 - i];
            if (a3 != -1) v[a3] = d3;
            vertex[iv++] = v;
        }

        return new Geom.Texture(GeomUtil.edgeRing(total), vertex);
    }

    private static Geom.Texture frame3int(double[] d)
    {
        return frame(3, d, 0, 2, -1, 0);
    }

    private static Geom.Texture frame4int(double[] d)
    {
        Geom.Texture m1 = frame(4, d, 0, 2, 3, -1).union(frame(4, d, 0, 2, 3, 1));
        Geom.Texture m2 = frame(4, d, 2, 3, 0, -1).union(frame(4, d, 2, 3, 0, 1));
        Geom.Texture m3 = frame(4, d, 3, 0, 2, -1).union(frame(4, d, 3, 0, 2, 1));
        return m1.union(m2).union(m3);
    }

    public static Geom.Texture frame4(double[] d)
    {
        return frame4int(d).normalize().merge();
    }

    public static Geom.Texture ringframe4(double[] d)
    {
        return ring4int(4 * (d.Length - 1), 0).union(frame4int(d)).normalize().merge();
    }

    private static double[] gcubeValue = new double[] { -1, 1 };

    public static Geom.Texture gcube3()
    {
        return frame3int(gcubeValue); // not normalized, no merge needed
    }

    public static Geom.Texture gcube4()
    {
        return frame4int(gcubeValue).merge(); // not normalized
    }

    /**
     * @param d An array starting with -1 and ending with 1.
     */
    private static Geom.Texture face(double[] d, int a1, int a2, int a3, double d3)
    {
        int n = d.Length;

        double[][] vertex = new double[n * n][];
        int iv = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double[] v = new double[/* dim = */ 4];
                v[a1] = d[i];
                v[a2] = d[j];
                if (a3 != -1) v[a3] = d3;
                vertex[iv++] = v;
            }
        }

        Geom.Edge[] edge = new Geom.Edge[2 * n * (n - 1)];
        int ie = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n - 1; j++)
            {
                iv = n * i + j;
                edge[ie++] = new Geom.Edge(iv, iv + 1);
                iv = n * j + i;
                edge[ie++] = new Geom.Edge(iv, iv + n);
            }
        }

        return new Geom.Texture(edge, vertex);
    }

    public static Geom.Texture cube4(double[] d)
    {
        Geom.Texture m1 = face(d, 0, 2, 3, -1).union(face(d, 0, 2, 3, 1));
        Geom.Texture m2 = face(d, 2, 3, 0, -1).union(face(d, 2, 3, 0, 1));
        Geom.Texture m3 = face(d, 3, 0, 2, -1).union(face(d, 3, 0, 2, 1));
        return m1.union(m2).union(m3).normalize().merge();
    }

    // --- height functions ---

    // these and the color functions can all assume the mesh is normalized and has y=0

    public interface HeightFunction
    {
        double getHeight(double[] d);
    }

    public class HeightConstant : HeightFunction
    {
        private double height;
        public HeightConstant(double height) { this.height = height; }
        public double getHeight(double[] d)
        {
            return height;
        }
    }

    public class HeightPower : HeightFunction
    {
        private double n;
        private double height;
        public HeightPower(double n, double height) { this.n = n; this.height = height; }
        public double getHeight(double[] d)
        {
            double sum = 0;
            for (int i = 0; i < d.Length; i++) sum += Math.Pow(Math.Abs(d[i]), n);
            return height * sum;
        }
    }

    // if pos is an axis then Vec.dot is the component in that direction,
    // so powers here will give you exactly the same mountains as powers
    // in HeightPower (if min is 0)
    //
    public class HeightMountain : HeightFunction
    {
        private double[] pos;
        private double min;
        private double n;
        private double height;
        public HeightMountain(double[] pos, double min, double n, double height) { Vec.normalize(pos, pos); this.pos = pos; this.min = min; this.n = n; this.height = height; }
        public double getHeight(double[] d)
        {
            double val = Vec.dot(d, pos) - min;
            return (val > 0) ? height * Math.Pow(val / (1 - min), n) : 0;
        }
    }

    public class HeightMax2 : HeightFunction
    {
        private HeightFunction f1;
        private HeightFunction f2;
        public HeightMax2(HeightFunction f1, HeightFunction f2) { this.f1 = f1; this.f2 = f2; }
        public double getHeight(double[] d)
        {
            return Math.Max(f1.getHeight(d), f2.getHeight(d));
        }
    }

    public class HeightMaxN : HeightFunction
    {
        private HeightFunction[] f;
        public HeightMaxN(HeightFunction[] f) { this.f = f; }
        public double getHeight(double[] d)
        {
            double max = Double.MinValue;
            for (int i = 0; i < f.Length; i++)
            {
                double val = f[i].getHeight(d);
                if (val > max) max = val;
            }
            return max;
        }
    }

    private static double[] cache(Geom.Texture mesh, HeightFunction f)
    {
        double[] h = new double[mesh.vertex.Length];
        for (int i = 0; i < mesh.vertex.Length; i++)
        {
            h[i] = f.getHeight(mesh.vertex[i]);
        }
        return h;
    }

    // --- color functions ---

    public interface ColorFunction
    {
        Color getColor(double[] d);
    }

    public class ColorConstant : ColorFunction
    {
        private Color color;
        public ColorConstant(Color color) { this.color = color; }
        public Color getColor(double[] d)
        {
            return color;
        }
    }

    private class Blender
    {

        private double[] accum;
        private float[] compArray;

        public Blender()
        {
            accum = new double[3];
            compArray = new float[3];
        }

        public void clear()
        {
            Vec.zero(accum);
        }

        public void add(Color color, double weight)
        {
            for (int i = 0; i < 3; i++) compArray[i] = color[i];
            if (weight > 0) Vec.addScaledFloat(accum, accum, compArray, weight);
            // the weight test can save some work,
            // but mostly it lets us use null for the y elements in ColorBlend
        }

        public void scale(double scale)
        {
            Vec.scale(accum, accum, scale);
        }

        public Color result()
        {
            return new Color((float)accum[0], (float)accum[1], (float)accum[2]);
        }

        public Color blend(Color c1, Color c2, double f)
        {
            if (f == 0) return c1;
            if (f == 1) return c2; // common cases where we don't need to construct a new object
            clear();
            add(c1, 1 - f);
            add(c2, f);
            return result();
        }
    }

    /**
     * Color array must have one entry per direction, not per axis.
     */
    public class ColorDir : ColorFunction
    {

        private Color[] color;
        private Blender blender;

        public ColorDir(Color[] color)
        {
            this.color = color;
            blender = new Blender();
        }

        public Color getColor(double[] d)
        {

            double max = -1;
            for (int i = 0; i < d.Length; i++)
            {
                double val = Math.Abs(d[i]);
                if (val > max) max = val;
            }

            const double epsilon = 0.000001;
            max -= epsilon;
            // in case boundary points aren't exact

            blender.clear();
            int n = 0;
            for (int i = 0; i < d.Length; i++)
            {
                if (Math.Abs(d[i]) > max)
                {
                    blender.add(color[Dir.forAxis(i, d[i] < 0)], 1);
                    n++;
                }
            }
            blender.scale(1 / (double)n);
            return blender.result();
        }
    }

    /**
     * Color array must have one entry per direction, not per axis.
     */
    public class ColorBlend : ColorFunction
    {

        private Color[] color;
        private double[] weight;
        private Blender blender;

        public ColorBlend(Color[] color)
        {
            this.color = color;
            weight = new double[color.Length];
            blender = new Blender();
        }

        public Color getColor(double[] d)
        {

            // we need some weights that sum to 1, and we're on a sphere.  hmm ...
            double w1, w2;
            for (int i = 0; i < d.Length; i++)
            {
                double w = d[i] * d[i];
                if (d[i] >= 0) { w1 = w; w2 = 0; }
                else { w1 = 0; w2 = w; }
                weight[Dir.forAxis(i, false)] = w1; // 2*i
                weight[Dir.forAxis(i, true)] = w2; // 2*i+1
            }

            blender.clear();
            for (int i = 0; i < color.Length; i++) blender.add(color[i], weight[i]);
            return blender.result();
        }
    }

    // --- compass ---

    public class Compass : SceneryBase
    {

        public int dim;
        public Color[] color;
        public double radius;
        public double length;
        public double width;

        public Compass(int dim, ColorFunction f, double radius, double length, double width) : base(dim)
        {
            this.dim = dim;
            color = new Color[2 * dim];
            this.radius = radius;
            this.length = length;
            this.width = width;

            for (int a = 0; a < dim; a++)
            {
                if (a == 1) continue;

                reg0[a] = 1;
                color[2 * a] = f.getColor(reg0);
                reg0[a] = -1;
                color[2 * a + 1] = f.getColor(reg0);
                reg0[a] = 0;
            }
            // basically turns ColorBlend back into an array, but also handles ColorConstant
        }

        protected override void draw(out double[][] texture, out Color[] textureColor)
        {
            tList.Clear();
            cList.Clear();
            for (int dir = 0; dir < color.Length; dir++)
            {
                int a = Dir.getAxis(dir);
                if (a == 1) continue;

                Vec.copy(reg0, origin);
                reg0[1] = 0;
                Vec.copy(reg1, reg0);
                Dir.apply(dir, reg0, radius);
                Dir.apply(dir, reg1, radius - length);
                Vec.copy(reg2, reg1);

                if (dim == 3)
                {
                    int b = (a == 2) ? 0 : 2;

                    reg1[b] -= width;
                    reg2[b] += width;

                    drawLine(reg0, reg1, color[dir]);
                    drawLine(reg0, reg2, color[dir]);
                    drawLine(reg1, reg2, color[dir]);

                }
                else
                {
                    int b = (a == 2) ? 0 : 2;
                    int c = (a == 3) ? 0 : 3;

                    reg1[b] -= width;
                    reg1[c] -= width;

                    reg2[b] += width;
                    reg2[c] -= width;

                    drawLine(reg0, reg2, color[dir]);
                    drawLine(reg1, reg2, color[dir]);

                    reg1[b] += 2 * width;
                    reg1[c] += 2 * width;

                    drawLine(reg0, reg1, color[dir]);
                    drawLine(reg1, reg2, color[dir]);

                    reg2[b] -= 2 * width;
                    reg2[c] += 2 * width;

                    drawLine(reg0, reg2, color[dir]);
                    drawLine(reg1, reg2, color[dir]);

                    reg1[b] -= 2 * width;
                    reg1[c] -= 2 * width;

                    drawLine(reg0, reg1, color[dir]);
                    drawLine(reg1, reg2, color[dir]);
                }
            }
            texture = tList.ToArray();
            textureColor = cList.ToArray();
        }
    }

    // --- grids ---

    // it would be fun to do a polar grid too, and the min-max calculations
    // aren't very different, but (1) hard to know how to subdivide theta,
    // (2) the 4D analogue is a pain, (3) I'm not sure what the application is.

    public class Grid : SceneryBase
    {

        public int dim;
        public Color color;
        public double radius;
        public bool round;
        public double _base;
      public double interval;

        public double[] dmin;
        public double[] dmax;
        public int[] imin;
        public int[] imax;

        public Grid(int dim, Color color, double radius, bool round, double _base, double interval) : base(dim)
        {
            this.dim = dim;
            this.color = color;
            this.radius = radius;
            this.round = round;
            this._base = _base;
            this.interval = interval;

            dmin = new double[dim];
            dmax = new double[dim];
            imin = new int[dim];
            imax = new int[dim];
        }

        protected override void draw(out double[][] texture, out Color[] textureColor)
        {
            tList.Clear();
            cList.Clear();

            for (int i = 0; i < dim; i++)
            {
                if (i == 1) continue; // not used
                dmin[i] = origin[i] - radius;
                dmax[i] = origin[i] + radius;
                imin[i] = (int)Math.Ceiling((dmin[i] - _base) / interval);
                imax[i] = (int)Math.Floor((dmax[i] - _base) / interval);
            }
            // if 2*radius < interval, (imin,imax) can be empty

            reg1[1] = 0;
            if (dim == 3)
            {
                draw1(0, 2);
                draw1(2, 0);
            }
            else
            {
                draw2(0, 2, 3);
                draw2(2, 3, 0);
                draw2(3, 0, 2);
            }
            texture = tList.ToArray();
            textureColor = cList.ToArray();
        }

        private void draw1(int a, int c)
        {
            for (int ia = imin[a]; ia <= imax[a]; ia++)
            {
                reg1[a] = _base + ia * interval;
                drawGridLine(c);
            }
        }

        private void draw2(int a, int b, int c)
        {
            for (int ia = imin[a]; ia <= imax[a]; ia++)
            {
                reg1[a] = _base + ia * interval;
                for (int ib = imin[b]; ib <= imax[b]; ib++)
                {
                    reg1[b] = _base + ib * interval;
                    drawGridLine(c);
                }
            }
        }

        private double radiusAt(int c)
        {
            double sum = 0;
            for (int i = 0; i < dim; i++)
            {
                if (i == 1 || i == c) continue;
                double d = reg1[i] - origin[i];
                sum += d * d;
            }
            return Math.Sqrt(radius * radius - sum);
        }

        private void drawGridLine(int c)
        {
            Vec.copy(reg2, reg1);
            if (round)
            {
                double r = radiusAt(c);
                reg1[c] = origin[c] - r;
                reg2[c] = origin[c] + r;
            }
            else
            {
                reg1[c] = dmin[c];
                reg2[c] = dmax[c];
            }
            drawLine(reg1, reg2, color);
        }
    }

    // --- ground ---

    public class Ground : SceneryBase
    {

        public Geom.Texture mesh;
        public Color color;
        public double radius;

        public Ground(Geom.Texture mesh, Color color, double radius) : base(mesh.getDimension())
        {
            this.mesh = mesh;
            this.color = color;
            this.radius = radius;
        }

        protected override void draw(out double[][] texture, out Color[] textureColor)
        {
            tList.Clear();
            cList.Clear();
            for (int i = 0; i < mesh.edge.Length; i++)
            {
                Geom.Edge edge = mesh.edge[i];

                Vec.addScaled(reg1, origin, mesh.vertex[edge.iv1], radius);
                Vec.addScaled(reg2, origin, mesh.vertex[edge.iv2], radius);

                reg1[1] = 0;
                reg2[1] = 0;
                // force to the ground

                Color c = (edge.color != null) ? edge.color : color;
                drawLine(reg1, reg2, c);
            }
            texture = tList.ToArray();
            textureColor = cList.ToArray();
        }
    }

    // --- ground texture ---

    // the one kind of scenery that isn't attached to the origin

    public class GroundTexture : SceneryBase
    {

        public Geom.Texture mesh;
        public Color color;

        public GroundTexture(Geom.Texture mesh, Color color, int mode, double[] value) : base(mesh.getDimension())
        {
            this.mesh = mesh;
            this.color = color;

            Vec.unitVector(reg0, 1);
            mesh.project(reg0, 0, mode, value);
        }

        protected override void draw(out double[][] texture, out Color[] textureColor)
        {
            tList.Clear();
            cList.Clear();
            for (int i = 0; i < mesh.edge.Length; i++)
            {
                Geom.Edge edge = mesh.edge[i];

                Color c = (edge.color != null) ? edge.color : color;
                drawLine(mesh.vertex[edge.iv1], mesh.vertex[edge.iv2], c);
            }
            texture = tList.ToArray();
            textureColor = cList.ToArray();
        }
    }

    // --- monolith ---

    // just attach an arbitrary mesh to the origin
    // and pretend it's a big object at infinity.
    // the idea with scenery is that it's behind everything else,
    // either in the distance or on the ground plane,
    // so this only works well if the object is at nonnegative y.
    // for things in the ground plane use Ground.

    public class Monolith : SceneryBase
    {

        public Geom.Texture mesh;
        public Color color;

        public Monolith(Geom.Texture mesh, Color color) : base(mesh.getDimension())
        {
            this.mesh = mesh;
            this.color = color;
        }

        protected override void draw(out double[][] texture, out Color[] textureColor)
        {
            tList.Clear();
            cList.Clear();
            for (int i = 0; i < mesh.edge.Length; i++)
            {
                Geom.Edge edge = mesh.edge[i];

                Vec.add(reg1, origin, mesh.vertex[edge.iv1]);
                Vec.add(reg2, origin, mesh.vertex[edge.iv2]);

                Color c = (edge.color != null) ? edge.color : color;
                drawLine(reg1, reg2, c);
            }
            texture = tList.ToArray();
            textureColor = cList.ToArray();
        }
    }

    // --- horizon ---

    public class Horizon : SceneryBase, HeightFunction
    {

        public Geom.Texture mesh;
        public Color color;
        public HeightFunction fMax;
        public double[] height;
        public double[] heightClip;

        /**
         * @param f2 The previous horizon height, if any.
         */
        public Horizon(Geom.Texture mesh, Color color, HeightFunction f, HeightFunction fClip) : base(mesh.getDimension())
        {
            this.mesh = mesh;
            this.color = color;
            fMax = (fClip != null) ? new HeightMax2(f, fClip) : f;
            height = cache(mesh, f);
            if (fClip != null) heightClip = cache(mesh, fClip);
        }

        public double getHeight(double[] d) { return fMax.getHeight(d); }
        // can't use cache values, might be different set of vertices

        protected override void draw(out double[][] texture, out Color[] textureColor)
        {
            tList.Clear();
            cList.Clear();
            for (int i = 0; i < mesh.edge.Length; i++)
            {
                Geom.Edge edge = mesh.edge[i];

                Vec.add(reg1, mesh.vertex[edge.iv1], origin);
                Vec.add(reg2, mesh.vertex[edge.iv2], origin);

                double h1 = height[edge.iv1];
                double h2 = height[edge.iv2];

                reg1[1] += h1;
                reg2[1] += h2;

                if (heightClip != null)
                {
                    double delta1 = h1 - heightClip[edge.iv1];
                    double delta2 = h2 - heightClip[edge.iv2];
                    if (delta1 < 0)
                    {
                        if (delta2 < 0)
                        {
                            continue; // totally clipped
                        }
                        else
                        {
                            Vec.mid(reg1, reg1, reg2, -delta1 / (delta2 - delta1));
                        }
                    }
                    else
                    {
                        if (delta2 < 0)
                        {
                            Vec.mid(reg2, reg2, reg1, -delta2 / (delta1 - delta2));
                        }
                        else
                        {
                            // not clipped, do nothing
                        }
                    }
                }
                // could precompute this but it's hard to store.
                // note this is not real clipping against the
                // full height function, just a sample of two endpoints.

                drawLine(reg1, reg2, color);
            }
            texture = tList.ToArray();
            textureColor = cList.ToArray();
        }
    }

    // --- sky ---

    public class Sky : SceneryBase
    {

        public Geom.Texture mesh; // we only use vertices here
        public int nh;
        public Color[][] color;
        public double[] height;
        public double[] heightClip;

        /**
         * @param height   An array of 2*nh heights.
         * @param sunBlend An array of   nh blend amounts.
         */
        public Sky(Geom.Texture mesh, ColorFunction f, double[] height, Color sunColor, double[] sunBlend, HeightFunction fClip) : base(mesh.getDimension())
        {
            this.mesh = mesh;
            nh = height.Length / 2;
            color = new Color[nh][];
            for (int i = 0; i < nh; i++) color[i] = new Color[mesh.vertex.Length];
            this.height = height;
            if (fClip != null) heightClip = cache(mesh, fClip);

            Blender blender = new Blender(); // maybe f also has one, but who knows
            for (int i = 0; i < mesh.vertex.Length; i++)
            {

                Color _base = f.getColor(mesh.vertex[i]);

                for (int h = 0; h < nh; h++)
                {
                    color[h][i] = blender.blend(_base, sunColor, sunBlend[h]);
                }
            }
        }

        protected override void draw(out double[][] texture, out Color[] textureColor)
        {
            tList.Clear();
            cList.Clear();
            for (int i = 0; i < mesh.vertex.Length; i++)
            {

                Vec.add(reg0, mesh.vertex[i], origin);
                Vec.copy(reg1, reg0);
                Vec.copy(reg2, reg0);
                double clip = height[0]; // double meaning for height[0]
                if (heightClip != null) clip += heightClip[i];

                for (int h = 0; h < nh; h++)
                {
                    double h2 = height[2 * h + 1];
                    if (h2 <= clip) continue;
                    double h1 = height[2 * h];
                    reg1[1] = reg0[1] + ((h1 < clip) ? clip : h1);
                    reg2[1] = reg0[1] + h2;
                    drawLine(reg1, reg2, color[h][i]);
                }
            }
            texture = tList.ToArray();
            textureColor = cList.ToArray();
        }
    }

    // --- sun ---

    public class Sun : SceneryBase
    {

        public Geom.Texture mesh;
        public Color color;
        public double height;

        public Sun(Geom.Texture mesh, Color color, double height) : base(mesh.getDimension())
        {
            this.mesh = mesh;
            this.color = color;
            this.height = height;
        }

        protected override void draw(out double[][] texture, out Color[] textureColor)
        {
            tList.Clear();
            cList.Clear();
            for (int i = 0; i < mesh.edge.Length; i++)
            {
                Geom.Edge edge = mesh.edge[i];

                Vec.add(reg1, mesh.vertex[edge.iv1], origin);
                Vec.add(reg2, mesh.vertex[edge.iv2], origin);

                reg1[1] += height;
                reg2[1] += height;
                // could pre-add height, but then we couldn't share the mesh

                drawLine(reg1, reg2, color);
            }
            texture = tList.ToArray();
            textureColor = cList.ToArray();
        }
    }

}

