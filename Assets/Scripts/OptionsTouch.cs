

public class OptionsTouch
{

    // --- fields ---
    
    public float iPD, fovscale, cameraDistanceScale;
    public bool allowDiagonalMovement, alternativeControlIn3D, leftTouchToggleMode, rightTouchToggleMode, showController, showHint, stereo, horizontalInputFollowing;

    // --- construction ---

    public OptionsTouch()
    {
    }

    // --- structure methods ---

    public static void copy(OptionsTouch dest, OptionsTouch src)
    {
        dest.iPD = src.iPD;
        dest.fovscale = src.fovscale;
        dest.cameraDistanceScale = src.cameraDistanceScale;
        dest.allowDiagonalMovement = src.allowDiagonalMovement;
        dest.alternativeControlIn3D = src.alternativeControlIn3D;
        dest.leftTouchToggleMode = src.leftTouchToggleMode;
        dest.rightTouchToggleMode = src.rightTouchToggleMode;
        dest.showController = src.showController;
        dest.showHint = src.showHint;
        dest.stereo = src.stereo;
        dest.horizontalInputFollowing = src.horizontalInputFollowing;
    }

    public const double TRANSPARENCY_MIN = 0;
    public const double TRANSPARENCY_MAX = 1;
    public const double LINETHICKNESS_MIN = 0.001;
    public const double LINETHICKNESS_MAX = 0.01;
    public const double BORDER_MIN = -1;
    public const double BORDER_MAX = 1;
    public const int CAMERADISTANCE_MIN = 0;
    public const int CAMERADISTANCE_MAX = 1;
    public const int TRAINSPEED_MIN = -5;
    public const int TRAINSPEED_MAX = 5;
}
