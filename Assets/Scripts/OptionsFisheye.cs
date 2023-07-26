/*
 * OptionsFisheye.java
 */

/**
 * Options for fisheye mode.  These are static for now, but
 * if we ever do integrate them with the rest of the options,
 * they should behave like OptionsStereo - only one instance,
 * not 3D vs. 4D, and not stored in saved games.
 */

public class OptionsFisheye
{

    // --- fields ---

    public bool fisheye;
    public bool adjust;
    public bool rainbow;
    public float width;
    public float flare;
    public float rainbowGap;
    public bool threeDMazeIn3DScene;

    // --- structure methods ---

    public static void copy(OptionsFisheye dest, OptionsFisheye src)
    {
        dest.fisheye = src.fisheye;
        dest.adjust = src.adjust;
        dest.rainbow = src.rainbow;
        dest.width = src.width;
        dest.flare = src.flare;
        dest.rainbowGap = src.rainbowGap;
        dest.threeDMazeIn3DScene = src.threeDMazeIn3DScene;
    }

    // --- implementation of IValidate ---

    //public void validate() throws ValidationException
    //{

    //      if (width <= 0 || width > 1) throw App.getException("OptionsFisheye.e1");
    //      if (flare <  0 || flare > 1) throw App.getException("OptionsFisheye.e2");
    //      if (rainbowGap < 0 || rainbowGap > 1) throw App.getException("OptionsFisheye.e3");
    //}

    // --- constants ---

    // unadjusted
    public const float UA_WIDTH = 1;
    public const float UA_FLARE = 0;
    public const float UA_RGAP = 0.33f;

    // adjusted defaults
    private const float AD_WIDTH = 0.75f;
    private const float AD_FLARE = 0.33f;
    private const float AD_RGAP = 0.5f;

    // --- instance ---

    public static OptionsFisheye of = new OptionsFisheye();
    public static OptionsFisheye ofDefault = new OptionsFisheye();

    static OptionsFisheye()
    {
        of.fisheye = false;
        of.adjust = true;
        of.rainbow = false;
        of.width = AD_WIDTH;
        of.flare = AD_FLARE;
        of.rainbowGap = AD_RGAP;
        of.threeDMazeIn3DScene = false;

        copy(ofDefault, of);

        recalculate();
    }

    // --- calculated properties ---

    public static float offset;
    public static float scale0; // for center cubes
    public static float scale1;
    public static float scale2a;
    public static float scale2b;
    public static float rdist;

    public static void recalculate()
    {

        float w = of.adjust ? of.width : UA_WIDTH;
        float f = of.adjust ? of.flare : UA_FLARE;
        float g = of.adjust ? of.rainbowGap : UA_RGAP;

        float s = 1 + 2 * w;
        // work in coordinates with center cell size 2
        // and side cells size 2w, then scale to [-1,1]

        offset = (1 + w) / s;
        scale0 = 1 / s;
        scale1 = w / s;
        scale2a = (1 + w * f) / s;
        scale2b = w * f / s;
        rdist = (1 + g) / s;

        // scale1 for the axis in the same side direction,
        // scale2a + coord*scale2b for all the other axes.
    }

}

