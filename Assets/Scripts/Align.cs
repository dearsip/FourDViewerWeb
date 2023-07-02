/*
 * Align.java
 */

using System;

/**
 * A helper object that implements alignment commands.
 */

public class Align
{

    // --- fields ---

    private int mode;
    private double[] origin; // origin and axis shared with engine
    private double[][] axis;
    private double d;
    private double theta;

    private int dim;
    private double[] originGoal;
    private double[][] axisGoal;

    private int state;
    private int index;

    private double[] reg1;
    private double[] reg2;

    // --- constants ---

    private const int STATE_TRANSLATE = 0;
    private const int STATE_ROTATE = 1;
    private const int STATE_COMPLETE = 2;

    public const int TRANSLATE_THEN_ROTATE = 0;
    public const int ROTATE_THEN_TRANSLATE = 1;
    public const int ONLY_ROTATE = 2;
    public const int ONLY_TRANSLATE = 3;
    //
    // I think I decided to use TRANSLATE_THEN_ROTATE
    // for player alignment because the other way around,
    // the translation part looks boring;
    // but for blocks we need ROTATE_THEN_TRANSLATE
    // so that things will snap into place properly.
    //
    // I've often wanted to be able to align just the axes,
    // maybe I'll use ONLY_ROTATE for that some day.
    // no plans for ONLY_TRANSLATE, it's just here for fun.

    // --- construction ---

    public Align(double[] origin, double[][] axis) : this(TRANSLATE_THEN_ROTATE, origin, axis)
    {
    }

    public Align(int mode, double[] origin, double[][] axis)
    {

        this.mode = mode;
        this.origin = origin;
        this.axis = axis;
        // d and theta are set in align

        dim = origin.Length;
        originGoal = new double[dim];
        axisGoal = new double[dim][];
        for (int i = 0; i < dim; i++) axisGoal[i] = new double[dim];

        computeGoal();

        state = (mode == TRANSLATE_THEN_ROTATE || mode == ONLY_TRANSLATE) ? STATE_TRANSLATE : STATE_ROTATE;
        index = 0;

        reg1 = new double[dim];
        reg2 = new double[dim];
    }

    // --- external interface ---

    /**
     * Perform a single alignment step.
     * @return True if the alignment is complete.
     */
    public bool align(double d, double theta)
    {
        this.d = d;
        this.theta = theta * Math.PI / 180.0;

        double f = 1;
        while (f > 0 && state != STATE_COMPLETE)
        {
            f = align(f);
        }

        // I thought that doing many things in a single step
        // would look confusing, but actually it's fine.

        // another possibility is to just call align(1).
        // the problem with that is, if you're already mostly aligned,
        // you spend a few frames doing nothing.

        return (state == STATE_COMPLETE);
    }

    /**
     * Perform some fraction of a single alignment step,
     * moving or rotating in one direction only.
     */
    private double align(double f)
    {
        switch (state)
        {

            case STATE_TRANSLATE:
                f = alignMove(index, f);
                if (f > 0)
                {
                    if (++index == dim)
                    {
                        if (mode == TRANSLATE_THEN_ROTATE)
                        {
                            state = STATE_ROTATE;
                            index = 0;
                        }
                        else
                        {
                            snap(); // get rid of small FP errors
                                    // snap also sets STATE_COMPLETE
                        }
                    }
                }
                break;

            case STATE_ROTATE:
                f = alignRotate(index, f);
                if (f > 0)
                {
                    if (++index == dim)
                    {
                        if (mode == ROTATE_THEN_TRANSLATE)
                        {
                            state = STATE_TRANSLATE;
                            index = 0;
                        }
                        else
                        {
                            snap(); // get rid of small FP errors
                                    // snap also sets STATE_COMPLETE
                        }
                    }
                }
                break;

            case STATE_COMPLETE:
                break;
        }

        return f;
    }

    /**
     * Perform the entire alignment all at once.
     */
    public void snap()
    {
        if (state != STATE_COMPLETE)
        {
            if (mode != ONLY_ROTATE)
            {
                Vec.copy(origin, originGoal);
            }
            if (mode != ONLY_TRANSLATE)
            {
                Vec.copyMatrix(axis, axisGoal);
            }
            state = STATE_COMPLETE;
            index = 0;
        }
    }

    // --- alignment testing ---

    // this doesn't quite belong in Grid

    public static bool isAligned(double[] origin, double[][] axis)
    {
        for (int i = 0; i < origin.Length; i++)
        {
            if (!isHalfInteger(origin[i])) return false;
        }
        for (int i = 0; i < axis.Length; i++)
        {
            if (!isUnitDirection(axis[i])) return false;
        }
        return true;
    }

    public static bool isHalfInteger(double d)
    {
        d += 0.5;
        return (d == (int)d);
    }

    public static bool isUnitDirection(double[] axis)
    {
        int count = 0;
        for (int i = 0; i < axis.Length; i++)
        {
            double d = axis[i];
            if (d == 0) ; // good
            else if (d == 1 || d == -1) count++;
            else return false;
        }
        return (count == 1);
    }

    // --- calculations ---

    private void computeGoal()
    {

        // move to cell center

        // it's unfortunate that alignment involves all this memory allocation,
        // but it's not necessary to avoid it,
        // since alignment happens on the order of once per keypress, not once per line.
        // in fact, there is probably some per-keypress allocation built into AWT

        int[] cell1 = new int[dim];
        int[] cell2 = new int[dim];
        int dir = Grid.toCell(cell1, cell2, origin);

        int[] cell = (dir != Dir.DIR_NONE /*&& Math.Random() < 0.5*/) ? cell2 : cell1;
        Grid.fromCell(originGoal, cell);

        // line up axes, starting with forward

        int[] dirs = cell1; // reuse

        computeDirs(dirs, axis);

        for (int a = 0; a < dim; a++)
        {
            Dir.apply(dirs[a], axisGoal[a], 1);
            // other coordinates can stay zero
        }
    }

    public static void computeDirs(int[] dirs, double[][] axis)
    {
        new ComputeContext(dirs, axis).runHybrid();
    }

    public class ComputeContext
    {

        public int[] dirs;
        public double[][] axis;
        public int dim;
        public bool[] usedAxis;
        public bool[] usedDir;
        public int aMax;
        public int iMax;
        public double cMax;

        public ComputeContext(int[] dirs, double[][] axis)
        {
            this.dirs = dirs;
            this.axis = axis;
            dim = dirs.Length;
            usedAxis = new bool[dim]; // all false to start
            usedDir = new bool[dim]; // all false to start
        }

        // basics

        public void reset()
        {
            aMax = -1;
            iMax = -1;
            cMax = -1; // actual c values are nonnegative
        }

        public void scanAll()
        {
            for (int a = 0; a < dim; a++)
            {
                if (usedAxis[a]) continue;

                scanDirs(a);
            }
        }

        public void scanDirs(int a)
        {
            for (int i = 0; i < dim; i++)
            {
                if (usedDir[i]) continue;

                double c = Math.Abs(axis[a][i]);
                if (c > cMax)
                {
                    aMax = a;
                    iMax = i;
                    cMax = c;
                }
            }
        }

        public void accept()
        {
            dirs[aMax] = Dir.forAxis(iMax, (axis[aMax][iMax] < 0));
            usedAxis[aMax] = true;
            usedDir[iMax] = true;
        }

        // pick

        public void pickAxis(int a)
        {
            reset();

            // first, see which real axis we are closest to
            // i.e. take dot products with real axes
            // i.e. look at coordinates of axis vector
            scanDirs(a);

            // now point to that axis, and mark it as used
            accept();
        }

        public void pickBest()
        {
            reset();

            // start with the largest element in the whole matrix
            // instead of the largest element in some arbitrary row.
            // I don't know if this is the ideal algorithm, but
            // at least it's symmetrical across the different axes.
            scanAll();

            accept();
        }

        // run

        public void runAxis()
        {
            for (int a = dim - 1; a >= 0; a--) pickAxis(a);
        }

        public void runBest()
        {
            for (int n = 0; n < dim; n++) pickBest();
        }

        public void runHybrid()
        {
            // forward direction is special, so pick that first,
            // but then use the other method for all the others.
            // this might always be the same as runAxis in 3D,
            // but I've seen it produce different results in 4D.
            pickAxis(dim - 1);
            for (int n = 0; n < dim - 1; n++) pickBest();
        }
    }

    private double alignMove(int a, double fAvailable)
    {
        double dAvailable = d * fAvailable;
        double dDesired = originGoal[a] - origin[a];
        double fDesired = Math.Abs(dDesired) / d;

        if (fDesired > fAvailable)
        { // can't do in one step

            origin[a] += (dDesired > 0) ? dAvailable : -dAvailable;
            fAvailable = 0;

        }
        else
        {

            origin[a] = originGoal[a]; // avoid FP error (but not necessary, see note below)
            fAvailable -= fDesired;
        }

        return fAvailable;
    }

    private double alignRotate(int a, double fAvailable)
    {
        double thetaAvailable = theta * fAvailable;

        double dot = Vec.dot(axisGoal[a], axis[a]);
        if (dot > 1) dot = 1;

        // dot > 1 could happen because of accumulated FP error,
        // and that would cause acos to return NaN
        //
        // dot < -1 can't happen ... we get to choose the sign of the goal axes,
        // so thetaDesired is never more than 90 degrees.

        double thetaDesired = Math.Acos(dot);
        double fDesired = thetaDesired / theta;

        if (fDesired > fAvailable)
        { // can't do in one step

            rotate(a, dot, thetaAvailable);
            fAvailable = 0;

        }
        else
        {

            rotate(a, dot, thetaDesired);
            fAvailable -= fDesired;

            // you might think we'd want to snap the axis now to avoid FP error,
            // but it would get rotated some more later anyway.  we will snap at the end.
        }

        return fAvailable;
    }

    /**
     * Imagine the rotation that moves axis[a] in the direction of axisGoal[a] by theta <i>radians</i>,
     * and apply it to all the axes.
     */
    private void rotate(int a, double dot, double theta)
    {
        if (theta == 0) return;
        // implies that axis and axisGoal are the same, in which case there's no orthogonal vector

        // copy axis[a] into reg1 so that we don't have to worry about changing axis[a]
        Vec.copy(reg1, axis[a]);

        // copy axisGoal[a] into reg2, and make it orthogonal to reg1
        Vec.addScaled(reg2, axisGoal[a], reg1, -dot);
        Vec.normalize(reg2, reg2);

        double cos = Math.Cos(theta);
        double sin = Math.Sin(theta);

        for (int i = 0; i < dim; i++)
        {

            // compute components in reg1-reg2 plane
            double x1 = (i == a) ? 1 : 0; // axes are orthonormal
            double x2 = Vec.dot(axis[i], reg2);

            // so, we have  axis[i] = something + x1*     reg1             + x2*     reg2
            // and we want to compute something + x1*(cos*reg1 + sin*reg2) + x2*(cos*reg2 - sin*reg1)

            Vec.addScaled(axis[i], axis[i], reg1, x1 * (cos - 1) - x2 * sin);
            Vec.addScaled(axis[i], axis[i], reg2, x2 * (cos - 1) + x1 * sin);
        }
    }

}

