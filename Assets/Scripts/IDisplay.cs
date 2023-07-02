using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * An interface for controlling a set of displays on screen
 * without really knowing anything about them.
 */

public interface IDisplay
{

    void setMode3D(PolygonBuffer buf);
    void setMode4DMono(PolygonBuffer buf);
    void setMode4DStereo(PolygonBuffer buf1, PolygonBuffer buf2);

    void nextFrame();

}

