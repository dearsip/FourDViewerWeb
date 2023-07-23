using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * An object that takes a set of lines oriented with respect to absolute coordinates
 * and converts them to relative coordinates, i.e., projects them onto a retina.
 */
public class RenderRelative
{
    // --- fields ---

    private PolygonBuffer bin;
    private PolygonBuffer bout;
    private int dim;

    private double retina;
    private Clip.CustomBoundary[] clip;
    private Polygon reg;

    // --- construction ---

    public RenderRelative(PolygonBuffer bin, PolygonBuffer bout, int dim, double retina)
    {
        this.bin = bin;
        this.bout = bout;
        this.dim = dim;

        clip = new Clip.CustomBoundary[2 * (dim - 1)];
        reg = new Polygon();

        setRetina(retina);
    }

    // --- options ---

    private double orthoRetina = 2;
    public void setRetina(double retina)
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

    public double getRetina() { return retina; }
    public double getOrtho() { return orthoRetina; }

    // --- processing ---

    // the call to projectRetina could cause division by zero
    // if the line being projected started or ended on the parallel plane through the origin.
    // that's not a problem here.
    // the clipping planes restrict the lines to a forward cone,
    // so the origin is the only dangerous point,
    // and we know there are no lines through the origin
    // because we're not allowed to move onto the walls.

    private bool convert(Polygon dest, Polygon src, double[][] axis, bool viewClip)
    {
        reg.vertex = new double[src.vertex.Length][];
        for (int i = 0; i < src.vertex.Length; i++)
        {
            reg.vertex[i] = new double[dim];
            Vec.toAxisCoordinates(reg.vertex[i], src.vertex[i], axis);
        }
        if (viewClip) {
            if (reg.vertex.Length == 2)
            {
                for (int i = 0; i < clip.Length; i++)
                {
                    if (Vec.clip(reg.vertex[0], reg.vertex[1], clip[i].n, clip[i].getThreshold(), 1)) return false;
                }
            }
            else
            {
                for (int i = 0; i < clip.Length; i++)
                {
                    if (Clip.clip(reg, clip[i])) return false;
                }
            }
        }

        dest.vertex = new double[reg.vertex.Length][];
        if (retina > 0)
        for (int i = 0; i < reg.vertex.Length; i++)
        {
            dest.vertex[i] = new double[dim-1];
            Vec.projectRetina(dest.vertex[i], reg.vertex[i], retina);
        }
        else
        for (int i = 0; i < reg.vertex.Length; i++)
        {
            dest.vertex[i] = new double[dim-1];
            Vec.projectOrtho(dest.vertex[i], reg.vertex[i], orthoRetina);
        }

        dest.color = src.color;
        return true;
    }

    public void run(double[][] axis, bool viewClip)
    {
        bout.clear();
        for (int i = 0; i <bin.getSize(); i++)
        {
            Polygon src = bin.get(i);
            Polygon dest = bout.getNext();

            if (!convert(dest, src, axis, viewClip)) bout.unget();
        }
    }

    public void run(double[][] axis, bool clear, PointTransform pt, bool viewClip)
    {
        if (clear) bout.clear();
        for (int i = 0; i <bin.getSize(); i++)
        {
            Polygon src = bin.get(i);
            Polygon dest = bout.getNext();

            if (convert(dest, src, axis, viewClip))
            {
                foreach (double[] v in dest.vertex) pt.transform(v);
            }
            else
            {
            bout.unget();
            }
        }
    }

    public void runObject(double[][] obj, int mask, PointTransform pt, Color color)
    {
        // no need for clear here
        for (int i = 0; i < obj.Length; i += 2)
        {
            if ((mask & 1) == 1)
            {
                Polygon dest = bout.getNext();

                dest.vertex = new double[2][];
                for (int j = 0; j < 2; j++)
                {
                    dest.vertex[j] = new double[dim-1];
                    Vec.copy(dest.vertex[j], obj[i + j]);
                    pt.transform(dest.vertex[j]);
                }
                dest.color = color;
            }
            mask >>= 1;
        }
    }

    public void runPolygon(double[][] obj, int mask, PointTransform pt, int n, Color color)
    {
        Color c = color;
        color.a = 0f;
        Polygon dest = null;
        for (int i = 0; i < obj.Length; i += n)
        {
            if ((mask & 1) == 1)
            {
                dest = bout.getNext();
                dest.vertex = new double[n][];
                for (int j = 0; j < n; j++)
                {
                    dest.vertex[j] = new double[dim-1];
                    Vec.copy(dest.vertex[j], obj[i + j]);
                    pt.transform(dest.vertex[j]);
                }
                dest.color = color;
            }
            mask >>= 1;
        }
    }
}
