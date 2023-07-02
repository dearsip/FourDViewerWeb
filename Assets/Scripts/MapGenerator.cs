/*
 * MapGenerator.java
 */

using System;
using System.Collections.Generic;

/**
 * An object for generating maps according to one particular algorithm.
 */

public class MapGenerator
{

    // --- fields ---

    private Map map;
    private int[] limits;
    private OptionsMap om;
    private Random random;

    private List<Borer> avail; // borers
    private List<Borer> reserve;

    private double singleBranchProbability;

    private int[] reg1; // temporary registers
    private int[] reg2;
    private int[] reg3;

    // --- construction ---

    public MapGenerator(Map map, int[] limits, OptionsMap om, int seed)
    {
        this.map = map;
        this.limits = limits;
        this.om = om;
        this.random = new Random(seed);

        avail = new List<Borer>();
        reserve = new List<Borer>();

        singleBranchProbability = computeSingleBranchProbability();

        reg1 = new int[limits.Length]; // dimSpace
        reg2 = new int[limits.Length];
        reg3 = new int[limits.Length];

        init();
    }

    // --- main loop ---

    private void init()
    {
        int[] start = DynamicArray.pick(limits, random);

        map.setStart(start);
        map.setOpen(start, true);

        branch(start, Dir.DIR_NONE, Dir.DIR_NONE);
        //
        // it doesn't really matter whether the new borers are available or on reserve,
        // this is just a convenient and uniform way to create them
        // the probabilities don't actually work out quite right, because of the absent directions
        //
        // (can't just start with one borer, it might immediately hit a wall and die)
    }

    int[] finish;
    public void generate()
    {
        finish = null;

        int count = computeCount();
        while (count-- > 0)
        {

            Borer borer = pickBorer();
            if (borer == null) break; // ran out

            bore(borer);
            finish = borer.p; // OK to share
        }

        map.setFinish(finish);
    }

    // --- borer class ---

    private class Borer
    {

        public int[] p;
        public int dir;

        public Borer(int[] p, int dir)
        {
            this.p = (int[])p.Clone();
            this.dir = dir;
        }

        public void move(int distance)
        {
            Dir.apply(dir, p, distance);
        }
    }

    // --- computations ---

    private int computeCount()
    {

        // total number of interior cells
        int cells = 1;
        for (int i = 0; i < om.dimMap /*&& i < om.size.Length*/; i++)
        {
            cells *= om.size[i];
        }

        // number of open cells we want
        int count = (int)Math.Round(cells * om.density);

        // we've already opened the start
        count -= 1;

        // we always need a finish, regardless of density
        if (count < 1) count = 1;

        return count;
    }

    private double computeSingleBranchProbability()
    {

        // special case when there is no room for branches
        // (the computation below doesn't work correctly in this case)

        if (om.dimMap == 1) return 0; // value doesn't matter

        // when a borer moves, there are 2*dimMap directions in the new cell,
        // but one is already covered by the existing borer, and one is backward,
        // so there are n = 2*dimMap - 2 possible branches.

        double n = 2 * om.dimMap - 2; // use double to avoid 1/n -> 0

        // we want to find a probability for a single branch
        // so that the <i>total</i> probability of obtaining one or more branches
        // is equal to the branch probability.  it goes like this:
        //
        // branch probability = p(some branches)
        //                    = 1 - p(no branches)
        //                    = 1 - [  p(no single branch) ]^n
        //                    = 1 - [ 1 - p(single branch) ]^n
        //
        // you can also think about it in terms of risk = -ln(1-p),
        // in which case the goal is to divide the risk of branching over n directions

        return 1 - Math.Pow(1 - om.branchProbability, 1 / n);
    }

    // --- helpers part 1 ---

    private Borer pickBorer()
    {

        while (avail.Count > 0)
        {
            int i = random.Next(avail.Count);
            Borer borer = (Borer)avail[i];

            if (usable(borer))
            {
                return borer; // done

            }
            else
            { // borer can never become usable, remove it
                avail.RemoveAt(i);
            }
        }

        while (reserve.Count > 0)
        {
            int i = random.Next(reserve.Count);
            Borer borer = (Borer)reserve[i];

            if (usable(borer))
            {
                reserve.RemoveAt(i); // move the borer to the available list, so that we will continue to use it
                avail.Add(borer);
                return borer; // done

            }
            else
            { // borer can never become usable, remove it
                reserve.RemoveAt(i);
            }
        }

        return null; // no more borers, can't reach desired density, sorry
    }

    private void bore(Borer borer)
    {

        // open new cell

        borer.move(1);
        map.setOpen(borer.p, true);

        // maybe turn

        int dirBackward = Dir.getOpposite(borer.dir); // save for branching

        if (om.dimMap > 1 && random.NextDouble() < om.twistProbability)
        {
            borer.dir = Dir.pickOrthogonal(borer.dir, om.dimMap, random);
        }

        // maybe branch

        branch(borer.p, borer.dir, dirBackward);

        // handle loops

        // the only reason we care about creating a loop
        // is that if the borer is pointing into an open cell, it will die.
        // so, in that case, maybe boost it forward one.
        // after that, it's on its own, even though it might still die

        borer.move(1);

        if (!(map.isOpen(borer.p)
                 && random.NextDouble() < om.loopCrossProbability))
        {
            borer.move(-1);
        }

        // note that if we move forward into an open cell,
        // that cell has already been branched, no need to do it again
    }

    /**
     * Create new borers leading off from a cell
     * in all directions except dirForward and dirBackward.
     * The branch probability is used
     * to determine whether the new borers are available.
     */
    private void branch(int[] p, int dirForward, int dirBackward)
    {
        if (!om.allowReservedPaths) reserve.Clear();

        for (int dir = 0; dir < 2 * om.dimMap; dir++)
        { // no need to consider non-map directions

            // dirForward is the direction already covered by the existing borer
            // dirBackward is already known to point to an open cell
            // (other directions might point to open cells; those borers will die when used)
            //
            if (dir == dirForward || dir == dirBackward) continue;

            Borer borer = new Borer(p, dir);
            if (random.NextDouble() < singleBranchProbability)
            {
                avail.Add(borer);
            }
            else
            {
                reserve.Add(borer);
            }
        }
    }

    // --- helpers part 2 ---

    // use registers to avoid accidentally making a permanent change to the borer location
    // without doing a lot of memory allocation

    private bool usable(Borer borer)
    {
        Grid.copy(reg1, borer.p);
        Dir.apply(borer.dir, reg1, 1);

        // don't go out of bounds
        if (!DynamicArray.inBounds(reg1, limits)) return false;

        // never re-open the same cell
        // (looping is when you open a cell <i>adjacent to</i> an open one)
        if (map.isOpen(reg1)) return false;

        // loop test is faster than square test, try it first
        if (!om.allowLoops && wouldMakeLoop(reg1, Dir.getOpposite(borer.dir))) return false;

        // prevent squares (and rooms), just because it is technically very convenient
        if (wouldMakeSquare(reg1)) return false;

        return true;
    }

    private bool wouldMakeLoop(int[] base_, int dirBackward)
    {
        Grid.copy(reg2, base_);

        // it's a loop if any adjacent cell is already open
        // (not counting the one we just came from)

        for (int dir = 0; dir < 2 * om.dimMap; dir++)
        { // no need to consider non-map directions
            if (dir == dirBackward) continue;

            Dir.apply(dir, reg2, 1);
            if (map.isOpen(reg2)) return true;
            Dir.apply(dir, reg2, -1);
        }

        return false;
    }

    private bool wouldMakeSquare(int[] base_)
    {
        Grid.copy(reg2, base_);

        // the plan is, look for an adjacent empty square to use as an axis,
        // then look for squares attached to that axis
        // this is a bit redundant with wouldMakeLoop, but I don't want to combine them,
        // it would be too confusing.

        for (int dir = 0; dir < 2 * om.dimMap; dir++)
        {

            Dir.apply(dir, reg2, 1);
            if (map.isOpen(reg2) && wouldMakeSquare(base_, dir)) return true;
            Dir.apply(dir, reg2, -1);
        }

        return false;
    }

    private bool wouldMakeSquare(int[] base_, int dirAxis)
    {

        // optimization, only need to look for squares in higher-numbered directions

        for (int dir = dirAxis + 1; dir < 2 * om.dimMap; dir++)
        {
            if (Dir.isOpposite(dir, dirAxis)) continue;

            Grid.copy(reg3, base_);

            Dir.apply(dir, reg3, 1);
            if (!map.isOpen(reg3)) continue;

            Dir.apply(dirAxis, reg3, 1);
            if (!map.isOpen(reg3)) continue;

            return true;
        }

        return false;
    }

}

