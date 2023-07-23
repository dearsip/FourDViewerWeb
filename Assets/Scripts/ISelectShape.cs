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
    List<NamedObject<Color>> getAvailableColors();
    List<NamedObject<Geom.Shape>> getAvailableShapes();

    Color getSelectedColor();
    Geom.Shape getSelectedShape();
    void setSelectedColor(Color color);
    void setSelectedShape(Geom.Shape shape);

    // special color objects that we recognize by object identity
    static readonly Color NO_EFFECT_COLOR = new Color(1,0,0,0);
    static readonly Color RANDOM_COLOR = new Color(0,1,0,0);
    static readonly Color REMOVE_COLOR = new Color(0,0,1,0);

    // ISelectPaint
    Color getPaintColor();
    void setPaintColor(Color color);
    int getPaintMode();
    void setPaintMode(int mode);

}

