using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * An interface for drawing lines.
 */
// 描画対象は Polygon に変更している。
public interface IDraw
{
    void drawPolygon(Polygon polygon, double[] origin);

    void drawLine(double[] p1, double[] p2, Color color, double[] origin);
}