using System;

/*
 * OptionsView.java
 */

/**
 * Options for how the maze is viewed.
 */

public class OptionsView
{

    // --- fields ---

    public int depth;
    public bool arrow;
    public bool[] texture; // 0 is for cell boundaries, 1-9 for wall texture
    public float retina;
    public float scale;

    // --- construction ---

    public OptionsView()
    {
        texture = new bool[10];
    }

    // --- structure methods ---

    public static void copy(OptionsView dest, OptionsView src)
    {
        copy(dest, src, src.texture);
    }

    public static void copy(OptionsView dest, OptionsView src, bool[] texture)
    {
        dest.depth = src.depth;
        dest.arrow = src.arrow;
        for (int i = 0; i < 10; i++) dest.texture[i] = texture[i];
        dest.retina = src.retina;
        dest.scale = src.scale;
    }

    public const int DEPTH_MIN = 0;
    public const int DEPTH_MAX = 10;

    public const float SCALE_MIN = 0;
    public const float SCALE_MAX = 1;
}

