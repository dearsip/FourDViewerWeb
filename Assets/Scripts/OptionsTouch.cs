

public class OptionsTouch
{

    // --- fields ---
    
    public float iPD, fovscale, cameraDistanceScale;
    public bool allowDiagonalMovement, alternativeControlIn3D, leftTouchToggleMode, rightTouchToggleMode, showController, showHint, stereo, cross, horizontalInputFollowing;

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
        dest.cross = src.cross;
        dest.horizontalInputFollowing = src.horizontalInputFollowing;
    }
}
