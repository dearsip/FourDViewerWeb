/*
 * DynamicArray.java
 */

using System;
using UnityEngine;

/**
 * A utility class for manipulating arrays with variable numbers of dimensions.
 */

public class DynamicArray
{

    // --- helpers ---

    public static int[] makeLimits(int dimSpace, int dimMap, int size)
    {
        int[] limits = new int[dimSpace];
        int i = 0;
        for (; i < dimMap; i++) limits[i] = size + 2;
        for (; i < dimSpace; i++) limits[i] = 3;
        return limits;
    }

    public static int[] makeLimits(int[] size, int dim)
    {

        // we don't need dimSpace and dimMap as arguments,
        // we know by construction that dimSpace = size.Length
        // and by validation that size[i] = 1 for i >= dimMap

        int[] limits = new int[dim];
        for (int i = 0; i < dim; i++) limits[i] = size[i] + 2;
        return limits;
    }

    /**
     * Check whether a cell is in the interior of an array.
     */
    public static bool inBounds(int[] p, int[] limits)
    {
        for (int i = 0; i < limits.Length; i++)
        {
            if (p[i] < 1 || p[i] > limits[i] - 2) return false;
        }
        return true;
    }

    /**
     * Pick a random cell in the interior of an array.
     */
    public static int[] pick(int[] limits, System.Random random)
    {
        int[] p = new int[limits.Length];
        for (int i = 0; i < limits.Length; i++) p[i] = 1 + random.Next(limits[i] - 2);
        return p;
    }

    // --- iterator ---

    /**
     * An object that iterates over an arbitrary-dimensional subspace of an array.
     */
    public class Iterator
    {

        // --- fields ---

        private int[] a;
        private int[] i;
        private int[] limits;
        private bool done;

        // --- construction ---

        /**
         * @param a The axes that define the subspace to iterate over.
         * @param i The initial point.
         *          i[a] should be zero for all elements of the array a.
         * @param limits The limits that define the size of the array.
         */
        public Iterator(int[] a, int[] i, int[] limits)
        {
            this.a = a;
            this.i = (int[])i.Clone();
            this.limits = limits;
            done = false;
        }

        // --- methods ---

        public bool hasCurrent()
        {
            return (!done);
        }

        public int[] current()
        {
            return i; // caller shouldn't modify
        }

        public void increment()
        {

            // this is just adding one to a number with digits in different bases

            for (int j = 0; j < a.Length; j++)
            {
                if (++i[a[j]] < limits[a[j]]) return; // no carry
                i[a[j]] = 0;
            }
            // overflow

            // when iterating over no dimensions, there is exactly one iteration;
            // iterating over any number of dimensions when the limits are 1 is the same

            done = true;
        }
    }

    // --- bool ---

    public class OfBoolean
    {

        // --- fields ---

        private int dim;
        private int[] limits;
        private object data;

        // --- construction ---

        public OfBoolean(int dim, int[] limits)
        {
            this.dim = dim;
            this.limits = limits;
            if (dim == 3)
                data = new bool[limits[0],limits[1],limits[2]];
            else
                data = new bool[limits[0],limits[1],limits[2],limits[3]];
        }

        // --- accessors ---

        public bool get(int[] p)
        {
            if (dim == 3)
            {
                return ((bool[,,])data)[p[0],p[1],p[2]];
            }
            else
            {
                return ((bool[,,,])data)[p[0],p[1],p[2],p[3]];
            }
        }

        public void set(int[] p, bool b)
        {
            if (dim == 3)
            {
                ((bool[,,])data)[p[0],p[1],p[2]] = b;
            }
            else
            {
                ((bool[,,,])data)[p[0],p[1],p[2],p[3]] = b;
            }
        }

        public bool inBounds(int[] p)
        {
            return DynamicArray.inBounds(p, limits);
        }
    }

    // --- Color ---

    public class OfColor
    {

        // --- fields ---

        private int dim;
        private int[] limits;
        private object data;

        // --- construction ---

        public OfColor(int dim, int[] limits)
        {
            this.dim = dim;
            this.limits = limits;
            if (dim == 3)
            {
                data = new Color[limits[0],limits[1],limits[2]];
            }
            else
            {
                data = new Color[limits[0],limits[1],limits[2],limits[3]];
            }
        }

        // --- accessors ---

        public Color get(int[] p)
        {
            if (dim == 3)
            {
                return ((Color[,,])data)[p[0],p[1],p[2]];
            }
            else
            {
                return ((Color[,,,])data)[p[0],p[1],p[2],p[3]];
            }
        }

        public void set(int[] p, Color color)
        {
            if (dim == 3)
            {
                ((Color[,,])data)[p[0],p[1],p[2]] = color;
            }
            else
            {
                ((Color[,,,])data)[p[0],p[1],p[2],p[3]] = color;
            }
        }

        public bool inBounds(int[] p)
        {
            return DynamicArray.inBounds(p, limits);
        }
    }

    // --- integer ---

    public class OfDir
    {

        // --- fields ---

        private int dim;
        private int[] limits;
        private object data;

        // --- construction ---

        public OfDir(int dim, int[] limits)
        {
            this.dim = dim;
            this.limits = limits;
            if (dim == 3)
            {
                data = new int[limits[0],limits[1],limits[2]];
            }
            else
            {
                data = new int[limits[0],limits[1],limits[2],limits[3]];
            }
        }

        // --- accessors ---

        public int get(int[] p)
        {
            if (dim == 3)
            {
                return ((int[,,])data)[p[0],p[1],p[2]] - 1;
            }
            else
            {
                return ((int[,,,])data)[p[0],p[1],p[2],p[3]] - 1;
            }
        }

        public void set(int[] p, int b)
        {
            if (dim == 3)
            {
                ((int[,,])data)[p[0],p[1],p[2]] = b + 1;
            }
            else
            {
                ((int[,,,])data)[p[0],p[1],p[2],p[3]] = b + 1;
            }
        }

        public bool inBounds(int[] p)
        {
            return DynamicArray.inBounds(p, limits);
        }
    }

}

