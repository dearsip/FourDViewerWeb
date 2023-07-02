/*
 * IMove.java
 */

/**
 * An interface for moving objects.
 */

public interface IMove
{

    bool canMove(int a, double d);
    void move(double[] d);
    void rotateAngle(double[] from, double[] to);
    Align align();
    bool isAligned();

    /**
     * Propagate changes to origin and axis objects.
     * The saveOrigin and saveAxis arguments are
     * just for reference, restore is handled below.
     */
    bool update(double[] saveOrigin, double[][] saveAxis, double[] viewOrigin);

    void save(double[] saveOrigin, double[][] saveAxis);
    void restore(double[] saveOrigin, double[][] saveAxis);

}

