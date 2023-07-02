/*
 * SceneryBase.java
 */

/**
 * Base class for scenery objects.
 */
using UnityEngine;
using System.Collections.Generic;

public abstract class SceneryBase : IScenery
{

    protected double[] reg;
    protected List<double[]> tList;
    protected List<Color> cList;
    protected double[] origin;

    protected double[] reg0;
    protected double[] reg1;
    protected double[] reg2;

    public SceneryBase(int dim)
    {
        reg0 = new double[dim];
        reg1 = new double[dim];
        reg2 = new double[dim];
        tList = new List<double[]>();
        cList = new List<Color>();
    }

    public void draw(out double[][] texture, out Color[] textureColor, double[] origin)
    {
        this.origin = origin;
        // so we don't have to pass them around everywhere
        draw(out texture, out textureColor);
    }

    protected void drawLine(double[] p1, double[] p2, Color color) {
        reg = new double[p1.Length]; Vec.copy(reg, p1); tList.Add(reg);
        reg = new double[p1.Length]; Vec.copy(reg, p2); tList.Add(reg);
        cList.Add(color);
    }
    protected abstract void draw(out double[][] texture, out Color[] textureColor);

}

