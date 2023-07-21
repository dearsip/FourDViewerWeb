/*
 * GeomUtil.java
 */

using System;

/**
 * Utility functions that operate on geometric shapes.
 */

public class GeomUtil
{

    public static Geom.Edge[] edgeRing(int n)
    {
        Geom.Edge[] edge = new Geom.Edge[n];
        for (int i = 0; i < n; i++)
        {
            edge[i] = new Geom.Edge(i, (i + 1) % n);
        }
        return edge;
    }

    public static Geom.Cell[] faceRing(double[][] vertex)
    {
        int n = vertex.Length;
        int sign = 1;
        Geom.Cell[] face = new Geom.Cell[n];
        for (int i = 0; i < n; i++)
        {
            Geom.Cell f = new Geom.Cell();
            f.ie = new int[1];
            f.ie[0] = i;
            f.normal = new double[2];

            // face i contains edge i which connects vertices i and i+1
            double[] v0 = vertex[i];
            double[] v1 = vertex[(i + 1) % n];

            f.normal[0] = sign * (v0[1] - v1[1]); // (x,y) -> (-y,x)
            f.normal[1] = sign * (v1[0] - v0[0]);

            if (i == 0)
            {
                double[] v2 = vertex[(i + 2) % n];
                if (Vec.dot(f.normal, v2) > Vec.dot(f.normal, v0))
                { // need to change sign
                    sign = -1;
                    Vec.scale(f.normal, f.normal, sign);
                }
            }

            face[i] = f;
        }
        return face;
    }

    public static Geom.Shape genpoly(double[][] vertex)
    {
        Geom.Edge[] edge = edgeRing(vertex.Length);
        Geom.Cell[] face = faceRing(vertex);
        return new Geom.Shape(face, null, edge, vertex);
    }

    public static double edgeToRadius(double e, int n)
    {
        double theta = Math.PI / n;
        return e / (2 * Math.Sin(theta));
    }

    public static double edgeToHeight(double r, int n, double e) //throws Exception
    {
        double theta = Math.PI / n;
        double he = r * Math.Sin(theta);  // half e from edgeToRadius
        double dr = r * (1 - Math.Cos(theta));
        double h2 = e * e - he * he - dr * dr;
        if (h2 <= 0) throw new Exception("Edge is too short.");
        return Math.Sqrt(h2);
    }

    public const double OFFSET_REG = 0;
    public const double OFFSET_ALT = 0.5;

    public static Geom.Shape polygon(double x, double y, double r, int n, double offset)
    {

        // same shape structure works even in 2D, fortunately

        double theta = 2 * Math.PI / n;

        double[][] vertex = new double[n][];
        for (int i = 0; i < n; i++)
        {
            vertex[i] = new double[2];
            vertex[i][0] = x + r * Math.Cos(theta * (i + offset));
            vertex[i][1] = y + r * Math.Sin(theta * (i + offset));
        }

        return genpoly(vertex);
    }

    public static double[] remove(double[] d, int a)
    {
        double[] r = new double[d.Length - 1];
        for (int i = 0; i < a; i++) r[i] = d[i];
        for (int i = a + 1; i < d.Length; i++) r[i - 1] = d[i];
        return r;
    }

    public static double[] insert(double[] d, int a, double val)
    {
        double[] r = new double[d.Length + 1];
        for (int i = 0; i < a; i++) r[i] = d[i];
        r[a] = val;
        for (int i = a; i < d.Length; i++) r[i + 1] = d[i];
        return r;
    }

    public static double[] concat(double[] d1, double[] d2)
    {
        double[] r = new double[d1.Length + d2.Length];
        Array.Copy(d1, 0, r, 0, d1.Length);
        Array.Copy(d2, 0, r, d1.Length, d2.Length);
        return r;
    }

    public static Geom.Shape product(Geom.Shape s1, Geom.Shape s2) //throws Exception
    {

        if (s1.getDimension() != 2 || s2.getDimension() != 2) throw new Exception("Product only works for two-dimensional shapes.");

        // prism is a product with one-dimensional shape,
        // but it's not implemented that way because
        // one-dimensional shapes have a weird structure.

        int n1 = s1.vertex.Length;
        int n2 = s2.vertex.Length;

        int ne1 = s1.edge.Length;
        int ne2 = s2.edge.Length;

        int nf1 = s1.cell.Length;
        int nf2 = s2.cell.Length;

        // vertices - n1*n2 numbered i1*n2+i2

        double[][] vertex = new double[n1 * n2][];
        for (int i1 = 0; i1 < n1; i1++)
        {
            for (int i2 = 0; i2 < n2; i2++)
            {
                vertex[i1 * n2 + i2] = concat(s1.vertex[i1], s2.vertex[i2]);
            }
        }

        // edges - n1*ne2 numbered        i1*ne2+i2 (vertex 1 edge 2)
        //         n2*ne1 numbered n1*ne2+i2*ne1+i1 (vertex 2 edge 1)

        Geom.Edge[] edge = new Geom.Edge[n1 * ne2 + n2 * ne1];

        for (int i1 = 0; i1 < n1; i1++)
        {
            for (int i2 = 0; i2 < ne2; i2++)
            {
                Geom.Edge e2 = s2.edge[i2];
                edge[i1 * ne2 + i2] = new Geom.Edge(i1 * n2 + e2.iv1, i1 * n2 + e2.iv2);
            }
        }

        for (int i2 = 0; i2 < n2; i2++)
        {
            for (int i1 = 0; i1 < ne1; i1++)
            {
                Geom.Edge e1 = s1.edge[i1];
                edge[n1 * ne2 + i2 * ne1 + i1] = new Geom.Edge(e1.iv1 * n2 + i2, e1.iv2 * n2 + i2);
            }
        }

        // faces - ne1*ne2 numbered            i1*ne2+i2 (  edge 1   edge 2) 
        //          n1     numbered ne1*ne2+          i2 (  cell 1 vertex 2)
        //              n2 numbered ne1*ne2+n2+       i1 (vertex 1   cell 2)

        Geom.Face[] face = new Geom.Face[ne1 * ne2 + n1 + n2];

        for (int i1 = 0; i1 < ne1; i1++)
        {
            Geom.Edge e1 = s1.edge[i1];
            for (int i2 = 0; i2 < ne2; i2++)
            {
                Geom.Edge e2 = s2.edge[i2];
                Geom.Face f = new Geom.Face();
                f.iv = new int[4];
                f.iv[0] = e1.iv1 * n2 + e2.iv1;
                f.iv[1] = e1.iv1 * n2 + e2.iv2;
                f.iv[2] = e1.iv2 * n2 + e2.iv2;
                f.iv[3] = e1.iv2 * n2 + e2.iv1;
                face[i1 * ne2 + i2] = f;
            }
        }

        for (int i2 = 0; i2 < n2; i2++)
        {
            Geom.Face f = new Geom.Face();
            f.iv = new int[n1];
            for (int i1 = 0; i1 < n1; i1++) f.iv[i1] = i1 * n2 + i2;
            face[ne1 * ne2 + i2] = f;
        }

        for (int i1 = 0; i1 < n1; i1++)
        {
            Geom.Face f = new Geom.Face();
            f.iv = new int[n2];
            for (int i2 = 0; i2 < n2; i2++) f.iv[i2] = i1 * n2 + i2;
            face[ne1 * ne2 + n2 + i1] = f;
        }

        // cell

        // faces - nf1 numbered     i1
        //         nf2 numbered nf1+i2

        // take a shortcut here.  I think the right formula is,
        // edges in the new face are this combination of parts:
        //
        //      old face vertices * other shape edges
        //    + old face edges    * other shape vertices
        //
        // but wow, that's a pain, especially since we don't have
        // any easy way to know the vertices in a face.
        // so, since we're really only handling the 2D x 2D case,
        // take advantage of that and know that the old face has
        // one edge and two vertices.

        Geom.Cell[] cell = new Geom.Cell[nf1 + nf2];

        double[] zero1 = new double[s1.getDimension()];
        double[] zero2 = new double[s2.getDimension()];

        for (int i1 = 0; i1 < nf1; i1++)
        {
            Geom.Cell f1 = s1.cell[i1];
            int ie1 = f1.ie[0];
            Geom.Edge e1 = s1.edge[ie1];

            Geom.Cell f = new Geom.Cell();

            // <f1 vertices>*ne2 + <f1 edges>*n2 = 2*ne2 + n2
            f.ie = new int[2 * ne2 + n2];
            for (int k = 0; k < ne2; k++)
            { // vertex 1 edge 2
                f.ie[k] = e1.iv1 * ne2 + k;
                f.ie[ne2 + k] = e1.iv2 * ne2 + k;
            }
            for (int k = 0; k < n2; k++)
            {  // vertex 2 edge 1
                f.ie[2 * ne2 + k] = n1 * ne2 + k * ne1 + ie1;
            }

            f.ifa = new int[2 + ne2];
            f.ifa[0] = ne1 * ne2 + n2 + e1.iv1;
            f.ifa[1] = ne1 * ne2 + n2 + e1.iv2;
            for (int i2 = 0; i2 < ne2; i2++) f.ifa[2 + i2] = ie1 * ne2 + i2;

            f.normal = concat(f1.normal, zero2);

            cell[i1] = f;
        }

        for (int i2 = 0; i2 < nf2; i2++)
        {
            Geom.Cell f2 = s2.cell[i2];
            int ie2 = f2.ie[0];
            Geom.Edge e2 = s2.edge[ie2];

            Geom.Cell f = new Geom.Cell();

            // <f2 vertices>*ne1 + <f2 edges>*n1 = 2*ne1 + n1
            f.ie = new int[2 * ne1 + n1];
            for (int k = 0; k < ne1; k++)
            { // vertex 2 edge 1
                f.ie[k] = n1 * ne2 + e2.iv1 * ne1 + k;
                f.ie[ne1 + k] = n1 * ne2 + e2.iv2 * ne1 + k;
            }
            for (int k = 0; k < n1; k++)
            {  // vertex 1 edge 2
                f.ie[2 * ne1 + k] = k * ne2 + ie2;
            }

            f.ifa = new int[2 + ne1];
            f.ifa[0] = ne1 * ne2 + e2.iv1;
            f.ifa[1] = ne1 * ne2 + e2.iv2;
            for (int i1 = 0; i1 < ne1; i1++) f.ifa[2 + i1] = i1 * ne2 + ie2;

            f.normal = concat(zero1, f2.normal);

            cell[nf1 + i2] = f;
        }

        // finish up

        return new Geom.Shape(cell, face, edge, vertex);
    }

    public static Geom.Shape rect(double xmin, double xmax, double ymin, double ymax)
    {

        // not using genpoly because I want the order of the faces to be x=0 x=1 y=0 y=1

        double[][] vertex = new double[][] {
         new double[] { xmin, ymin },
         new double[] { xmin, ymax },
         new double[] { xmax, ymin },
         new double[] { xmax, ymax }  };

        Geom.Edge[] edge = new Geom.Edge[] {
         new Geom.Edge(0,1),
         new Geom.Edge(0,2),
         new Geom.Edge(1,3),
         new Geom.Edge(2,3)  };

        Geom.Cell[] face = new Geom.Cell[4];
        for (int i = 0; i < face.Length; i++) face[i] = new Geom.Cell();

        face[0].ie = new int[] { 0 };
        face[1].ie = new int[] { 3 };
        face[2].ie = new int[] { 1 };
        face[3].ie = new int[] { 2 };

        face[0].normal = new double[] { -1, 0 };
        face[1].normal = new double[] { 1, 0 };
        face[2].normal = new double[] { 0, -1 };
        face[3].normal = new double[] { 0, 1 };

        return new Geom.Shape(face, null, edge, vertex);
    }

    public static Geom.Shape rect(double[][] d) //throws Exception
    {

        if (d.Length < 2) throw new Exception("Rectangle must have at least two dimensions.");

        Geom.Shape s = rect(d[0][0], d[0][1], d[1][0], d[1][1]);
        for (int a = 2; a < d.Length; a++)
        {
            s = prism(s, a, d[a][0], d[a][1]);
        }

        return s;
    }

    public static Geom.Shape prism(Geom.Shape s, int a, double min, double max)
    {

        int n = s.vertex.Length;
        int d = s.getDimension();

        int ne = s.edge.Length;
        int nf = s.cell.Length;

        int j;

        // vertices 0-(n-1) are at min, n-(2n-1) are at max

        double[][] vertex = new double[2 * n][];
        for (int i = 0; i < n; i++)
        {
            vertex[i] = insert(s.vertex[i], a, min);
            vertex[i + n] = insert(s.vertex[i], a, max);
        }

        // edges are at min, at max, and then one per vertex

        Geom.Edge[] edge = new Geom.Edge[2 * ne + n];
        j = 0;

        for (int i = 0; i < ne; i++)
        {
            Geom.Edge e = new Geom.Edge();
            e.iv1 = s.edge[i].iv1;
            e.iv2 = s.edge[i].iv2;
            edge[j++] = e;
        }
        for (int i = 0; i < ne; i++)
        {
            Geom.Edge e = new Geom.Edge();
            e.iv1 = s.edge[i].iv1 + n;
            e.iv2 = s.edge[i].iv2 + n;
            edge[j++] = e;
        }
        for (int i = 0; i < n; i++)
        {
            Geom.Edge e = new Geom.Edge();
            e.iv1 = i;
            e.iv2 = i + n;
            edge[j++] = e;
        }

        // faces are one per old face plus caps at the ends

        Geom.Cell[] face = new Geom.Cell[nf + 2];
        j = 0;

        for (int i = 0; i < nf; i++)
        {
            Geom.Cell f = new Geom.Cell();

            // two edges for every edge on old face, plus one per vertex

            int[] ie = s.cell[i].ie;
            int[] iv = s.cell[i].getVertices(s.edge, s.vertex.Length);

            int fe = ie.Length;
            int fv = iv.Length;

            f.ie = new int[2 * fe + fv];
            for (int k = 0; k < fe; k++)
            {
                f.ie[k] = ie[k];
                f.ie[k + fe] = ie[k] + ne;
            }
            for (int k = 0; k < fv; k++)
            {
                f.ie[k + 2 * fe] = iv[k] + 2 * ne;
            }

            if (s.cell[i].normal != null)
            {
                f.normal = insert(s.cell[i].normal, a, 0);
            }
            else
            {
                f.normal = null; // unknown
            }

            face[j++] = f;
        }

        for (int i = 0; i < 2; i++)
        {
            Geom.Cell f = new Geom.Cell();

            f.ie = new int[ne];
            for (int k = 0; k < ne; k++)
            {
                f.ie[k] = (i == 0) ? k : k + ne;
            }

            f.normal = insert(new double[d], a, (i == 0) ? -1 : 1);

            face[j++] = f;
        }

        // finish up

        return new Geom.Shape(face, null, edge, vertex);
    }

    // cross between prism and cone, but more like cone because same vertex count
    public static Geom.Shape frustum(Geom.Shape s, double[] p, int a, double min, double max, double tip)
    {

        int n = s.vertex.Length;
        int d = s.getDimension();

        int ne = s.edge.Length;
        int nf = s.cell.Length;

        int j;

        double[] temp = new double[d];
        double scale = (tip - max) / (tip - min);

        // vertices 0-(n-1) are at min, n-(2n-1) are at max and scaled

        double[][] vertex = new double[2 * n][];
        for (int i = 0; i < n; i++)
        {
            Vec.mid(temp, p, s.vertex[i], scale);
            vertex[i] = insert(s.vertex[i], a, min);
            vertex[i + n] = insert(temp, a, max);
        }

        // edges are at min, at max, and then one per vertex

        Geom.Edge[] edge = new Geom.Edge[2 * ne + n];
        j = 0;

        for (int i = 0; i < ne; i++)
        {
            Geom.Edge e = new Geom.Edge();
            e.iv1 = s.edge[i].iv1;
            e.iv2 = s.edge[i].iv2;
            edge[j++] = e;
        }
        for (int i = 0; i < ne; i++)
        {
            Geom.Edge e = new Geom.Edge();
            e.iv1 = s.edge[i].iv1 + n;
            e.iv2 = s.edge[i].iv2 + n;
            edge[j++] = e;
        }
        for (int i = 0; i < n; i++)
        {
            Geom.Edge e = new Geom.Edge();
            e.iv1 = i;
            e.iv2 = i + n;
            edge[j++] = e;
        }

        // faces are one per old face plus caps at the ends

        Geom.Cell[] face = new Geom.Cell[nf + 2];
        j = 0;

        for (int i = 0; i < nf; i++)
        {
            Geom.Cell f = new Geom.Cell();

            // two edges for every edge on old face, plus one per vertex

            int[] ie = s.cell[i].ie;
            int[] iv = s.cell[i].getVertices(s.edge, s.vertex.Length);

            int fe = ie.Length;
            int fv = iv.Length;

            f.ie = new int[2 * fe + fv];
            for (int k = 0; k < fe; k++)
            {
                f.ie[k] = ie[k];
                f.ie[k + fe] = ie[k] + ne;
            }
            for (int k = 0; k < fv; k++)
            {
                f.ie[k + 2 * fe] = iv[k] + 2 * ne;
            }

            if (s.cell[i].normal != null)
            {

                // the idea here is that you get the same value if you take the normal dot any
                // point on the face.  parametrize the new normal by values A and B, and then ...
                // (A*n,B) . (fc,min) = (A*n,B) . (p+s(fc-p),max)
                // which boils down to
                // (1-s) * A * n.(fc-p) = B * (max-min)
                // and then we just set A and B equal to the opposite halves, no division needed.
                // at s=0 we get a cone, at s=1 we get a prism, check.
                //
                double A = max - min;
                Vec.sub(temp, s.cell[i].center, p);
                double B = (1 - scale) * Vec.dot(s.cell[i].normal, temp);
                //
                Vec.scale(temp, s.cell[i].normal, A);
                f.normal = insert(temp, a, B);

            }
            else
            {
                f.normal = null; // unknown
            }

            face[j++] = f;
        }

        for (int i = 0; i < 2; i++)
        {
            Geom.Cell f = new Geom.Cell();

            f.ie = new int[ne];
            for (int k = 0; k < ne; k++)
            {
                f.ie[k] = (i == 0) ? k : k + ne;
            }

            f.normal = insert(new double[d], a, (i == 0) ? -1 : 1);

            face[j++] = f;
        }

        // finish up

        return new Geom.Shape(face, null, edge, vertex);
    }

    public static Geom.Shape cone(Geom.Shape s, double[] p, int a, double min, double max)
    {

        int n = s.vertex.Length;
        int d = s.getDimension();

        int ne = s.edge.Length;
        int nf = s.cell.Length;

        int j;

        double[] temp = new double[d];

        // vertices 0-(n-1) are at min, n is at max

        double[][] vertex = new double[n + 1][];
        for (int i = 0; i < n; i++)
        {
            vertex[i] = insert(s.vertex[i], a, min);
        }
        vertex[n] = insert(p, a, max);

        // edges are at min, then one per vertex

        Geom.Edge[] edge = new Geom.Edge[ne + n];
        j = 0;

        for (int i = 0; i < ne; i++)
        {
            Geom.Edge e = new Geom.Edge();
            e.iv1 = s.edge[i].iv1;
            e.iv2 = s.edge[i].iv2;
            edge[j++] = e;
        }
        for (int i = 0; i < n; i++)
        {
            Geom.Edge e = new Geom.Edge();
            e.iv1 = i;
            e.iv2 = n;
            edge[j++] = e;
        }

        // faces are one per old face plus cap at min end

        Geom.Cell[] face = new Geom.Cell[nf + 1];
        j = 0;

        for (int i = 0; i < nf; i++)
        {
            Geom.Cell f = new Geom.Cell();

            // one edge for every edge on old face, plus one per vertex

            int[] ie = s.cell[i].ie;
            int[] iv = s.cell[i].getVertices(s.edge, s.vertex.Length);

            int fe = ie.Length;
            int fv = iv.Length;

            f.ie = new int[fe + fv];
            for (int k = 0; k < fe; k++)
            {
                f.ie[k] = ie[k];
            }
            for (int k = 0; k < fv; k++)
            {
                f.ie[k + fe] = iv[k] + ne;
            }

            if (s.cell[i].normal != null)
            {

                // the idea here is that you get the same value if you take the normal dot any
                // point on the face.  parametrize the new normal by values A and B, and then ...
                // (A*n,B) . (fc,min) = (A*n,B) . (p,max)
                // which boils down to
                // A * n.(fc-p) = B * (max-min)
                // and then we just set A and B equal to the opposite halves, no division needed
                //
                double A = max - min;
                Vec.sub(temp, s.cell[i].center, p);
                double B = Vec.dot(s.cell[i].normal, temp);
                //
                Vec.scale(temp, s.cell[i].normal, A);
                f.normal = insert(temp, a, B);

            }
            else
            {
                f.normal = null; // unknown
            }

            face[j++] = f;
        }

        // same as i == 0 case in prism
        {
            Geom.Cell f = new Geom.Cell();

            f.ie = new int[ne];
            for (int k = 0; k < ne; k++)
            {
                f.ie[k] = k;
            }

            f.normal = insert(new double[d], a, -1);

            face[j++] = f;
        }

        // finish up

        return new Geom.Shape(face, null, edge, vertex);
    }

    public static Geom.Shape antiprism(double x, double y, double r, int n, double offset, // same as polygon
                                       int a, double min, double max)
    {                    // same as prism
        int n2 = 2 * n;
        int j;
        double[] temp = new double[2];

        // vertices are interleaved

        double theta = Math.PI / n;
        offset *= 2; // since theta reduced by a factor of 2

        double[][] vertex = new double[n2][];
        for (int i = 0; i < n2; i++)
        {
            temp[0] = x + r * Math.Cos(theta * (i + offset));
            temp[1] = y + r * Math.Sin(theta * (i + offset));
            vertex[i] = insert(temp, a, ((i & 1) == 0) ? min : max);
        }

        // edges are in pairs ordered by vertex

        Geom.Edge[] edge = new Geom.Edge[4 * n];
        j = 0;

        for (int i = 0; i < n2; i++)
        {
            Geom.Edge e_;

            e_ = new Geom.Edge();
            e_.iv1 = i;
            e_.iv2 = (i + 1) % n2;
            edge[j++] = e_;

            e_ = new Geom.Edge();
            e_.iv1 = i;
            e_.iv2 = (i + 2) % n2;
            edge[j++] = e_;
        }

        // faces are ordered by vertex plus caps at the ends

        Geom.Cell[] face = new Geom.Cell[n2 + 2];
        j = 0;

        // cf. edgeToHeight
        double e = 2 * r * Math.Sin(theta);  // e from edgeToRadius (this will be the Length of temp)
        double dr = r * (1 - Math.Cos(theta));
        double h = max - min;
        double dn = e * dr / h;

        // disadvantage of building polygon and inserting all at once
        int ix = (a == 0) ? 1 : 0;
        int iy = (a == 2) ? 1 : 2;

        for (int i = 0; i < n2; i++)
        {
            Geom.Cell f = new Geom.Cell();

            f.ie = new int[] { 2 * i, 2 * i + 1, 2 * i + 2 };
            if (i == n2 - 1) f.ie[2] = 0;

            double[] v0 = vertex[i];
            double[] v2 = vertex[(i + 2) % n2];
            temp[0] = v2[iy] - v0[iy]; // (x,y) -> (y,-x), correct sign for polygon
            temp[1] = v0[ix] - v2[ix];
            f.normal = insert(temp, a, ((i & 1) == 0) ? -dn : dn);

            face[j++] = f;
        }

        temp[0] = 0;
        temp[1] = 0;

        for (int i = 0; i < 2; i++)
        {
            Geom.Cell f = new Geom.Cell();

            f.ie = new int[n];
            for (int k = 0; k < n; k++)
            {
                f.ie[k] = 4 * k + 2 * i + 1;
            }

            f.normal = insert(temp, a, (i == 0) ? -1 : 1);

            face[j++] = f;
        }

        // finish up

        return new Geom.Shape(face, null, edge, vertex);
    }

    public static double[] pair(double x, double y)
    {
        double[] d = new double[2];
        d[0] = x;
        d[1] = y;
        return d;
    }

    public static Geom.Shape train(double len, double bf, double bb)
    { // bevel front and back

        // all standardized to unit cube shape

        int n = 4;
        if (bf != 0 && bf != 1) n++;
        if (bb != 0 && bb != 1) n++;

        double[][] vertex = new double[n][];
        int i = 0;

        vertex[i++] = pair(0, 0);
        vertex[i++] = pair(len, 0);
        if (bf != 1) vertex[i++] = pair(len, 1 - bf);
        if (bf != 0) vertex[i++] = pair(len - bf, 1);
        if (bb != 0) vertex[i++] = pair(bb, 1);
        if (bb != 1) vertex[i++] = pair(0, 1 - bb);

        return genpoly(vertex);
    }

    /**
     * Prism-like operation on Geom.Texture.
     */
    public static Geom.Texture project(Geom.Texture t, int a)
    {

        int n = t.vertex.Length;

        double[][] vertex = new double[n][];
        for (int i = 0; i < n; i++)
        {
            vertex[i] = remove(t.vertex[i], a);
        }

        return new Geom.Texture(t.edge, vertex);
    }

    /**
     * Prism-like operation on Geom.Texture.
     */
    public static Geom.Texture noSplit(Geom.Texture t, int a, double mid)
    {

        int n = t.vertex.Length;

        double[][] vertex = new double[n][];
        for (int i = 0; i < n; i++)
        {
            vertex[i] = insert(t.vertex[i], a, mid);
        }

        return new Geom.Texture(t.edge, vertex);
    }

    /**
     * Prism-like operation on Geom.Texture.
     */
    public static Geom.Texture split(Geom.Texture t, int a, double min, double max)
    {

        int n = t.vertex.Length;
        int ne = t.edge.Length;

        // vertices 0-(n-1) are at min, n-(2n-1) are at max

        double[][] vertex = new double[2 * n][];
        for (int i = 0; i < n; i++)
        {
            vertex[i] = insert(t.vertex[i], a, min);
            vertex[i + n] = insert(t.vertex[i], a, max);
        }

        // edges are at min and at max

        Geom.Edge[] edge = new Geom.Edge[2 * ne];
        Geom.Edge e;
        for (int i = 0; i < ne; i++)
        {

            e = new Geom.Edge();
            e.iv1 = t.edge[i].iv1;
            e.iv2 = t.edge[i].iv2;
            edge[i] = e;

            e = new Geom.Edge();
            e.iv1 = t.edge[i].iv1 + n;
            e.iv2 = t.edge[i].iv2 + n;
            edge[i + ne] = e;
        }

        // finish up

        return new Geom.Texture(edge, vertex);
    }

}

