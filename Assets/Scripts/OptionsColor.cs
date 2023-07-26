/*
 * OptionsColor.java
 */

using UnityEngine;

/**
 * Options for how the walls are colored.
 */

public class OptionsColor
{

    // --- fields ---

    public int colorMode;
    public int dimSameParallel;
    public int dimSamePerpendicular;
    public bool[] enable;

    // --- constants ---

    public const int COLOR_MODE_EXTERIOR = 0;
    public const int COLOR_MODE_INTERIOR = 1;
    public const int COLOR_MODE_BY_ORIENTATION = 2;
    public const int COLOR_MODE_BY_DIRECTION = 3;
    public const int COLOR_MODE_BY_TRACE = 4;

    public const int NCOLOR_MODE = 5;

    // assign numbers to all the colors listed in java.awt.Color (except black)
    // so that we can do array stuff with them

    public const int COLOR_RED = 0;
    public const int COLOR_GREEN = 1;
    public const int COLOR_BLUE = 2;

    public const int COLOR_CYAN = 3;
    public const int COLOR_MAGENTA = 4;
    public const int COLOR_YELLOW = 5;

    public const int COLOR_ORANGE = 6;
    public const int COLOR_PINK = 7;

    public const int COLOR_DARK_GRAY = 8;
    public const int COLOR_GRAY = 9;
    public const int COLOR_LIGHT_GRAY = 10;
    public const int COLOR_WHITE = 11;

    public const int NCOLOR = 12;

    // the following table must be kept in sync with the numbers

    public static readonly Color ORANGE = new Color(1f, 0.25f, 0);
    //public static readonly Color PINK = new Color(1, 0.3f, 0.5f);
    public static readonly Color DARKGREEN = new Color(0, 0.3f, 0.0f);
    public static readonly Color PURPLE = new Color(0.6f, 0.2f, 0.9f);
    public static readonly Color DARKGRAY = new Color(0.25f, 0.25f, 0.25f);
    public static readonly Color RIGHTGRAY = new Color(0.7f, 0.7f, 0.7f);

    private static readonly Color[] table = {

      Color.red,
      Color.green,
      Color.blue,

      Color.cyan,
      Color.magenta,
      PURPLE,

      ORANGE,
      DARKGREEN,

      DARKGRAY,
      Color.gray,
      RIGHTGRAY,
      Color.white
};

// --- construction ---

public OptionsColor()
{
    enable = new bool[NCOLOR];
}

// --- structure methods ---

public static void copy(OptionsColor dest, OptionsColor src)
{
    dest.colorMode = src.colorMode;
    dest.dimSameParallel = src.dimSameParallel;
    dest.dimSamePerpendicular = src.dimSamePerpendicular;
    for (int i = 0; i < NCOLOR; i++) dest.enable[i] = src.enable[i];
}

public static bool equals(OptionsColor oc1, OptionsColor oc2)
{
    if (oc1.colorMode != oc2.colorMode) return false;
    if (oc1.dimSameParallel != oc2.dimSameParallel) return false;
    if (oc1.dimSamePerpendicular != oc2.dimSamePerpendicular) return false;
    for (int i = 0; i < NCOLOR; i++) if (oc1.enable[i] != oc2.enable[i]) return false;
    return true;
}

// --- helpers ---

private int getColorCount()
{
    int count = 0;
    for (int i = 0; i < NCOLOR; i++)
    {
        if (enable[i]) count++;
    }
    return count;
}

public Color[] getColors()
{
    Color[] color = new Color[getColorCount()];

    int next = 0;
    for (int i = 0; i < NCOLOR; i++)
    {
        if (enable[i]) color[next++] = table[i];
    }

    return color;
}

    public const int DIM_SAME_MIN = 0;
    public const int DIM_SAME_MAX = 4;
}

