using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    public double[][] vertex;
    public Color color;
    public double dist;

    public Polygon() { }

    public Polygon(double[][] vertex, Color color)
    {
        this.vertex = vertex;
        this.color = color;
    }

    public void copy(Polygon polygon)
    {
        this.vertex = Geom.clone2(polygon.vertex);
        this.color = polygon.color;
    }
}
