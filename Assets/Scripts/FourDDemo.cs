using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/**
 * A model that lets the user move around geometric shapes.
 */
// 元のコードの GeomModel（特にその中の render(..)） を主体とした、プログラムの本体。

public class FourDDemo
{
    public bool error = true;

    private Geom.Shape[][] shapelist;
    private List<int> randomShape;
    private System.Random random;

    private Geom.Shape[] shapes;
    private bool[][] inFromt;
    private PolygonBuffer buf;
    private IDraw currentDraw;
    private PolygonBuffer bufRelative;
    private RenderRelative renderRelative;
    private Clip.Draw[] clipUnits;
    private bool[][] inFront;
    private Geom.Separator[][] separators;
    private Clip.Result clipResult;

    private double[] origin;
    private double[][] axis;
    private double[] zero = new double[] { 0, 0, 0, 0 };
    private double[] reg0, reg1, reg2, reg3, reg4, reg5, reg6, reg7, reg8;

    private List<Vector3> verts;
    private List<int> tris;
    private List<Color> cols;

    private int shapeNum = 1;
    private int colorNum = 1;

    public bool sliceMode;
    public bool drEdge;
    public bool drHdnEdge;
    public bool drFace;
    public bool drCell;
    public bool invertNormals;
    private float cellAlpha = 0.4f;
    private Color faceColor = Color.white;
    private Color edgeColor = Color.white;
    private double offset = 0.7;
    private double border = -1;
    private int[][] colors;
    private int colorVal;
    private int selectedShape = -1;
    private int selectedCell = -1;

    private static double PHI = (1 + Math.Sqrt(5)) / 2;
    private double[][] spinv = new double[][]
    {
        new double[] {     0,     0,  1, -1 },
        new double[] { PHI-1, PHI+1,  0,  0 },
        new double[] {     0,     0,  1,  1 },
        new double[] { 1+PHI, 1-PHI,  0,  0 }
    };
    private double count = 0;

    public FourDDemo()
    {
        origin = new double[] { 0, 0, 0, -30 };
        reg0 = new double[4];
        reg1 = new double[4];
        reg2 = new double[4];
        reg3 = new double[4];
        reg4 = new double[3];
        reg5 = new double[3];
        reg6 = new double[3];
        reg7 = new double[3];
        reg8 = new double[3];
        axis = new double[4][];
        for (int i = 0; i < 4; i++) axis[i] = new double[4];
        Vec.unitMatrix(axis);
        drEdge = true;
        drHdnEdge = false;
        drFace = true;
        drCell = true;
        invertNormals = false;
        sliceMode = false;

        initShape();
        setShape();
        buf = new PolygonBuffer(4);
        bufRelative = new PolygonBuffer(3);
        renderRelative = new RenderRelative(buf, bufRelative, 4, 2.0/30.0);
        clipResult = new Clip.Result();

        faceColor.a = 0.1f;

        verts = new List<Vector3>();
        tris = new List<int>();
        cols = new List<Color>();

        for (int i = 0; i < spinv.Length; i++) Vec.normalize(spinv[i], spinv[i]);
    }

    private void initShape()
    {
        ShapeBuilder sb = new ShapeBuilder();
        shapelist = new Geom.Shape[][]
        {
            new Geom.Shape[] { Shapes.cell_5() },
            new Geom.Shape[] { Shapes.cell_8() },
            //new Geom.Shape[2],
            new Geom.Shape[] { Shapes.cell_16() },
            new Geom.Shape[] { Shapes.cell_24(sb) },
            new Geom.Shape[] { Shapes.cell_120(sb) },
            new Geom.Shape[] { Shapes.cell_600(sb) },
            new Geom.Shape[7],
            new Geom.Shape[7],
            new Geom.Shape[] { Shapes.convex(sb) },
            new Geom.Shape[] { Shapes.cell_8() },
            new Geom.Shape[] { Shapes.cell_8() },
            new Geom.Shape[] { Shapes.cell_8() },
            new Geom.Shape[] { Shapes.cell_8() },
            new Geom.Shape[] { Shapes.cell_8() },
            new Geom.Shape[] { Shapes.cell_8() },
        };
        //shapelist[1][0] = Shapes.cell_8();
        //shapelist[1][1] = Shapes.cell_5();
        //shapelist[1][0].scale(new double[] { .6,.6,.6,.6 });
        //shapelist[1][1].scale(new double[] { .6,.6,.6,.6 });
        //shapelist[1][0].translate(new double[] { .8, 0, 0, 0 });
        //shapelist[1][1].translate(new double[] { -.8, 0, 0, 0 });
        //shapelist[1][0].calculate();
        //shapelist[1][1].calculate();

        shapelist[6][0] = Shapes.cone(sb);
        for (int i = 1; i < 7; i++) shapelist[6][i] = shapelist[6][0].copy();
            //Vec.unitVector(reg0, 3);
            //Vec.scale(reg0, reg0, 0.1);
            //shapelist[6][0].translateFrame(reg0);
            //shapelist[6][0].place();
        Vec.unitVector(reg1, 3);
        for (int i = 0; i < 6; i++)
        {
            Vec.unitVector(reg0, i / 2);
            if (i % 2 != 0) Vec.scale(reg0, reg0, -1);
            shapelist[6][i + 1].rotateFrame(reg1, reg0, zero, reg2, reg3);
            //Vec.scale(reg0, reg0, 0.2);
            //shapelist[6][i + 1].translateFrame(reg0);
            shapelist[6][i + 1].place();
            Vec.unitMatrix(shapelist[6][i + 1].axis);
        }
        for (int i = 1; i < 7; i++) shapelist[6][i].ideal = shapelist[6][i].copy();

        shapelist[7][0] = Shapes.flat(sb);
        //for (int i = 1; i < 6; i++) shapelist[7][i] = shapelist[7][0].copy();
        //    //Vec.unitVector(reg0, 0);
        //    //Vec.scale(reg0, reg0, 0.1);
        //    //shapelist[6][0].translateFrame(reg0);
        //    //shapelist[6][0].place();
        //Vec.unitVector(reg1, 0);
        //for (int i = 0; i < 4; i++)
        //{
        //    Vec.unitVector(reg0, i / 2 + 1);
        //    if (i % 2 != 0) Vec.scale(reg0, reg0, -1);
        //    shapelist[7][i + 1].rotateFrame(reg1, reg0, zero, reg2, reg3);
        //    //Vec.scale(reg0, reg0, 0.2);
        //    //shapelist[7][i + 1].translateFrame(reg0);
        //    shapelist[7][i + 1].place();
        //    Vec.unitMatrix(shapelist[7][i + 1].axis);
        //}
        //for (int i = 0; i < 2; i++) shapelist[7][5].rotateFrame(reg1, reg0, zero, reg2, reg3);
        //    //Vec.unitVector(reg0, 0);
        //    //Vec.scale(reg0, reg0, -0.1);
        //    //shapelist[7][5].translateFrame(reg0);
        //shapelist[7][5].place();
        //Vec.unitMatrix(shapelist[7][5].axis);
        //for (int i = 1; i < 6; i++) shapelist[7][i].ideal = shapelist[7][i].copy();
        //shapelist[7][6] = shapelist[6][0].copy();
        ////Vec.unitVector(reg0, 3);
        ////Vec.scale(reg0, reg0, 0.1);
        ////shapelist[7][6].translateFrame(reg0);
        ////shapelist[7][6].place();

        ////Debug.Log(shapelist[7][0].vertex.Length + " vertices " + shapelist[7][0].edge.Length + " edges " + shapelist[7][0].face.Length + " faces " + shapelist[7][0].cell.Length + " cells");
        ////for (int i = 0; i < shapelist[7][0].edge.Length; i++) Debug.Log(shapelist[7][0].edge[i].iv1 + ", " + shapelist[7][0].edge[i].iv2);
        //foreach (Geom.Cell cell in shapelist[7][0].cell) Debug.Log(Vec.ToString(cell.normal));
        //foreach (Geom.Cell cell in shapelist[7][1].cell) Debug.Log(Vec.ToString(cell.normal));
        shapelist[6][1] = Shapes.cone1(sb);
        shapelist[6][2] = Shapes.cone2(sb);
        shapelist[6][3] = Shapes.cone3(sb);
        shapelist[6][4] = Shapes.cone4(sb);
        shapelist[6][5] = Shapes.cone5(sb);
        shapelist[6][6] = Shapes.cone6(sb);
        shapelist[7][1] = Shapes.flat1(sb);
        shapelist[7][2] = Shapes.flat2(sb);
        shapelist[7][3] = Shapes.flat3(sb);
        shapelist[7][4] = Shapes.flat4(sb);
        shapelist[7][5] = Shapes.flat5(sb);
        shapelist[7][6] = Shapes.cone(sb);

        shapelist[9][0].scale(new double[4] { 2, .01, 2, 1 } );
        shapelist[10][0].scale(new double[4] { .03, 2, .03, 1 } );
        shapelist[11][0].scale(new double[4] { .1, .1, .1, 1 } );
        shapelist[12][0].scale(new double[4] { 2, .01, 2, 1 } );
        shapelist[13][0].scale(new double[4] { .03, 2, .03, 1 } );
        shapelist[14][0].scale(new double[4] { .1, .1, .1, 1 } );

        randomShape = new List<int>();
        for (int i = 0; i < shapelist.Length; i++) randomShape.Add(i);
        random = new System.Random();
    }

    public void changeShape(int shapeNum)
    {
        this.shapeNum = shapeNum;
        colorNum = 1;
        setShape();
    }

    public void toggleRandom(int shapeNum)
    {
        if (randomShape.Contains(shapeNum))
            randomShape.Remove(shapeNum);
        else
            randomShape.Add(shapeNum);
    }

    public void changeRandom() { if (randomShape.Count > 0) changeShape(randomShape[random.Next(randomShape.Count)]); }

    public void changeColor(int colorNum)
    {
        this.colorNum = colorNum;
        setShape();
    }

    public void changeFaceAlpha(float a) { faceColor.a = a; }

    public void changeCellAlpha(float a) { cellAlpha = a; }

    public void changeRetina(double r)
    {
        origin[3] = -(r * r + 1) * 3;
        renderRelative.setRetina(1 / (r * r + 1) * (6 / (6 + r)));
    }

    public void changeOrtho()
    {
        origin[3] = -99999999;
        renderRelative.setRetina(0);
    }

    public void changeBorder(double r) { border = r; }

    public void changeOffset(double r) { offset = r * 0.999; } // 最大値でのZ-Fightingを防ぐ

    private void setShape()
    {
        shapes = shapelist[shapeNum];
        colorVal = 0;
        for (int k = 0; k < shapes.Length; k++) colorVal = Math.Max(colorVal, Vec.max(Shapes.colorList[shapeNum][colorNum][k]) + 1);
        if (colorVal == 1) colorVal = 12;
        for (int k = 0; k < shapes.Length; k++)
        {
            colors = Shapes.colorList[shapeNum][colorNum];
            for (int i = 0; i < shapes[k].cell.Length; i++) shapes[k].cell[i].color = Color.HSVToRGB(1f * colors[k][i] / colorVal, 1, 1);
            clipUnits = new Clip.Draw[shapes.Length];
            for (int i = 0; i < shapes.Length; i++) clipUnits[i] = new Clip.Draw(4);
            inFront = new bool[shapes.Length][];
            for (int i = 0; i < shapes.Length; i++) inFront[i] = new bool[shapes.Length];
            separators = new Geom.Separator[shapes.Length][];
            for (int i = 0; i < shapes.Length; i++) separators[i] = new Geom.Separator[shapes.Length];
        }
    }

    private void spin()
    {
        double theta = Time.deltaTime;
        if (count < 1)
        {
            count += theta;
            if (count >= 1) theta = 1 + theta - count;
        }
        else
        {
            count += theta;
            if (count >= 2)
            {
                count -= 2;
                theta = count;
            }
            else theta = 0;
        }
        if (theta > 0)
        {
            theta *= 72;
            Vec.rotateAngle(reg1, reg2, spinv[0], spinv[1], 2 * theta);
            for (int i = 0; i < shapes.Length; i++) shapes[i].rotateRelative(spinv[0], reg1, reg2, reg3);
            Vec.rotateAngle(reg1, reg2, spinv[2], spinv[3], theta);
            for (int i = 0; i < shapes.Length; i++) shapes[i].rotateRelative(spinv[2], reg1, reg2, reg3);
        }
    }

    public void reset()
    {
        for (int i = 0; i < shapes.Length; i++) shapes[i].reset();
    }

    public void Run(ref Vector3[] vertices, ref int[] triangles, ref Color[] colors,
        double[][] rotate, double[] eyeVector, double[] cursor, double[][] cursorAxis, bool edit, bool select, bool spin)
    {
        if (spin) this.spin();

        for (int i = 0; i < shapes.Length; i++) // 回転
        {
            shapes[i].rotateFrame(rotate[0], rotate[1], zero, reg2, reg3);
            shapes[i].rotateFrame(rotate[2], rotate[3], zero, reg2, reg3);
            shapes[i].place();
        }

        click(cursor, edit, select);

        buf.clear();

        // calcViewBoundaries is expensive, but we always need the boundaries
        // to draw the scenery correctly
        currentDraw = buf;
        for (int i = 0; i < shapes.Length; i++)
        {
            if (shapes[i] == null) continue;
            calcVisShape(shapes[i]);
            clipUnits[i].setBoundaries(Clip.calcViewBoundaries(origin, shapes[i]));
        }

        calcInFront();

        for (int i = 0; i < shapes.Length; i++)
        {
            if (shapes[i] == null) continue;
            currentDraw = buf;
            for (int h = 0; h < shapes.Length; h++)
            {
                if (shapes[h] == null) continue;
                if (inFront[h][i]) currentDraw = clipUnits[h].chain(currentDraw);
            }
            drawShape(shapes[i], eyeVector);
        }

        renderRelative.run(axis, false);
        //for (int i = 0; i < objRetina3.Length; i += 2) bufRelative.add(objRetina3[i], objRetina3[i + 1], edgeColor);

        bufRelative.sort(eyeVector); // できる限り自然な描画にするために、meshを大まかに並べ替える。
        //applyBorder();

        convert(bufRelative, ref vertices, ref triangles, ref colors, eyeVector);
    }

    private void calcInFront()
    {
        for (int i = 0; i < shapes.Length - 1; i++)
        {
            Geom.Shape s1 = shapes[i];
            if (s1 == null) continue;
            for (int j = i + 1; j < shapes.Length; j++)
            {
                Geom.Shape s2 = shapes[j];
                if (s2 == null) continue;

                // prefer dynamic separation even when we know a static
                // separator because dynamic ones are better at finding
                // the desired value NO_FRONT.
                int result = Clip.dynamicSeparate(s1, s2, origin, reg1, reg2);
                if (result == Geom.Separator.UNKNOWN)
                {
                    Geom.Separator sep;


                    sep = separators[i][j];
                    //if (sep == null)
                    {
                        sep = Clip.staticSeparate(s1, s2,/* any = */ false);
                        //sep = gjk.separate(s1, s2);
                        separators[i][j] = sep;
                    }

                    result = sep.apply(origin);
                }

                inFront[i][j] = (result == Geom.Separator.S1_FRONT);
                inFront[j][i] = (result == Geom.Separator.S2_FRONT);
            }
        }
        // note that in general inFront is not transitive.  with long blocks
        // you can easily construct cycles where one is in front of the next
        // all the way around.
    }

    private static readonly double[][] objRetina3 = new double[][] {
      new double[] {-1,-1,-1}, new double[] { 1,-1,-1},
         new double[] { 1,-1,-1}, new double[] { 1, 1,-1},
         new double[] { 1, 1,-1}, new double[] {-1, 1,-1},
         new double[] {-1, 1,-1}, new double[] {-1,-1,-1},

         new double[] {-1,-1, 1}, new double[] { 1,-1, 1},
         new double[] { 1,-1, 1}, new double[] { 1, 1, 1},
         new double[] { 1, 1, 1}, new double[] {-1, 1, 1},
         new double[] {-1, 1, 1}, new double[] {-1,-1, 1},

         new double[] {-1,-1,-1}, new double[] {-1,-1, 1},
         new double[] { 1,-1,-1}, new double[] { 1,-1, 1},
         new double[] { 1, 1,-1}, new double[] { 1, 1, 1},
         new double[] {-1, 1,-1}, new double[] {-1, 1, 1}
   };

    // 見える Cell に属する Face の visible を true にする。
    private void calcVisShape(Geom.Shape shape)
    {
        for (int i = 0; i < shape.face.Length; i++) shape.face[i].visible = false;
        for (int i = 0; i < shape.edge.Length; i++) shape.edge[i].visible = false;
        for (int i = 0; i < shape.cell.Length; i++)
            if (calcVisCell(shape.cell[i]))
            {
                for (int j = 0; j < shape.cell[i].ifa.Length; j++)
                    shape.face[shape.cell[i].ifa[j]].visible = true;
                for (int j = 0; j < shape.cell[i].ie.Length; j++)
                    shape.edge[shape.cell[i].ie[j]].visible = true;
            }
    }

    private bool calcVisCell(Geom.Cell cell)
    {
        if (cell.normal != null)
        {

            // late to be adding an epsilon here, but without this you can sometimes
            // see a flat face between two other faces that ought to be in contact.
            // the example is geom3-project4b (delete the front face and slide right).
            const double epsilon = -1e-9;

            Vec.sub(reg1, cell.center, origin); // vector from origin to face center
            cell.visible = (Vec.dot(reg1, cell.normal) < epsilon) ^ invertNormals;
        }
        else
        {
            cell.visible = true; // glass
        }
        return cell.visible;
    }

    private void drawShape(Geom.Shape shape, double[] eyeVector)
    {
        if (drFace) for (int i = 0; i < shape.face.Length; i++) if (drHdnEdge || shape.face[i].visible) drawFace(shape, shape.face[i]);
        if (drEdge) for (int i = 0; i < shape.edge.Length; i++) if (drHdnEdge || shape.edge[i].visible) drawEdge(shape, shape.edge[i]);
        if (drCell) for (int i = 0; i < shape.cell.Length; i++) if (shape.cell[i].visible) drawCell(shape, shape.cell[i], i == selectedCell, eyeVector);
    }

    private void drawFace(Geom.Shape shape, Geom.Face face)
    {
        double[][] vertex = new double[face.iv.Length][];
        for (int i = 0; i < face.iv.Length; i++) vertex[i] = (double[])shape.vertex[face.iv[i]].Clone();
        Polygon polygon = new Polygon(vertex, faceColor);
        currentDraw.drawPolygon(polygon, origin);
    }

    private void drawEdge(Geom.Shape shape, Geom.Edge edge)
    {
        double[][] vertex = new double[2][];
        vertex[0] = (double[])shape.vertex[edge.iv1].Clone();
        vertex[1] = (double[])shape.vertex[edge.iv2].Clone();
        currentDraw.drawLine(vertex[0], vertex[1], edgeColor, origin);
    }

    // 隙間を空けた胞表示。
    private void drawCell(Geom.Shape shape, Geom.Cell cell, bool selected, double[] eyeVector)
    {
        // border 処理のために視点からの距離を測る。
        Vec.sub(reg1, cell.center, origin);
        Vec.toAxisCoordinates(reg1, reg1, axis);
        if (renderRelative.getRetina() > 0) Vec.projectRetina(reg4, reg1, renderRelative.getRetina());
        else Vec.projectOrtho(reg4, reg1, renderRelative.getOrtho());
        bool beyond = Vec.dot(reg4, eyeVector) < border;
        for (int i = 0; i < cell.ifa.Length; i++) drawFace(shape, cell, shape.face[cell.ifa[i]], selected, beyond);
        //for (int i = 0; i < cell.ie.Length; i++) drawEdge(shape, cell, shape.edge[cell.ie[i]], selected, beyond);
    }

    private void drawFace(Geom.Shape shape, Geom.Cell cell, Geom.Face face, bool selected, bool beyond)
    {
        // offset を掛けて縮める。
        double[][] vertex = new double[face.iv.Length][];
        for (int i = 0; i < face.iv.Length; i++)
        {
            vertex[i] = new double[shape.vertex[face.iv[i]].Length];
            Vec.mid(vertex[i], cell.center, shape.vertex[face.iv[i]], offset);
        }
        cell.color.a = (beyond) ? cellAlpha * 0.1f : cellAlpha;
        Polygon polygon = new Polygon(vertex, selected ? cell.color +Color.white * 0.2f : cell.color);
        currentDraw.drawPolygon(polygon, origin);
    }

    private void drawEdge(Geom.Shape shape, Geom.Cell cell, Geom.Edge edge, bool selected, bool beyond)
    {
        // offset を掛けて縮める。
        double[][] vertex = new double[2][];
        vertex[0] = new double[shape.vertex[edge.iv1].Length];
        Vec.mid(vertex[0], cell.center, shape.vertex[edge.iv1], offset);
        vertex[1] = new double[shape.vertex[edge.iv2].Length];
        Vec.mid(vertex[1], cell.center, shape.vertex[edge.iv2], offset);
        cell.color.a = (beyond) ? 0.1f : 1.0f;
        currentDraw.drawLine(vertex[0], vertex[1], cell.color, origin);
    }

    private void applyBorder()
    {
        int size = bufRelative.getSize();
        int target = size / 2;
        int n = target / 2;
        while (n > 0)
        {
            if (target < size && bufRelative.get(target).dist < border) target -= n;
            else target += n;
            n /= 2;
        }
        for (int i = target; i < size; i++) bufRelative.get(i).color.a *= 0.1f;
    }

    public double click(double[] vector, bool edit, bool select)
    {
        if (renderRelative.getRetina() > 0) Vec.addScaled(reg3, axis[3], vector, renderRelative.getRetina());
        else Vec.scale(reg3, axis[3], renderRelative.getOrtho());
        Vec.addScaled(reg2, origin, reg3, 10000); // infinity
        double dMin = 1;
        for (int i = 0; i < shapes.Length; i++)
        {
            Geom.Shape shape = shapes[i];
            if (Clip.closestApproach(shape.shapecenter, origin, reg3, reg1) <= shape.radius * shape.radius)
            { // could be a hit
                Clip.clip(origin, reg2, shape, clipResult);
                if ((clipResult.clip & Clip.KEEP_A) != 0)
                {
                    if (clipResult.a < dMin)
                    {
                        dMin = clipResult.a;
                        selectedShape = i;
                        selectedCell = clipResult.ia;
                    }
                }
            }
        }
        if (dMin < 1 && select)
        {
            if (edit)
            {
                colors[selectedShape][selectedCell]++;
                colors[selectedShape][selectedCell] %= colorVal;
                shapes[selectedShape].cell[selectedCell].color = Color.HSVToRGB(1f * colors[selectedShape][selectedCell] / colorVal, 1, 1);
            }
        }
        else selectedCell = -1;
        return dMin;
    }

    private double calcHapTest(double[] cursor)
    {
        int n = (shapeNum - 9) % 3;
        double d = 0;
        switch (n)
        {
            case 0:
                d = Math.Abs(Vec.dot(cursor, shapes[0].axis[1]) * .00025);
                break;
            case 1:
                d = Math.Sqrt(Math.Pow(Vec.dot(cursor, shapes[0].axis[0]),2) + Math.Pow(Vec.dot(cursor, shapes[0].axis[2]),2)) * .00025;
                break;
            case 2:
                d = Vec.norm(cursor) * .00025;
                break;
            default:
                break;
        }
        if (shapeNum < 12) d = -d+1;
        return d;
    }
    public void save()
    {
        StreamWriter sw = new StreamWriter("./LogData.txt", true);
        foreach (int[] c in colors) sw.WriteLine(Vec.ToString(c));
        sw.Flush();
        sw.Close();
    }

    private double width = 0.005;
    private float t3 = 0.2f;
    private float t2 = 1f;
    // Polygon の情報を vertices, triangles, colors に変換する。
    private void convert(PolygonBuffer buf, ref Vector3[] vertices, ref int[] triangles, ref Color[] colors, double[] eyeVector)
    {
        int count = 0;
        Polygon p;
        verts.Clear();
        tris.Clear();
        cols.Clear();
        for (int i=0; i<buf.getSize(); i++)
        {
            p = buf.get(i);
            int v = p.vertex.Length;
            if (sliceMode) p.color.a *= t3;
            if (v == 2)
            {
                v = 4;
                Vec.sub(reg5, p.vertex[1], p.vertex[0]);
                Vec.cross(reg4, reg5, eyeVector);
                Vec.normalize(reg4, reg4);
                Vec.scale(reg4, reg4, width);

                Vec.add(reg5, p.vertex[0], reg4);
                verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                cols.Add(p.color);

                Vec.addScaled(reg5, p.vertex[0], reg4, -1);
                verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                cols.Add(p.color);

                Vec.add(reg5, p.vertex[1], reg4);
                verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                cols.Add(p.color);

                Vec.addScaled(reg5, p.vertex[1], reg4, -1);
                verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                cols.Add(p.color);

                tris.Add(count);
                tris.Add(count + 1);
                tris.Add(count + 2);
                tris.Add(count + 2);
                tris.Add(count + 1);
                tris.Add(count + 3);
            }
            else
            {
                for (int j = 0; j < v; j++)
                {
                    reg4 = p.vertex[j];
                    verts.Add(new Vector3((float)reg4[0], (float)reg4[1], (float)reg4[2]));
                    cols.Add(p.color);
                }
                for (int j = 0; j < v - 2; j++)
                {
                    tris.Add(count);
                    tris.Add(count + j + 1);
                    tris.Add(count + j + 2);
                }
                if (sliceMode)
                {
                    int k = 0;
                    for (int j = 0; j < v - 1; j++)
                    {
                        if (p.vertex[j][2] * p.vertex[j + 1][2] < 0)
                        {
                            if (k == 0)
                            {
                                reg7[0] = (p.vertex[j][0] * Math.Abs(p.vertex[j + 1][2]) + p.vertex[j + 1][0] * Math.Abs(p.vertex[j][2])) / (Math.Abs(p.vertex[j][2]) + Math.Abs(p.vertex[j + 1][2]));
                                reg7[1] = (p.vertex[j][1] * Math.Abs(p.vertex[j + 1][2]) + p.vertex[j + 1][1] * Math.Abs(p.vertex[j][2])) / (Math.Abs(p.vertex[j][2]) + Math.Abs(p.vertex[j + 1][2]));
                                reg7[2] = 0;
                            }
                            else
                            {
                                reg8[0] = (p.vertex[j][0] * Math.Abs(p.vertex[j + 1][2]) + p.vertex[j + 1][0] * Math.Abs(p.vertex[j][2])) / (Math.Abs(p.vertex[j][2]) + Math.Abs(p.vertex[j + 1][2]));
                                reg8[1] = (p.vertex[j][1] * Math.Abs(p.vertex[j + 1][2]) + p.vertex[j + 1][1] * Math.Abs(p.vertex[j][2])) / (Math.Abs(p.vertex[j][2]) + Math.Abs(p.vertex[j + 1][2]));
                                reg8[2] = 0;
                            }
                            k += 1;
                        }
                    }
                    if (k == 1)
                    {
                        reg8[0] = (p.vertex[0][0] * Math.Abs(p.vertex[v - 1][2]) + p.vertex[v - 1][0] * Math.Abs(p.vertex[0][2])) / (Math.Abs(p.vertex[0][2]) + Math.Abs(p.vertex[v - 1][2]));
                        reg8[1] = (p.vertex[0][1] * Math.Abs(p.vertex[v - 1][2]) + p.vertex[v - 1][1] * Math.Abs(p.vertex[0][2])) / (Math.Abs(p.vertex[0][2]) + Math.Abs(p.vertex[v - 1][2]));
                        reg8[2] = 0;
                    }
                    if (k > 0)
                    {
                        count += v;
                        p.color.a = t2;
                        v = 4;
                        Vec.sub(reg5, reg8, reg7);
                        Vec.cross(reg6, reg5, eyeVector);
                        Vec.normalize(reg6, reg6);
                        Vec.scale(reg6, reg6, width * 2);

                        Vec.add(reg5, reg7, reg6);
                        verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                        cols.Add(p.color);

                        Vec.addScaled(reg5, reg7, reg6, -1);
                        verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                        cols.Add(p.color);

                        Vec.add(reg5, reg8, reg6);
                        verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                        cols.Add(p.color);

                        Vec.addScaled(reg5, reg8, reg6, -1);
                        verts.Add(new Vector3((float)reg5[0], (float)reg5[1], (float)reg5[2]));
                        cols.Add(p.color);

                        tris.Add(count);
                        tris.Add(count + 1);
                        tris.Add(count + 2);
                        tris.Add(count + 2);
                        tris.Add(count + 1);
                        tris.Add(count + 3);
                    }
                }
            }
            count += v;
        }
        vertices = verts.ToArray();
        triangles = tris.ToArray();
        colors = cols.ToArray();
    }
}
