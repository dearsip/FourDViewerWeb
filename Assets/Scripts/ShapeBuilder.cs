using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 頂点座標、辺の長さ、面の辺の数から多胞体を定義するためのクラス。
public class ShapeBuilder
{
    private static double epsilon = 0.000001;
    private double[][] vertex;
    private double[] el; // 辺の長さ
    private int[] fen; // 面の辺の数
    private int maxf; // fn の最大値
    private int[] cfn; // 胞の面の数
    private int maxc; // cn の最大値

    private double[] reg1, reg2, reg3, reg4;

    private List<int[]> ev; // 辺が持つ頂点
    private List<double[]> vector; // 辺のベクトル
    private List<int>[] ve; // 頂点が属する辺
    private bool[,] isUsedEdges; // 辺の組を含む面が既に定義されているかを示すフラグ
    private List<int> reg; // 面及び胞を作るために一時的に辺及び面を格納する
    private List<int[]> fe; // 面が持つ辺
    private List<int>[] ef; // 辺が属する面
    private bool[,] isUsedFaces; // 面の組を含む胞が既に定義されているかを示すフラグ
    private List<int[]> cf; // 胞が持つ面
    private List<int>[] fc; // 面が属する胞
    private List<double[]> cn; // 胞の法線
    private int[][] fv; // 面が持つ頂点
    private int[][] ce; // 胞が持つ辺
    private Color[] color;

    public ShapeBuilder()
    {
        reg1 = new double[4];
        reg2 = new double[4];
        reg3 = new double[4];
        reg4 = new double[4];
        ev = new List<int[]>();
        vector = new List<double[]>();
        reg = new List<int>();
        fe = new List<int[]>();
        cf = new List<int[]>();
        cn = new List<double[]>();
    }

    public Geom.Shape build(double[][] vertex, double[] el, int[] fen, int[] cfn)
    {
        this.vertex = vertex;
        this.el = el;
        this.fen = fen;
        maxf = max(fen);
        this.cfn = cfn;
        maxc = max(cfn);
        initialize();
        setEdge();
        isUsedEdges = new bool[ev.Count, ev.Count];
        ef = new List<int>[ev.Count];
        for (int i = 0; i < ev.Count; i++) ef[i] = new List<int>();
        setFace();
        isUsedFaces = new bool[fe.Count, fe.Count];
        fc = new List<int>[fe.Count];
        for (int i = 0; i < fe.Count; i++) fc[i] = new List<int>();
        setCell();
        calcOthers();
        return new Geom.Shape(vertex, ev.ToArray(), fv, ce, cf.ToArray(), cn.ToArray(), color);
    }

    private int max(int[] n)
    {
        int m = n[0];
        for (int i = 1; i < n.Length; i++) if (m < n[i]) m = n[i];
        return m;
    }

    private void initialize()
    {
        ev.Clear();
        vector.Clear();
        ve = new List<int>[vertex.Length];
        for (int i = 0; i < vertex.Length; i++) ve[i] = new List<int>();
        reg.Clear();
        fe.Clear();
        cf.Clear();
        cn.Clear();
    }

    private void setEdge()
    {
        double m;
        for (int i = 0; i < vertex.Length; i++)
        {
            for (int j = i + 1; j < vertex.Length; j++)
            {
                Vec.sub(reg1, vertex[j], vertex[i]);
                m = Vec.norm(reg1);
                for (int k = 0; k < el.Length; k++)
                {
                    if (Math.Abs(m - el[k]) < epsilon)
                    {
                        ve[i].Add(ev.Count);
                        ve[j].Add(ev.Count);
                        vector.Add((double[])reg1.Clone());
                        ev.Add(new int[] { i, j });
                        break;
                    }
                }
            }
        }
    }

    private void setFace()
    {
        for (int i = 0; i < ev.Count; i++)
        {
            foreach (int j in ve[ev[i][1]])
            {
                if (j == i || isUsedEdges[i, j]) continue;
                reg.Clear();
                reg.Add(i);
                reg.Add(j);
                Vec.normalize(reg1, vector[i]);
                Vec.project(reg2, vector[j], reg1);
                Vec.normalize(reg2, reg2);
                int nowv = otherVertex(j, ev[i][1]);
                int lastv = ev[i][1];
                int firstv = ev[i][0];
                bool finish = false;
                int e = 2;
                for (; e < maxf; e++)
                {
                    foreach (int k in ve[nowv])
                    {
                        int nextv = otherVertex(k, nowv);
                        if (nextv == lastv) continue;
                        else if (nextv == firstv)
                        {
                            reg.Add(k);
                            finish = true;
                            break;
                        }
                        else if (onPlane(vector[k], reg1, reg2))
                        {
                            reg.Add(k);
                            lastv = nowv;
                            nowv = nextv;
                            break;
                        }
                    }
                    if (finish) break;
                }
                if (e == maxf) continue;
                foreach (int k in fen) if (reg.Count == k)
                    {
                        for (int n = 0; n < reg.Count; n++)
                        {
                            ef[reg[n]].Add(fe.Count);
                            for (int m = n + 1; m < reg.Count; m++)
                            {
                                isUsedEdges[reg[n], reg[m]] = true;
                                isUsedEdges[reg[m], reg[n]] = true;
                            }
                        }
                        fe.Add(reg.ToArray());
                        break;
                    }
            }
        }
    }

    // 指定された辺の、指定された頂点と異なる方の頂点を取得。
    private int otherVertex(int edge, int vertex) { return (ev[edge][0] == vertex) ? ev[edge][1] : ev[edge][0]; }

    // ベクトル v が基底 e1, e2 の張る平面上にあるかどうかの判定。
    private bool onPlane(double[] v, double[] e1, double[] e2)
    {
        double a1 = Vec.dot(v, e1);
        double a2 = Vec.dot(v, e2);
        Vec.scale(reg3, e1, a1);
        Vec.addScaled(reg3, reg3, e2, a2);
        return Vec.approximatelyEquals(v, reg3, epsilon);
    }

    private void setCell()
    {
        for (int i = 0; i < fe.Count; i++)
        {
            foreach (int h in fe[i])
            {
                foreach (int j in ef[h])
                {
                    if (j == i || isUsedFaces[i, j]) continue;
                    Vec.outerProduct(reg1, vector[fe[i][0]], vector[fe[i][1]], vector[otherEdge(j, h)]);
                    int pn;
                    if ((pn = isSurface(vertex[ev[fe[i][0]][0]], reg1)) == 0) continue;
                    Vec.scale(reg1, reg1, pn);
                    Vec.normalize(reg1, reg1);
                    reg.Clear();
                    reg.Add(i);
                    reg.Add(j);
                    gatherFace(j, reg1, reg);
                    foreach (int k in cfn) if (reg.Count == k)
                        {
                            cn.Add((double[])reg1.Clone());
                            for (int n = 0; n < reg.Count; n++)
                            {
                                fc[n].Add(cf.Count);
                                for (int m = n + 1; m < reg.Count; m++)
                                {
                                    isUsedFaces[reg[n], reg[m]] = true;
                                    isUsedFaces[reg[m], reg[n]] = true;
                                }
                            }
                            cf.Add(reg.ToArray());
                        }
                }
            }
        }
    }

    // 指定された面の、指定された辺の隣の辺を取得。
    private int otherEdge(int face, int edge)
    {
        foreach(int e in fe[face])
        {
            if (e == edge) continue;
            if (ev[e][0] == ev[edge][0] || ev[e][1] == ev[edge][0]) return e;
        }
        return -1;
    }

    // vertex を通る超平面 normal が多胞体の表面になっているか調べる。
    private int isSurface(double[] v, double[] normal)
    {
        int pn = 0;
        double dot;
        foreach(double[] w in vertex)
        {
            Vec.sub(reg3, v, w);
            dot = Vec.dot(reg3, normal);
            if (dot > epsilon)
                switch(pn)
                {
                    case 0: pn = 1; break;
                    case 1: break;
                    case -1: return 0;
                }
            else if (dot < -epsilon)
                switch(pn)
                {
                    case 0: pn = -1; break;
                    case -1: break;
                    case 1: return 0;
                }
        }
        return pn;
    }

    private void gatherFace(int face, double[] normal, List<int> cell)
    {
        foreach(int edge in fe[face])
        {
            foreach(int f in ef[edge])
            {
                if (f == face) continue;
                if (cell.Contains(f)) break;
                if (Math.Abs(Vec.dot(vector[otherEdge(f, edge)], normal)) < epsilon)
                {
                    cell.Add(f);
                    gatherFace(f, normal, cell);
                    break;
                }
            }
        }
    }

    private void calcOthers()
    {
        fv = new int[fe.Count][];
        for (int i = 0; i < fe.Count; i++)
        {
            reg.Clear();
            foreach (int j in fe[i])
            {
                foreach (int k in ev[j]) if (!reg.Contains(k)) reg.Add(k);
            }
            fv[i] = reg.ToArray();
        }
        ce = new int[cf.Count][];
        for(int i = 0; i < cf.Count; i++)
        {
            reg.Clear();
            foreach(int j in cf[i])
            {
                foreach (int k in fe[j]) if (!reg.Contains(k)) reg.Add(k);
            }
            ce[i] = reg.ToArray();
        }
        color = new Color[cf.Count];
    }
}