/*
 * Permute.java
 */

using System;

/**
 * A utility class for permuting various things in various ways.
 */

public class Permute
{

    // --- the basic function ---

    /**
     * Permute the elements of p.
     */
    public static void permute(int[] p, Random random)
    {
        int temp;

        // a decent algorithm, I think, and in any case better than random retries

        // you could optimize by calling random.nextInt(p.length!)
        // and taking remainders on division by i,
        // but that would only work if p.length! fit within the precision.

        for (int i = p.Length; i > 1; i--)
        {

            // choose one of the i remaining values to go in spot i-1
            // note, iterate only down to i=2, at i=1 there is only one value and one spot left

            int j = random.Next(i);
            if (j != i - 1)
            { // swap j and i-1
                temp = p[i - 1];
                p[i - 1] = p[j];
                p[j] = temp;
            }
        }
    }

    // --- helpers ---

    /**
     * Make an array of length n using the numbers from base to base+(n-1), in order.
     */
    public static int[] sequence(int base_, int n)
    {
        int[] p = new int[n];
        for (int i = 0; i < n; i++) p[i] = base_ + i;
        return p;
    }

    /**
     * Permute the numbers from 0 to n-1.
     */
    public static int[] permute(int n, Random random)
    {
        int[] p = sequence(0, n);
        permute(p, random);
        return p;
    }

    /**
     * Make a random array of length n1 using the numbers from 0 to n2-1,
     * using every number once before using any number twice.
     */
    public static int[] permute(int n1, int n2, Random random)
    {
        int[] p = new int[n1];

        if (n1 <= n2)
        { // there are enough numbers to go around

            int[] q = permute(n2, random);
            Array.Copy(q, 0, p, 0, n1);

        }
        else
        { // have to use some numbers twice

            int i = 0;
            for (; i < n2; i++) p[i] = i;
            for (; i < n1; i++) p[i] = random.Next(n2);
            permute(p, random);
        }

        return p;
    }

}

