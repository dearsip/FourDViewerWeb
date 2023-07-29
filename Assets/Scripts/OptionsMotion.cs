/*
 * OptionsMotion.java
 */

/**
 * Options for speed and granularity of motion.
 */

public class OptionsMotion
{

    // --- fields ---

    public float timeMove; // all times in seconds
    public float timeRotate;
    public float timeAlignMove;
    public float timeAlignRotate;
    public bool paintWithAddButton;

    public static void copy(OptionsMotion dest, OptionsMotion src)
    {
        dest.timeMove = src.timeMove;
        dest.timeRotate = src.timeRotate;
        dest.timeAlignMove = src.timeAlignMove;
        dest.timeAlignRotate = src.timeAlignRotate;
        dest.paintWithAddButton = src.paintWithAddButton;
    }

}

