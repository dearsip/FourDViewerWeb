/*
 * Options.java
 */

using UnityEngine;

/**
 * An object that contains all the option settings that are stored in the options files.
 */
public class Options : ScriptableObject //implements IValidate
{

    // --- fields ---

    public OptionsMap /*om3,*/ om4;
    public OptionsColor /*oc3,*/ oc4;
    public OptionsView /*ov3,*/ ov4;
    public OptionsDisplay od;
    public OptionsControl oo;
    //public OptionsStereo os;
    //public OptionsKeys ok3, ok4;
    //public OptionsKeysConfig okc;
    public OptionsMotion /*ot3,*/ ot4;
    //public OptionsImage oi;

    // --- construction ---

    void OnEnable()
    {

        //om3 = new OptionsMap(3);
        om4 = new OptionsMap(4);
        //oc3 = new OptionsColor();
        oc4 = new OptionsColor();
        //ov3 = new OptionsView();
        ov4 = new OptionsView();
        od = new OptionsDisplay();
        oo = new OptionsControl();
        //os = new OptionsStereo();
        //ok3 = new OptionsKeys(3);
        //ok4 = new OptionsKeys(4);
        //okc = new OptionsKeysConfig();
        //ot3 = new OptionsMotion();
        ot4 = new OptionsMotion();
        //oi = new OptionsImage();
    }

    // --- implementation of IValidate ---

    //public void validate(Key[] key1, Key[] key2) //throws ValidationException
    //{

    //    for (int i1 = 0; i1 < key1.length; i1++)
    //    {
    //        if (!key1[i1].isDefined()) continue; // duplicates of undefined are OK

    //        int base = (key1 == key2) ? i1 + 1 : 0;
    //        for (int i2 = base; i2 < key2.length; i2++)
    //        {
    //            // equals will report false for undefined

    //            if (key1[i1].equals(key2[i2])) throw App.getException("Options.e1", new Object[] { key1[i1]});
    //        }
    //    }
    //}

    //public void validate() //throws ValidationException
    //{

    //    // it turns out there is one cross-validation,
    //    // that there should be no duplicate keys

    //    validate(ok3.key, ok3.key);
    //    validate(ok4.key, ok4.key);
    //    validate(okc.key, okc.key);

    //    validate(ok3.key, okc.key);
    //    validate(ok4.key, okc.key);
    //}

}

