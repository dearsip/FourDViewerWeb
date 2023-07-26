/*
 * Options.java
 */

using UnityEngine;

/**
 * An object that contains all the option settings that are stored in the options files.
 */
public class Options : ScriptableObject
{

    // --- fields ---

    public OptionsMap om4;
    public OptionsColor oc4;
    public OptionsView ov4;
    public OptionsDisplay od;
    public OptionsControl oo;
    public OptionsMotion ot4;
    public OptionsTouch oh;

    // --- construction ---

    void OnEnable()
    {
        om4 = new OptionsMap(4);
        oc4 = new OptionsColor();
        ov4 = new OptionsView();
        od = new OptionsDisplay();
        oo = new OptionsControl();
        ot4 = new OptionsMotion();
        oh = new OptionsTouch();
    }

}

