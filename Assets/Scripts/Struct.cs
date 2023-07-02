/*
 * Struct.java
 */

/**
 * Some small structure classes.
 */

public class Struct
{

    // --- view info ---

    public class ViewInfo : IDimension
    {

        public double[] origin;
        public double[][] axis;

        public int getDimension()
        {
            return origin.Length; // ugly but it will do for now
                                  // note, not axis.length, since the axis vectors
                                  // might have come from the 4D enumeration constants
        }
    }

    // --- draw info ---

    public class DrawInfo
    {

        public bool[] texture;
        public bool useEdgeColor;
    }

    // --- dimension marker ---

    public const int NONE = 0;
    public const int FOOT = 1;
    public const int COMPASS = 2;
    public const int FOOT_COMPASS = 3;

    public class DimensionMarker : IDimension
    {

        private int dim;
        public DimensionMarker(int dim) { this.dim = dim; }

        public int getDimension() { return dim; }
    }

    // --- finish info ---
    public class FinishInfo
    {

        public int[] finish;
    }

    // --- foot info ---
    public class FootInfo
    {

        public bool foot;
        public bool compass;
    }

    // --- block info ---
    public class BlockInfo { }
}
