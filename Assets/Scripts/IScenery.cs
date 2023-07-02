/*
 * IScenery.java
 */

/**
 * An interface for scenery objects to draw themselves.
 */
using UnityEngine;

public interface IScenery
{

    void draw(out double[][] texture, out Color[] textureColor, double[] origin);

}

