/*
 * Grid.java
 */

using System;

/**
 * A utility class for things related to integer coordinates.
 */

public class Grid
{

    // --- non-mathematical operations ---

    public static void copy(int[] dest, int[] src)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src[i];
        }
    }

    public static bool equals(int[] p1, int[] p2)
    {
        for (int i = 0; i < p1.Length; i++)
        {
            if (p1[i] != p2[i]) return false;
        }
        return true;
    }

    public static bool bounds(int[] p1, int[] p2)
    {
        for (int i = 0; i < p1.Length; i++)
        {
            if (p1[i] > p2[i]) return false;
        }
        return true;
    }

    // --- conversions ---

    public static void fromCell(double[] dest, int[] src)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i] = src[i] + 0.5;
        }
    }

    /**
     * Determine the cell or cells that the source point occupies.
     * The calculation assumes that the source point is an open point.
     * @return The direction from dest1 to dest2,
     *         or DIR_NONE if there is only one cell.
     */
    public static int toCell(int[] dest1, int[] dest2, double[] src)
    {

        int dir = Dir.DIR_NONE;

        for (int i = 0; i < dest1.Length; i++)
        {
            dest1[i] = (int)Math.Floor(src[i]); // coordinates can be negative in geom mode
            dest2[i] = dest1[i];

            // is the coordinate an exact integer?
            // if the source point is open, this will happen at most once
            if (dest1[i] == src[i])
            {
                dest2[i]--; // if src were slightly smaller, it would have truncated downward
                dir = Dir.forAxis(i, true); // negative direction from dest1 to dest2
            }
        }

        return dir;
    }

    // --- map functions ---

    /**
     * Check whether the point p is an open point of the map.
     */
    public static bool isOpen(double[] p, Map map, int[] reg1)
    {

        // see how many coordinates are on cell boundaries (cf. toCell)

        int count = 0;
        int dir = Dir.DIR_NONE; // direction to other cell

        for (int i = 0; i < p.Length; i++)
        {
            reg1[i] = (int)p[i];

            if (reg1[i] == p[i])
            {
                count++;
                dir = Dir.forAxis(i, true); // the other cell is always in a negative direction
            }
        }

        // figure out about openness

        switch (count)
        {

            case 0: // cell interior, open if cell is
                if (!map.isOpen(reg1)) return false;
                return true;

            case 1: // cell boundary, open if both cells are
                if (!map.isOpen(reg1)) return false;
                Dir.apply(dir, reg1, 1);
                if (!map.isOpen(reg1)) return false;
                return true;

            default: // more than one boundary, never open
                return false;
        }
    }

    const double epsilon = 0.00001;
    /**
     * Check whether the move from p1 to p2 goes through open points of the map.
     * The point p1 is assumed to be an open point.
     */
    public static bool isOpenMove(double[] p1, double[] p2, Map map, int[] reg1, double[] reg2, bool detectHits, bool glide)
    {

        // first, check the obvious ... the endpoint must be open

        // if (!isOpen(p2, map, reg1)) return false;

        // as for the rest, the important thing is to prevent sneaking diagonally
        // (and the various higher-dimensional analogues)
        //
        //    []
        //      []
        //
        // one could, for example, require that there be an open cells connecting the start and end.
        // however, it turns out that an exact calculation is possible.
        // the idea is, we move p1 (reg2) toward p2, testing the boundary points along the way.
        //
        // the reason we can test just the boundary points for openness,
        // rather than, say, looking at the midpoints between the boundary points,
        // is that the non-open regions are topologically closed.
        // that's also why it's not a special case when the endpoints are boundary points.

        Vec.copy(reg2, p1);

        while (true)
        {

            // find the next boundary point

            int iMin = 0;
            double pMin = 0;
            double fMin = 1; // f = 1 corresponds to moving to p2

            for (int i = 0; i < p1.Length; i++)
            {
                if (reg2[i] == p2[i]) continue; // don't divide by zero

                double p = boundary(reg2[i], p2[i]);
                double f = (p - reg2[i]) / (p2[i] - reg2[i]); // fraction of the way to p2

                if (f < fMin)
                {
                    iMin = i;
                    pMin = p;
                    fMin = f;
                }
            }

            if (fMin == 1) break; // no more boundary points

            // advance to it, taking care to get exactly to the boundary coordinate

            Vec.mid(reg2, reg2, p2, fMin);
            reg2[iMin] = pMin;

            // check that it's open

            if (!isOpen(reg2, map, reg1)) 
            {
                if (!glide || detectHits) return false;
                else p2[iMin] = pMin + ((p2[iMin] < pMin) ? epsilon : -epsilon);
            }
        }

        return true;
    }

    /**
     * Find the next boundary point between d1 and d2, possibly going beyond d2.
     */
    private static double boundary(double d1, double d2)
    {

        double d = (int)d1;
        if (d == d1)
        {
            return (d2 > d1) ? d + 1 : d - 1;
        }
        else
        {
            return (d2 > d1) ? d + 1 : d;
        }
    }

}

