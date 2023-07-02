/*
 * OptionsSeed.java
 */

/**
 * Options for random-number generation.
 */

public class OptionsSeed //: IValidate
{

    // --- fields ---

    public bool mapSeedSpecified;
    public bool colorSeedSpecified;
    public int mapSeed;
    public int colorSeed;

    // --- helpers ---

    public bool isSpecified()
    {
        return mapSeedSpecified && colorSeedSpecified;
    }

    public void forceSpecified()
    {

        int l = System.Environment.TickCount;

        if (!mapSeedSpecified)
        {
            mapSeed = l * 137;
            mapSeedSpecified = true;
        }

        if (!colorSeedSpecified)
        {
            colorSeed = l * 223;
            colorSeedSpecified = true;
        }

        // multiplying by the extra numbers accomplishes two things:
        //
        //  * it expands the seed from the approximately 40 bits in currentTimeMillis
        //    to the 48 bits used by java.util.Random ... but that doesn't matter much,
        //    because zero bits are as good as any others.
        //
        //  * it makes the two seeds different,
        //    the two random number sequences will still be related,
        //    but not in any way that could be noticed in the game.
    }

    // --- implementation of IValidate ---

    //public void validate()
    //{

    //      if (mapSeed   != 0 && ! mapSeedSpecified  ) throw App.getException("OptionsSeed.e1");
    //      if (colorSeed != 0 && ! colorSeedSpecified) throw App.getException("OptionsSeed.e2");
    //}

}