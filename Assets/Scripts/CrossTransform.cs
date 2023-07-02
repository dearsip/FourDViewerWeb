/*
 * CrossTransform.java
 */

/**
 * A helper class for RenderRelative and OptionsFisheye.
 */

public class CrossTransform : PointTransform
{

    private double[] offset;

    public CrossTransform(double[] offset)
    {
        this.offset = offset;
    }

    public void transform(double[] p)
    {
        Vec.add(p, offset, p);
    }

}

