/*
 * OptionsAll.java
 */

/**
 * An object that contains all the option settings that can be edited in the options dialog.<p>
 *
 * Unlike the other options objects, OptionsAll does not automatically create its substructures.
 * They may be null, and they may be shared among themselves or with other objects.
 */

public class OptionsAll
{

    public Options opt;
    public OptionsMap omCurrent;
    public OptionsColor ocCurrent;
    public OptionsView ovCurrent; // out of order, but similar to ocCurrent
    public OptionsSeed oeCurrent;
    public OptionsSeed oeNext;

}

