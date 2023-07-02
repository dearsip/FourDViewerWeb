using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/**
 * A line buffer that reuses line objects to avoid memory allocation.
 */
// 描画対象は Polygon に変更している。
public class PolygonBuffer : IDraw
{
    private int dim;
    private List<Polygon> polygons;
    private int size;
    private IComparer<Polygon> comparer;

    public PolygonBuffer(int dim)
    {
        this.dim = dim;
        polygons = new List<Polygon>();
        size = 0;
        comparer = new PolygonComparer();
    }

    public void clear()
    {
        size = 0;
    }

    public Polygon getNext()
    {
        Polygon polygon;

        if (size < polygons.Count)
        {
            polygon = (Polygon)polygons[size];
        }
        else
        {
            polygon = new Polygon();
            polygons.Add(polygon);
        }

        size++;
        return polygon;
    }

    public void unget()
    {
        size--;
    }

    public void add(Polygon poly)
    {
        Polygon polygon = getNext();
        polygon.copy(poly);
    }

    public void add(PolygonBuffer buf)
    {
        for (int i = 0; i < buf.getSize(); i++)
        {
            Polygon polygon = getNext();
            polygon.copy(buf.get(i));
        }
    }

    public void add(double[] p1, double[] p2, Color color)
    {
        Polygon polygon = getNext();

        polygon.vertex = new double[2][];
        polygon.vertex[0] = new double[dim];
        polygon.vertex[1] = new double[dim];
        Array.Copy(p1, 0, polygon.vertex[0], 0, dim);
        Array.Copy(p2, 0, polygon.vertex[1], 0, dim);
        polygon.color = color;
    }

    public void drawPolygon(Polygon face, double[] origin)
    {
        Polygon polygon = getNext();
        polygon.copy(face);
        for (int i = 0; i < polygon.vertex.Length; i++) Vec.sub(polygon.vertex[i], polygon.vertex[i], origin);
    }

    public void drawLine(double[] p1, double[] p2, Color color, double[] origin)
    {
        Polygon poly = getNext();
        poly.vertex = new double[2][];
        poly.vertex[0] = new double[p1.Length];
        poly.vertex[1] = new double[p1.Length];
        Vec.copy(poly.vertex[0], p1);
        Vec.copy(poly.vertex[1], p2);
        poly.color = color;
        for (int i = 0; i < poly.vertex.Length; i++) Vec.sub(poly.vertex[i], poly.vertex[i], origin);
    }

    public int getSize() { return size; }

    public Polygon get(int i)
    {
        return (Polygon)polygons[i];
    }

    public void sort(double[] eyeVector)
    {
        double[] sum = new double[3];
        for (int i = 0; i < size; i++)
        {
            Vec.copy(sum, polygons[i].vertex[0]);
            for (int j = 1; j < polygons[i].vertex.Length; j++) Vec.add(sum, sum, polygons[i].vertex[j]);
            polygons[i].dist = Vec.dot(sum, eyeVector) / polygons[i].vertex.Length;
        }
        polygons.Sort(0, size, comparer);
    }

    class PolygonComparer : IComparer<Polygon>
    {
        public int Compare(Polygon x, Polygon y)
        {
            return (x.dist - y.dist > 0) ? -1 : 1;
        }
    }
}