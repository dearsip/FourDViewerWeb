/*
 * Command.java
 */

using UnityEngine;
using System;
using System.Reflection;
//import java.awt.Color;
//import java.lang.reflect.Array;
//import java.lang.reflect.Method;

/**
 * Utility class containing built-in commands.
 */

public class Command
{

    // --- helpers ---

    // numbers are all stored as Double on the stack

    private static int toInt(object o) { return (int)(double)o; }
    private static double toDbl(object o) { return (double)o; }
    private static bool toBool(object o) { return (bool)o; }

    private static int[] toIntArray(double[] d)
    {
        int[] a = new int[d.Length];
        for (int i = 0; i < d.Length; i++) a[i] = (int)d[i];
        return a;
    }

    private static bool[] toBoolArray(double[] d)
    {
        bool[] a = new bool[d.Length];
        for (int i = 0; i < d.Length; i++) a[i] = (d[i] != 0); // so basically 0 / 1
        return a;
    }

    // --- context commands ---

    public class Dup : ICommand
    { // stack copy
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(c.stack.Peek());
        }
    }

    public class Exch : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            object o2 = c.stack.Pop();
            object o1 = c.stack.Pop();
            c.stack.Push(o2);
            c.stack.Push(o1);
        }
    }

    public class Pop : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            c.stack.Pop();
        }
    }

    public class Index : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            int i = toInt(c.stack.Pop());
            c.stack.Push(c.stack.ToArray()[i]);
        }
    }

    public class Def : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            string s = (string)c.stack.Pop();
            object o = c.stack.Pop();
            try { c.dict.Add(s, o); }
            catch (ArgumentException) { c.dict[s] = o; }
            if (c.isTopLevel())
            {
                c.topLevelDef.Add(s);
            }
            else
            {
                c.topLevelDef.Remove(s);
            }
            // if there are any shape defs in DefaultContext,
            // they'll correctly be counted as not top level
        }
    }

    // --- math commands ---

    public class Add : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double d2 = toDbl(c.stack.Pop());
            double d1 = toDbl(c.stack.Pop());
            c.stack.Push(d1 + d2);
        }
    }

    public class Sub : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double d2 = toDbl(c.stack.Pop());
            double d1 = toDbl(c.stack.Pop());
            c.stack.Push(d1 - d2);
        }
    }

    public class Mul : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double d2 = toDbl(c.stack.Pop());
            double d1 = toDbl(c.stack.Pop());
            c.stack.Push(d1 * d2);
        }
    }

    public class Div : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double d2 = toDbl(c.stack.Pop());
            double d1 = toDbl(c.stack.Pop());
            c.stack.Push(d1 / d2);
        }
    }

    public class Neg : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double d = toDbl(c.stack.Pop());
            c.stack.Push(-d);
        }
    }

    // --- array commands ---

    private static object arrayMark = new object();

    public class ArrayStart : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(arrayMark);
        }
    }

    public class ArrayEnd : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Array arr = c.stack.ToArray();
            int len = Array.IndexOf(arr, arrayMark);
            if (len < 0) throw new Exception("Array start not found.");
            Type cl = c.stack.Peek().GetType();
            object array;
            if (cl == typeof(double))
            { // special handling, convert to double
                double[] d = new double[len];
                array = d;
                for (int i = len - 1; i >= 0; i--) d[i] = toDbl(c.stack.Pop());
            }
            else
            {
                Array a = Array.CreateInstance(cl, len);
                array = a;
                for (int i = len - 1; i >= 0; i--) a.SetValue(c.stack.Pop(), i);
            }
            c.stack.Pop(); // remove mark
            c.stack.Push(array);
        }
    }

    // --- include ---

    public class Include : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            String filename = (String)c.stack.Pop();
            // Language.include(c, filename);
            Language.includeFile = filename;
        }
    }

    // --- setup commands ---

    public class ViewInfo : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Struct.ViewInfo vi = new Struct.ViewInfo();
            vi.axis = (double[][])c.stack.Pop();
            vi.origin = (double[])c.stack.Pop();
            c.stack.Push(vi);
        }
    }

    public class DrawInfo : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Struct.DrawInfo di = new Struct.DrawInfo();
            di.useEdgeColor = toBool(c.stack.Pop());
            di.texture = toBoolArray((double[])c.stack.Pop());
            c.stack.Push(di);
        }
    }

    // --- shape definition ---

    public class Null : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(null);
        }
    }

    public class Edge : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Edge e = new Geom.Edge();
            e.iv2 = toInt(c.stack.Pop());
            e.iv1 = toInt(c.stack.Pop());
            c.stack.Push(e);
        }
    }

    public class ColorEdge : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Edge e = new Geom.Edge();
            e.color = (Color)c.stack.Pop();
            e.iv2 = toInt(c.stack.Pop());
            e.iv1 = toInt(c.stack.Pop());
            c.stack.Push(e);
        }
    }

    public class Face : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Cell f = new Geom.Cell();

            f.normal = (double[])c.stack.Pop(); // can stay null if we can't be bothered

            // center stays null, calculated later

            f.ie = toIntArray((double[])c.stack.Pop());

            c.stack.Push(f);
        }
    }

    public class Shape : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Cell[] face = (Geom.Cell[])c.stack.Pop();
            Geom.Edge[] edge = (Geom.Edge[])c.stack.Pop();
            double[][] vertex = (double[][])c.stack.Pop();
            Geom.Shape s = new Geom.Shape(face, null, edge, vertex);
            c.stack.Push(s);
        }
    }

    public class Texture : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Texture t = new Geom.Texture();
            t.edge = (Geom.Edge[])c.stack.Pop();
            t.vertex = (double[][])c.stack.Pop();
            c.stack.Push(t);
        }
    }

    public class ShapeTexture : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Shape s = (Geom.Shape)c.stack.Pop();
            Geom.Texture t = new Geom.Texture();
            t.edge = s.edge;
            t.vertex = s.vertex;
            c.stack.Push(t);
        }
    }

    public class Glue : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(new Geom.CompositeShape((Geom.ShapeInterface[])c.stack.Pop()));
        }
    }

    public class Unglue : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Pop();
            shape.unglue(c.stack); // so, no effect on Geom.Shape
                                   // not sure this command is useful, but it's harmless
        }
    }

    // --- shape commands ---

    private static void checkUserMove(Geom.MoveInterface shape) //throws Exception
    {
        if (shape.noUserMove()) throw new Exception("Shape cannot be moved.");
    }

    public class NoUserMove : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Peek();
            shape.setNoUserMove();
        }
    }

    public class Idealize : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Shape shape = (Geom.Shape)c.stack.Peek();
            shape.idealize();
        }
    }

    public class Copy : ICommand
    { // shape copy
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(Language.tryCopy(c.stack.Peek()));
        }
    }

    public class Place : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[][] a = (double[][])c.stack.Pop();
            double[] d = (double[])c.stack.Pop();
            Geom.Shape shape = (Geom.Shape)c.stack.Peek();
            shape.place(d, a);
        }
    }

    public class Translate : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[] d = (double[])c.stack.Pop();
            Geom.MoveInterface shape = (Geom.MoveInterface)c.stack.Peek();
            checkUserMove(shape);
            shape.translate(d);
        }
    }

    public class Scale : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[] d = (double[])c.stack.Pop();
            Geom.MoveInterface shape = (Geom.MoveInterface)c.stack.Peek();
            checkUserMove(shape);
            shape.scale(d);
        }
    }

    public class AlignCenter : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[] d = (double[])c.stack.Pop();
            Geom.Shape shape = (Geom.Shape)c.stack.Peek();
            shape.setAlignCenter(d);
        }
    }

    public class Rotate : ICommand
    {
        private bool setOrigin;
        public Rotate(bool setOrigin) { this.setOrigin = setOrigin; }
        public void exec(Context c) //throws Exception
        {
            double[] origin = setOrigin ? (double[])c.stack.Pop() : null;
            double theta = toDbl(c.stack.Pop());
            int dir2 = toInt(c.stack.Pop());
            int dir1 = toInt(c.stack.Pop());
            Geom.MoveInterface shape = (Geom.MoveInterface)c.stack.Peek();
            checkUserMove(shape);
            shape.rotate(dir1, dir2, theta, origin);
        }
    }

    public class Glass : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Peek();
            shape.glass();
        }
    }

    public class ShapeColor : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Color color = (Color)(c.stack.Pop() ?? Color.clear);
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Peek();
            shape.setShapeColor(color);
        }
    }

    public class FaceColor : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Color color = (Color)c.stack.Pop();
            int j = toInt(c.stack.Pop());
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Peek();
            shape.setFaceColor(j, color,/* xor = */ false);
        }
    }

    public class EdgeColor : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Color color = (Color)c.stack.Pop();
            int j = toInt(c.stack.Pop());
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Peek();
            shape.setEdgeColor(j, color);
        }
    }

    public class FaceTexture : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[] value = (double[])c.stack.Pop();
            int mode = toInt(c.stack.Pop());
            Geom.Texture texture = (Geom.Texture)c.stack.Pop();
            int j = toInt(c.stack.Pop());
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Peek();
            shape.setFaceTexture(j, texture, mode, value);
        }
    }

    public class GeneralPolygon : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[][] vertex = (double[][])c.stack.Pop();
            Geom.Shape shape = GeomUtil.genpoly(vertex);
            c.stack.Push(shape);
        }
    }

    public class EdgeToRadius : ICommand
    { // convert "e n" to "r n"
        public void exec(Context c) //throws Exception
        {
            object o = c.stack.Pop(); // might as well save object
            int n = toInt(o);
            double e = toDbl(c.stack.Pop());
            double r = GeomUtil.edgeToRadius(e, n);
            c.stack.Push(r);
            c.stack.Push(o);
        }
    }

    public class EdgeToHeight : ICommand
    { // convert "r n a [min e]" to "r n a [min max]"
        public void exec(Context c) //throws Exception
        {

            // it's unfortunate that you can't control how the height
            // is distributed between min and max, but it's not worth
            // getting into.  cone has a similar problem.

            // don't pop and re-push the arguments,
            // just peek at everything.
            // we can modify the array in place.

            double[] d = (double[])c.stack.Peek();
            int size = c.stack.Count;
            int n = toInt(c.stack.ToArray()[size - 3]);
            double r = toDbl(c.stack.ToArray()[size - 4]);

            d[1] = d[0] + GeomUtil.edgeToHeight(r, n, d[1]);
        }
    }

    public class Polygon : ICommand
    {
        private double offset;
        public Polygon(double offset) { this.offset = offset; }
        public void exec(Context c) //throws Exception
        {
            int n = toInt(c.stack.Pop());
            double r = toDbl(c.stack.Pop());
            double[] d = (double[])c.stack.Pop();
            Geom.Shape shape = GeomUtil.polygon(d[0], d[1], r, n, offset);
            c.stack.Push(shape);
        }
    }

    public class Product : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Shape s2 = (Geom.Shape)c.stack.Pop();
            Geom.Shape s1 = (Geom.Shape)c.stack.Pop();
            c.stack.Push(GeomUtil.product(s1, s2));
        }
    }

    public class Rect : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[][] d = (double[][])c.stack.Pop();
            c.stack.Push(GeomUtil.rect(d));
        }
    }

    public class Prism : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[] d = (double[])c.stack.Pop();
            int a = toInt(c.stack.Pop());
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Pop();
            shape = shape.prism(a, d[0], d[1]);
            c.stack.Push(shape);
        }
    }

    public class Frustum : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[] d = (double[])c.stack.Pop();
            int a = toInt(c.stack.Pop());
            double[] p = (double[])c.stack.Pop();
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Pop();
            shape = shape.frustum(p, a, d[0], d[1], d[2]);
            c.stack.Push(shape);
        }
    }

    public class Cone : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[] d = (double[])c.stack.Pop();
            int a = toInt(c.stack.Pop());
            double[] p = (double[])c.stack.Pop();
            Geom.ShapeInterface shape = (Geom.ShapeInterface)c.stack.Pop();
            shape = shape.cone(p, a, d[0], d[1]);
            c.stack.Push(shape);
        }
    }

    public class Antiprism : ICommand
    {
        private double offset;
        public Antiprism(double offset) { this.offset = offset; }
        public void exec(Context c) //throws Exception
        {
            double[] m = (double[])c.stack.Pop();
            int a = toInt(c.stack.Pop());
            int n = toInt(c.stack.Pop());
            double r = toDbl(c.stack.Pop());
            double[] d = (double[])c.stack.Pop();
            Geom.Shape shape = GeomUtil.antiprism(d[0], d[1], r, n, offset, a, m[0], m[1]);
            c.stack.Push(shape);
        }
    }

    public class TrainPoly : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double bb = toDbl(c.stack.Pop());
            double bf = toDbl(c.stack.Pop());
            double len = toDbl(c.stack.Pop());
            Geom.Shape shape = GeomUtil.train(len, bf, bb);
            c.stack.Push(shape);
        }
    }

    // --- texture commands ---

    public class TextureColor : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Color color = (Color)c.stack.Pop();
            Geom.Texture t = (Geom.Texture)c.stack.Peek();
            t.setTextureColor(color);
        }
    }

    public class Union : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Texture t1 = (Geom.Texture)c.stack.Pop();
            Geom.Texture t2 = (Geom.Texture)c.stack.Pop();
            c.stack.Push(t1.union(t2));
        }
    }

    public class Merge : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Texture t = (Geom.Texture)c.stack.Peek();
            t.merge();
        }
    }

    public class Normalize : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Geom.Texture t = (Geom.Texture)c.stack.Peek();
            t.normalize();
        }
    }

    public class Lift : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double mid = toDbl(c.stack.Pop());
            int a = toInt(c.stack.Pop());
            Geom.Texture t = (Geom.Texture)c.stack.Pop();
            c.stack.Push(GeomUtil.noSplit(t, a, mid));
        }
    }

    public class Project : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            int a = toInt(c.stack.Pop());
            Geom.Texture t = (Geom.Texture)c.stack.Pop();
            c.stack.Push(GeomUtil.project(t, a));
        }
    }

    // --- train commands ---

        public class NewTrack : ICommand
        {
            public void exec(Context c) //throws Exception
            {
                double velStep = toDbl(c.stack.Pop());
                bool expand = toBool(c.stack.Pop());
                Color colorSel = (Color)c.stack.Pop();
                Color color = (Color)c.stack.Pop();
                int arcn = toInt(c.stack.Pop());
                double margin = toDbl(c.stack.Pop());
                double width = toDbl(c.stack.Pop());
                double carScale = toDbl(c.stack.Pop());
                double carLen = toDbl(c.stack.Pop());
                int dim = toInt(c.stack.Pop());
                Track track = new Track(dim, carLen, carScale, width, margin, arcn, color, colorSel, expand, velStep);
                c.stack.Push(track);
            }
        }

        public class NewTrack2 : ICommand
        {
            public void exec(Context c) //throws Exception
            {
                double vradius = toDbl(c.stack.Pop());
                double vtheta = toDbl(c.stack.Pop());
                double vscale = toDbl(c.stack.Pop());
                double velStep = toDbl(c.stack.Pop());
                bool expand = toBool(c.stack.Pop());
                Color colorSel = (Color)c.stack.Pop();
                Color color = (Color)c.stack.Pop();
                int arcn = toInt(c.stack.Pop());
                double margin = toDbl(c.stack.Pop());
                double width = toDbl(c.stack.Pop());
                double carScale = toDbl(c.stack.Pop());
                double carLen = toDbl(c.stack.Pop());
                int dim = toInt(c.stack.Pop());
                Track track = new Track(dim, carLen, carScale, width, margin, arcn, color, colorSel, expand, velStep,
                                        vscale, vtheta, vradius);
                c.stack.Push(track);
            }
        }

        public class Set : ICommand
        {
            private MethodInfo m;
            public Set(Type cl, String methodName) : this(cl, methodName, typeof(double))
            {
            }
            public Set(Type cl, String methodName, Type argType)
            {
                //try
                //{
                    m = cl.GetMethod(methodName, new Type[] { argType });
                //}
                //catch (NoSuchMethodException e)
                //{
                    //throw new RuntimeException(e);
                //}
            }
            public void exec(Context c) //throws Exception
            {
                object arg = c.stack.Pop();
                object o = c.stack.Peek();
                m.Invoke(o, new object[] { arg });
            }
        }

        public class AddTrack : ICommand
        {
            public void exec(Context c) //throws Exception
            {
                String s = ((String)c.stack.Pop()).ToLower();
                int dir2 = toInt(c.stack.Pop());
                int dir1 = toInt(c.stack.Pop());
                int[]
           pos = toIntArray((double[])c.stack.Pop());
                Track track = (Track)c.stack.Peek();
                track.build(pos, dir1, dir2, s);
            }
        }

        public class AddPlatforms : ICommand
        {
            public void exec(Context c) //throws Exception
            {
                int ymax = toInt(c.stack.Pop());
                int ymin = toInt(c.stack.Pop());
                Track track = (Track)c.stack.Pop();
                foreach (Geom.ShapeInterface s in (new Platform()).createPlatforms(track, ymin, ymax))
                    c.stack.Push(s);
                c.stack.Push(track); // float up the stack
            }
        }

        public class AddPlatform : ICommand
        {
            private bool setRounding;
            public AddPlatform(bool setRounding) { this.setRounding = setRounding; }
            public void exec(Context c) //throws Exception
            {
                int rounding = setRounding ? toInt(c.stack.Pop()) : -1;
                int[]
           max = toIntArray((double[])c.stack.Pop());
                int[]
           min = toIntArray((double[])c.stack.Pop());
                Track track = (Track)c.stack.Pop();
                c.stack.Push((new Platform()).createPlatform(track, min, max, rounding));
                c.stack.Push(track); // float up the stack
            }
        }

        public class RoundShape : ICommand
        {
            public void exec(Context c) //throws Exception
            {

                // get parameters

                bool corner = toBool(c.stack.Pop());
                double margin = toDbl(c.stack.Pop());
                int rounding = toInt(c.stack.Pop());
                int[]
           max = toIntArray((double[])c.stack.Pop());
                int[]
           min = toIntArray((double[])c.stack.Pop());

                // compute others

                // note that the y coordinates of min and max will not be used!

                int dim = min.Length;
                // could validate against max.Length, but rplatform doesn't

                if (min[1] != max[1]) throw new Exception("Inconsistent platform level.");
                // only thing rplatform does that we want

                Arc arc = Arc.curve((dim == 4) ? 3 : 7, false, margin, 1);
                // set arcn to 7 in 3D, not worth having more options

                // make the call

                c.stack.Push((new Platform()).getRoundOutline(dim, min, max, rounding, arc, corner));
            }
        }

        public class AddRamp : ICommand
        {
            public void exec(Context c) //throws Exception
            {
                int[]
           pos = toIntArray((double[])c.stack.Pop());
                Track track = (Track)c.stack.Pop();
                c.stack.Push(Platform.createRamp(track, pos));
                c.stack.Push(track); // float up the stack
            }
        }

        public class AddPylon : ICommand
        {
            private bool setBase;
            public AddPylon(bool setBase) { this.setBase = setBase; }
            public void exec(Context c) //throws Exception
            {
                int base_ = setBase ? toInt(c.stack.Pop()) : -1;
                double[]
           dpos = (double[])c.stack.Pop();
                Track track = (Track)c.stack.Pop();
                c.stack.Push(Platform.createPylon(track, dpos, base_));
                c.stack.Push(track); // float up the stack
            }
        }

        public class TrainCtor : ICommand
        {
            public void exec(Context c) //throws Exception
            {
                int trainMode = toInt(c.stack.Pop());
                double d0 = toDbl(c.stack.Pop());
                int toDir = toInt(c.stack.Pop());
                int fromDir = toInt(c.stack.Pop());
                int[]
           pos = toIntArray((double[])c.stack.Pop());
                double gap = toDbl(c.stack.Pop());
                Car[]
                cars = (Car[])c.stack.Pop();
                Train train = new Train(cars, gap, pos, fromDir, toDir, d0, trainMode);
                c.stack.Push(train);
            }
        }

        public class CarCtor : ICommand
        {
            private bool override_;
          public CarCtor(bool override_) { this.override_ = override_; }
        public void exec(Context c) //throws Exception
        {
            double carLenOverride = override_ ? toDbl(c.stack.Pop()) : 0;
        Geom.Shape shape = (Geom.Shape)c.stack.Pop();
        Car car = new Car(shape, carLenOverride);
        c.stack.Push(car);
    }
       }

    // --- scenery ---

    // can use for anything with a no-arg constructor
    public class Construct : ICommand
    {
        private Type cl;
        public Construct(Type cl) { this.cl = cl; }
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(Activator.CreateInstance(cl));
        }
    }

    public class ConstructSetColor : ICommand
    {
        private Type cl;
        public ConstructSetColor(Type cl) { this.cl = cl; }
        public void exec(Context c) //throws Exception
        {
            Color color = (Color)c.stack.Pop();
            object o = Activator.CreateInstance(cl);
            ((Mat.SetColor)o).setColor(color);
            c.stack.Push(o);
        }
    }

    public class MeshRing3 : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double offset = toDbl(c.stack.Pop());
            int n = toInt(c.stack.Pop());
            c.stack.Push(Scenery.ring3(n, offset));
        }
    }

    public class MeshRing4 : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double offset = toDbl(c.stack.Pop());
            int n = toInt(c.stack.Pop());
            c.stack.Push(Scenery.ring4(n, offset));
        }
    }

    public class MeshSphere4 : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double offset = toDbl(c.stack.Pop());
            int n = toInt(c.stack.Pop());
            double[]
       lat = (double[])c.stack.Pop();
            c.stack.Push(Scenery.sphere4(lat, n, offset));
        }
    }

    public class MeshFrame4 : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[]
       d = (double[])c.stack.Pop();
            c.stack.Push(Scenery.frame4(d));
        }
    }

    public class MeshRingFrame4 : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[]
       d = (double[])c.stack.Pop();
            c.stack.Push(Scenery.ringframe4(d));
        }
    }

    public class MeshCube4 : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[]
       d = (double[])c.stack.Pop();
            c.stack.Push(Scenery.cube4(d));
        }
    }

    public class GroundCube3 : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(Scenery.gcube3());
        }
    }

    public class GroundCube4 : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(Scenery.gcube4());
        }
    }

    public class HeightConst : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double height = toDbl(c.stack.Pop());
            c.stack.Push(new Scenery.HeightConstant(height));
        }
    }

    public class HeightPower : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double height = toDbl(c.stack.Pop());
            double n = toDbl(c.stack.Pop());
            c.stack.Push(new Scenery.HeightPower(n, height));
        }
    }

    public class HeightMountain : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double height = toDbl(c.stack.Pop());
            double n = toDbl(c.stack.Pop());
            double min = toDbl(c.stack.Pop());
            double[]
       pos = (double[])c.stack.Pop();
            c.stack.Push(new Scenery.HeightMountain(pos, min, n, height));
        }
    }

    public class HeightMaxN : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Scenery.HeightFunction[]
            f = (Scenery.HeightFunction[])c.stack.Pop();
            c.stack.Push(new Scenery.HeightMaxN(f));
        }
    }

    public class ColorConst : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Color color = (Color)c.stack.Pop();
            c.stack.Push(new Scenery.ColorConstant(color));
        }
    }

    public class ColorDir : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Color[]
            color = (Color[])c.stack.Pop();
            c.stack.Push(new Scenery.ColorDir(color));
        }
    }

    public class ColorBlend : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Color[] color = (Color[])c.stack.Pop();
            c.stack.Push(new Scenery.ColorBlend(color));
        }
    }

    public class Compass : ICommand
    {
        private int dim;
        public Compass(int dim) { this.dim = dim; }
        public void exec(Context c) //throws Exception
        {
            double width = toDbl(c.stack.Pop());
            double length = toDbl(c.stack.Pop());
            double radius = toDbl(c.stack.Pop());
            Scenery.ColorFunction f = (Scenery.ColorFunction)c.stack.Pop();
            c.stack.Push(new Scenery.Compass(dim, f, radius, length, width));
        }
    }

    public class Grid : ICommand
    {
        private int dim;
        public Grid(int dim) { this.dim = dim; }
        public void exec(Context c) //throws Exception
        {
            double interval = toDbl(c.stack.Pop());
            double _base = toDbl(c.stack.Pop());
            bool round = toBool(c.stack.Pop());
            double radius = toDbl(c.stack.Pop());
            Color color = (Color)c.stack.Pop();
            c.stack.Push(new Scenery.Grid(dim, color, radius, round, _base, interval));
        }
    }

    public class Ground : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double radius = toDbl(c.stack.Pop());
            Color color = (Color)c.stack.Pop();
            Geom.Texture mesh = (Geom.Texture)c.stack.Pop();
            c.stack.Push(new Scenery.Ground(mesh, color, radius));
        }
    }

    public class GroundTexture : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double[]
       value = (double[])c.stack.Pop();
            int mode = toInt(c.stack.Pop());
            Color color = (Color)c.stack.Pop();
            Geom.Texture mesh = (Geom.Texture)c.stack.Pop();
            c.stack.Push(new Scenery.GroundTexture(mesh, color, mode, value));
        }
    }

    public class Monolith : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Color color = (Color)c.stack.Pop();
            Geom.Texture mesh = (Geom.Texture)c.stack.Pop();
            c.stack.Push(new Scenery.Monolith(mesh, color));
        }
    }

    public class Horizon : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Scenery.HeightFunction fClip = (Scenery.HeightFunction)c.stack.Pop();
            Scenery.HeightFunction f = (Scenery.HeightFunction)c.stack.Pop();
            Color color = (Color)c.stack.Pop();
            Geom.Texture mesh = (Geom.Texture)c.stack.Pop();
            c.stack.Push(new Scenery.Horizon(mesh, color, f, fClip));
        }
    }

    public class Sky : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Scenery.HeightFunction fClip = (Scenery.HeightFunction)c.stack.Pop();
            double[]
       sunBlend = (double[])c.stack.Pop();
            Color sunColor = (Color)c.stack.Pop();
            double[]
       height = (double[])c.stack.Pop();
            Scenery.ColorFunction f = (Scenery.ColorFunction)c.stack.Pop();
            Geom.Texture mesh = (Geom.Texture)c.stack.Pop();
            c.stack.Push(new Scenery.Sky(mesh, f, height, sunColor, sunBlend, fClip));
        }
    }

    public class Sun : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            double height = toDbl(c.stack.Pop());
            Color color = (Color)c.stack.Pop();
            Geom.Texture mesh = (Geom.Texture)c.stack.Pop();
            c.stack.Push(new Scenery.Sun(mesh, color, height));
        }
    }

    public class FinishInfo : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Struct.FinishInfo fi = new Struct.FinishInfo();
            double[] fn = (double[])c.stack.Pop();
            int[] fni = new int[fn.Length];
            double[][] d = new double[fn.Length][];
            for (int i = 0; i < fn.Length; i++)
            {
                fni[i] = (int)fn[i];
                d[i] = new double[2];
                d[i][0] = fn[i] + 0.25;
                d[i][1] = fn[i] + 0.75;
            }
            Geom.Shape s = GeomUtil.rect(d);
            s.setShapeColor(Color.yellow);
            s.glass();
            c.stack.Push(s);
            fi.finish = fni;
            c.stack.Push(fi);
        }
    }

    public class FootInfo : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            Struct.FootInfo fi = new Struct.FootInfo();
            int fn = toInt(c.stack.Pop());
            fi.foot = ((fn & Struct.FOOT) != 0);
            fi.compass = ((fn & Struct.COMPASS) != 0);
            c.stack.Push(fi);
        }
    }

    public class BlockInfo : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            c.stack.Push(new Struct.BlockInfo());
        }
    }

    public class NewEnemy : ICommand
    {
        public void exec(Context c) //throws Exception
        {
            int enemyType = toInt(c.stack.Pop());
            Geom.Shape shape = (Geom.Shape)c.stack.Pop();
            Enemy e = Enemy.createEnemy(shape, enemyType);
            c.stack.Push(e);
        }
    }
}

