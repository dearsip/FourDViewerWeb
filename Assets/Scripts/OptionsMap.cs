/*
 * OptionsMap.java
 */

/**
 * Options for the size and shape of the map.
 */

public class OptionsMap // implements IValidate
{

    // --- fields ---

    private int dim;

    public int dimMap;
    public int[] size;
    public double density;
    public double twistProbability;
    public double branchProbability;
    public bool allowLoops;
    public double loopCrossProbability;
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

    // --- implementation of IValidate ---

    public const int DIM_MAP_MIN = 1;
    public const int DIM_MAP_MAX = 4;

    public const int SIZE_MIN = 2;
    public const int SIZE_UNUSED = 1;

    public const double DENSITY_MIN = 0;
    public const double DENSITY_MAX = 1;

    public const double PROBABILITY_MIN = 0;
    public const double PROBABILITY_MAX = 1;

    //public void validateDimMap() throws ValidationException
    //{
    //      if (dimMap < DIM_MAP_MIN || dimMap > dim) throw App.getException("OptionsMap.e1",new Object[] { new Integer(DIM_MAP_MIN), new Integer(dim) });
    //   }

    //   public void validate() throws ValidationException
    //{

    //    validateDimMap();

    //      int i = 0;
    //      for ( ; i<dimMap; i++) {
    //        if (size[i] < SIZE_MIN) throw App.getException("OptionsMap.e2", new Object[] { new Integer(SIZE_MIN) });
    //    }
    //      for ( ; i<dim; i++) {
    //        if (size[i] != SIZE_UNUSED) throw App.getException("OptionsMap.e7", new Object[] { new Integer(SIZE_UNUSED) });
    //    }

    //      if (density < DENSITY_MIN || density > DENSITY_MAX) throw App.getException("OptionsMap.e3",new Object[] { new Double(DENSITY_MIN), new Double(DENSITY_MAX) });

    //      if (twistProbability<PROBABILITY_MIN || twistProbability> PROBABILITY_MAX) throw App.getException("OptionsMap.e4",new Object[] { new Double(PROBABILITY_MIN), new Double(PROBABILITY_MAX) });
    //      if (branchProbability<PROBABILITY_MIN || branchProbability> PROBABILITY_MAX) throw App.getException("OptionsMap.e5",new Object[] { new Double(PROBABILITY_MIN), new Double(PROBABILITY_MAX) });
    //      if (loopCrossProbability<PROBABILITY_MIN || loopCrossProbability> PROBABILITY_MAX) throw App.getException("OptionsMap.e6",new Object[] { new Double(PROBABILITY_MIN), new Double(PROBABILITY_MAX) });
    //   }

}

