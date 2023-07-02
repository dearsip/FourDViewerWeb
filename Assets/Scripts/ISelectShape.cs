/*
 * ISelectShape.java
 */

using System.Collections.Generic;
using UnityEngine;
//import java.awt.Color;
//import java.util.Vector;

/**
 * An interface to connect DialogSelectShape to GeomModel.
 */

public interface ISelectShape
{

    // these are vectors of NamedObject
    List<Color> getAvailableColors();
    List<Geom.Shape> getAvailableShapes();

    Color getSelectedColor();
    Geom.Shape getSelectedShape();
    void setSelectedColor(Color color);
    void setSelectedShape(Geom.Shape shape);

    // special color objects that we recognize by object identity
    //readonly Color RANDOM_COLOR = new Color(0);
    //readonly Color REMOVE_COLOR = new Color(0);

    // ISelectPaint
    Color getPaintColor();
    void setPaintColor(Color color);
    int getPaintMode();
    void setPaintMode(int mode);

}

