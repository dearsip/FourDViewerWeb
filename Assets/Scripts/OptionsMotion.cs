/*
 * OptionsMotion.java
 */

/**
 * Options for speed and granularity of motion.
 */

public class OptionsMotion //implements IValidate
{

    // --- fields ---

    public double frameRate; // per second
    public double timeMove; // all times in seconds
    public double timeRotate;
    public double timeAlignMove;
    public double timeAlignRotate;

    // --- implementation of IValidate ---

    //public void validate() //throws ValidationException
    //{

    //    if (frameRate <= 0) throw App.getException("OptionsMotion.e1");
    //    if (timeMove <= 0) throw App.getException("OptionsMotion.e2");
    //    if (timeRotate <= 0) throw App.getException("OptionsMotion.e3");
    //    if (timeAlignMove <= 0) throw App.getException("OptionsMotion.e4");
    //    if (timeAlignRotate <= 0) throw App.getException("OptionsMotion.e5");
    //}

}

