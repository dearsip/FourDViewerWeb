using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * A model that lets the user move around geometric shapes.
 */
public class Vec
{
    // --- non-mathematical operations ---

    public static void zero(double[] dest)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = 0;
        }
    }

    public static void copy(double[] dest, double[] src)
    {
        // could use System.arraycopy
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src[i];
        }
    }

    public static void swap(double[] p1, double[] p2, double[] reg)
    {
        copy(reg, p1);
        copy(p1, p2);
        copy(p2, reg);
    }

    public static void copyMatrix(double[][] dest, double[][] src)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            Vec.copy(dest[i], src[i]);
        }
    }

    // equals not recommended for double because of FP error
    // but this is OK
    public static bool approximatelyEquals(double[] p1, double[] p2, double epsilon)
    {
        for (int i = 0; i < p1.Length; i++)
        {
            if (Math.Abs(p1[i] - p2[i]) >= epsilon) return false;
        }
        return true;
    }

    public static bool exactlyEquals(double[] p1, double[] p2)
    {
        for (int i = 0; i < p1.Length; i++)
        {
            if (p1[i] != p2[i]) return false;
        }
        return true;
    }

    // --- unit vectors ---

    public static void unitVector(double[] dest, int a)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = (i == a) ? 1 : 0;
        }
    }

    public static void unitMatrix(double[][] dest)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            unitVector(dest[i], i);
        }
    }

    // --- simple arithmetic ---

    public static void add(double[] dest, double[] src1, double[] src2)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src1[i] + src2[i];
        }
    }

    public static void sub(double[] dest, double[] src1, double[] src2)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src1[i] - src2[i];
        }
    }

    public static void scale(double[] dest, double[] src, double scale)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = scale * src[i];
        }
    }

    public static void addScaled(double[] dest, double[] src1, double[] src2, double scale)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src1[i] + scale * src2[i];
        }
    }

    public static void addScaledFloat(double[] dest, double[] src1, float[] src2, double scale)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src1[i] + scale * src2[i];
        }
    }

    public static void scaleMultiCo(double[] dest, double[] src, double[] scale)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src[i] * scale[i];
        }
    }

    public static void scaleMultiContra(double[] dest, double[] src, double[] scale)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src[i] / scale[i];
        }
    }

    // --- vector arithmetic ---

    public static double dot(double[] p1, double[] p2)
    {
        double sum = 0;
        for (int i = 0; i < p1.Length; i++)
        {
            sum += p1[i] * p2[i];
        }
        return sum;
    }

    public static double norm2(double[] p)
    {
        return dot(p, p);
    }

    public static double norm(double[] p)
    {
        return Math.Sqrt(dot(p, p));
    }

    public static void normalize(double[] dest, double[] src)
    {
        scale(dest, src, 1 / norm(src));
    }

    /**
     * @return False if the vector is zero.
     */
    public static bool normalizeTry(double[] dest, double[] src)
    {
        double d = norm(src);
        if (d == 0) return false;
        scale(dest, src, 1 / d);
        return true;
    }

    public static double dist2(double[] p1, double[] p2)
    {
        double sum = 0;
        for (int i = 0; i < p1.Length; i++)
        {
            double d = p1[i] - p2[i];
            sum += d * d;
        }
        return sum;
    }

    public static double dist(double[] p1, double[] p2)
    {
        return Math.Sqrt(dist2(p1, p2));
    }

    public static void perpendicular(double[] dest, double[] p, double epsilon)
    {
        double d = 1;
        for (int i = 0; i < dest.Length; i++)
        {
            unitVector(dest, i);
            addScaled(dest, dest, p, -dot(p, dest) / norm2(p));
            d = norm(dest);
            if (d > epsilon) break;
        }
        scale(dest, dest, 1 / d);
    }

    public static void perpendicular(double[] dest, double[] p1, double[] p2, double[] reg, double epsilon)
    {
        double d = 1;
        double d1 = norm2(p1);
        addScaled(reg, p2, p1, -dot(p1, p2) / d1);
        double d2 = norm2(reg);
        for (int i = 0; i < dest.Length; i++)
        {
            unitVector(dest, i);
            addScaled(dest, dest, p1, -dot(p1, dest) / d1);
            addScaled(dest, dest, reg, -dot(reg, dest) / d2);
            d = norm(dest);
            if (d > epsilon) break;
        }
        scale(dest, dest, 1 / d);
    }

    public static void cross(double[] dest, double[] p1, double[] p2)
    {
        dest[0] = p1[1] * p2[2] - p1[2] * p2[1];
        dest[1] = p1[2] * p2[0] - p1[0] * p2[2];
        dest[2] = p1[0] * p2[1] - p1[1] * p2[0];
    }

    public static void cross(double[] dest, double[] p1, double[] p2, double[] p3)
    {

        dest[0] = p1[1] * p2[2] * p3[3]
                  + p1[3] * p2[1] * p3[2]
                  + p1[2] * p2[3] * p3[1]
                  - p1[3] * p2[2] * p3[1]
                  - p1[1] * p2[3] * p3[2]
                  - p1[2] * p2[1] * p3[3];
        dest[1] = -p1[2] * p2[3] * p3[0]
                  - p1[3] * p2[0] * p3[2]
                  - p1[0] * p2[2] * p3[3]
                  + p1[0] * p2[3] * p3[2]
                  + p1[2] * p2[0] * p3[3]
                  + p1[3] * p2[2] * p3[0];
        dest[2] = p1[3] * p2[0] * p3[1]
                  + p1[0] * p2[1] * p3[3]
                  + p1[1] * p2[3] * p3[0]
                  - p1[1] * p2[0] * p3[3]
                  - p1[3] * p2[1] * p3[0]
                  - p1[0] * p2[3] * p3[1];
        dest[3] = -p1[0] * p2[1] * p3[2]
                  - p1[1] * p2[2] * p3[0]
                  - p1[2] * p2[0] * p3[1]
                  + p1[2] * p2[1] * p3[0]
                  + p1[0] * p2[2] * p3[1]
                  + p1[1] * p2[0] * p3[2];
    }

    // --- rotation ---

    /**
     * Rotate src1 toward src2 using the given cosine and sine.
     * The vectors src1 and src2 should be orthogonal and have the same Length.
     */
    public static void rotateCosSin(double[] dest1, double[] dest2, double[] src1, double[] src2, double cos, double sin)
    {
        for (int i = 0; i < dest1.Length; i++)
        {
            double s1 = src1[i];
            double s2 = src2[i];
            dest1[i] = cos * s1 + sin * s2;
            dest2[i] = cos * s2 - sin * s1;
        }
    }

    /**
     * Rotate src1 toward src2 by theta degrees.
     * The vectors src1 and src2 should be orthogonal and have the same Length.
     */
    public static void rotateAngle(double[] dest1, double[] dest2, double[] src1, double[] src2, double theta)
    {
        theta *= Math.PI / 180;
        rotateCosSin(dest1, dest2, src1, src2, Math.Cos(theta), Math.Sin(theta));
    }

    /**
     * Rotate src1 toward src2 so that src1 points at the point with coordinates (x1,x2).
     * The vectors src1 and src2 should be orthogonal and have the same Length.
     */
    public static void rotatePoint(double[] dest1, double[] dest2, double[] src1, double[] src2, double x1, double x2)
    {
        double r = Math.Sqrt(x1 * x1 + x2 * x2);
        rotateCosSin(dest1, dest2, src1, src2, x1 / r, x2 / r);
    }

    /**
     * The functions above are for shuffling axis vectors around, and therefore are
     * relative to the viewer, but the ones here and below use absolute coordinates.
     * They rotate axis a1 toward axis a2.
     */
    public static void rotateAbsoluteCosSin(double[] dest, double[] src, int a1, int a2, double cos, double sin)
    {
        copy(dest, src);
        double s1 = src[a1];
        double s2 = src[a2];
        dest[a1] = cos * s1 - sin * s2; // yes, sign is different than in rotateCosSin
        dest[a2] = cos * s2 + sin * s1;
    }

    public static void rotateAbsoluteAngle(double[] dest, double[] src, int a1, int a2, double theta)
    {

        // make sure multiples of 90 degrees are exact so that for example bottom
        // faces will still be detectable for railcars.  this isn't needed for
        // in-game operations since those rotations are normally through small angles.
        int temp = (int)theta;
        bool exact = (temp == theta && temp % 90 == 0);

        theta *= Math.PI / 180;
        double cos = Math.Cos(theta);
        double sin = Math.Sin(theta);

        if (exact)
        {
            cos = Math.Round(cos);
            sin = Math.Round(sin);
        }
        // maybe not the best way but it'll do

        rotateAbsoluteCosSin(dest, src, a1, a2, cos, sin);
    }

    public static void rotateAbsoluteAngleDir(double[] dest, double[] src, int dir1, int dir2, double theta)
    {
        int a1 = Dir.getAxis(dir1);
        int a2 = Dir.getAxis(dir2);
        if (Dir.isPositive(dir1) != Dir.isPositive(dir2)) theta = -theta;
        rotateAbsoluteAngle(dest, src, a1, a2, theta);
    }

    // --- projection ---

    /**
     * Project a vector onto a screen at a given distance,
     * using lines radiating from the origin.
     * The result vector has lower dimension than the original.
     */
    public static void projectDistance(double[] dest, double[] src, double distance)
    {
        double scale = distance / src[dest.Length];

        // same as scale, but with different-sized arrays
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = scale * src[i];
        }
    }

    /**
     * Project a vector onto a retina at distance 1,
     * using lines radiating from the origin,
     * and then scale so the retina has size 1.
     * The result vector has lower dimension than the original.
     */
    public static void projectRetina(double[] dest, double[] src, double retina)
    {
        projectDistance(dest, src, 1 / retina);
    }

    public const int PROJ_NONE = 0;
    public const int PROJ_NORMAL = 1;
    public const int PROJ_ORTHO = 2; // value is vector
    public const int PROJ_PERSPEC = 3; // value is point

    public static void projectOrtho(double[] dest, double[] src, double retina)
    {
        double scale = 1 / retina;
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = scale * src[i];
        }
    }

    /**
     * Project into a plane without reducing the dimension.
     */
    public static void project(double[] dest, double[] src, double[] normal, double threshold, int mode, double[] value)
    {
        double a, sn, vn;
        switch (mode)
        {
            case PROJ_NORMAL:
                value = normal;
                goto case PROJ_ORTHO;
            // fall through
            case PROJ_ORTHO:
                sn = dot(src, normal);
                vn = dot(value, normal);
                a = (threshold - sn) / vn;
                addScaled(dest, src, value, a);
                break;
            case PROJ_PERSPEC:
                // this is the same as an orthographic projection
                // with value = value-src, but I can't find a way
                // to unify the paths without a register.
                sn = dot(src, normal);
                vn = dot(value, normal);
                a = (threshold - sn) / (vn - sn);
                // careful, this has to work when dest equals src
                scale(dest, src, 1 - a);
                addScaled(dest, dest, value, a);
                break;
            default: // PROJ_NONE
                break;
        }
    }

    public static void project(double[] dest, double[] src, double[] normal)
    {
        project(dest, src, normal, 0, PROJ_ORTHO, normal);
    }

    // --- coordinate conversion ---

    /**
     * Express a vector in terms of a set of axes.
     * The vectors src and dest must be different objects.
     */
    public static void toAxisCoordinates(double[] dest, double[] src, double[][] axis)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = dot(axis[i], src);
        }
    }

    /**
     * Take a vector expressed in terms of a set of axes
     * and convert it back to the original coordinate system.
     * The vectors src and dest must be different objects.
     */
    public static void fromAxisCoordinates(double[] dest, double[] src, double[][] axis)
    {
        zero(dest);
        for (int i = 0; i < dest.Length; i++)
        {
            addScaled(dest, dest, axis[i], src[i]);
        }
    }

    // --- clipping ---

    /**
     * Find the point that is a fraction f of the way from src1 to src2.
     */
    public static void mid(double[] dest, double[] src1, double[] src2, double f)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src1[i] + f * (src2[i] - src1[i]);
        }
    }

    /**
     * Clip the line from p1 to p2 into the half-space that the vector n points into.
     * The vector n does not need to be normalized.<p>
     * Since clipping often does nothing, the vectors p1 and p2 are modified in place.
     *
     * @return True if the line is entirely removed by clipping.
     */
    public static bool clip(double[] p1, double[] p2, double[] n)
    {
        return clip(p1, p2, n,/* t = */ 0,/* sign = */ 1);
    }
    public static bool clip(double[] p1, double[] p2, double[] n, double t, double sign)
    {

        double d1 = sign * (dot(p1, n) - t);
        double d2 = sign * (dot(p2, n) - t);

        if (d1 >= 0)
        {
            if (d2 >= 0)
            {
                // not clipped
            }
            else
            {
                mid(p2, p2, p1, d2 / (d2 - d1));
            }
        }
        else
        {
            if (d2 >= 0)
            {
                mid(p1, p1, p2, d1 / (d1 - d2));
            }
            else
            {
                // completely clipped
                return true;
            }
        }

        return false;
    }

    // --- random ---

    public static void randomNormalized(double[] dest, System.Random random)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = 2 * random.NextDouble() - 1;
        }
        if (!normalizeTry(dest, dest)) unitVector(dest, 0);
    }

    // normal vector needs to be normalized

    public static void reflect(double[] dest, double[] src, double[] normal, double[] reg)
    {
        scale(reg, normal, -2 * dot(src, normal));
        add(dest, reg, src);
    }

    // from & to vectors needs to be normalized
    public static void rotate(double[] dest, double[] src, double[] from, double[] to, double[] reg1, double[] reg2)
    {
        add(reg1, from, to);
        normalize(reg1, reg1);
        reflect(dest, src, from, reg2);
        reflect(dest, dest, reg1, reg2);
    }

    public static void outerProduct(double[] dest, double[] src1, double[] src2, double[] src3)
    {
        dest[0] =   src1[1] * src2[2] * src3[3]
                  + src1[2] * src2[3] * src3[1]
                  + src1[3] * src2[1] * src3[2]
                  - src1[3] * src2[2] * src3[1]
                  - src1[1] * src2[3] * src3[2]
                  - src1[2] * src2[1] * src3[3];
        dest[1] = - src1[2] * src2[3] * src3[0]
                  - src1[3] * src2[0] * src3[2]
                  - src1[0] * src2[2] * src3[3]
                  + src1[0] * src2[3] * src3[2]
                  + src1[2] * src2[0] * src3[3]
                  + src1[3] * src2[2] * src3[0];
        dest[2] =   src1[3] * src2[0] * src3[1]
                  + src1[0] * src2[1] * src3[3]
                  + src1[1] * src2[3] * src3[0]
                  - src1[1] * src2[0] * src3[3]
                  - src1[3] * src2[1] * src3[0]
                  - src1[0] * src2[3] * src3[1];
        dest[3] = - src1[0] * src2[1] * src3[2]
                  - src1[1] * src2[2] * src3[0]
                  - src1[2] * src2[0] * src3[1]
                  + src1[2] * src2[1] * src3[0]
                  + src1[0] * src2[2] * src3[1]
                  + src1[1] * src2[0] * src3[2];
    }
    
    public static String ToString(double[] dest)
    {
        String s = dest[0].ToString();
        for (int i = 1; i < dest.Length; i++)
        {
            s += ",";
            s += dest[i].ToString();
        }
        return s;
    }

    public static String ToString(int[] dest)
    {
        String s = dest[0].ToString();
        for (int i = 1; i < dest.Length; i++)
        {
            s += ",";
            s += dest[i].ToString();
        }
        return s;
    }

    public static String ToString(bool[] dest)
    {
        String s = dest[0].ToString();
        for (int i = 1; i < dest.Length; i++)
        {
            s += ",";
            s += dest[i].ToString();
        }
        return s;
    }

    public static String ToString(float[] dest)
    {
        String s = dest[0].ToString();
        for (int i = 1; i < dest.Length; i++)
        {
            s += ",";
            s += dest[i].ToString();
        }
        return s;
    }

    public static int max(int[] src)
    {
        int m = src[0];
        for (int i = 1; i < src.Length; i++) if (m < src[i]) m = src[i];
        return m;
    }

    public static double max(double[] src)
    {
        double m = src[0];
        for (int i = 1; i < src.Length; i++) if (m < src[i]) m = src[i];
        return m;
    }

    public static int min(int[] src)
    {
        int m = src[0];
        for (int i = 1; i < src.Length; i++) if (m > src[i]) m = src[i];
        return m;
    }

    public static double min(double[] src)
    {
        double m = src[0];
        for (int i = 1; i < src.Length; i++) if (m > src[i]) m = src[i];
        return m;
    }
}
