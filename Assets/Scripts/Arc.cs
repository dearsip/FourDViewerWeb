/*
 * Arc.java
 */

using System;

/**
 * Class that stores 2D arc coordinates.
 */

public class Arc
{

    // the point here is to store coordinates so that I don't have
    // to do as much math at runtime, especially cosines and sines.
    // they're not stored as 3D / 4D points because we permute them
    // around to different coordinates depending on the direction.

    // also note, for train motion we don't follow the arc segments,
    // we use the idealized curved path.

    public int n;
    public double[] u;
    public double[] v;

    private Arc(int n)
    {
        this.n = n;
        u = new double[n];
        v = new double[n];
    }

    /**
     * Make a standard curve from (offset,0) to (0,offset).
     * @param mflag True to include extra vertices if margin is nonzero.
     */
    public static Arc curve(int arcn, bool mflag, double margin, double offset)
    {

        if (margin == 0) mflag = false; // now mflag means include extra vertices

        Arc a = new Arc(arcn + (mflag ? 3 : 1));
        int n = 0;

        if (mflag)
        {
            a.u[n] = offset;
            a.v[n] = 0;
            n++;
        }

        double r = offset - margin;
        double theta = Math.PI / (2 * arcn);

        for (int i = 0; i <= arcn; i++)
        {
            a.u[n] = margin + r * Math.Cos(theta * i);
            a.v[n] = margin + r * Math.Sin(theta * i);
            n++;
        }

        if (mflag)
        {
            a.u[n] = 0;
            a.v[n] = offset;
            n++;
        }

        return a;
    }

    /**
     * Make a standard helix from -45 degrees to +45 degrees around (0,0).
     */
    public static Arc helix(int arcn, double width)
    {
        Arc a = new Arc(arcn + 1);

        double r = width / Math.Sqrt(2);
        double theta = Math.PI / (2 * arcn);
        double start = -Math.PI / 4;

        for (int i = 0; i <= arcn; i++)
        {
            a.u[i] = r * Math.Cos(start + theta * i);
            a.v[i] = r * Math.Sin(start + theta * i);
        }

        return a;
    }

}

