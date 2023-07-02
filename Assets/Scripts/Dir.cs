using System;

/**
 * A utility class for manipulating direction numbers.
 * In D dimensions there are 2*D directions, numbered from 0 to 2*D - 1.
 */
public class Dir
{
// --- constants ---

    public const int DIR_NONE = -1;

// --- accessors ---

    public static int forAxis(int a)
    {
        return a << 1;
    }

    public static int forAxis(int a, bool opposite)
    {
        return (a << 1) + (opposite ? 1 : 0);
    }

    public static int getAxis(int dir)
    {
        return dir >> 1;
    }

    public static bool isPositive(int dir)
    {
        return !((dir & 1) == 1);
    }

    public static int getSign(int dir)
    {
        return ((dir & 1) == 1) ? -1 : 1;
    }

    // --- opposites ---

    public static bool isOpposite(int dir1, int dir2)
    {
        return ((dir1 ^ dir2) == 1);
    }

    public static int getOpposite(int dir)
    {
        return dir ^ 1;
    }

    // --- apply ---

    public static void apply(int dir, int[] p, int distance)
    {
        if ((dir & 1) == 1) distance = -distance;
        p[dir >> 1] += distance;
    }

    public static void apply(int dir, double[] p, double distance)
    {
        if ((dir & 1) == 1) distance = -distance;
        p[dir >> 1] += distance;
    }

    // --- random selection ---

    public static int pick(int dim, Random random)
    {
        return random.Next(2 * dim);
    }

    /**
     * Pick a random direction that is orthogonal to the given direction.
     * (Don't call this when dim is 1.)
     */
    public static int pickOrthogonal(int dir, int dim, Random random)
    {

        // the idea is, if dir is a forward direction,
        // we can add from 2 to (2*D - 1) mod 2*D to get a random orthogonal direction.

        // so, first convert dir to a forward direction
        // you could do this with dir &= ~1, but I prefer the following
        if ((dir & 1) == 1) dir--;

        // then add mod 2*D
        dir += 2 + random.Next(2 * dim - 2);
        dir = dir % (2 * dim);

        return dir;
    }

}
