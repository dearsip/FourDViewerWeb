/*
 * OptionsMap.java
 */

/**
 * Options for the size and shape of the map.
 */

public class OptionsMap
{

    // --- fields ---

    private int dim;

    public int dimMap;
    public int[] size;
    public float density;
    public float twistProbability;
    public float branchProbability;
    public bool allowLoops;
    public float loopCrossProbability;
    public bool allowReservedPaths;

    // --- construction ---

    public OptionsMap(int dim)
    {
        this.dim = dim;
        size = new int[dim];
    }

    // --- structure methods ---

    public static void copy(OptionsMap dest, OptionsMap src)
    {
        dest.dim = src.dim;
        dest.dimMap = src.dimMap;
        dest.size = (int[])src.size.Clone(); // can't just copy values, length may be different
        dest.density = src.density;
        dest.twistProbability = src.twistProbability;
        dest.branchProbability = src.branchProbability;
        dest.allowLoops = src.allowLoops;
        dest.loopCrossProbability = src.loopCrossProbability;
        dest.allowReservedPaths = src.allowReservedPaths;
    }

    public const int DIM_MAP_MIN = 1;
    public const int DIM_MAP_MAX = 4;

    public const int SIZE_MIN = 2;
    public const int SIZE_UNUSED = 1;

    public const float DENSITY_MIN = 0;
    public const float DENSITY_MAX = 1;

    public const float PROBABILITY_MIN = 0;
    public const float PROBABILITY_MAX = 1;
}

