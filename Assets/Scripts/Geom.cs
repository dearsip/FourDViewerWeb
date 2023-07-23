using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Classes that are used to represent geometric shapes.
 * The same representation works for both 3D and 4D since we don't (currently) care about 2D faces in 4D.
 */
// 2D Face was added as a drawing target, so Face in the original code was rewritten as Cell
public class Geom
{

    // --- helpers ---

    public static double[] clone1(double[] d)
    {
        return (double[])d.Clone();
    }

    public static double[][] clone2(double[][] d)
    {
        d = (double[][])d.Clone();
        for (int i = 0; i < d.Length; i++) d[i] = (double[])d[i].Clone();
        return d;
    }

    public static int[][] clone2(int[][] d)
    {
        d = (int[][])d.Clone();
        for (int i = 0; i < d.Length; i++) d[i] = (int[])d[i].Clone();
        return d;
    }

    public static Cell[] clone2(Cell[] c)
    {
        c = (Cell[])c.Clone();
        for (int i = 0; i < c.Length; i++) c[i] = c[i].copy();
        return c;
    }

    public static Face[] clone2(Face[] f)
    {
        f = (Face[])f.Clone();
        for (int i = 0; i < f.Length; i++) f[i] = f[i].copy();
        return f;
    }

    public static Edge[] clone2(Edge[] e)
    {
        e = (Edge[])e.Clone();
        for (int i = 0; i < e.Length; i++) e[i] = e[i].copy();
        return e;
    }

    private static void vatranslate(double[][] vertex, double[] d)
    {
        for (int i = 0; i < vertex.Length; i++) Vec.add(vertex[i], vertex[i], d);
    }

    private static void vascale(double[][] vertex, double[] d)
    {
        for (int i = 0; i < vertex.Length; i++) Vec.scaleMultiCo(vertex[i], vertex[i], d);
    }

    private static void varotate(double[][] vertex, int dir1, int dir2, double theta, double[] origin)
    {
        for (int i = 0; i < vertex.Length; i++) vrotate(vertex[i], dir1, dir2, theta, origin);
    }

    private static void vrotate(double[] vec, int dir1, int dir2, double theta, double[] origin)
    {
        Vec.sub(vec, vec, origin);
        Vec.rotateAbsoluteAngleDir(vec, vec, dir1, dir2, theta);
        Vec.add(vec, vec, origin);
    }

    private static void varotate(double[][] vertex, double[] from, double[] to, double[] origin, double[] reg1, double[] reg2)
    {
        for (int i = 0; i < vertex.Length; i++) vrotate(vertex[i], from, to, origin, reg1, reg2);
    }

    private static void vrotate(double[] vec, double[] from, double[] to, double[] origin, double[] reg1, double[] reg2)
    {
        Vec.sub(vec, vec, origin);
        Vec.rotate(vec, vec, from, to, reg1, reg2);
        Vec.add(vec, vec, origin);
    }

    public static Color getColor(Color c1)
    {
        if (c1 != null && c1.a != 0) return c1;
        return Color.green * OptionsColor.fixer; // the default color
    }

    public static Color getColor(Color c1, Color c2)
    {
        if (c1 != null && c1.a > 0) return c1;
        if (c2 != null && c2.a > 0) return c2;
        return Color.green * OptionsColor.fixer; // the default color
    }

    // --- move interface ---

    public interface MoveInterface
    {

        bool noUserMove();
        void translate(double[] d);
        void scale(double[] d);
        void rotate(int dir1, int dir2, double theta, double[] origin);
    }

    // --- shape interface ---

    // shared interface for Shape and CompositeShape

    public interface ShapeInterface : MoveInterface, IDimensionMultiSrc
    {

        void unglue(List<ShapeInterface> c);
        void unglue(Stack c);
        ShapeInterface copySI();
        void setNoUserMove();
        // move interface functions go here
        double[] getAlignCenter();
        void glass();
        void setShapeColor(Color color);
        int setFaceColor(int j, Color color, bool xor);
        int setEdgeColor(int j, Color color);
        int setFaceTexture(int j, Texture texture, int mode, double[] value);

        ShapeInterface prism(int a, double min, double max);
        ShapeInterface frustum(double[] p, int a, double min, double max, double tip);
        ShapeInterface cone(double[] p, int a, double min, double max);
    }

    // --- composite shape ---

    public class CompositeShape : ShapeInterface
    {

        public ShapeInterface[] component;

        public CompositeShape(ShapeInterface[] component)
        {
            this.component = component;
        }

        public ShapeInterface getComponent(int i) { return component[i]; } // usually not OK

        public void unglue(List<ShapeInterface> c)
        {
            for (int i = 0; i < component.Length; i++) component[i].unglue(c);
        }

        public void unglue(Stack c)
        {
            for (int i = 0; i < component.Length; i++) component[i].unglue(c);
        }

        public ShapeInterface copySI()
        {
            ShapeInterface[] cnew = new ShapeInterface[component.Length];
            for (int i = 0; i < component.Length; i++) cnew[i] = component[i].copySI();
            return new CompositeShape(cnew);
        }

        public void setNoUserMove()
        {
            for (int i = 0; i < component.Length; i++) component[i].setNoUserMove();
        }

        public bool noUserMove()
        {
            for (int i = 0; i < component.Length; i++) { if (component[i].noUserMove()) return true; }
            return false;
        }

        public void translate(double[] d)
        {
            for (int i = 0; i < component.Length; i++) component[i].translate(d);
        }

        public void scale(double[] d)
        {
            for (int i = 0; i < component.Length; i++) component[i].scale(d);
        }

        public double[] getAlignCenter()
        {
            return component[0].getAlignCenter(); // stupid but it'll do for now
        }

        public void rotate(int dir1, int dir2, double theta, double[] origin)
        {
            if (origin == null) origin = clone1(getAlignCenter()); // use same origin for all rotations
            for (int i = 0; i < component.Length; i++) component[i].rotate(dir1, dir2, theta, origin);
        }

        public void glass()
        {
            for (int i = 0; i < component.Length; i++) component[i].glass();
        }

        public void setShapeColor(Color color)
        {
            for (int i = 0; i < component.Length; i++) component[i].setShapeColor(color);
        }

        public int setFaceColor(int j, Color color, bool xor)
        {
            for (int i = 0; i < component.Length; i++) { j = component[i].setFaceColor(j, color, xor); if (j < 0) break; }
            return j;
        }

        public int setEdgeColor(int j, Color color)
        {
            for (int i = 0; i < component.Length; i++) { j = component[i].setEdgeColor(j, color); if (j < 0) break; }
            return j;
        }

        public int setFaceTexture(int j, Texture texture, int mode, double[] value)
        {
            for (int i = 0; i < component.Length; i++) { j = component[i].setFaceTexture(j, texture, mode, value); if (j < 0) break; }
            return j;
        }

        public void getDimension(IDimensionMultiDest dest)
        {
            for (int i = 0; i < component.Length; i++) component[i].getDimension(dest);
        }

        public ShapeInterface prism(int a, double min, double max)
        {
            ShapeInterface[] cnew = new ShapeInterface[component.Length];
            for (int i = 0; i < component.Length; i++) cnew[i] = component[i].prism(a, min, max);
            return new CompositeShape(cnew);
        }

        public ShapeInterface frustum(double[] p, int a, double min, double max, double tip)
        {
            ShapeInterface[] cnew = new ShapeInterface[component.Length];
            for (int i = 0; i < component.Length; i++) cnew[i] = component[i].frustum(p, a, min, max, tip);
            return new CompositeShape(cnew);
        }

        public ShapeInterface cone(double[] p, int a, double min, double max)
        {
            ShapeInterface[] cnew = new ShapeInterface[component.Length];
            for (int i = 0; i < component.Length; i++) cnew[i] = component[i].cone(p, a, min, max);
            return new CompositeShape(cnew);
        }
    }

    // --- hint interface ---

    public interface HintInterface
    {
        /**
         * @param invert 1 if the owner is S1, or -1 if the owner is S2.
         */
        Separator getHint(Shape owner, Shape target, int invert);
    }

    // these are little things that get attached to shapes to tell how to
    // find separators in tough cases.  "hint" is a good name except that
    // they're more like commands than suggestions.

    // --- place helper ---

    // could generalize and use this same interface for rotations
    // and other things, but for now it's just for the place call

    public class PlaceHelper
    {

        private double[][] axis;
        private double[] originNew;
        private double[] originOld;
        private double[] reg1;

        public PlaceHelper(double[][] axis, double[] originNew, double[] originOld)
        {
            this.axis = axis;
            this.originNew = originNew;
            this.originOld = originOld;
            reg1 = new double[axis.Length];
        }

        /**
         * Change originNew to make src map to dest.
         */
        public void solvePos(double[] dest, double[] src)
        {
            Vec.sub(reg1, src, originOld);
            Vec.fromAxisCoordinates(originNew, reg1, axis);
            Vec.addScaled(originNew, dest, originNew, -1);
        }

        public void placePos(double[] dest, double[] src)
        {
            Vec.sub(reg1, src, originOld);
            Vec.fromAxisCoordinates(dest, reg1, axis);
            Vec.add(dest, dest, originNew);
        }

        public void placeDir(double[] dest, double[] src)
        {
            Vec.fromAxisCoordinates(dest, src, axis);
        }

        public void idealPos(double[] dest, double[] src)
        {
            Vec.sub(reg1, src, originNew);
            Vec.toAxisCoordinates(dest, reg1, axis);
            Vec.add(dest, dest, originOld);
        }
    }

    // --- shape ---

    public class Shape : ShapeInterface, Clip.BoundaryList, IDimension
    {

        public Cell[] cell;
        public Subface[] subface;
        public Face[] face;
        public Edge[] edge;
        public double[][] vertex;
        public int[][] nbv;
        public double[] shapecenter; // for size testing; connected to radius
        public double[] aligncenter; // for alignment, snapping, and rotation
        public double radius;
        public double[][] axis; // for block motion only
        public Shape ideal;
        public bool systemMove;
        public bool isNoUserMove;
        public HintInterface hint;
        public Cell bottomFace; // for railcars only

        public Shape() { }

        public Shape(Cell[] cell, Face[] face, Edge[] edge, double[][] vertex)
        {
            this.cell = cell;
            this.face = face ?? new Face[0];
            this.edge = edge;
            this.vertex = vertex;
            calculate();
            ideal = copy(); // so, the only difference right now is that the ideal has no ideal
        }

        public void idealize()
        {
            Vec.unitMatrix(axis);
            ideal = null; // so new ideal won't point to old ideal
            ideal = copy();
        }

        public void editIdeal()
        {
            ideal = ideal.copy(); // no way to tell if it's shared, so we have to copy
        }

        public int getSize() { return cell.Length; }
        public Clip.Boundary getBoundary(int i) { return cell[i]; }
        public void sort(double[] from) {}

        public void unglue(List<ShapeInterface> c)
        {
            c.Add(this);
        }

        public void unglue(Stack c)
        {
            c.Push(this);
        }

        public ShapeInterface copySI()
        {
            return copy();
        }

        public Shape copy()
        {
            Shape s = new Shape();

            s.cell = clone2(cell);
            if (face != null) s.face = clone2(face);
            s.subface = subface; // share
            s.edge = clone2(edge);
            s.vertex = clone2(vertex);
            s.nbv = clone2(nbv);
            s.shapecenter = clone1(shapecenter);
            s.aligncenter = clone1(aligncenter);
            s.radius = radius;
            s.axis = clone2(axis);

            s.ideal = ideal; // share ideals!  we'll make a new copy
                             // whenever we do anything that needs to alter the ideal.
                             // also note, ideals rarely change, it's just memory.

            // systemMove and noUserMove don't need to be copied.
            // systemMove is for train cars, and the copy won't be one.
            // noUserMove is for platform textures
            // (and ramps by analogy), and the copy will lose that.

            return s;
        }

        public void setNoUserMove()
        {
            isNoUserMove = true;
        }

        public bool noUserMove()
        {
            return isNoUserMove;
        }

        public void reset()
        {
            // use this to avoid accumulation of FP error in trains
            for (int i = 0; i < cell.Length; i++) cell[i].reset(ideal.cell[i]);
            for (int i = 0; i < vertex.Length; i++) Vec.copy(vertex[i], ideal.vertex[i]);
            Vec.copy(shapecenter, ideal.shapecenter);
            Vec.copy(aligncenter, ideal.aligncenter);
            // radius doesn't change under these transformations
            for (int i = 0; i < axis.Length; i++) Vec.copy(axis[i], ideal.axis[i]);
        }

        /**
         * Place the rest of the shape after you've modified aligncenter and axis.
         */
        public void place()
        {
            place(/* useShapeCenter = */ false);
        }

        public void place(bool useShapeCenter)
        {
            PlaceHelper helper = new PlaceHelper(axis, aligncenter, ideal.aligncenter);

            if (useShapeCenter)
            {
                helper.solvePos(shapecenter, ideal.shapecenter);
                // must go first since it changes aligncenter!
            }
            else
            {
                helper.placePos(shapecenter, ideal.shapecenter);
            }

            for (int i = 0; i < cell.Length; i++) cell[i].place(ideal.cell[i], helper);
            for (int i = 0; i < vertex.Length; i++) helper.placePos(vertex[i], ideal.vertex[i]);
            // we took care of shapecenter and aligncenter above
            // radius doesn't change under these transformations
            // axis is fixed
        }

        public void place(double[] d, double[][] a)
        {
            Vec.copy(aligncenter, d);
            Vec.copyMatrix(axis, a);
            // note, copyMatrix uses dest size to copy,
            // so it's OK if matrix vectors are larger
            place();
        }

        public void translate(double[] d)
        {
            for (int i = 0; i < cell.Length; i++) cell[i].translate(d);
            vatranslate(vertex, d);
            Vec.add(shapecenter, shapecenter, d);
            Vec.add(aligncenter, aligncenter, d);
            // radius doesn't change
            // the axes don't change
        }

        public void translateFrame(double[] d)
        {
            // do aligncenter and axes now, update the rest later
            Vec.add(aligncenter, aligncenter, d);
        }

        public void scale(double[] d)
        {
            for (int i = 0; i < cell.Length; i++) cell[i].scale(d);
            vascale(vertex, d);
            Vec.scaleMultiCo(shapecenter, shapecenter, d);
            Vec.scaleMultiCo(aligncenter, aligncenter, d);
            if (isUniform(d)) radius *= d[0]; else calcRadius();

            // what about the axes?  in general they're not well defined,
            // but it doesn't matter here.  scaling is a change of shape,
            // so we have to idealize and reset the axes anyway.
            idealize();
        }

        private static bool isUniform(double[] d)
        {
            for (int i = 1; i < d.Length; i++)
            {
                if (d[i] != d[0]) return false;
            }
            return true;
        }

        public double[] getAlignCenter()
        {
            return aligncenter;
        }

        public void setAlignCenter(double[] aligncenter)
        {
            Vec.copy(this.aligncenter, aligncenter);

            // we have to make a copy of the ideal anyway since we can't
            // tell whether it's shared, so keep it simple and idealize.
            // the only difference is that the axes get reset, and you probably
            // shouldn't set the align center after inexact rotations anyway.
            idealize();
        }

        public void rotate(int dir1, int dir2, double theta, double[] origin)
        {
            if (origin == null) origin = clone1(getAlignCenter());
            // since rotations are orthogonal, covariant vs. contravariant doesn't matter
            for (int i = 0; i < cell.Length; i++) cell[i].rotate(dir1, dir2, theta, origin);
            varotate(vertex, dir1, dir2, theta, origin);
            vrotate(shapecenter, dir1, dir2, theta, origin); // the clone1 call prevents this step from going awry
            vrotate(aligncenter, dir1, dir2, theta, origin);
            // radius doesn't change
            for (int i = 0; i < axis.Length; i++) Vec.rotateAbsoluteAngleDir(axis[i], axis[i], dir1, dir2, theta);
            // no origin shift for axes!
        }

        public void rotateFrame(int dir1, int dir2, double theta, double[] origin)
        {
            // do aligncenter and axes now, update the rest later
            if (origin == null) origin = clone1(getAlignCenter());
            vrotate(aligncenter, dir1, dir2, theta, origin);
            for (int i = 0; i < axis.Length; i++) Vec.rotateAbsoluteAngleDir(axis[i], axis[i], dir1, dir2, theta);
        }

        public void rotate(double[] from, double[] to, double[] origin, double[] reg1, double[] reg2)
        {
            if (origin == null) origin = clone1(getAlignCenter());
            // since rotations are orthogonal, covariant vs. contravariant doesn't matter
            for (int i = 0; i < cell.Length; i++) cell[i].rotate(from, to, origin, reg1, reg2);
            varotate(vertex, from, to, origin, reg1, reg2);
            vrotate(shapecenter, from, to, origin, reg1, reg2); // the clone1 call prevents this step from going awry
            vrotate(aligncenter, from, to, origin, reg1, reg2);
            // radius doesn't change
            for (int i = 0; i < axis.Length; i++) Vec.rotate(axis[i], axis[i], from, to, reg1, reg2);
            // no origin shift for axes!
        }

        public void rotateFrame(double[] from, double[] to, double[] origin, double[] reg1, double[] reg2)
        {
            // do aligncenter and axes now, update the rest later
            if (origin == null) origin = clone1(getAlignCenter());
            vrotate(aligncenter, from, to, origin, reg1, reg2);
            for (int i = 0; i < axis.Length; i++) Vec.rotate(axis[i], axis[i], from, to, reg1, reg2);
        }

        public void rotateRelative(double[] from, double[] to, double[] reg1, double[] reg2)
        {
            for (int i = 0; i < vertex.Length; i++) Vec.rotate(ideal.vertex[i], ideal.vertex[i], from, to, reg1, reg2);
            for (int i = 0; i < cell.Length; i++)
            {
                Vec.rotate(ideal.cell[i].center, ideal.cell[i].center, from, to, reg1, reg2);
                Vec.rotate(ideal.cell[i].normal, ideal.cell[i].normal, from, to, reg1, reg2);
            }
        }

        public void glass()
        {
            glassImpl();

            // this is something you might reasonably do in inexact coordinates, so
            // don't idealize, just make corresponding changes to the current ideal.
            editIdeal();
            ideal.glassImpl();
        }

        private void glassImpl()
        {
            for (int i = 0; i < cell.Length; i++) cell[i].normal = null;
        }

        public void calculate()
        {
            calcCenters();
            if (face.Length == 0 && getDimension() == 4) calcSubfacesAndFaces(); else calcSubfaces();
            calcNeighbors();

            int dim = getDimension();

            shapecenter = new double[dim];
            for (int i = 0; i < vertex.Length; i++) Vec.add(shapecenter, shapecenter, vertex[i]);
            Vec.scale(shapecenter, shapecenter, 1 / (double)vertex.Length);
            // not always the best center value, but we only use it for separators
            aligncenter = clone1(shapecenter);

            calcRadius(); // uses shapecenter

            axis = new double[dim][];
            for (int i = 0; i < dim; i++) axis[i] = new double[dim];
            Vec.unitMatrix(axis);
        }

        public void calcRadius()
        {
            double[] temp = new double[getDimension()];
            double r = 0;
            for (int i = 0; i < vertex.Length; i++)
            {
                Vec.sub(temp, vertex[i], shapecenter);
                double d = Vec.norm(temp);
                if (d > r) r = d;
            }
            radius = r;
        }

        public void calcCenters()
        {
            for (int i = 0; i < cell.Length; i++) calcCenter(cell[i]);
        }

        public bool[] getCellVertices(Cell c)
        {
            bool[] b = new bool[vertex.Length];
            for (int i = 0; i < c.ie.Length; i++)
            {
                Edge e = edge[c.ie[i]];
                b[e.iv1] = true;
                b[e.iv2] = true;
            }
            return b;
        }

        public double vmin(bool[] b, int axis)
        {
            double d = 0;
            bool first = true;
            for (int i = 0; i < vertex.Length; i++)
            {
                if (!b[i]) continue;
                double temp = vertex[i][axis];
                if (first)
                {
                    d = temp;
                    first = false;
                }
                else
                {
                    if (temp < d) d = temp;
                }
            }
            return d;
        }

        public double vmax(bool[] b, int axis)
        {
            double d = 0;
            bool first = true;
            for (int i = 0; i < vertex.Length; i++)
            {
                if (!b[i]) continue;
                double temp = vertex[i][axis];
                if (first)
                {
                    d = temp;
                    first = false;
                }
                else
                {
                    if (temp > d) d = temp;
                }
            }
            return d;
        }

        public void calcCenter(Cell c)
        {

            // find what vertices are involved and average them

            bool[] b = getCellVertices(c);

            double[] d = null;
            int n = 0;

            for (int i = 0; i < vertex.Length; i++)
            {
                if (!b[i]) continue;
                if (d == null) d = clone1(vertex[i]);
                else Vec.add(d, d, vertex[i]);
                n++;
            }

            Vec.scale(d, d, 1 / (double)n);
            c.center = d;
            c.calcThreshold();
        }

        public void getDimension(IDimensionMultiDest dest)
        {
            dest.putDimension(getDimension());
        }

        public int getDimension()
        {
            return vertex[0].Length; // ugly but it will do for now
        }

        public void calcSubfaces()
        {

            // in 3D, we get a subface when two cells have an edge in common.
            // in 4D, we get a subface when two cells have two or more edges
            // in common.

            List<Subface> list = new List<Subface>();
            int nGoal = (getDimension() == 3) ? 1 : 2; // only place we test dimension!

            for (int i1 = 0; i1 < cell.Length - 1; i1++)
            {
                for (int i2 = i1 + 1; i2 < cell.Length; i2++)
                {
                    if (countEdgesInCommon(cell[i1].ie, cell[i2].ie) >= nGoal)
                    {
                        Subface sf = new Subface();
                        sf.ic1 = i1;
                        sf.ic2 = i2;
                        list.Add(sf);
                    }
                }
            }

            subface = list.ToArray();
        }

        public int countEdgesInCommon(int[] ie1, int[] ie2)
        {

            // if we knew the arrays were sorted we could do
            // some kind of fast walk-through, but since the
            // calculation only happens once it hardly matters.

            int n = 0;
            for (int i1 = 0; i1 < ie1.Length; i1++)
            {
                for (int i2 = 0; i2 < ie2.Length; i2++)
                {
                    if (ie1[i1] == ie2[i2]) n++;
                }
            }
            return n;
        }

        public void calcSubfacesAndFaces()
        {

            // in 3D, we get a subface when two cells have an edge in common.
            // in 4D, we get a subface when two cells have two or more edges
            // in common.

            List<Subface> list = new List<Subface>();
            List<int>[] cfList = new List<int>[cell.Length];
            for (int i = 0; i < cell.Length; i++) cfList[i] = new List<int>();
            List<Face> faceList = new List<Face>();
            List<Edge> edgeList = new List<Edge>();
            int nGoal = (getDimension() == 3) ? 1 : 2; // only place we test dimension!

            for (int i1 = 0; i1 < cell.Length - 1; i1++)
            {
                for (int i2 = i1 + 1; i2 < cell.Length; i2++)
                {
                    int ne;
                    if ((ne = getEdgesInCommon(cell[i1].ie, cell[i2].ie, edgeList).Count) >= nGoal)
                    {
                        Subface sf = new Subface();
                        sf.ic1 = i1;
                        sf.ic2 = i2;
                        list.Add(sf);

                        Face f = new Face();
                        f.iv = new int[ne];
                        f.iv[0] = edgeList[0].iv1;
                        f.iv[1] = edgeList[0].iv2;
                        int n = 0;
                        for (int i = 2; i < edgeList.Count; i++)
                        {
                            f.iv[i] = findNextVertex(edgeList, f.iv[i - 1], ref n);
                        }
                        faceList.Add(f);
                        cfList[i1].Add(faceList.Count - 1);
                        cfList[i2].Add(faceList.Count - 1);
                    }
                }
            }

            subface = list.ToArray();
            face = faceList.ToArray();
            for (int i = 0; i < cell.Length; i++) cell[i].ifa = cfList[i].ToArray();
        }

        public List<Edge> getEdgesInCommon(int[] ie1, int[] ie2, List<Edge> list)
        {
            list.Clear();
            for (int i1 = 0; i1 < ie1.Length; i1++)
            {
                for (int i2 = 0; i2 < ie2.Length; i2++)
                {
                    if (ie1[i1] == ie2[i2]) list.Add(edge[ie1[i1]]);
                }
            }
            return list;
        }

        public int findNextVertex(List<Edge> list, int dest, ref int except)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (i == except) continue;
                if (dest == list[i].iv1)
                {
                    except = i;
                    return list[i].iv2;
                }
                if (dest == list[i].iv2)
                {
                    except = i;
                    return list[i].iv1;
                }
            }
            return -1;
        }

        public void calcNeighbors()
        {
            List<int>[] list = new List<int>[vertex.Length];
            for (int i = 0; i < vertex.Length; i++)
            {
                list[i] = new List<int>();
            }
            for (int i = 0; i < edge.Length; i++)
            {
                list[edge[i].iv1].Add(edge[i].iv2);
                list[edge[i].iv2].Add(edge[i].iv1);
            }
            nbv = new int[vertex.Length][];
            for (int i = 0; i < vertex.Length; i++)
            {
                nbv[i] = new int[list[i].Count];
                for (int j = 0; j < nbv[i].Length; j++)
                {
                    nbv[i][j] = (int)list[i][j];
                }
            }
        }

        public void setShapeColor(Color color)
        {
            for (int i = 0; i < cell.Length; i++)
            {
                cell[i].color = color;
            }
            for (int i = 0; i < edge.Length; i++)
            { // a bit faster than updateEdgeColor per face
                edge[i].color = color;
            }
        }

        public int setFaceColor(int j, Color color, bool xor)
        {
            if (j < cell.Length)
            {
                if (xor && color != null && color.Equals(cell[j].color)) color = Color.clear;
                cell[j].color = color;
                updateEdgeColor(cell[j]); // resolve conflicts by overwriting
            }
            return j - cell.Length;
        }

        public int setEdgeColor(int j, Color color)
        {
            if (j < edge.Length)
            {
                edge[j].color = color;
            }
            return j - edge.Length;
        }

        public int setFaceTexture(int j, Texture texture, int mode, double[] value)
        {
            if (j < cell.Length)
            {

                if (texture != null
                     && mode != Vec.PROJ_NONE
                     && cell[j].normal != null)
                {

                    texture.project(cell[j].normal, cell[j].threshold, mode, value);
                    texture.clip(this, cell[j]);
                    // clip after project is right order
                }
                // what about the other cases?
                // 1. don't project null
                // 2. PROJ_NONE is debug mode, want to see the whole texture
                // 3. if we don't have a face normal, project makes no sense

                cell[j].customTexture = texture;

                // this is something you might reasonably do in inexact coordinates, so
                // don't idealize, just make corresponding changes to the current ideal.
                // we aren't too sensitive to FP error in texture vertices, so transform
                // should be no problem.
                editIdeal();
                if (texture != null)
                {
                    PlaceHelper helper = new PlaceHelper(axis, aligncenter, ideal.aligncenter);
                    ideal.cell[j].customTexture = texture.toIdeal(helper);
                }
                else
                {
                    ideal.cell[j].customTexture = null;
                }
            }
            return j - cell.Length;
        }

        public void updateEdgeColor(Cell f)
        {
            for (int i = 0; i < f.ie.Length; i++)
            {
                edge[f.ie[i]].color = f.color;
            }
        }

        public int findFace(int axis, int sign)
        {
            for (int i = 0; i < cell.Length; i++)
            {
                if (match(cell[i].normal, axis, sign)) return i;
            }
            return -1;
        }

        private static bool match(double[] normal, int axis, int sign)
        {
            if (normal == null) return false;
            double e1 = 0.1; // epsilons, use two so there's no ambiguity
            double e2 = 0.000001;
            for (int i = 0; i < normal.Length; i++)
            {
                if (i == axis)
                {
                    if (normal[i] * sign < e1) return false;
                }
                else
                {
                    if (System.Math.Abs(normal[i]) > e2) return false;
                }
            }
            return true;
        }

        public ShapeInterface prism(int a, double min, double max) { return GeomUtil.prism(this, a, min, max); }
        public ShapeInterface frustum(double[] p, int a, double min, double max, double tip) { return GeomUtil.frustum(this, p, a, min, max, tip); }
        public ShapeInterface cone(double[] p, int a, double min, double max) { return GeomUtil.cone(this, p, a, min, max); }
    }

    // --- texture interface ---

    // originally I imagined having all kinds of custom texture objects,
    // but in practice there are only two kinds: Geom.Texture instances
    // that the user can manipulate and elevated platform textures
    // that have limited functionality and are poked into place by code.
    // don't overgeneralize!

    public interface CustomTexture
    {
        void draw(out double[][] texture, out Color[] textureColor, Cell cell, double[] origin);
    }

    // --- texture ---

    // vertices and edges with no other structure,
    // also useful for scenery.
    // edge colors might not be used in that case.

    public class Texture : CustomTexture, MoveInterface
    {

        public Edge[] edge;
        public double[][] vertex;
        public double[] reg1, reg2;

        public Texture() { }
        public Texture(Edge[] edge, double[][] vertex)
        {
            this.edge = edge;
            this.vertex = vertex;
            reg1 = new double[getDimension()];
            reg2 = new double[getDimension()];
        }

        public Texture copy()
        {
            Texture t = new Texture();
            t.edge = clone2(edge);
            t.vertex = clone2(vertex);
            return t;
        }

        public Texture toIdeal(PlaceHelper helper)
        {
            if (vertex.Length == 0) return copy(); // avoid error from empty texture and getDimension
            Texture t = new Texture();
            t.edge = clone2(edge);
            t.vertex = new double[vertex.Length][];
            for (int i = 0; i < vertex.Length; i++) t.vertex[i] = new double[getDimension()];
            for (int i = 0; i < vertex.Length; i++)
            {
                helper.idealPos(t.vertex[i], vertex[i]);
            }
            return t;
        }

        public void setTextureColor(Color color)
        {
            for (int i = 0; i < edge.Length; i++)
            {
                edge[i].color = color;
            }
        }

        public int getDimension()
        {
            return vertex[0].Length; // ugly but it will do for now
        }

        public Texture normalize()
        {
            for (int i = 0; i < vertex.Length; i++)
            {
                Vec.normalize(vertex[i], vertex[i]);
            }
            return this; // convenience
        }

        public void project(double[] normal, double threshold, int mode, double[] value)
        {
            for (int i = 0; i < vertex.Length; i++)
            {
                Vec.project(vertex[i], vertex[i], normal, threshold, mode, value);
            }
        }

        public virtual void draw(out double[][] texture, out Color[] textureColor, Cell cell, double[] origin)
        {
            texture = new double[edge.Length*2][];
            textureColor = new Color[edge.Length];
            for (int i=0; i<edge.Length; i++) {
                Edge e = edge[i];
                texture[i*2  ] = new double[vertex[0].Length];
                Vec.copy(texture[i*2  ], vertex[e.iv1]);
                texture[i*2+1] = new double[vertex[0].Length];
                Vec.copy(texture[i*2+1], vertex[e.iv2]);
                textureColor[i] = getColor(e.color,cell.color);
            }
        }

        public void reset(Texture ideal)
        {
            for (int i = 0; i < vertex.Length; i++) Vec.copy(vertex[i], ideal.vertex[i]);
        }

        public void place(Texture ideal, PlaceHelper helper)
        {
            for (int i = 0; i < vertex.Length; i++) helper.placePos(vertex[i], ideal.vertex[i]);
        }

        public bool noUserMove()
        {
            return false;
        }

        public void translate(double[] d)
        {
            vatranslate(vertex, d);
        }

        public void scale(double[] d)
        {
            vascale(vertex, d);
        }

        public void rotate(int dir1, int dir2, double theta, double[] origin)
        {
            if (origin == null) origin = clone1(vertex[0]); // no shapeCenter
            varotate(vertex, dir1, dir2, theta, origin);
        }

        public Texture union(Texture texture) { return union(texture,/* preadded = */ false); }
        public Texture union(Texture texture, bool preadded)
        {

            Texture t1 = this;
            Texture t2 = texture;

            // we do want to unify equal vertices so that for example we don't
            // draw the same vertical rays several times, but that should wait
            // until all the unions are done.

            double[][] vNew = new double[t1.vertex.Length + t2.vertex.Length][];
            Array.Copy(t1.vertex, 0, vNew, 0, t1.vertex.Length);
            Array.Copy(t2.vertex, 0, vNew, t1.vertex.Length, t2.vertex.Length);

            Edge[] eNew = new Edge[t1.edge.Length + t2.edge.Length];
            Array.Copy(t1.edge, 0, eNew, 0, t1.edge.Length);
            if (preadded)
            {
                Array.Copy(t2.edge, 0, eNew, t1.edge.Length, t2.edge.Length);
            }
            else
            {
                // that's the idea, but actually we have to adjust
                // the t2 edges so they point at the new t2 vertices
                for (int i = 0; i < t2.edge.Length; i++)
                {
                    eNew[t1.edge.Length + i] = new Edge(t2.edge[i].iv1 + t1.vertex.Length,
                                                      t2.edge[i].iv2 + t1.vertex.Length,
                                                      t2.edge[i].color);
                }
            }

            return new Texture(eNew, vNew);
        }

        public Texture merge()
        {

            Builder b = new Builder(true, true, false, -1);
            int[] vmap = new int[vertex.Length];

            for (int i = 0; i < vertex.Length; i++)
            {
                vmap[i] = b.addVertex(vertex[i]);
            }

            for (int i = 0; i < edge.Length; i++)
            {
                b.addEdge(new Edge(vmap[edge[i].iv1], vmap[edge[i].iv2], edge[i].color));
            }

            edge = b.toEdgeArray();
            vertex = b.toVertexArray();

            return this; // convenience
        }

        public bool clip(Clip.BoundaryList boundaryList, Clip.Boundary exceptBoundary, double[] p1, double[] p2)
        {
            for (int i = 0; i < boundaryList.getSize(); i++)
            {
                Clip.Boundary b = boundaryList.getBoundary(i);
                if (b == exceptBoundary) continue;
                double[] normal = b.getNormal();
                if (normal == null) continue;
                if (Vec.clip(p1, p2, normal, b.getThreshold(),/* sign = */ -1)) return true;
            }
            return false;
        }

        public void clip(Clip.BoundaryList boundaryList, Clip.Boundary exceptBoundary)
        {
            if (vertex.Length == 0) return; // avoid error from empty texture and getDimension

            Builder b = new Builder(true, true, false, -1);

            int dim = getDimension();
            double[] p1 = new double[dim];
            double[] p2 = new double[dim];

            for (int i = 0; i < edge.Length; i++)
            {
                Edge e = edge[i];

                Vec.copy(p1, vertex[e.iv1]);
                Vec.copy(p2, vertex[e.iv2]);
                if (clip(boundaryList, exceptBoundary, p1, p2)) continue; // fully clipped

                // there's no real correspondence between vertices,
                // just add whatever new ones we need
                int iv1 = b.addVertex((double[])p1.Clone());
                int iv2 = b.addVertex((double[])p2.Clone());
                b.addEdge(new Edge(iv1, iv2, e.color));
            }

            edge = b.toEdgeArray();
            vertex = b.toVertexArray();
        }
    }

    // --- builder ---

    // can use this to build both shapes and textures

    public class Builder
    {

        private List<double[]> vnew;
        private List<Edge> enew;
        private List<Face> fnew;
        private List<Cell> cnew;

        // face vertex accumulator
        private int[] aiv;
        private int alen;

        public Builder(bool v, bool e, bool c, int amax)
        {
            if (v) vnew = new List<double[]>();
            if (e) enew = new List<Edge>();
            if (c) cnew = new List<Cell>();
            if (amax != -1) aiv = new int[amax];
        }

        // vertex functions

        private int indexOf(double[] v)
        {
            const double epsilon = 0.001;
            for (int i = 0; i < vnew.Count; i++)
            {
                if (Vec.approximatelyEquals(vnew[i], v, epsilon)) return i;
            }
            return -1;
        }

        public int addVertex(double[] v)
        {
            int index = indexOf(v);
            if (index == -1)
            {
                index = vnew.Count;
                vnew.Add(v);
            }
            return index;
        }

        public double[][] toVertexArray()
        {
            return vnew.ToArray();
        }

        // edge functions (that don't call vertex functions at all)

        private int indexOf(int iv1, int iv2)
        {
            for (int i = 0; i < enew.Count; i++)
            {
                if (enew[i].sameVertices(iv1, iv2)) return i;
                // can't do anything about the color;
                // it's the same edge, one color has to win out.
            }
            return -1;
        }

        public int addEdge(int iv1, int iv2)
        {
            int index = indexOf(iv1, iv2);
            if (index == -1)
            {
                index = enew.Count;
                enew.Add(new Edge(iv1, iv2));
            }
            return index;
        }

        public int addEdge(Edge e)
        {
            int index = indexOf(e.iv1, e.iv2);
            if (index == -1)
            {
                index = enew.Count;
                enew.Add(e);
            }
            return index;
        }

        public Edge[] toEdgeArray()
        {
            return enew.ToArray();
        }

        public Face[] toFaceArray()
        {
            return (fnew != null) ? fnew.ToArray() : null;
        }

        // face functions (that do call edge functions)

        // of course there's some similarity to GeomUtil.edgeRing,
        // but there are big differences too.  here we're making
        // one face of a three-dimensional object, not a complete
        // two-dimensional object, so there's just one face, not
        // one per edge, and the normals aren't in the face plane.

        public Cell makeCell(int[] iv)
        {
            return makeCell(iv, iv.Length);
        }

        /**
         * @param len The number of elements of iv to use.
         */
        public Cell makeCell(int[] iv, int len)
        {
            Cell f = new Cell();

            f.ie = new int[len];
            int n = 0;
            for (int i = 0; i < len; i++)
            {
                int iv1 = iv[i];
                int iv2 = iv[(i + 1) % len];
                if (iv2 != iv1)
                {
                    f.ie[n++] = addEdge(iv1, iv2);
                }
                // else it was duplicate
            }

            if (n != len)
            {
                int[] temp = new int[n];
                Array.Copy(f.ie, 0, temp, 0, n);
                f.ie = temp;
                // rare, so OK to be inefficient
            }

            f.normal = new double[3];

            cnew.Add(f);
            return f;
        }

        public Cell[] toCellArray()
        {
            return cnew.ToArray();
        }

        // face vertex functions (that call face, edge, and maybe vertex functions)

        public void addFaceVertex(double[] v)
        {
            addFaceVertex(addVertex(v));
        }

        public void addFaceVertex(int iv)
        {
            aiv[alen++] = iv;
        }

        public Cell makeFace()
        {
            Cell f = makeCell(aiv, alen);
            alen = 0;
            return f;
        }
    }

    // --- cell ---

    public class Cell : Clip.Boundary
    {

        public int[] ie; // indices of edges
        public int[] ifa;
        public double[] center;
        public double[] normal;
        public double threshold;
        public Color color;
        public CustomTexture customTexture;

        public bool visible;

        public double[] getNormal() { return normal; }
        public double getThreshold() { return threshold; }
        public void calcThreshold() { threshold = (normal != null) ? Vec.dot(center, normal) : 0; }

        public Cell() { this.ifa = new int[0]; }

        public Cell(int[] ie, int[] ifa, double[] normal)
        {
            this.ie = ie;
            this.ifa = ifa ?? new int[0];
            this.normal = normal;
        }

        public Cell copy()
        {
            Cell c = new Cell();
            c.ie = ie; // share
            c.ifa = ifa;
            c.center = clone1(center); // don't copy before adding to shape
            c.normal = (normal != null) ? clone1(normal) : null;
            c.threshold = threshold;
            c.color = color;
            if (customTexture is Geom.Texture) {
                c.customTexture = ((Geom.Texture)customTexture).copy();
            }
            // visible gets recalculated as needed
            return c;
        }

        public int[] getVertices(Edge[] e, int nv)
        {
            bool[] b = new bool[nv];
            for (int i = 0; i < ie.Length; i++)
            {
                b[e[ie[i]].iv1] = true;
                b[e[ie[i]].iv2] = true;
            }
            int fv = 0;
            for (int i = 0; i < nv; i++)
            {
                if (b[i]) fv++;
            }
            int[] iv = new int[fv];
            int j = 0;
            for (int i = 0; i < nv; i++)
            {
                if (b[i]) iv[j++] = i;
            }
            return iv;
        }

        public void reset(Cell ideal)
        {
            Vec.copy(center, ideal.center);
            if (ideal.normal != null)
            {
                Vec.copy(normal, ideal.normal);
            }
            else
            {
                normal = null;
            }
            threshold = ideal.threshold;
        }

        public void place(Cell ideal, PlaceHelper helper)
        {
            helper.placePos(center, ideal.center);
            if (ideal.normal != null)
            {
                helper.placeDir(normal, ideal.normal);
            }
            else
            {
                normal = null;
            }
            calcThreshold(); // can't transform it
            if (customTexture is Geom.Texture) {
                ((Geom.Texture)customTexture).place((Geom.Texture)ideal.customTexture, helper);
            }
        }

        public void translate(double[] d)
        {
            Vec.add(center, center, d);
            calcThreshold();
            if (customTexture is Geom.Texture) {
                ((Geom.Texture)customTexture).translate(d);
            }
        }

        public void scale(double[] d)
        {
            Vec.scaleMultiCo(center, center, d);
            if (normal != null) Vec.scaleMultiContra(normal, normal, d);
            calcThreshold();
            if (customTexture is Geom.Texture) {
                ((Geom.Texture)customTexture).scale(d);
            }
        }

        public void rotate(int dir1, int dir2, double theta, double[] origin)
        {
            vrotate(center, dir1, dir2, theta, origin);
            if (normal != null) Vec.rotateAbsoluteAngleDir(normal, normal, dir1, dir2, theta);
            // no origin shift for normals!
            calcThreshold(); // threshold changes if origin isn't coordinate origin
            if (customTexture is Geom.Texture) {
                ((Geom.Texture)customTexture).rotate(dir1, dir2, theta, origin);
            }
        }

        public void rotate(double[] from, double[] to, double[] origin, double[] reg1, double[] reg2)
        {
            vrotate(center, from, to, origin, reg1, reg2);
            if (normal != null) Vec.rotate(normal, normal, from, to, reg1, reg2);
            // no origin shift for normals!
            calcThreshold(); // threshold changes if origin isn't coordinate origin
        }
    }

    // --- subface ---

    public class Subface
    {

        public int ic1; // index of cell
        public int ic2;
    }

    // --- face ---

    public class Face
    {

        public int[] iv;
        public bool visible;

        public Face() { }
        public Face(int[] iv) { this.iv = iv; }

        public Face copy()
        {
            return new Face(iv);
        }
    }

    // --- edge ---

    public class Edge
    {

        public int iv1; // index of vertex 1
        public int iv2; // index of vertex 2
        public Color color;
        public bool visible;

        public Edge() { }
        public Edge(int iv1, int iv2)
        {
            this.iv1 = iv1;
            this.iv2 = iv2;
        }
        public Edge(int iv1, int iv2, Color color)
        {
            this.iv1 = iv1;
            this.iv2 = iv2;
            this.color = color;
        }

        public Edge copy()
        {
            return new Edge(iv1, iv2, color);
        }

        public bool sameVertices(int iv1, int iv2)
        {
            return (this.iv1 == iv1 && this.iv2 == iv2)
                   || (this.iv1 == iv2 && this.iv2 == iv1);
        }

        public bool sameVertices(Edge edge)
        {
            return (iv1 == edge.iv1 && iv2 == edge.iv2)
                   || (iv1 == edge.iv2 && iv2 == edge.iv1);
        }
    }

    // --- separator ---

    public abstract class Separator
    {

        public virtual int apply(double[] origin) { return 0; }

        public const int S1_FRONT = -1;
        public const int NO_FRONT = 0;
        public const int S2_FRONT = 1;
        public const int UNKNOWN = 2; // only Clip.dynamicSeparate returns this
    }

    public class NormalSeparator : Separator
    {

        public double[] normal;
        public double threshMin;
        public double threshMax;
        public int invert;

        public NormalSeparator(double[] normal, double threshMin, double threshMax, int invert)
        {
            this.normal = normal;
            this.threshMin = threshMin;
            this.threshMax = threshMax;
            this.invert = invert;
        }

        public NormalSeparator(Cell cell, int invert)
        { // convenience
            this.normal = cell.normal;
            this.threshMin = cell.threshold;
            this.threshMax = cell.threshold;
            this.invert = invert;
        }

        public override int apply(double[] origin)
        {
            double value = Vec.dot(origin, normal);
            int result;
            if (value < threshMin) result = S1_FRONT;
            else if (value > threshMax) result = S2_FRONT;
            else result = NO_FRONT;
            // if value is in between, neither is in front!
            return result * invert;
        }
    }

    public class NullSeparator : Separator
    {
        public override int apply(double[] origin) { return NO_FRONT; }
    }

    public static Separator nullSeparator = new NullSeparator();
}