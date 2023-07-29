

public class OptionsDisplay
{

    // --- fields ---
    
    public float transparency, lineThickness, cameraDistance;
    public float border, mapDistance, size;
    public bool usePolygon, useEdgeColor, hidesel, invertNormals, toggleSkyBox, separate, map, glass, focus, glide;
    public int trainSpeed;

    // --- construction ---

    public OptionsDisplay()
    {
    }

    // --- structure methods ---

    public static void copy(OptionsDisplay dest, OptionsDisplay src)
    {
        dest.transparency = src.transparency;
        dest.lineThickness = src.lineThickness;
        dest.usePolygon = src.usePolygon;
        dest.border = src.border;
        dest.size = src.size;
        dest.useEdgeColor = src.useEdgeColor;
        dest.hidesel = src.hidesel;
        dest.invertNormals = src.invertNormals;
        dest.toggleSkyBox = src.toggleSkyBox;
        dest.separate = src.separate;
        dest.map = src.map;
        dest.glass = src.glass;
        dest.focus = src.focus;
        dest.mapDistance = src.mapDistance;
        dest.cameraDistance = src.cameraDistance;
        dest.trainSpeed = src.trainSpeed;
        dest.glide = src.glide;
    }

    public const float TRANSPARENCY_MIN = 0;
    public const float TRANSPARENCY_MAX = 1;
    public const float LINETHICKNESS_MIN = 0.001f;
    public const float LINETHICKNESS_MAX = 0.01f;
    public const float BORDER_MIN = -1;
    public const float BORDER_MAX = 1;
    public const int CAMERADISTANCE_MIN = 0;
    public const int CAMERADISTANCE_MAX = 100;
    public const int TRAINSPEED_MIN = -5;
    public const int TRAINSPEED_MAX = 5;
}
