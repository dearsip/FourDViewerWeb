using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStore {

// --- get functions ---

    bool    getBool   (string key);
    int     getInteger(string key);
    long    getLong   (string key);
    float   getSingle (string key);
    double  getDouble (string key);
    string  getString (string key);

    int? getNullableInteger(string key);

    /**
        * Get data from the store into an object.
        * The object and any substructures and subarrays must exist and be correctly sized.
        */
    void getObject(string key, object o);

    // --- put functions ---

    void putBool   (string key, bool b);
    void putInteger(string key, int i);
    void putLong   (string key, long l);
    void putSingle (string key, float d);
    void putDouble (string key, double d);
    void putString (string key, string s);

    /**
        * Put an object into the store.
        */
    void putObject(string key, object o);
}

