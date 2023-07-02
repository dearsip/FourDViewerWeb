using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Shape を返すメソッド。
public class Shapes
{
    public static Color defc = new Color(1.0f, 0.0f, 0.0f, 0.25f);

    public static Geom.Shape cell_5()
    {
        double[][] vertex = new double[][]
        {
            new double[] { 0, 0, 0, 1 },
            new double[] { -Math.Sqrt(5)/4, -Math.Sqrt(5)/4, -Math.Sqrt(5)/4, -1.0/4 },
            new double[] { -Math.Sqrt(5)/4,  Math.Sqrt(5)/4,  Math.Sqrt(5)/4, -1.0/4 },
            new double[] {  Math.Sqrt(5)/4, -Math.Sqrt(5)/4,  Math.Sqrt(5)/4, -1.0/4 },
            new double[] {  Math.Sqrt(5)/4,  Math.Sqrt(5)/4, -Math.Sqrt(5)/4, -1.0/4 },
        };
        for (int i = 0; i < 5; i++) Vec.scale(vertex[i], vertex[i], 2);
        int[][] eiv = new int[10][];
        int count = 0;
        for (int i = 0; i < 4; i++)
            for (int j = i + 1; j < 5; j++) eiv[count++] = new int[] { i, j };
        int[][] fiv = new int[10][];
        count = 0;
        for (int i = 0; i < 3; i++)
            for (int j = i + 1; j < 4; j++)
                for (int k = j + 1; k < 5; k++) fiv[count++] = new int[] { i, j, k };
        int[][] cie = new int[][]
        {
            new int[] { 0,1,2,4,5,7 },
            new int[] { 0,1,3,4,6,8 },
            new int[] { 0,2,3,5,6,9 },
            new int[] { 1,2,3,7,8,9 },
            new int[] { 4,5,6,7,8,9 }
        };
        int[][] cif = new int[][]
        {
            new int[] { 0,1,3,6 },
            new int[] { 0,2,4,7 },
            new int[] { 1,2,5,8 },
            new int[] { 3,4,5,9 },
            new int[] { 6,7,8,9 }
        };
        double[][] cn = new double[5][];
        for (int i = 0; i < 5; i++)
        {
            cn[i] = new double[4];
            Vec.scale(cn[i], vertex[4 - i], -1);
        }
        Color[] color = new Color[5];
        return new Geom.Shape(vertex, eiv, fiv, cie, cif, cn, color);
    }
    
    public static Geom.Shape cell_8()
    {
        double[][] vertex = new double[][]
        {
            new double[] {  1,  1,  1,  1 },
            new double[] {  1,  1, -1,  1 },
            new double[] {  1, -1, -1,  1 },
            new double[] {  1, -1,  1,  1 },
            new double[] { -1, -1,  1,  1 },
            new double[] { -1, -1, -1,  1 },
            new double[] { -1,  1, -1,  1 },
            new double[] { -1,  1,  1,  1 },
            new double[] {  1,  1,  1, -1 },
            new double[] {  1,  1, -1, -1 },
            new double[] {  1, -1, -1, -1 },
            new double[] {  1, -1,  1, -1 },
            new double[] { -1, -1,  1, -1 },
            new double[] { -1, -1, -1, -1 },
            new double[] { -1,  1, -1, -1 },
            new double[] { -1,  1,  1, -1 }
        };
        int[][] eiv = new int[][]
        {
            new int[] {0,1 },
            new int[] {1,2 },
            new int[] {2,3 },
            new int[] {3,0 },
            new int[] {0,7 },
            new int[] {1,6 },
            new int[] {2,5 },
            new int[] {3,4 },
            new int[] {4,5 },
            new int[] {5,6 },
            new int[] {6,7 },
            new int[] {7,4 },

            new int[] {0,8 },
            new int[] {1,9 },
            new int[] {2,10 },
            new int[] {3,11 },
            new int[] {4,12 },
            new int[] {5,13 },
            new int[] {6,14 },
            new int[] {7,15 },

            new int[] {8,9 },
            new int[] {9,10 },
            new int[] {10,11 },
            new int[] {11,8 },
            new int[] {8,15 },
            new int[] {9,14 },
            new int[] {10,13 },
            new int[] {11,12 },
            new int[] {12,13 },
            new int[] {13,14 },
            new int[] {14,15 },
            new int[] {15,12 }
        };
        int[][] fiv = new int[][]
        {
            new int[] {0,1,2,3 },
            new int[] {0,1,6,7 },
            new int[] {1,2,5,6 },
            new int[] {2,3,4,5 },
            new int[] {3,0,7,4 },
            new int[] {4,5,6,7 },

            new int[] {0,1,9,8 },
            new int[] {1,2,10,9 },
            new int[] {2,3,11,10 },
            new int[] {3,0,8,11 },
            new int[] {0,7,15,8 },
            new int[] {1,6,14,9 },
            new int[] {2,5,13,10 },
            new int[] {3,4,12,11 },
            new int[] {4,5,13,12 },
            new int[] {5,6,14,13 },
            new int[] {6,7,15,14 },
            new int[] {7,4,12,15 },

            new int[] {8,9,10,11 },
            new int[] {8,9,14,15 },
            new int[] {9,10,13,14 },
            new int[] {10,11,12,13 },
            new int[] {11,8,15,12 },
            new int[] {12,13,14,15 }
        };
        int[][] cie = new int[][]
        {
            new int[] { 0,1,2,3,4,5,6,7,8,9,10,11 },
            new int[] { 20,21,22,23,24,25,26,27,28,29,30,31 },
            new int[] { 0,1,2,3,12,13,14,15,20,21,22,23 },
            new int[] { 8,9,10,11,16,17,18,19,28,29,30,31 },
            new int[] { 0,4,5,10,12,13,18,19,20,24,25,30 },
            new int[] { 2,6,7,8,14,15,16,17,22,26,27,27 },
            new int[] { 3,4,7,11,12,15,16,19,23,24,27,31 },
            new int[] { 1,5,6,9,13,14,17,18,21,25,26,29 }
        };
        int[][] cif = new int[][]
        {
            new int[] {0,1,2,3,4,5 },
            new int[] {18,19,20,21,22,23 },
            new int[] {0,6,7,8,9,18 },
            new int[] {5,14,15,16,17,23 },
            new int[] {1,6,10,11,16,19 },
            new int[] {3,8,12,13,14,21 },
            new int[] {4,9,10,13,17,22 },
            new int[] {2,7,11,12,15,20 }
        };
        double[][] cn = new double[][]
        {
            new double[] {0,0,0,1 },
            new double[] {0,0,0,-1 },
            new double[] {1,0,0,0 },
            new double[] {-1,0,0,0 },
            new double[] {0,1,0,0 },
            new double[] {0,-1,0,0 },
            new double[] {0,0,1,0 },
            new double[] {0,0,-1,0 }
        };
        Color[] color = new Color[]
        {
            new Color( 1.0f, 1.0f, 1.0f, 0.3f ),
            new Color( 0.4f, 0.4f, 0.4f, 0.3f ),
            new Color( 1.0f, 1.0f, 0.0f, 0.3f ),
            new Color( 0.0f, 0.0f, 1.0f, 0.3f ),
            new Color( 1.0f, 0.0f, 1.0f, 0.3f ),
            new Color( 0.0f, 1.0f, 0.0f, 0.3f ),
            new Color( 0.0f, 1.0f, 1.0f, 0.3f ),
            new Color( 1.0f, 0.0f, 0.0f, 0.3f ),
        };
        return new Geom.Shape(vertex, eiv, fiv, cie, cif, cn, color);
    }
    
    public static Geom.Shape cell_16()
    {
        double[][] vertex = new double[][]
        {
            new double[] { 1, 0, 0, 0 },
            new double[] { -1, 0, 0, 0 },
            new double[] { 0, 1, 0, 0 },
            new double[] { 0, -1, 0, 0 },
            new double[] { 0, 0, 1, 0 },
            new double[] { 0, 0, -1, 0 },
            new double[] { 0, 0, 0, 1 },
            new double[] { 0, 0, 0, -1 },
        };
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 4; j++)
                vertex[i][j] *= 2;
        int[][] eiv = new int[][] 
        {
            new int[] { 0,2 },
            new int[] { 0,3 },
            new int[] { 0,4 },
            new int[] { 0,5 },
            new int[] { 0,6 },
            new int[] { 0,7 },
            new int[] { 1,2 },
            new int[] { 1,3 },
            new int[] { 1,4 },
            new int[] { 1,5 },
            new int[] { 1,6 },
            new int[] { 1,7 },
            new int[] { 2,4 },
            new int[] { 2,5 },
            new int[] { 2,6 },
            new int[] { 2,7 },
            new int[] { 3,4 },
            new int[] { 3,5 },
            new int[] { 3,6 },
            new int[] { 3,7 },
            new int[] { 4,6 },
            new int[] { 4,7 },
            new int[] { 5,6 },
            new int[] { 5,7 }
        };
        int[][] fiv = new int[][] 
        {
            new int[] { 0,2,4 }, //0
            new int[] { 0,2,6 }, //1
            new int[] { 0,4,6 }, //2
            new int[] { 2,4,6 }, //3

            //new int[] { 0,2,4 }, 0
            new int[] { 0,2,7 }, //4
            new int[] { 0,4,7 }, //5
            new int[] { 2,4,7 }, //6

            new int[] { 0,2,5 }, //7
            //new int[] { 0,2,6 }, 1
            new int[] { 0,5,6 }, //8
            new int[] { 2,5,6 }, //9

            //new int[] { 0,2,5 }, 7
            //new int[] { 0,2,7 }, 4
            new int[] { 0,5,7 }, //10
            new int[] { 2,5,7 }, //11

            new int[] { 0,3,4 }, //12
            new int[] { 0,3,6 }, //13
            //new int[] { 0,4,6 }, 2
            new int[] { 3,4,6 }, //14

            //new int[] { 0,3,4 }, 12
            new int[] { 0,3,7 }, //15
            //new int[] { 0,4,7 }, 5
            new int[] { 3,4,7 }, //16

            new int[] { 0,3,5 }, //17
            //new int[] { 0,3,6 }, 13
            //new int[] { 0,5,6 }, 8
            new int[] { 3,5,6 }, //18

            //new int[] { 0,3,5 }, 17
            //new int[] { 0,3,7 }, 15
            //new int[] { 0,5,7 }, 10
            new int[] { 3,5,7 }, //19

            new int[] { 1,2,4 }, //20
            new int[] { 1,2,6 }, //21
            new int[] { 1,4,6 }, //22
            //new int[] { 2,4,6 }, 3
            
            //new int[] { 1,2,4 }, 20
            new int[] { 1,2,7 }, //23
            new int[] { 1,4,7 }, //24
            //new int[] { 2,4,7 }, 6

            new int[] { 1,2,5 }, //25
            //new int[] { 1,2,6 }, 21
            new int[] { 1,5,6 }, //26
            //new int[] { 2,5,6 }, 9

            //new int[] { 1,2,5 }, 25
            //new int[] { 1,2,7 }, 23
            new int[] { 1,5,7 }, //27
            //new int[] { 2,5,7 }, 11

            new int[] { 1,3,4 }, //28
            new int[] { 1,3,6 }, //29
            //new int[] { 1,4,6 }, 22
            //new int[] { 3,4,6 }, 14
            
            //new int[] { 1,3,4 }, 28
            new int[] { 1,3,7 }, //30
            //new int[] { 1,4,7 }, 24
            //new int[] { 3,4,7 }, 16

            new int[] { 1,3,5 }, //31
            //new int[] { 1,3,6 }, 29
            //new int[] { 1,5,6 }, 26
            //new int[] { 3,5,6 }, 18

            //new int[] { 1,3,5 }, 31
            //new int[] { 1,3,7 }, 30
            //new int[] { 1,5,7 }, 27
            //new int[] { 3,5,7 }  19
        };
        int[][] cie = new int[][]
        {
            new int[] { 0,2,4,12,14,20 },
            new int[] { 0,2,5,12,15,21 },
            new int[] { 0,3,4,13,14,22 },
            new int[] { 0,3,5,13,15,23 },
            new int[] { 1,2,4,16,18,20 },
            new int[] { 1,2,5,16,19,21 },
            new int[] { 1,3,4,17,18,22 },
            new int[] { 1,3,5,17,19,23 },
            new int[] { 6,8,10,12,14,20 },
            new int[] { 6,8,11,12,15,21 },
            new int[] { 6,9,10,13,14,22 },
            new int[] { 6,9,11,13,15,23 },
            new int[] { 7,8,10,16,18,20 },
            new int[] { 7,8,11,16,19,21 },
            new int[] { 7,9,10,17,18,22 },
            new int[] { 7,9,11,17,19,23 }
        };
        int[][] cif = new int[][]
        {
            new int[] { 0,1,2,3 },
            new int[] { 0,4,5,6 },
            new int[] { 7,1,8,9 },
            new int[] { 7,4,10,11 },
            new int[] { 12,13,2,14 },
            new int[] { 12,15,5,16 },
            new int[] { 17,13,8,18 },
            new int[] { 17,15,10,19 },
            new int[] { 20,21,22,3 },
            new int[] { 20,23,24,6 },
            new int[] { 25,21,26,9 },
            new int[] { 25,23,27,11 },
            new int[] { 28,29,22,14 },
            new int[] { 28,30,24,16 },
            new int[] { 31,29,26,18 },
            new int[] { 31,30,27,19 }
        };
        double[][] cn = new double[][]
        {
            new double[] {  1,  1,  1,  1 },
            new double[] {  1,  1,  1, -1 },
            new double[] {  1,  1, -1,  1 },
            new double[] {  1,  1, -1, -1 },
            new double[] {  1, -1,  1,  1 },
            new double[] {  1, -1,  1, -1 },
            new double[] {  1, -1, -1,  1 },
            new double[] {  1, -1, -1, -1 },
            new double[] { -1,  1,  1,  1 },
            new double[] { -1,  1,  1, -1 },
            new double[] { -1,  1, -1,  1 },
            new double[] { -1,  1, -1, -1 },
            new double[] { -1, -1,  1,  1 },
            new double[] { -1, -1,  1, -1 },
            new double[] { -1, -1, -1,  1 },
            new double[] { -1, -1, -1, -1 }
        };
        Color[] color = new Color[]
        {
            new Color(1.0f, 1.0f, 1.0f, 0.3f),
            new Color(1.0f, 0.5f, 0.5f, 0.3f),
            new Color(0.5f, 1.0f, 0.5f, 0.3f),
            new Color(1.0f, 1.0f, 0.0f, 0.3f),
            new Color(0.5f, 0.5f, 1.0f, 0.3f),
            new Color(1.0f, 0.0f, 1.0f, 0.3f),
            new Color(0.0f, 1.0f, 1.0f, 0.3f),
            new Color(0.6f, 0.6f, 0.6f, 0.3f),
            new Color(0.8f, 0.8f, 0.8f, 0.3f),
            new Color(1.0f, 0.0f, 0.0f, 0.3f),
            new Color(0.0f, 1.0f, 0.0f, 0.3f),
            new Color(0.0f, 0.0f, 0.5f, 0.3f),
            new Color(0.0f, 0.0f, 1.0f, 0.3f),
            new Color(0.0f, 0.5f, 0.0f, 0.3f),
            new Color(0.5f, 0.0f, 0.0f, 0.3f),
            new Color(0.4f, 0.4f, 0.4f, 0.3f),
        };
        return new Geom.Shape(vertex, eiv, fiv, cie, cif, cn, color);
    }

    public static Geom.Shape cell_24(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[24][];
        int n = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = i + 1; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    vertex[n] = new double[4];
                    vertex[n][i] = k / 2 * 2 - 1;
                    vertex[n][j] = k % 2 * 2 - 1;
                    n++;
                }
            }
        }
        return shapeBuilder.build(vertex, new double[] { Math.Sqrt(2) }, new int[] { 3 }, new int[] { 8 });
    }

    // 参考: https://qiita.com/HMMNRST/items/31b1f226f08d00f1cf95
    private static float SQRT5 = Mathf.Sqrt(5);
    private static float PHI = (1f + SQRT5)/2;
    private static Quaternion[] qi = new Quaternion[] { new Quaternion(0, 0, 0, -1),
                                     new Quaternion(1, 0, 0, 0),
                                     new Quaternion(0, 1, 0, 0),
                                     new Quaternion(1, 1, 1, 1).normalized,
                                     new Quaternion(1, 1/PHI, 0, PHI).normalized};
    private static int[] d = new int[] { 2, 2, 2, 3, 5 };
    private static Quaternion[] qt = new Quaternion[] { new Quaternion(0, 0, 0, 1),
                                     new Quaternion(-SQRT5, -SQRT5, -SQRT5, -1).normalized,
                                     new Quaternion(-SQRT5,  SQRT5,  SQRT5, -1).normalized,
                                     new Quaternion( SQRT5, -SQRT5,  SQRT5, -1).normalized,
                                     new Quaternion( SQRT5,  SQRT5, -SQRT5, -1).normalized,};

    public static Geom.Shape cell_120(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[600][];
        for (int i = 0; i < 600; i++)
        {
            Quaternion q = new Quaternion(0, 0, 0, Mathf.Sqrt(2));
            int n = i;
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < n % d[j]; k++) q *= qi[j];
                n /= d[j];
            }
            q *= qt[n];
            vertex[i] = new double[] { q[0], q[1], q[2], q[3] };
        }
        return shapeBuilder.build(vertex, new double[] { 1/(PHI*PHI) }, new int[] { 5 }, new int[] { 12 });
    }

    public static Geom.Shape cell_600(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[120][];
        for (int i = 0; i < 120; i++)
        {
            Quaternion q = new Quaternion(0, 0, 0, Mathf.Sqrt(2));
            int n = i;
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < n % d[j]; k++) q *= qi[j];
                n /= d[j];
            }
            vertex[i] = new double[] { q[0], q[1], q[2], q[3] };
        }
        return shapeBuilder.build(vertex, new double[] { Mathf.Sqrt(2)/PHI }, new int[] { 3 }, new int[] { 4 });
    }

    public static Geom.Shape cone(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[9][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { i % 2 * 2 - 1, i / 2 % 2 * 2 - 1, i / 4 * 2 - 1, 1 };
        vertex[8] = new double[] { 0, 0, 0, 0 };
        return shapeBuilder.build(vertex, new double[] { 2 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape cone1(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[9][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { 1, i / 2 % 2 * 2 - 1, i / 4 * 2 - 1, -(i % 2 * 2 - 1) };
        vertex[8] = new double[] { 0, 0, 0, 0 };
        return shapeBuilder.build(vertex, new double[] { 2 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape cone2(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[9][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { -1, i / 2 % 2 * 2 - 1, i / 4 * 2 - 1, i % 2 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0, 0 };
        return shapeBuilder.build(vertex, new double[] { 2 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape cone3(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[9][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { i % 2 * 2 - 1, 1, i / 4 * 2 - 1, -(i / 2 % 2 * 2 - 1) };
        vertex[8] = new double[] { 0, 0, 0, 0 };
        return shapeBuilder.build(vertex, new double[] { 2 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape cone4(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[9][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { i % 2 * 2 - 1, -1, i / 4 * 2 - 1, i / 2 % 2 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0, 0 };
        return shapeBuilder.build(vertex, new double[] { 2 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape cone5(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[9][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { i % 2 * 2 - 1, i / 2 % 2 * 2 - 1, 1, -(i / 4 * 2 - 1) };
        vertex[8] = new double[] { 0, 0, 0, 0 };
        return shapeBuilder.build(vertex, new double[] { 2 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape cone6(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[9][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { i % 2 * 2 - 1, i / 2 % 2 * 2 - 1, -1, i / 4 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0, 0 };
        return shapeBuilder.build(vertex, new double[] { 2 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape flat(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[10][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { 1, i % 2 * 2 - 1, i / 2 % 2 * 2 - 1, i / 4 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0,  -0.5 };
        vertex[9] = new double[] { 0, 0, 0, -1 };
        return shapeBuilder.build(vertex, new double[] { Math.Sqrt(5.25), 2, Math.Sqrt(3), 0.5 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape flat1(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[10][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { -(i % 2 * 2 - 1), 1, i / 2 % 2 * 2 - 1, i / 4 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0,  -0.5 };
        vertex[9] = new double[] { 0, 0, 0, -1 };
        return shapeBuilder.build(vertex, new double[] { Math.Sqrt(5.25), 2, Math.Sqrt(3), 0.5 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape flat2(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[10][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { i % 2 * 2 - 1, -1, i / 2 % 2 * 2 - 1, i / 4 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0,  -0.5 };
        vertex[9] = new double[] { 0, 0, 0, -1 };
        return shapeBuilder.build(vertex, new double[] { Math.Sqrt(5.25), 2, Math.Sqrt(3), 0.5 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape flat3(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[10][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { -(i / 2 % 2 * 2 - 1), i % 2 * 2 - 1, 1, i / 4 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0,  -0.5 };
        vertex[9] = new double[] { 0, 0, 0, -1 };
        return shapeBuilder.build(vertex, new double[] { Math.Sqrt(5.25), 2, Math.Sqrt(3), 0.5 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape flat4(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[10][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { i / 2 % 2 * 2 - 1, i % 2 * 2 - 1, -1, i / 4 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0,  -0.5 };
        vertex[9] = new double[] { 0, 0, 0, -1 };
        return shapeBuilder.build(vertex, new double[] { Math.Sqrt(5.25), 2, Math.Sqrt(3), 0.5 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape flat5(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[10][];
        for (int i = 0; i < 8; i++) vertex[i] = new double[] { -1, i % 2 * 2 - 1, -(i / 2 % 2 * 2 - 1), i / 4 * 2 - 1 };
        vertex[8] = new double[] { 0, 0, 0,  -0.5 };
        vertex[9] = new double[] { 0, 0, 0, -1 };
        return shapeBuilder.build(vertex, new double[] { Math.Sqrt(5.25), 2, Math.Sqrt(3), 0.5 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static Geom.Shape convex(ShapeBuilder shapeBuilder)
    {
        double[][] vertex = new double[17][];
        for (int i = 0; i < 16; i++) vertex[i] = new double[] { i % 2 * 2 - 1, i / 2 % 2 * 2 - 1, i / 4  % 2 * 2 - 1, i / 8 * 2 - 1 };
        vertex[16] = new double[] { 0, 0, 0, -2 };
        return shapeBuilder.build(vertex, new double[] { 2 }, new int[] { 3, 4 }, new int[] { 5, 6 });
    }

    public static int[][][][] colorList = new int[][][][]
    {
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 3, 4, 1, 2, 0 } },
            new int[][] { new int[] { 1, 0, 0, 0, 0 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 0, 0, 0, 0, 1, 1, 1, 1 } },
            new int[][] { new int[] { 0, 4, 1, 5, 2, 6, 3, 7 } }
            //new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0 } },
            //new int[][] { new int[] { 0, 0, 0, 0, 1, 1, 1, 1 }, new int[] { 3, 4, 1, 2, 0 } },
            //new int[][] { new int[] { 0, 4, 1, 5, 2, 6, 3, 7 }, new int[] { 4, 5, 6, 7, 2 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 3, 3, 2, 2, 1, 1, 0, 0, 0, 0, 1, 1, 2, 2, 3, 3 } },
            new int[][] { new int[] { 2, 1, 3, 2, 3, 2, 4, 3, 1, 0, 2, 1, 2, 1, 3, 2 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 0, 2, 3, 0, 3, 2, 2, 0, 1, 1, 2, 0, 1, 1, 0, 2, 3, 0, 3, 2, 3, 3, 1, 1 } },
            new int[][] { new int[] { 3, 4, 2, 5, 1, 0, 2, 4, 1, 5, 0, 3, 5, 1, 4, 2, 1, 5, 2, 4, 3, 3, 0, 0 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 6, 2, 3, 6, 6, 2, 3, 6, 0, 8, 9, 0, 0, 8, 9, 0, 3, 11, 11, 2, 3, 11, 11, 2, 9, 5, 5, 8, 9, 5, 5, 8, 4, 3, 7, 3, 4, 3, 7, 3, 10, 9, 1, 9, 10, 9, 1, 9, 8, 4, 8, 7, 8, 4, 8, 7, 2, 10, 2, 1, 2, 10, 2, 1, 5, 6, 4, 4, 5, 6, 4, 4, 11, 0, 10, 10, 11, 0, 10, 10, 1, 1, 6, 5, 1, 1, 6, 5, 7, 7, 0, 11, 7, 7, 0, 11, 11, 11, 5, 5, 0, 0, 6, 6, 7, 7, 1, 1, 4, 4, 10, 10, 3, 3, 9, 9, 2, 2, 8, 8 } },
            new int[][] { new int[] { 5, 5, 6, 5, 1, 1, 0, 1, 4, 3, 3, 4, 2, 3, 3, 2, 2, 2, 3, 3, 4, 4, 3, 3, 3, 4, 3, 4, 3, 2, 3, 2, 4, 4, 4, 5, 2, 2, 2, 1, 3, 3, 2, 3, 3, 3, 4, 3, 2, 1, 2, 2, 4, 5, 4, 4, 1, 2, 2, 2, 5, 4, 4, 4, 2, 2, 2, 3, 4, 4, 4, 3, 2, 3, 2, 2, 4, 3, 4, 1, 3, 2, 2, 2, 3, 4, 4, 4, 1, 1, 2, 1, 5, 5, 4, 5, 5, 1, 2, 4, 2, 4, 3, 3, 3, 3, 2, 4, 1, 5, 2, 4, 1, 5, 3, 3, 2, 4, 2, 4 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 7, 1, 1, 7, 1, 0, 2, 2, 2, 0, 5, 0, 1, 7, 2, 5, 0, 5, 5, 7, 7, 1, 1, 7, 1, 0, 2, 2, 2, 0, 5, 0, 1, 7, 2, 5, 0, 5, 5, 7, 7, 1, 1, 7, 1, 0, 2, 2, 2, 0, 5, 0, 1, 7, 2, 5, 0, 5, 5, 7, 7, 1, 1, 7, 1, 0, 2, 2, 2, 0, 5, 0, 1, 7, 2, 5, 0, 5, 5, 7, 3, 3, 8, 8, 8, 7, 3, 7, 3, 7, 6, 1, 6, 6, 1, 6, 1, 1, 7, 8, 3, 3, 8, 8, 8, 7, 3, 7, 3, 7, 6, 1, 6, 6, 1, 6, 1, 1, 7, 8, 3, 3, 8, 8, 8, 7, 3, 7, 3, 7, 6, 1, 6, 6, 1, 6, 1, 1, 7, 8, 3, 3, 8, 8, 8, 7, 3, 7, 3, 7, 6, 1, 6, 6, 1, 5, 1, 1, 7, 8, 3, 3, 7, 7, 3, 3, 9, 2, 4, 7, 4, 4, 4, 9, 9, 9, 2, 2, 7, 2, 3, 3, 7, 7, 3, 3, 9, 2, 4, 7, 4, 4, 4, 9, 9, 9, 2, 2, 7, 2, 3, 3, 7, 7, 3, 3, 9, 2, 4, 7, 4, 4, 4, 9, 9, 9, 2, 2, 7, 2, 3, 3, 7, 7, 3, 3, 9, 2, 4, 7, 4, 4, 4, 9, 9, 9, 2, 2, 7, 2, 9, 5, 9, 9, 5, 4, 6, 6, 6, 4, 1, 1, 5, 5, 1, 1, 4, 9, 4, 6, 9, 5, 9, 9, 5, 4, 6, 6, 6, 4, 1, 1, 5, 5, 1, 1, 4, 9, 4, 6, 9, 5, 9, 9, 5, 4, 6, 6, 6, 4, 1, 1, 5, 5, 1, 1, 4, 9, 4, 6, 9, 5, 9, 9, 5, 4, 6, 6, 6, 4, 1, 1, 5, 5, 1, 1, 4, 9, 4, 6, 8, 8, 6, 6, 6, 4, 2, 2, 4, 4, 0, 0, 2, 2, 0, 8, 4, 8, 6, 0, 8, 8, 6, 7, 6, 4, 2, 2, 4, 4, 0, 0, 2, 2, 0, 8, 4, 8, 6, 0, 8, 8, 6, 6, 6, 4, 2, 2, 4, 4, 0, 0, 2, 2, 0, 8, 4, 8, 6, 0, 8, 8, 6, 6, 6, 4, 2, 2, 4, 4, 0, 0, 2, 2, 0, 8, 4, 8, 6, 0, 9, 5, 0, 8, 3, 0, 0, 8, 8, 8, 5, 9, 5, 5, 9, 9, 0, 3, 3, 3, 9, 5, 0, 8, 3, 0, 0, 8, 8, 8, 5, 9, 5, 5, 9, 9, 0, 3, 3, 3, 9, 5, 0, 8, 3, 0, 0, 8, 8, 8, 5, 9, 5, 5, 9, 9, 0, 3, 3, 3, 9, 5, 0, 8, 3, 0, 0, 8, 8, 8, 5, 9, 5, 5, 9, 9, 0, 3, 3, 3, 8, 1, 6, 8, 3, 8, 1, 6, 8, 3, 8, 1, 6, 8, 3, 8, 1, 6, 8, 3, 5, 3, 0, 0, 0, 5, 3, 0, 0, 0, 5, 3, 0, 0, 0, 5, 3, 0, 0, 0, 9, 9, 3, 9, 2, 9, 9, 3, 9, 2, 9, 9, 3, 9, 2, 9, 9, 3, 9, 2, 4, 6, 4, 2, 8, 4, 6, 4, 2, 8, 4, 6, 4, 2, 8, 4, 6, 4, 2, 8, 5, 2, 7, 7, 7, 5, 2, 6, 7, 7, 5, 2, 7, 7, 7, 5, 2, 7, 7, 7, 4, 1, 1, 5, 6, 4, 1, 1, 5, 6, 4, 1, 1, 5, 6, 4, 1, 1, 5, 6 } },
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 9, 13, 5, 5, 9, 10, 2, 10, 2, 14, 2, 10, 9, 13, 6, 2, 14, 10, 6, 9, 10, 6, 14, 14, 10, 9, 2, 9, 2, 5, 2, 9, 10, 6, 13, 2, 5, 9, 13, 10, 9, 5, 2, 2, 9, 9, 13, 10, 6, 2, 6, 10, 10, 14, 14, 13, 2, 9, 5, 10, 10, 14, 2, 2, 10, 10, 6, 9, 13, 2, 13, 9, 9, 5, 5, 6, 2, 10, 14, 9, 9, 2, 6, 13, 10, 9, 5, 9, 5, 13, 14, 10, 10, 2, 2, 14, 6, 10, 2, 9, 10, 2, 13, 6, 9, 10, 14, 10, 14, 6, 5, 9, 9, 2, 2, 5, 13, 9, 2, 10, 1, 1, 7, 11, 7, 14, 10, 10, 6, 14, 14, 10, 1, 1, 14, 14, 1, 7, 1, 14, 1, 1, 12, 8, 12, 5, 9, 9, 13, 5, 5, 9, 1, 1, 5, 5, 1, 12, 1, 5, 1, 12, 5, 5, 1, 1, 12, 5, 1, 5, 5, 1, 5, 1, 12, 8, 13, 9, 9, 9, 1, 7, 14, 14, 1, 1, 7, 14, 1, 14, 14, 1, 14, 1, 7, 11, 6, 10, 10, 10, 1, 5, 1, 5, 9, 12, 1, 12, 1, 8, 5, 5, 9, 13, 9, 5, 1, 5, 12, 1, 1, 14, 1, 14, 10, 7, 1, 7, 1, 11, 14, 14, 10, 6, 10, 14, 1, 14, 7, 1, 13, 9, 9, 5, 5, 9, 5, 1, 1, 5, 8, 12, 12, 1, 1, 5, 1, 1, 5, 12, 6, 10, 10, 14, 14, 10, 14, 1, 1, 14, 10, 7, 7, 1, 1, 14, 1, 1, 14, 7, 1, 12, 1, 5, 5, 5, 5, 9, 13, 9, 8, 12, 1, 12, 1, 1, 1, 5, 9, 5, 1, 7, 1, 14, 14, 14, 14, 10, 6, 10, 11, 7, 1, 7, 1, 1, 1, 14, 10, 14, 1, 1, 12, 8, 12, 1, 5, 5, 1, 12, 5, 9, 9, 5, 13, 5, 1, 5, 1, 9, 1, 1, 7, 11, 7, 1, 14, 14, 1, 7, 14, 10, 10, 14, 6, 14, 1, 14, 1, 10, 13, 9, 5, 5, 9, 12, 8, 12, 1, 1, 5, 5, 12, 1, 1, 9, 1, 5, 5, 1, 6, 10, 14, 14, 10, 7, 11, 7, 1, 1, 14, 14, 7, 1, 1, 10, 1, 14, 14, 1, 1, 5, 9, 5, 1, 9, 5, 1, 1, 5, 5, 1, 5, 9, 1, 12, 13, 8, 12, 12, 1, 14, 10, 14, 1, 10, 14, 1, 1, 14, 14, 1, 14, 10, 1, 7, 6, 11, 7, 7, 1, 11, 11, 7, 11, 1, 8, 8, 12, 8, 8, 9, 1, 12, 1, 11, 10, 1, 7, 1, 9, 1, 2, 2, 2, 10, 1, 2, 2, 2, 2, 2, 10, 2, 9, 2, 2, 9, 2, 10, 2, 2, 10, 9, 2, 2, 2, 9, 10, 2, 12, 8, 1, 1, 9, 7, 11, 1, 1, 10, 12, 1, 8, 8, 8, 7, 1, 11, 11, 11, 2, 9, 2, 2, 1, 2, 10, 2, 2, 1, 8, 8, 12, 1, 8, 10, 11, 7, 1, 10, 2, 9, 2, 2, 10, 2, 10, 2, 2, 9, 9, 1, 12, 8, 1, 10, 1, 7, 11, 1, 1, 2, 2, 9, 2, 1, 2, 2, 10, 2 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 }
            },
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 1, 1, 1, 1, 1, 1, 1 },
                          new int[] { 2, 2, 2, 2, 2, 2, 2 },
                          new int[] { 3, 3, 3, 3, 3, 3, 3 },
                          new int[] { 4, 4, 4, 4, 4, 4, 4 },
                          new int[] { 5, 5, 5, 5, 5, 5, 5 },
                          new int[] { 6, 6, 6, 6, 6, 6, 6 }
            },
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 }
            },
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
            },
            new int[][] { new int[] { 1, 1, 1, 1, 1, 1, 1 },
                          new int[] { 3, 3, 3, 3, 3, 3, 3 },
                          new int[] { 4, 4, 4, 4, 4, 4, 4 },
                          new int[] { 5, 5, 5, 5, 5, 5, 5 },
                          new int[] { 6, 6, 6, 6, 6, 6, 6 },
                          new int[] { 2, 2, 2, 2, 2, 2, 2 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 }
            },
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
                          new int[] { 0, 0, 0, 0, 0, 0, 0 },
            },
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new int[][] { new int[] { 6, 6, 4, 4, 2, 2, 1, 1, 3, 3, 5, 5, 0 }
            },
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 0, 0, 0, 0, 1, 1, 1, 1 } },
            new int[][] { new int[] { 0, 4, 1, 5, 2, 6, 3, 7 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 0, 0, 0, 0, 1, 1, 1, 1 } },
            new int[][] { new int[] { 0, 4, 1, 5, 2, 6, 3, 7 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 0, 0, 0, 0, 1, 1, 1, 1 } },
            new int[][] { new int[] { 0, 4, 1, 5, 2, 6, 3, 7 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 1, 1, 1, 1, 0, 0, 0, 0 } },
            new int[][] { new int[] { 0, 4, 1, 5, 2, 6, 3, 7 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 1, 1, 1, 1, 0, 0, 0, 0 } },
            new int[][] { new int[] { 0, 4, 1, 5, 2, 6, 3, 7 } }
        },
        new int[][][]
        {
            new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            new int[][] { new int[] { 1, 1, 1, 1, 0, 0, 0, 0 } },
            new int[][] { new int[] { 0, 4, 1, 5, 2, 6, 3, 7 } }
        },
    };
}