/*
 * OptionsSeed.java
 */

/**
 * Options for random-number generation.
 */

public class OptionsSeed
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
    }
}