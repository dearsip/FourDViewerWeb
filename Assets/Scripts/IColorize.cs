/*
 * IColorize.java
 */

using UnityEngine;

/**
 * An interface for determining face colors.
 */

public interface IColorize
{

    Color getColor(int[] p, int dir);
    void setTrace(int[] p);
    int getTrace(int[] p);
    void ResetTrace();

}

