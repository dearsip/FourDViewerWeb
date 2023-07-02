using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/**
 * A helper class to hold the line-clipping algorithm.
 */
// 多角形のクリッピングを追加している。
public class Clip
{
    // --- constants and structures ---

    // we have a line from p0 to p1 parametrized by a number from 0 to 1.
    // when clipping occurs, the result might include the interval [0,a],
    // the interval [b,1], or both.

    public const int KEEP_NONE = 0;
    public const int KEEP_A = 1; // note, these two can also be used as masks
    public const int KEEP_B = 2; //
    public const int KEEP_AB = 3;
    public const int KEEP_LINE = 4;

    public class Result
    {

        public double a;
        public double b;
        public int clip;

        public int ia; // index of boundary on "a" side, valid if KEEP_A
        public int ib; //                      "b"                KEEP_B

        // must test for KEEP_LINE first
        public bool hasSegA() { return ((clip & KEEP_A) != 0); }
        public bool hasSegB() { return ((clip & KEEP_B) != 0); }

        // computation of the actual points isn't always needed, so don't
        // do it unless requested.
        public void getPointA(double[] dest, double[] p0, double[] p1) { Vec.mid(dest, p0, p1, a); }
        public void getPointB(double[] dest, double[] p0, double[] p1) { Vec.mid(dest, p0, p1, b); }
    }

    public interface BoundaryList
    {
        int getSize();
        Boundary getBoundary(int i);
        void sort(double[] from);
    }

    public interface Boundary
    {
        double[] getNormal();
        double getThreshold();
    }

    // --- clipping ---

    /**
    * See if the line from p0 to p1 is clipped by the poly-whatever.
    * @return A copy of the result.clip value.
    */
    public static int clip(double[] p0, double[] p1, BoundaryList boundaryList, Result result)
    {

        if (boundaryList.getSize() == 0)
        { // result of glass in calcViewBoundaries
            result.clip = KEEP_LINE;
            return result.clip;
        }

        // the theory here is, a point p is blocked by the poly-whatever
        // if for every boundary, p dot n is less than the threshold;
        // but here it's easier to think in terms of what's safe, not clipped.
        // once we find p dot n greater than the threshold, that part is safe.
        // the safe parts encroach from the two ends of the line.

        const double epsilon = 0.000001;
        // when two cubes are perfectly stacked, I want the top face of the bottom cube
        // to be hidden even if there's FP error/rounding, so expand the clip region
        // by epsilon.  the normals ought to be unit vectors to get consistent epsilons,
        // but it's not a big deal either way.

        // these will expand out as things become safe
        result.a = 0;
        result.b = 1;
        result.clip = KEEP_NONE;

        int size = boundaryList.getSize();
        for (int i = 0; i < size; i++)
        {
            Boundary boundary = boundaryList.getBoundary(i);

            double[] n = boundary.getNormal();
            double t = boundary.getThreshold();

            if (n == null)
            { // glass, can't clip
                result.clip = KEEP_LINE;
                return result.clip;
            }

            double v0 = Vec.dot(p0, n) - t - epsilon;
            double v1 = Vec.dot(p1, n) - t - epsilon;

            if (v0 > 0)
            {
                if (v1 > 0)
                { // all safe, we're done
                    result.a = 1;
                    result.b = 0;
                }
                else
                { // add to safe left part
                    double x = v0 / (v0 - v1);
                    if (x > result.a) { result.a = x; result.ia = i; }
                }
            }
            else
            {
                if (v1 > 0)
                { // add to safe right part
                    double x = v0 / (v0 - v1);
                    if (x < result.b) { result.b = x; result.ib = i; }
                }
                // else nothing is safe, keep checking
            }

            if (result.a >= result.b)
            { // all safe
                result.clip = KEEP_LINE;
                return result.clip;
            }
        }

        if (result.a > 0) result.clip |= KEEP_A;
        if (result.b < 1) result.clip |= KEEP_B;
        return result.clip;
    }

    const double epsilon = 0.000001;
    // polygon（凸多角形）を boundaryList（主に多胞体の投影の輪郭）で切り取る。
    // 出力は複数の凸多角形になるので、list に入れる。
    public static void clip(Polygon polygon, BoundaryList boundaryList, List<Polygon> list)
    {
        Polygon reg = new Polygon();
        reg.copy(polygon);
        if (boundaryList.getSize() == 0)
        {
            list.Add(polygon);
            return;
        }
        boundaryList.sort(polygon.vertex[0]);
        for (int i = 0; i < boundaryList.getSize(); i++)
        {
            Boundary boundary = boundaryList.getBoundary(i);
            polygon = clip(polygon, boundary, list); // 隠れる部分が返されるので、次の boundary を適用する
            if (polygon == null) { // 隠れる部分が無ければ、元のpolygonを返す
                list.Clear();
                list.Add(reg);
                break;
            }
        }

    }

    // polygon（凸多角形）を boundary で分け、隠れない部分を list に追加し、隠れる部分を返す。
    // 凸多角形なので、隠れない部分と隠れる部分それぞれの頂点は離れず並んでいる。
    // 頂点を順番にチェックして、隠れない部分から隠れる部分に入る直前の頂点番号を va（番号a）、その逆を vb（番号b）とする。
    // それぞれの次の頂点のなす線分は boundary で切り取られるので、その座標を取得する。
    // それらの頂点と元の多角形の頂点とで、隠れない多角形（頂点vp）、隠れる多角形（頂点vn）を定義する。
    public static Polygon clip(Polygon polygon, Boundary boundary, List<Polygon> list)
    {
        double[] n = boundary.getNormal();
        double t = boundary.getThreshold();
        int size = polygon.vertex.Length;
        int a = -1;
        int b = -1;
        double[] va = new double[polygon.vertex[0].Length];
        double[] vb = new double[polygon.vertex[0].Length];
        double v0, v1;
        double[][] vp, vn;

        bool np = Vec.dot(polygon.vertex[0], n) - t > epsilon; // 最初の頂点が隠れないか
        bool _np;
        for (int i = 1; i < size; i++)
        {
            _np = Vec.dot(polygon.vertex[i], n) - t > epsilon;
            if (np ^ _np) // 変化あり
            {
                if (np) a = i - 1; // 隠れない -> 隠れる
                else b = i - 1; // 隠れる -> 隠れない
            }
            np = _np;
        }
        if (a < 0) // 隠れない -> 隠れる が起きていない
            if (b < 0)
            {
                if (np) // 全て隠れない
                {
                    list.Add(polygon); // 元の polygon をそのまま追加
                    return null; // 隠れない部分はない、終了
                }
                else // 全て隠れる
                {
                    return polygon; // list への追加はなし、元の polygon を返す
                }
            }
            else // 隠れる -> 隠れない はある、すなわち最後の頂点から最初の頂点へ移るときが 隠れない -> 隠れる
            {
                a = size - 1;
                v0 = Vec.dot(polygon.vertex[a], n) - t - epsilon; // va の計算
                v1 = Vec.dot(polygon.vertex[0], n) - t - epsilon;
                Vec.mid(va, polygon.vertex[a], polygon.vertex[0], v0 / (v0 - v1));
                v0 = Vec.dot(polygon.vertex[b], n) - t - epsilon; // vb の計算
                v1 = Vec.dot(polygon.vertex[b + 1], n) - t - epsilon;
                Vec.mid(vb, polygon.vertex[b], polygon.vertex[b + 1], v0 / (v0 - v1));
            }
        else if (b < 0) // 隠れない -> 隠れる はある、すなわち最後の頂点から最初の頂点へ移るときが 隠れる -> 隠れない
        {
            v0 = Vec.dot(polygon.vertex[a], n) - t - epsilon; // va の計算
            v1 = Vec.dot(polygon.vertex[a + 1], n) - t - epsilon;
            Vec.mid(va, polygon.vertex[a], polygon.vertex[a + 1], v0 / (v0 - v1));
            b = size - 1;
            v0 = Vec.dot(polygon.vertex[b], n) - t - epsilon; // vb の計算
            v1 = Vec.dot(polygon.vertex[0], n) - t - epsilon;
            Vec.mid(vb, polygon.vertex[b], polygon.vertex[0], v0 / (v0 - v1));
        }
        else // 共にある
        {
            v0 = Vec.dot(polygon.vertex[a], n) - t - epsilon; // va の計算
            v1 = Vec.dot(polygon.vertex[a + 1], n) - t - epsilon;
            Vec.mid(va, polygon.vertex[a], polygon.vertex[a + 1], v0 / (v0 - v1));
            v0 = Vec.dot(polygon.vertex[b], n) - t - epsilon; // vb の計算
            v1 = Vec.dot(polygon.vertex[b + 1], n) - t - epsilon;
            Vec.mid(vb, polygon.vertex[b], polygon.vertex[b + 1], v0 / (v0 - v1));
        }

        if (a < b) // 最初の頂点は隠れない部分が含む
        {
            int sub = b - a;
            vp = new double[size - sub + 2][]; // 隠れない部分
            for (int i = 0; i <= a; i++) vp[i] = Geom.clone1(polygon.vertex[i]);
            vp[a + 1] = va;
            vp[a + 2] = vb;
            for (int i = 0; i < size - b - 1; i++) vp[a + 3 + i] = Geom.clone1(polygon.vertex[b + 1 + i]);

            vn = new double[sub + 2][]; // 隠れる部分
            vn[0] = va;
            for (int i = 0; i < sub; i++) vn[1 + i] = Geom.clone1(polygon.vertex[a + 1 + i]);
            vn[sub + 1] = vb;
        }
        else // 最初の頂点は隠れる部分が含む
        {
            int sub = a - b;
            vn = new double[size - sub + 2][]; // 隠れる部分
            for (int i = 0; i <= b; i++) vn[i] = Geom.clone1(polygon.vertex[i]);
            vn[b + 1] = vb;
            vn[b + 2] = va;
            for (int i = 0; i < size - a - 1; i++) vn[b + 3 + i] = Geom.clone1(polygon.vertex[a + 1 + i]);

            vp = new double[sub + 2][]; // 隠れない部分
            vp[0] = vb;
            for (int i = 0; i < sub; i++) vp[1 + i] = Geom.clone1(polygon.vertex[b + 1 + i]);
            vp[sub + 1] = va;
        }

        Polygon poly = new Polygon(vp, polygon.color); // 隠れない部分を追加
        list.Add(poly);

        return new Polygon(vn, polygon.color); // 隠れる部分を返す
    }

    // 単一の boundary によるクリッピング（視錐台用）。完全に隠れたら true を返す。
    public static bool clip(Polygon polygon, Boundary boundary)
    {
        List<Polygon> list = new List<Polygon>(); // 上の clip(..) を流用するために生成
        if (clip(polygon, boundary, list) == polygon) return true;
        polygon.copy(list[0]);
        return false;
    }

    // --- view boundaries ---

    public class CustomBoundary : Boundary
    {

        public double[] n;
        public double t;

        public CustomBoundary(double[] n, double t)
        {
            this.n = n;
            this.t = t;
        }

        public double[] getNormal() { return n; }
        public double getThreshold() { return t; }
    }

    public static CustomBoundary calcViewBoundary(double[] origin, Boundary b1, Boundary b2)
    {

        // the theory here is, b1 and b2 are faces (d-1), their intersection is a subface (d-2)
        // and then we want to find the boundary that includes the subface and the origin (d-1).
        // I think this way (clipping up front) is simpler than trying to clip after projection.

        // normals n1 and n2 define a plane, and we just solve for a new normal (1-x) n1 + x n2
        // that includes the origin in the perpendicular (d-1)-space through the subface.

        double[] n1 = b1.getNormal();
        double[] n2 = b2.getNormal();
        double t1 = b1.getThreshold();
        double t2 = b2.getThreshold();

        if (n1 == null || n2 == null) return null;

        double k1 = Vec.dot(n1, origin) - t1;
        double k2 = Vec.dot(n2, origin) - t2;
        double x = k1 / (k1 - k2);
        // we get sign change issues when k1 crosses k2, but we should only be calling this
        // when b1 is visible and b2 isn't, in which case k1 > 0 and k2 < 0 and all is well.
        // also works when b1 and b2 are swapped.

        // new normal is weighted sum of old normals,
        // new thresh is weighted sum of old threshs
        //
        double[] n3 = new double[origin.Length];
        Vec.mid(n3, n1, n2, x);
        double t3 = (1 - x) * t1 + x * t2;

        return new CustomBoundary(n3, t3);
    }

    public class CustomBoundaryList : BoundaryList
    {

        private List<Boundary> boundaries;
        private double[] center;

        public CustomBoundaryList()
        {
            boundaries = new List<Boundary>();
        }

        public void addBoundary(Boundary b) { boundaries.Add(b); }
        public void setCenter(double[] c) { center = new double[c.Length]; Vec.copy(center, c); }

        public int getSize() { return boundaries.Count; }
        public Boundary getBoundary(int i) { return boundaries[i]; }
        public void sort(double[] from) {
            double[] ns = new double[boundaries.Count];
            double[] vector = new double[center.Length];
            Vec.sub(vector, center, from);
            for (int i = 0; i < boundaries.Count; i++) ns[i] = Vec.dot(boundaries[i].getNormal(), vector);
            boundaries = boundaries.Select((x, i) => new KeyValuePair<Boundary,int>(x,i))
                                   .OrderBy(x => ns[x.Value])
                                   .Select(x => x.Key)
                                   .ToList();
        }
    }

    public static CustomBoundaryList calcViewBoundaries(double[] origin, Geom.Shape shape)
    {

        CustomBoundaryList list = new CustomBoundaryList();

        list.setCenter(shape.aligncenter);
        // clip by subfaces where one face is visible and the other not
        for (int i = 0; i < shape.subface.Length; i++)
        {
            Geom.Subface sf = shape.subface[i];
            Geom.Cell c1 = shape.cell[sf.ic1];
            Geom.Cell c2 = shape.cell[sf.ic2];
            if (c1.visible != c2.visible)
            {
                Boundary b = calcViewBoundary(origin, c1, c2);
                if (b != null) list.addBoundary(b); // only null if glass
            }
        }

        return list;
    }

    // --- clip unit ---

    public class Draw : IDraw
    {

        public BoundaryList bl;
        public IDraw next;
        public Result clipResult;
        public double[] temp;
        public List<Polygon> list;

        public Draw(int dim)
        {
            // bl and next vary now
            clipResult = new Result();
            temp = new double[dim];
            list = new List<Polygon>();
        }

        public void setBoundaries(BoundaryList bl)
        {
            this.bl = bl;
        }

        public IDraw chain(IDraw next)
        {
            this.next = next;
            return this; // convenience
        }

        public void drawLine(double[] p1, double[] p2, Color color, double[] origin)
        {
            if (clip(p1, p2, bl, clipResult) == KEEP_LINE)
            {
                next.drawLine(p1, p2, color, origin);
            }
            else
            {
                if (clipResult.hasSegA())
                {
                    clipResult.getPointA(temp, p1, p2);
                    next.drawLine(p1, temp, color, origin);
                }
                if (clipResult.hasSegB())
                {
                    clipResult.getPointB(temp, p1, p2);
                    next.drawLine(temp, p2, color, origin);
                }
            }
        }

        public void drawPolygon(Polygon polygon, double[] origin)
        {
            list.Clear();
            clip(polygon, bl, list);
            for (int i = 0; i < list.Count; i++) next.drawPolygon(list[i], origin);
        }
    }

    // --- static separation ---

    public static double vmin(Geom.Shape s, int axis)
    {
        double d = s.vertex[0][axis];
        for (int i = 1; i < s.vertex.Length; i++)
        {
            double temp = s.vertex[i][axis];
            if (temp < d) d = temp;
        }
        return d;
    }

    public static double vmax(Geom.Shape s, int axis)
    {
        double d = s.vertex[0][axis];
        for (int i = 1; i < s.vertex.Length; i++)
        {
            double temp = s.vertex[i][axis];
            if (temp > d) d = temp;
        }
        return d;
    }

    public static double nmin(Geom.Shape s, double[] normal)
    {
        double d = Vec.dot(s.vertex[0], normal);
        for (int i = 1; i < s.vertex.Length; i++)
        {
            double temp = Vec.dot(s.vertex[i], normal);
            if (temp < d) d = temp;
        }
        return d;
    }

    public static double nmax(Geom.Shape s, double[] normal)
    {
        double d = Vec.dot(s.vertex[0], normal);
        for (int i = 1; i < s.vertex.Length; i++)
        {
            double temp = Vec.dot(s.vertex[i], normal);
            if (temp > d) d = temp;
        }
        return d;
    }

    public const double overlap = 0.000001;
    // allow a teeny bit of overlap, otherwise it'll be too hard
    // to move shapes until they're touching in non-aligned mode

    public static Geom.Separator tryNormal(Geom.Shape s1, Geom.Shape s2, double[] normal)
    {

        double nmin1 = nmin(s1, normal);
        double nmax1 = nmax(s1, normal);
        double nmin2 = nmin(s2, normal);
        double nmax2 = nmax(s2, normal);

        if (nmin2 - nmax1 >= -overlap) return new Geom.NormalSeparator(normal, nmax1, nmin2, 1);
        if (nmin1 - nmax2 >= -overlap) return new Geom.NormalSeparator(normal, nmax2, nmin1, -1);

        return null;
    }

    public static Geom.Separator tryCellNormals(Geom.Shape shape, Geom.Shape s1, Geom.Shape s2, double[] normal)
    {
        Geom.Separator sep;

        for (int i = 0; i < shape.cell.Length; i++)
        {
            if (shape.cell[i].normal == null) continue;

            Vec.copy(normal, shape.cell[i].normal);
            // could maybe share the vector
            // with the shape face, but it seems flaky

            sep = tryNormal(s1, s2, normal);
            if (sep != null) return sep;
        }

        return null;
    }

    /**
     * @param any True if you just want any separator to show that there's no collision.
     */
    public static Geom.Separator staticSeparate(Geom.Shape s1, Geom.Shape s2, bool any)
    {
        Geom.Separator sep = null;

        // try separating along axis between centers.
        // if that works it's probably the best plan.

        double[] normal = new double[4]; // must be new so we can hand off to separator object
        Vec.sub(normal, s2.shapecenter, s1.shapecenter);

        // if the normal vector is near zero it's no good
        const double epsilon = 0.000001;
        if (Vec.dot(normal, normal) > epsilon)
        {

            sep = tryNormal(s1, s2, normal);
            if (sep != null) return sep;
        }

        // search along axes for one that gives greatest distance

        double q; // quality of separator
        double qsep = 0;

        for (int axis = 0; axis < 4; axis++)
        {

            double vmin1 = vmin(s1, axis);
            double vmax1 = vmax(s1, axis);
            double vmin2 = vmin(s2, axis);
            double vmax2 = vmax(s2, axis);

            q = vmin2 - vmax1; // 2 above 1
            if (q >= -overlap && (sep == null || q > qsep))
            {
                Vec.unitVector(normal, axis);
                sep = new Geom.NormalSeparator(normal, vmax1, vmin2, 1);
                qsep = q;
                if (any) return sep;
            }

            q = vmin1 - vmax2; // 1 above 2
            if (q >= -overlap && (sep == null || q > qsep))
            {
                Vec.unitVector(normal, axis);
                sep = new Geom.NormalSeparator(normal, vmax2, vmin1, -1);
                qsep = q;
                if (any) return sep;
            }
        }

        if (sep != null) return sep;

        // try face normals

        // this seems like it ought to be true collision detection,
        // but it's not - think of the example of cubes with edges
        // touching crosswise.  but, it's pretty good!

        sep = tryCellNormals(s1, s1, s2, normal);
        if (sep != null) return sep;

        sep = tryCellNormals(s2, s1, s2, normal);
        if (sep != null) return sep;

        // can't find separator

        // System.out.println("Unable to find separator for shapes " + is1 + " and " + is2 + ".");
        return Geom.nullSeparator;
    }

    public static bool isSeparated(Geom.Shape s1, Geom.Shape s2, GJKTester gjk)
    {

        // super-fast test, even better than the test in dynamicSeparate
        double d = s1.radius + s2.radius;
        if (Vec.dist2(s1.shapecenter, s2.shapecenter) >= d * d) return true;

        // if we get here, the spheres are in contact, so dynamicSeparate
        // will also fail and any separator we find would be useful there.
        // I thought about setting up some communication, but no, it'd be
        // too fragile.  for example: we might find some separators and then
        // have to cancel the motion because of something else.  we might
        // find a separator for a railcar, but railcars haven't animated yet.
        // it's just not worth getting into it.

        //return (gjk.separate(s1, s2) != Geom.nullSeparator);
        return (staticSeparate(s1,s2,/* any = */ true) != Geom.nullSeparator);
    }

    public static bool isSeparated(Geom.Shape shape, List<Geom.Shape> list, GJKTester gjk)
    {
        foreach (Geom.Shape s in list)
        {
            if (!isSeparated(shape, s, gjk)) return false;
        }
        return true;
    }

    // --- dynamic separation ---

    /**
     * Do some geometry with spheres and the origin to quickly catch
     * a bunch of cases where neither shape is in front of the other.
     * It's quick because there's no iterating over vertices.
     *
     * @param normal A register, just with a different name for readability.
     * @return A value from the enumeration in Geom.Separator.
     */
    public static int dynamicSeparate(Geom.Shape s1, Geom.Shape s2, double[] origin, double[] reg1, double[] normal)
    {

        Vec.sub(normal, s2.shapecenter, s1.shapecenter);

        double d = Vec.norm(normal);
        double r1 = s1.radius;
        double r2 = s2.radius;
        if (d <= r1 + r2) return Geom.Separator.UNKNOWN; // spheres are in contact

        double ratio = r1 / (r1 + r2);
        double dist1 = d * ratio;
        Vec.addScaled(reg1, s1.shapecenter, normal, ratio); // cone point
        Vec.sub(reg1, origin, reg1);

        double adj = Vec.dot(reg1, normal) / d;
        double neg = r1 - dist1;
        double pos = d - r2 - dist1;
        if (adj >= neg && adj <= pos) return Geom.Separator.NO_FRONT;

        // calc triangle parts for vec from cone point to origin
        double hyp2 = Vec.dot(reg1, reg1);
        double adj2 = adj * adj;
        double opp2 = hyp2 - adj2;

        // the cone triangle has hyp = dist1 and opp = r1, so ...
        // (working with squares because sqrt is relatively slow)

        double rcone = r1 / dist1;
        if (opp2 >= hyp2 * rcone * rcone) return Geom.Separator.NO_FRONT;

        return (adj > 0) ? Geom.Separator.S2_FRONT : Geom.Separator.S1_FRONT;
        // save this for last since we want to detect NO_FRONT when possible
    }

    // --- misc ---

    // these aren't exactly clipping functions, but they solve similar geometric problems

    public static double closestApproach(double[] p, double[] origin, double[] axis, double[] reg1)
    {
        // assume axis is normalized, then here's the answer
        Vec.sub(reg1, p, origin);
        double d = Vec.dot(reg1, axis);
        Vec.addScaled(reg1, reg1, axis, -d); // subScaled
        return Vec.dot(reg1, reg1);
    }

    /**
     * A fast test that you can use before the main clip function.
     */
    public static bool outsideRadius(double[] p1, double[] p2, Geom.Shape shape)
    {
        double d = Vec.dist(p2, shape.shapecenter) - shape.radius; // distance from sphere
        if (d < 0) return false; // definitely not outside
        double s = Vec.dist(p1, p2);
        return (d >= s / 2); // check this to prevent flythroughs
                             // the furthest you can get on a flythrough is actually root(r^2+s^2)-r ~ s^2/2r,
                             // but this is supposed to be a fast test, and s/2 is so small it hardly matters.
    }

    public static bool projectToPlane(double[] dest, double[] origin, double[] viewAxis, double height)
    {
        Vec.copy(dest, origin);
        if (dest[1] < height) return false;
        if (dest[1] > height)
        {
            if (viewAxis[1] >= 0) return false; // pointing sideways or up
            Vec.addScaled(dest, dest, viewAxis, (height - dest[1]) / viewAxis[1]);
        }
        // now y = height
        return true;
    }

    // --- GJK algorithm ---

    public class GJKTester
    {

        int dim;
        Geom.Shape[] s;
        double[][] p, reg;
        int[][] v;
        double[] t, n;
        const double epsilon = 0.000001;

        public GJKTester(int dim)
        {
            this.dim = dim;
            s = new Geom.Shape[2];
            p = new double[dim + 1][];
            v = new int[dim + 1][];
            for (int i = 0; i <= dim; i++)
            {
                p[i] = new double[dim];
                v[i] = new int[2];
            }
            t = new double[2];
            n = new double[dim];
            reg = new double[dim][];
            for (int i = 0; i < reg.Length; i++)
            {
                reg[i] = new double[dim];
            }
        }

        public Geom.Separator separate(Geom.Shape s0, Geom.Shape s1)
        {
            s[0] = s0;
            s[1] = s1;
            n = new double[dim];
            Vec.sub(reg[dim - 1], s[1].shapecenter, s[0].shapecenter);
            Vec.scale(n, reg[dim - 1], 1);
            if (!Vec.normalizeTry(n, n)) return Geom.nullSeparator;
            v[0][0] = 0; v[0][1] = 0;
            minkSupport(0, 0);
            if (Vec.dot(n, p[0]) < epsilon) return new Geom.NormalSeparator(n, t[1], t[0], -1);

            Vec.sub(reg[0], p[0], reg[dim - 1]);
            //Vec.scale(n, n, -1);
            Vec.perpendicular(n, reg[0], epsilon);
            if (Vec.dot(n, p[0]) < 0) Vec.scale(n, n, -1);
            minkSupport(0, 1);
            if (Vec.dot(n, p[1]) < epsilon) return new Geom.NormalSeparator(n, t[1], t[0], -1);

            Vec.sub(reg[0], p[0], reg[dim - 1]);
            Vec.sub(reg[1], p[1], reg[dim - 1]);
            if (dim == 3) Vec.cross(n, reg[0], reg[1]);
            else Vec.perpendicular(n, reg[0], reg[1], reg[2], epsilon);
            if (Vec.dot(n, p[0]) < 0) Vec.scale(n, n, -1);
            minkSupport(0, 2);
            if (Vec.dot(n, p[2]) < epsilon) return new Geom.NormalSeparator(n, t[1], t[0], -1);

            if (dim == 4)
            {
                Vec.sub(reg[0], p[0], reg[dim - 1]);
                Vec.sub(reg[1], p[1], reg[dim - 1]);
                Vec.sub(reg[2], p[2], reg[dim - 1]);
                Vec.cross(n, reg[0], reg[1], reg[2]);
                if (Vec.dot(n, p[0]) < 0) Vec.scale(n, n, -1);
                minkSupport(0, 3);
                if (Vec.dot(n, p[3]) < epsilon) return new Geom.NormalSeparator(n, t[1], t[0], -1);
            }

            Vec.sub(reg[0], p[1], p[0]);
            Vec.sub(reg[1], p[2], p[0]);
            if (dim == 3) Vec.cross(n, reg[0], reg[1]);
            else
            {
                Vec.sub(reg[2], p[3], p[0]);
                Vec.cross(n, reg[0], reg[1], reg[2]);
            }
            Vec.normalize(n, n);
            double d = Vec.dot(n, p[0]);
            if (d > 0)
            {
                Vec.swap(p[1], p[2], reg[0]);
                int i = v[1][0]; v[1][0] = v[2][0]; v[2][0] = i;
                i = v[1][1]; v[1][1] = v[2][1]; v[2][1] = i;
                Vec.scale(n, n, -1);
            }
            minkSupport(0, dim);
            if (Vec.dot(n, p[dim]) < epsilon) return new Geom.NormalSeparator(n, t[1], t[0], -1);
            for (int count = 0; count < 20; count++)
            {
            label:
                for (int i = 0; i < dim; i++)
                {
                    int a = (i + 1) % dim;
                    int b = (i + ((dim == 3 || i % 2 == 0) ? 2 : 3)) % dim;
                    int c = (i + ((dim == 3 || i % 2 == 0) ? 3 : 2)) % dim;
                    Vec.sub(reg[0], p[a], p[dim]);
                    Vec.sub(reg[1], p[b], p[dim]);
                    if (dim == 3) Vec.cross(n, reg[0], reg[1]);
                    else
                    {
                        Vec.sub(reg[2], p[c], p[dim]);
                        Vec.cross(n, reg[0], reg[1], reg[2]);
                    }
                    Vec.normalize(n, n);
                    d = Vec.dot(n, p[dim]);
                    if (d < 0)
                    {
                        Vec.copy(p[i], p[dim]); v[i][0] = v[dim][0]; v[i][1] = v[dim][1];
                        minkSupport(i, dim);
                        if (Vec.dot(n, p[dim]) < epsilon) return new Geom.NormalSeparator(n, t[1], t[0], -1);
                        count++;
                        goto label;
                    }
                    if (d < epsilon)
                    {
                        minkSupport(dim, dim);
                        if (Vec.dot(n, p[dim]) < epsilon) return new Geom.NormalSeparator(n, t[1], t[0], -1);
                    }
                }
                return Geom.nullSeparator;
            }
            return new Geom.NormalSeparator(n, t[1], t[0], -1);
        }

        private void minkSupport(int from, int to)
        {
            support(p[to], false, from, to, 1);
            support(reg[0], true, from, to, 0);
            Vec.sub(p[to], p[to], reg[0]);
        }

        private void support(double[] dest, bool inv, int from, int to, int r)
        {
            Vec.scale(reg[1], n, (inv) ? -1 : 1);
            int next = v[from][r];
            int now = -1;
            int prev;
            double m = Vec.dot(s[r].vertex[next], reg[1]);
            while (next != now)
            {
                prev = now;
                now = next;
                foreach (int i in s[r].nbv[now])
                {
                    if (i == prev) continue;
                    double d = Vec.dot(s[r].vertex[i], reg[1]);
                    if (d > m)
                    {
                        m = d;
                        next = i;
                    }
                }
            }
            Vec.copy(dest, s[r].vertex[now]);
            t[r] = (inv) ? -m : m;
            v[to][r] = now;
        }
    }

}
