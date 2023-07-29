

public class OptionsControl
{

    // --- fields ---

    public int inputTypeLeftAndRight, inputTypeForward, inputTypeYawAndPitch, inputTypeRoll;
    public bool invertLeftAndRight, invertForward, invertYawAndPitch, invertRoll, sliceMode, limit3D, showInput, keepUpAndDown;
    public float baseTransparency, sliceTransparency;
    public int sliceDir; // x direction (for intuitive switching)

    // --- constants ---

    public const int INPUTTYPE_JOYSTICK = 0;
    public const int INPUTTYPE_DRAG = 1;

    // --- construction ---

    public OptionsControl()
    {
    }

    // --- structure methods ---

    public static void copy(OptionsControl dest, OptionsControl src)
    {
        dest.inputTypeLeftAndRight = src.inputTypeLeftAndRight;
        dest.inputTypeForward = src.inputTypeForward;
        dest.inputTypeYawAndPitch = src.inputTypeYawAndPitch;
        dest.inputTypeRoll = src.inputTypeRoll;
        dest.invertLeftAndRight = src.invertLeftAndRight;
        dest.invertForward = src.invertForward;
        dest.invertYawAndPitch = src.invertYawAndPitch;
        dest.invertRoll = src.invertRoll;
        dest.sliceMode = src.sliceMode;
        dest.baseTransparency = src.baseTransparency;
        dest.sliceTransparency = src.sliceTransparency;
        dest.limit3D = src.limit3D;
        dest.showInput = src.showInput;
        dest.keepUpAndDown = src.keepUpAndDown;
        dest.sliceDir = src.sliceDir;
    }

}