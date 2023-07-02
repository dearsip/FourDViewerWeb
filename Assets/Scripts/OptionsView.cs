using System;

/*
 * OptionsView.java
 */

/**
 * Options for how the maze is viewed.
 */

public class OptionsView// : IValidate
{

    // --- fields ---

    public int depth;
    public bool[] texture; // 0 is for cell boundaries, 1-9 for wall texture
    public double retina;
    public double scale;

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
        for (int i = 0; i < 10; i++) dest.texture[i] = texture[i];
        dest.retina = src.retina;
        dest.scale = src.scale;
    }

    // --- implementation of IValidate ---

    public const int DEPTH_MIN = 0;
    public const int DEPTH_MAX = 10;

    public const double SCALE_MIN = 0;
    public const double SCALE_MAX = 1;

    //public void validate()
    //{

    //      if (depth < DEPTH_MIN || depth > DEPTH_MAX) throw App.getException("OptionsView.e1",new Object[] { DEPTH_MIN, DEPTH_MAX });

    //      if (retina <= 0) throw App.getException("OptionsView.e2");

    //      if (scale <= SCALE_MIN || scale > SCALE_MAX) throw App.getException("OptionsView.e3",new Object[] { SCALE_MIN, SCALE_MAX });
    //   }

}

