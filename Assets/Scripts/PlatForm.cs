/*
 * Platform.java
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//import java.util.Iterator;
//import java.util.List<Tile>;
//import java.util.ListIterator;

/**
 * Utility class for creating platforms for elevated trains.
 */

public class Platform
{

    public const int PS_SQUARE = 0;
    public const int PS_ROUND = 1;
    public const int PS_ROUND_MORE = 2;
    public const int PS_THIN = 3;
    public const int PS_THIN_ROUND = 4;

    public static bool isThin(int style)
    {
        return (style == PS_THIN || style == PS_THIN_ROUND);
    }

    // --- auto-generation ---

    public List<Geom.ShapeInterface> createPlatforms(Track track, int ymin, int ymax) //throws Exception
    {
        List<Geom.ShapeInterface> platforms = new List<Geom.ShapeInterface>();
        for (int y = ymin; y <= ymax; y++) createPlatforms(track, y, platforms);
        return platforms;
    }

    public void createPlatforms(Track track, int y, List<Geom.ShapeInterface> platforms) //throws Exception
    {

        List<Track.Tile> tiles = track.findAllTiles(y);

        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (tiles[i].isEmpty() || tiles[i].attached || tiles[i].isRampBottom()) { tiles.RemoveAt(i); continue; }
            // count ramps as part of top level

            if (tiles[i].isRampTop()) { platforms.Add(createRamp(track, tiles[i].pos)); tiles.RemoveAt(i); continue; }
        }

        // now the list contains only eligible platform positions.
        // we never auto-build platforms with empty spaces,
        // so we don't need to keep track of ramps and user-built platforms.

        if (isThin(track.getPlatformStyle()))
        {
            createThinPlatforms(track, tiles, platforms);
        }
        else
        {
            createNormalPlatforms(track, tiles, platforms, y);
        }
    }

    public void createNormalPlatforms(Track track, List<Track.Tile> tiles, List<Geom.ShapeInterface> platforms, int y) //throws Exception
    {

        // this is long but fundamentally not very complicated.
        // what we're doing is finding the largest rectangle
        // (with squareness as tie-breaker), generating a platform,
        // and repeating.

        if (!tiles.Any()) return; // otherwise min and max fail

        // calculate min and max

        int dim = track.getDimension();
        int[]
      min = new int[dim];
        int[] max = new int[dim];

        scan(tiles, min, max);

        min = removeVertical(min);
        max = removeVertical(max);

        // set up the main array

        GenerationContext gc = new GenerationContext(min, max);

        foreach ( Track.Tile tile in tiles)
        {
            gc.main.set(removeVertical(tile.pos), tile);
        }

        // the main loop

        int count = tiles.Count;
        while (count > 0)
        {

            gc.pminSearch(); // get the best platform

            platforms.Add(createPlatform(track, insertVertical(gc.bestMin, y),
                                               insertVertical(gc.bestMax, y), -1));

            gc.main.clear(gc.bestMin, gc.bestMax);
            // no use extracting, we want to let createPlatform do its full
            // normal processing as a double-check that our algorithm is OK

            count -= gc.bestSize; // side benefit of this scoring method
        }
    }

    public static void scan(List<Track.Tile> tiles, int[] min, int[] max)
    {
        bool first = true;
        foreach (Track.Tile tile in tiles)
        {
            int[] pos = tile.pos;
            if (first)
            {
                Grid.copy(min, pos);
                Grid.copy(max, pos);
                first = false;
            }
            else
            {
                for (int j = 0; j < pos.Length; j++)
                {
                    if (pos[j] < min[j]) min[j] = pos[j];
                    if (pos[j] > max[j]) max[j] = pos[j];
                }
            }
        }
    }

    public static int[] removeVertical(int[] p)
    {
        int[] r = new int[p.Length - 1];
        r[0] = p[0];
        for (int i = 2; i < p.Length; i++) r[i - 1] = p[i];
        return r;
    }

    public static int[] insertVertical(int[] p, int yVal)
    {
        int[] r = new int[p.Length + 1];
        r[0] = p[0];
        r[1] = yVal;
        for (int i = 1; i < p.Length; i++) r[i + 1] = p[i];
        return r;
    }

    public class GenerationContext
    {

        public int n; // dim-1
        public int[] min;
        public int[] max;

        public PlatformArray main;
        public PlatformArray mark;

        public int[] pmin;
        public int[] pmax;

        public int[] lim;
        public int[] reg;

        public int[] bestMin;
        public int[] bestMax;
        public int bestSize; // product of dimensions, maximize this
        public int bestScore; // sum of dimensions, secondarily minimize this

        public GenerationContext(int[] min, int[] max)
        {

            n = min.Length;
            this.min = min;
            this.max = max;

            main = new PlatformArray(min, max);
            mark = new PlatformArray(min, max);

            lim = new int[n];
            reg = new int[n];
        }

        public void resetScore()
        {
            bestMin = null;
            bestMax = null;
            bestSize = 0; // there's always at least a size 1 somewhere
            bestScore = 0;
        }

        /**
         * See how the platform from pmin to pmax stacks up.
         */
        public void evaluate()
        {
            int prod = 1;
            int sum = 0;
            for (int i = 0; i < n; i++)
            {
                int d = pmax[i] - pmin[i] + 1;
                prod *= d;
                sum += d;
            }
            if (prod > bestSize || (prod == bestSize && sum < bestScore))
            {
                bestMin = (int[])pmin.Clone();
                bestMax = (int[])pmax.Clone();
                bestSize = prod;
                bestScore = sum;
            }
        }

        public void pminSearch()
        {
            resetScore();

            PlatformArray.Iterator i = new PlatformArray.Iterator(min, max);
            while (i.hasCurrent())
            {
                pmin = i.current();
                object tile = main.get(pmin);

                if (tile != null)
                {
                    calcLimits();
                    pmaxSearch();
                }

                i.increment();
            }
        }

        public void pmaxSearch()
        {
            mark.clearAll();

            PlatformArray.Iterator i = new PlatformArray.Iterator(pmin, lim);
            while (i.hasCurrent())
            {
                pmax = i.current();
                object tile = main.get(pmax);

                if (tile != null && canGrow())
                {
                    mark.set(pmax, tile);
                    evaluate();
                }

                i.increment();
            }
        }

        /**
         * Scan along axes so we don't have to go to max every time.
         */
        public void calcLimits()
        {
            Grid.copy(reg, pmin);
            for (int i = 0; i < n; i++)
            {
                while (true)
                {
                    // can we go further?
                    if (reg[i] == max[i]) break;
                    reg[i]++;
                    if (main.get(reg) == null) { reg[i]--; break; }
                }
                lim[i] = reg[i];
                reg[i] = pmin[i];
            }
        }

        /**
         * Check backward along the axes to see if we can grow here.
         */
        public bool canGrow()
        {
            Grid.copy(reg, pmax);
            for (int i = 0; i < n; i++)
            {
                if (reg[i] == pmin[i]) continue;
                reg[i]--;
                if (mark.get(reg) == null) return false;
                reg[i]++;
            }
            return true;
        }
    }

    public void createThinPlatforms(Track track, List<Track.Tile> tiles, List<Geom.ShapeInterface> platforms) //throws Exception
    {
        foreach (Track.Tile tile in tiles)
        {
            if (tile.attached) continue; // straight platform we already built

            Track.Tile min = tile;
            Track.Tile max = tile;

            int a = tile.getStraightAxis();
            if (a != -1)
            {
                min = elongate(min, a, true);
                max = elongate(max, a, false);
            }

            platforms.Add(createPlatform(track, min.pos, max.pos, -1));
        }
    }

    public static Track.Tile elongate(Track.Tile tile, int a, bool opposite)
    {
        int dir = Dir.forAxis(a, opposite);
        while (true)
        {
            Track.Tile next = tile.neighbor[dir];
            if (next.attached || next.getStraightAxis() != a) break;
            tile = next;
        }
        return tile;
    }

    // --- platforms ---

    public Geom.Shape createPlatform(Track track, int[] min, int[] max, int rounding) //throws Exception
    {

        List<Track.Tile> tiles = track.findAllTiles(min, max);

        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (tiles[i].isEmpty()) { tiles.RemoveAt(i); continue; } // just ignore these

            if (tiles[i].isVertical()) throw new Exception("Platforms must not contain ramps.");

            if (tiles[i].attached) throw new Exception("Tile is already on a platform.");
            tiles[i].attached = true;
        }

        if (min[1] != max[1]) throw new Exception("Inconsistent platform level.");
        double height = min[1] * track.getVScale();

        double thickness = track.getPlatformThickness();
        if (thickness > height) thickness = height;

        Geom.Shape shape;

        switch (track.getPlatformStyle())
        {
            case PS_SQUARE:
                shape = getSquareOutline(track, min, max);
                break;
            case PS_ROUND:
                shape = getRoundOutline(track, min, max, tiles, rounding,/* more = */ false);
                break;
            case PS_ROUND_MORE:
                shape = getRoundOutline(track, min, max, tiles, rounding,/* more = */ true);
                break;
            case PS_THIN:
                shape = getThinOutline(track, min, max, tiles,/* round = */ false);
                break;
            case PS_THIN_ROUND:
                shape = getThinOutline(track, min, max, tiles,/* round = */ true);
                break;
            default:
                throw new Exception("Unimplemented platform style.");
        }

        shape = extend(shape, height, thickness);
        shape.isNoUserMove = true;
        if (track.getPlatformColor() != null) shape.setShapeColor(track.getPlatformColor());
        shape.cell[shape.cell.Length - 1].customTexture = new Track.TileTexture(tiles, height, track);
        return shape;
    }

    public static Track.TileTexture getTileTexture(Geom.Shape shape)
    {
        object o = shape.cell[shape.cell.Length - 1].customTexture;
        return (o is Track.TileTexture) ? (Track.TileTexture)o : null;
    }

    public static Geom.Shape extend(Geom.Shape shape, double height, double thickness)
    {
        return GeomUtil.prism(shape, 1, height - thickness, height);
    }

    // --- square style ---

    public static Geom.Shape getSquareOutline(Track track, int[] min, int[] max)
    {
        if (track.getDimension() == 3)
        {
            return rect(min, max);
        }
        else
        {
            return cube(min, max);
        }
    }

    public static Geom.Shape rect(double xmin, double xmax, double zmin, double zmax)
    {
        return GeomUtil.rect(xmin, xmax, zmin, zmax);
    }

    public static Geom.Shape cube(double xmin, double xmax, double zmin, double zmax, double wmin, double wmax)
    {
        return GeomUtil.prism(GeomUtil.rect(xmin, xmax, zmin, zmax), 2, wmin, wmax);
    }

    public static Geom.Shape rect(int[] min, int[] max)
    {
        return rect(min[0], max[0] + 1, min[2], max[2] + 1);
    }

    public static Geom.Shape cube(int[] min, int[] max)
    {
        return cube(min[0], max[0] + 1, min[2], max[2] + 1, min[3], max[3] + 1);
    }

    // --- round style ---

    public Geom.Shape getRoundOutline(Track track, int[] min, int[] max, List<Track.Tile> tiles, int rounding, bool more) //throws Exception
    {

        int dim = track.getDimension();
        Arc arc = track.getRoundedArc();
        bool corner = track.getPlatformCorner();

        // compute rounding

        // this is the only place the tiles and more arguments are used

        if (rounding == -1)
        {
            rounding = 0;
            int a0 = (dim == 3) ? 2 : 0;
            for (int axis = a0; axis < dirTable.Length; axis++)
            { // excluded axis
                for (int idir = 0; idir < 4; idir++)
                {
                    int dir1 = dirTable[axis][idir];
                    int dir2 = dirTable[axis][(idir + 1) % 4];

                    if (canRound(min, max, tiles, more, dir1, dir2))
                    {
                        rounding |= bit(axis, idir);
                    }
                }
            }
        }

        return getRoundOutline(dim, min, max, rounding, arc, corner);
    }
    public Geom.Shape getRoundOutline(int dim, int[] min, int[] max, int rounding, Arc arc, bool corner) //throws Exception
    {

        // un-round where necessary

        // this could be described in code, but it's a mess,
        // let's just use a big table.
        //
        // the idea is that depending on which directions are collapsed,
        // there are some roundings that are incompatible, then at most
        // one of them can be turned on.  it can only happen in three cases:
        //
        // (1) PS_ROUND_MORE in 4D
        // (2) empty cells at platform edge
        // (3) user specified rounding

        int index = 0;
        if (max[0] == min[0]) index |= 1;
        if (max[2] == min[2]) index |= 2;
        if (dim == 4 && max[3] == min[3]) index |= 4;

        int[] collapse = collapseTable[index];
        for (int i = 0; i < collapse.Length; i++)
        {
            int mask = collapse[i];
            if (countBits(rounding & mask) > 1) rounding &= ~mask;
        }

        // compute the shape

        Geom.Builder builder = new Geom.Builder(true, true, true, 4 * arc.n);
        double[] pos = new double[4];
        double[] reg1 = new double[dim - 1];
        double[] reg2 = new double[dim - 1];
        bool[] rreg = new bool[4];

        if (dim == 3)
        {

            getLoopVertices(min, max,/* dir3 = */ -1,/* axis = */ 2, rounding, arc, builder, pos, reg1, reg2, rreg);

            return GeomUtil.genpoly(builder.toVertexArray());
            // we've accumulated some face vertices too,
            // but in 3D we just want the raw unique vertex array

        }
        else
        {

            // starting here, we're free to use the fact that arc.n is 4.
            // the geometry is much more complicated in the general case,
            // which of course is why I avoided it.

            Geom.Cell cell;
            double[] adjmin = new double[4];
            double[] adjmax = new double[4];

            // main faces

            for (int axis = 0; axis < dirTable.Length; axis++)
            { // could loop over dir3, but this is fine too
                int la = liftAxis(axis);

                reg1[axis] = min[la];
                if (getLoopVertices(min, max, Dir.forAxis(axis, true), axis, rounding, arc, builder, pos, reg1, reg2, rreg))
                {
                    cell = builder.makeFace();
                    cell.normal[axis] = -1;
                }

                reg1[axis] = max[la] + 1;
                if (getLoopVertices(min, max, Dir.forAxis(axis, false), axis, rounding, arc, builder, pos, reg1, reg2, rreg))
                {
                    cell = builder.makeFace();
                    cell.normal[axis] = +1;
                }
            }

            // edges

            for (int axis = 0; axis < dirTable.Length; axis++)
            {
                for (int idir = 0; idir < 4; idir++)
                {
                    if (isBitSet(rounding, axis, idir))
                    {
                        makeEdgeFaces(min, max, axis, idir, rounding, arc, builder, reg1, reg2, adjmin, adjmax, corner);
                    }
                }
            }

            // rounded corners

            if (corner)
            {
                for (int dir1 = 0; dir1 < 2; dir1++)
                {
                    for (int dir2 = 2; dir2 < 4; dir2++)
                    {
                        for (int dir3 = 4; dir3 < 6; dir3++)
                        {

                            if (isBitSet2(rounding, dir1, dir2)
                                 && isBitSet2(rounding, dir2, dir3)
                                 && isBitSet2(rounding, dir3, dir1))
                            {

                                getTriangleVertices(min, max, dir1, dir2, dir3, arc, builder, reg1);
                                cell = builder.makeFace();
                                Dir.apply(dir1, cell.normal, 0.577);
                                Dir.apply(dir2, cell.normal, 0.577);
                                Dir.apply(dir3, cell.normal, 0.577);
                            }
                        }
                    }
                }
            }

            // finish up

            return new Geom.Shape(builder.toCellArray(), builder.toFaceArray(), builder.toEdgeArray(), builder.toVertexArray());
        }
    }

    private readonly int[][] collapseTable = new int[][] {
         new int[] {},                             // zyx
         new int[] { 0x090, 0x060, 0x00C, 0x003 }, // 001
         new int[] { 0x009, 0x006, 0xC00, 0x300 }, // 010

  new int[] { 0x00F, 0x090, 0x060, 0xC00, 0x300 }, // 011

         new int[] { 0x900, 0x600, 0x0C0, 0x030 }, // 100

  new int[] { 0x0F0, 0x900, 0x600, 0x00C, 0x003 }, // 101
  new int[] { 0xF00, 0x009, 0x006, 0x0C0, 0x030 }, // 110

         new int[] { 0x00F, 0x0F0, 0xF00 }
      };

    private readonly int[] bits = new int[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4 };

    public int countBits(int n)
    {
        return bits[n & 15] + bits[(n >> 4) & 15] + bits[(n >> 8) & 15];
    }

    public static int bit(int axis, int idir)
    {
        int bitNumber = 4 * axis + idir;
        return 1 << 11 - bitNumber;
    }

    public static bool isBitSet(int n, int axis, int idir)
    {
        int bitNumber = 4 * axis + idir;
        return (((n >> 11 - bitNumber) & 1) != 0);
    }
    // bit numbering here corresponds to dirTable entries,
    // with the first entry of dirTable[0] being the high bit
    // and the last entry of dirTable[2] being the low bit.
    // the fortunate natural result is that 3D bit masks have
    // just the low four bits set.

    public bool isBitSet2(int n, int dir1, int dir2)
    {
        int bitNumber = roundingTable[dir1][dir2];
        return (((n >> 11 - bitNumber) & 1) != 0);
    }

    private readonly int[][] roundingTable = new int[][] { // map direction pair to bit number
      new int[] { -1,-1, 8,11, 4, 5 },
      new int[] { -1,-1, 9,10, 7, 6 },
      new int[] {  8, 9,-1,-1, 0, 3 },
      new int[] { 11,10,-1,-1, 1, 2 },
      new int[] {  4, 7, 0, 1,-1,-1 },
      new int[] {  5, 6, 3, 2,-1,-1 }
   };

    public static int liftAxis(int axis)
    {
        return (axis >= 1) ? axis + 1 : axis;
    }

    public static int liftDir(int dir)
    {
        return (dir >= 2) ? dir + 2 : dir;
    }

    public static bool canRound(int[] min, int[] max, List<Track.Tile> tiles, bool more, int dir1, int dir2)
    {

        dir1 = liftDir(dir1);
        dir2 = liftDir(dir2);

        int opp1 = Dir.getOpposite(dir1);
        int opp2 = Dir.getOpposite(dir2);

        int a1 = Dir.getAxis(dir1);
        int a2 = Dir.getAxis(dir2);

        int val1 = Dir.isPositive(dir1) ? max[a1] : min[a1];
        int val2 = Dir.isPositive(dir2) ? max[a2] : min[a2];

        // look for tiles with pos[a1] == val1 and pos[a2] == val2.
        // maybe building a full index array as in the auto-generation code
        // would be more efficient, but I'm guessing a list scan will be OK.

        foreach (Track.Tile tile in tiles)
        {
            if (tile.pos[a1] == val1 && tile.pos[a2] == val2)
            { // unique tile in 3D

                // in "more" mode, only the two input directions are excluded.
                // in "less" mode, only the two opposite directions are allowed.
                // in 3D that's the same thing, but in 4D it tells whether
                // there can be track running along the direction of rounding.
                // if the track is really wide it might overflow off the tile,
                // in which case don't use "more" mode.  :)

                foreach (Track.Seg seg in tile.segs)
                {
                    if (more)
                    {
                        if (seg.hasDir(dir1) || seg.hasDir(dir2)) return false;
                    }
                    else
                    {
                        if (!seg.isSamePathAs(opp1, opp2)) return false;
                        // only two dirs are allowed,
                        // so if there's a seg it must go along that path
                    }
                }
            }
        }

        return true; // no obstructions
    }

    public static double getCoordinate(int[] min, int[] max, int dir)
    {

        int[] pos = Dir.isPositive(dir) ? max : min;
        return pos[Dir.getAxis(liftDir(dir))] + 0.5;

        // careful: dir1, dir2, and the output vertices are in outline space (xy)
        // but min and max are in the real 3D space (xyz where we want to see xz)
    }

    public static void load(double[] reg1, int dir, double pos)
    {
        reg1[Dir.getAxis(dir)] = pos;
    }

    public static void loadCoordinate(int[] min, int[] max, int dir, double[] reg1)
    {
        load(reg1, dir, getCoordinate(min, max, dir));
    }

    public static void getVertices(double pos1, double pos2, int dir1, int dir2, bool round,
                                   Arc arc, Geom.Builder builder, double[] reg1, double[] reg2)
    {

        load(reg1, dir1, pos1);
        load(reg1, dir2, pos2);

        if (round)
        {
            for (int i = 0; i < arc.n; i++)
            {
                Vec.copy(reg2, reg1);
                Dir.apply(dir1, reg2, arc.u[i] - 0.5);
                Dir.apply(dir2, reg2, arc.v[i] - 0.5);
                builder.addFaceVertex((double[])reg2.Clone());
            }
        }
        else
        {
            // go to outer corner
            Dir.apply(dir1, reg1, 0.5);
            Dir.apply(dir2, reg1, 0.5);
            builder.addFaceVertex((double[])reg1.Clone());
        }
        // either way, the list of vertices is flush with platform edge at start and finish
    }

    public bool getLoopVertices(int[] min, int[] max, int dir3, int axis, int rounding,
                                          Arc arc, Geom.Builder builder, double[] pos, double[] reg1, double[] reg2, bool[] rreg)
    {

        double margin = arc.v[0];

        for (int idir = 0; idir < 4; idir++)
        {
            int dir = dirTable[axis][idir];

            pos[idir] = getCoordinate(min, max, dir);

            rreg[idir] = (dir3 != -1) ? isBitSet2(rounding, dir, dir3) : false; // perpendicular rounding?
            if (rreg[idir]) pos[idir] -= (Dir.isPositive(dir) ? 1 : -1) * (1 - margin); // pull edge back
        }

        // skip totally absent faces.  note, dirTable always has positive
        // directions in the first two positions, so the signs don't vary.
        // compare to -1 because we store the center positions.
        //
        if (pos[0] - pos[2] <= -1 || pos[1] - pos[3] <= -1) return false;

        for (int idir = 0; idir < 4; idir++)
        {
            int ialt = (idir + 1) % 4;

            double pos1 = pos[idir];
            double pos2 = pos[ialt];

            int dir1 = dirTable[axis][idir];
            int dir2 = dirTable[axis][ialt];

            bool round = isBitSet(rounding, axis, idir) && !rreg[idir] && !rreg[ialt];

            getVertices(pos1, pos2, dir1, dir2, round, arc, builder, reg1, reg2);
        }

        return true;
    }

    public int classify(bool flat, int dir, int dir3, int rounding)
    {

        if (isBitSet2(rounding, dir, dir3)) return 1; // join

        // in the flat case it's not possible to have both kinds of rounding
        // at the same time, so the classification is not arbitrary.
        if (flat && isBitSet2(rounding, Dir.getOpposite(dir), dir3)) return -1; // reverse join

        return 0;
    }

    private const int VX01 = 1;
    private const int VX12 = 2;
    private const int VX23 = 4;
    //
    // could combine all three to get one side of double reverse join,
    // but the other two sides are more difficult.
    // actually no, the extra middle vertex has different height then.

    public int getEdgeAdjustment(int[] min, int[] max, bool flat1, bool flat2, int dir1, int dir2, int dir3, int rounding, Arc arc, double[] adj, bool corner, double dx1) //throws Exception
    {

        // min and max are only in here for error messages

        int class1 = classify(flat1, dir1, dir3, rounding);
        int class2 = classify(flat2, dir2, dir3, rounding);

        if (class1 == 0 && class2 == 0)
        {
            for (int i = 0; i < adj.Length; i++) adj[i] = 1; // arc.v[3]
            return 0;
            // most common case, check it up front
        }

        if (class1 == 1 && class2 == 1)
        {
            adj[0] = arc.v[0];
            adj[1] = arc.v[1];
            adj[2] = arc.v[1];
            adj[3] = arc.v[0];
            return corner ? 0 : VX12;
        }

        if (class1 == 1)
        {
            Vec.copy(adj, arc.v);
            return 0;
        }

        if (class2 == 1)
        {
            Vec.copy(adj, arc.u);
            return 0;
        }

        if (arc.v[0] != 0) throw new Exception("Reverse join not supported when margin is nonzero.\n" + details(min, max, rounding));
        // of all the errors in this file, this is the only one that can happen with
        // automatically-generated platforms, so I'd better give some details about how it failed.
        // the error message below has the same info, but that's just to be parallel.

        if (class1 == -1 && class2 == 0)
        {
            adj[0] = arc.v[3];
            adj[1] = dx1;
            adj[2] = arc.v[2];
            adj[3] = arc.v[0];
            return VX23; // extra vertex at large i
        }

        if (class2 == -1 && class1 == 0)
        {
            adj[0] = arc.v[0];
            adj[1] = arc.v[2];
            adj[2] = dx1;
            adj[3] = arc.v[3];
            return VX01; // extra vertex at small i
        }

        throw new Exception("Double reverse join not supported.\n" + details(min, max, rounding));
    }

    public static string details(int[] min, int[] max, int rounding)
    {
        return format(min) + " " + format(max) + " " + format(rounding);
    }

    public static string format(int[] pos)
    {
        StringBuilder b = new StringBuilder();
        b.Append("[");
        for (int i = 0; i < pos.Length; i++)
        {
            if (i != 0) b.Append(" ");
            b.Append(pos[i]);
        }
        b.Append("]");
        return b.ToString();
    }

    public static string format(int rounding)
    {
        string s = Convert.ToString(rounding, 2);
        string zero = "%000000000000";
        return zero.Substring(0, zero.Length - s.Length) + s;
        // in 3D we'd want four digits,
        // but the errors we're reporting are 4D only
    }

    public static void extraVertex(double[] reg2, int dir1, int dir2, int axis, double pos1, double pos2, double pos3, double d1, double d2, double d3)
    {
        load(reg2, dir1, pos1);
        load(reg2, dir2, pos2);
        Dir.apply(dir1, reg2, d1);
        Dir.apply(dir2, reg2, d2);
        reg2[axis] = pos3 + d3;
    }

    public void makeEdgeFaces(int[] min, int[] max, int axis, int idir, int rounding,
                                     Arc arc, Geom.Builder builder, double[] reg1, double[] reg2, double[] adjmin, double[] adjmax, bool corner) //throws Exception
    {

        int ialt = (idir + 1) % 4;

        // gather information

        int dir1 = dirTable[axis]
      [idir];
        int dir2 = dirTable[axis]
      [ialt];
        int dir3min = Dir.forAxis(axis, true);
        int dir3max = Dir.forAxis(axis, false);

        double pos1 = getCoordinate(min, max, dir1);
        double pos2 = getCoordinate(min, max, dir2);
        double pos3min = getCoordinate(min, max, dir3min);
        double pos3max = getCoordinate(min, max, dir3max);

        int la1 = Dir.getAxis(liftDir(dir1));
        int la2 = Dir.getAxis(liftDir(dir2));

        bool flat1 = (max[la1] == min[la1]);
        bool flat2 = (max[la2] == min[la2]);

        double k = 2 * Math.Sqrt(3) - 2.5;
        double margin = arc.v[0];
        double dx1 = margin + k * (1 - margin);
        double dx2 = (arc.v[1] + arc.v[2]) / 2;

        int vxmin = getEdgeAdjustment(min, max, flat1, flat2, dir1, dir2, dir3min, rounding, arc, adjmin, corner, dx1);
        int vxmax = getEdgeAdjustment(min, max, flat1, flat2, dir1, dir2, dir3max, rounding, arc, adjmax, corner, dx1);
        // "vx" for "vertex exception", or maybe "vertex extra"

        // build faces

        for (int i = 0; i < arc.n; i++)
        {

            load(reg1, dir1, pos1);
            load(reg1, dir2, pos2);
            Dir.apply(dir1, reg1, arc.u[i] - 0.5);
            Dir.apply(dir2, reg1, arc.v[i] - 0.5);

            if (i != 0)
            {

                reg1[axis] = pos3max + adjmax[i] - 0.5;
                builder.addFaceVertex((double[])reg1.Clone());
                reg1[axis] = pos3min - adjmin[i] + 0.5;
                builder.addFaceVertex((double[])reg1.Clone());

                Geom.Cell cell = builder.makeFace();
                double theta = Math.PI * (30 * i - 15) / 180; // using known value of arc.n
                Dir.apply(dir1, cell.normal, Math.Cos(theta));
                Dir.apply(dir2, cell.normal, Math.Sin(theta));
            }

            if (i != arc.n - 1)
            {

                if (i == 0 && (vxmin & VX01) != 0)
                {
                    extraVertex(reg2, dir1, dir2, axis, pos1, pos2, pos3min, dx1 - 0.5, 0.5 - arc.v[2], 0.5 - arc.v[1]);
                    builder.addFaceVertex((double[])reg2.Clone());
                }
                if (i == 1 && (vxmin & VX12) != 0)
                {
                    extraVertex(reg2, dir1, dir2, axis, pos1, pos2, pos3min, dx2 - 0.5, dx2 - 0.5, 0.5 - dx2);
                    builder.addFaceVertex((double[])reg2.Clone());
                }
                if (i == 2 && (vxmin & VX23) != 0)
                {
                    extraVertex(reg2, dir1, dir2, axis, pos1, pos2, pos3min, 0.5 - arc.v[2], dx1 - 0.5, 0.5 - arc.v[1]);
                    builder.addFaceVertex((double[])reg2.Clone());
                }

                reg1[axis] = pos3min - adjmin[i] + 0.5;
                builder.addFaceVertex((double[])reg1.Clone());
                reg1[axis] = pos3max + adjmax[i] - 0.5;
                builder.addFaceVertex((double[])reg1.Clone());

                if (i == 0 && (vxmax & VX01) != 0)
                {
                    extraVertex(reg2, dir1, dir2, axis, pos1, pos2, pos3max, dx1 - 0.5, 0.5 - arc.v[2], arc.v[1] - 0.5);
                    builder.addFaceVertex((double[])reg2.Clone());
                }
                if (i == 1 && (vxmax & VX12) != 0)
                {
                    extraVertex(reg2, dir1, dir2, axis, pos1, pos2, pos3max, dx2 - 0.5, dx2 - 0.5, dx2 - 0.5);
                    builder.addFaceVertex((double[])reg2.Clone());
                }
                if (i == 2 && (vxmax & VX23) != 0)
                {
                    extraVertex(reg2, dir1, dir2, axis, pos1, pos2, pos3max, 0.5 - arc.v[2], dx1 - 0.5, arc.v[1] - 0.5);
                    builder.addFaceVertex((double[])reg2.Clone());
                }
            }
        }

        // there's not a lot of theory about the extra vertices,
        // I just tested them and made sure they worked,
        // but just to give you the idea, look at the final case.
        // if we have an extra vertex at large i on the max side,
        // that means the other rounding is from dir3max to Dir.getOpposite(dir1).
        // the relevant point on that curve has v[1] in dir3 and v[2] in -dir1,
        // so we add those, and use the constant from getEdgeAdjustment for dir2.

        // note, it's possible to get duplicate vertices in here,
        // but Geom.Builder takes care of them.  one case
        // is when you have three round edges going around a face of width 2;
        // another is when you have rounding added to a reverse join.
    }

    public static void getTriangleVertices(int[] min, int[] max, int dir1, int dir2, int dir3,
                                           Arc arc, Geom.Builder builder, double[] reg1)
    {

        int[] dir = new int[] { dir1, dir2, dir3 };

        double v1 = arc.v[1];
        double v2 = arc.v[2]; // v2 is the one closest to the exterior plane

        for (int i = 0; i < dir.Length; i++)
        {
            loadCoordinate(min, max, dir[i], reg1);
            Dir.apply(dir[i], reg1, v1 - 0.5);
        }

        for (int i = 0; i < dir.Length; i++)
        {
            Dir.apply(dir[i], reg1, v2 - v1);
            builder.addFaceVertex((double[])reg1.Clone());
            Dir.apply(dir[i], reg1, v1 - v2);
        }
    }

    // --- thin style ---

    public Geom.Shape getThinOutline(Track track, int[] min, int[] max, List<Track.Tile> tiles, bool round) //throws Exception
    {

        if (tiles.Count == 0) throw new Exception("Thin platform contains no tiles.");

        int dim = track.getDimension();
        double w2 = track.getPlatformWidth() / 2;

        // check for long platforms

        int a = -1;
        for (int i = 0; i < dim; i++)
        {
            if (max[i] != min[i])
            {
                if (a != -1) throw new Exception("Thin platforms can only extend in one direction.");
                a = i;
            }
        }

        // handle long platforms and isolated straights

        if (a == -1) a = ((Track.Tile)tiles.First()).getStraightAxis(); // tiles.size must be 1 in this case
        if (a != -1)
        {

            if (tiles.Count != max[a] - min[a] + 1) throw new Exception("Thin platforms cannot contain empty positions.");

            foreach (Track.Tile t in tiles)
            {
                if (t.getStraightAxis() != a) throw new Exception("Track on extended thin platform isn't straight.");
            }

            return (dim == 3) ? thinStraight2(min, max, a, w2)
                              : thinStraight3(min, max, a, w2);
        }

        // now we have a single tile

        int[] pos = min;
        double m = track.getMargin(); // bend and 3_0 are the only ones that use this

        Track.Tile tile = (Track.Tile)tiles.First();
        if (tile.segs.Count == 1)
        { // can't be zero because of createPlatform check
            Track.Seg seg = (Track.Seg)tile.segs.First();
            int fromDir, toDir;
            if (seg.fromDir < seg.toDir)
            { // put in order, it will help the functions
                fromDir = seg.fromDir;
                toDir = seg.toDir;
            }
            else
            {
                fromDir = seg.toDir;
                toDir = seg.fromDir;
            }
            return (dim == 3) ? thinBend2(pos[0], pos[2], fromDir, toDir, w2, m, round, track.getArcn())
                              : thinBend3(pos, fromDir, toDir, w2, m, round, track.getArcn());
        }

        // have to work out the connections

        bool[] conn = getConnections(dim, tile.segs);
        int nDir = 0;
        for (int i = 0; i < conn.Length; i++) if (conn[i]) nDir++;
        int nStr = 0;
        for (int i = 0; i < conn.Length; i += 2) if (conn[i] && conn[i + 1]) nStr++;

        // counting number of directions involved and number of straight segments
        // happens to distinguish between all possible cases.
        //    in 3D/2D: 4 3
        //    in 4D/3D: 6 5 4_2 4_1 3_1 3_0
        // both also have straight 2_1 and bend 2_0, but we've done those already

        // go ahead and split into two cases to avoid DynamicArray-type complications

        return (dim == 3) ? thinJunction2(pos, nDir, conn, w2)
                          : thinJunction3(pos, nDir, nStr, conn, w2, m);
    }

    public static int compact(int dir)
    {
        return (dir >= 4) ? dir - 2 : dir;
    }

    public static bool[] getConnections(int dim, List<Track.Seg> segs)
    {
        bool[] conn = new bool[2 * (dim - 1)];
        foreach (Track.Seg seg in segs)
        {
            conn[compact(seg.fromDir)] = true;
            conn[compact(seg.toDir)] = true;
        }
        return conn;
    }

    public static Geom.Shape thinStraight2(int[] min, int[] max, int a, double w2)
    {
        if (a == 0) return rect(min[0], max[0] + 1, min[2] + 0.5 - w2, min[2] + 0.5 + w2);
        return rect(min[0] + 0.5 - w2, min[0] + 0.5 + w2, min[2], max[2] + 1);
    }

    public static Geom.Shape thinStraight3(int[] min, int[] max, int a, double w2)
    {
        if (a == 0) return cube(min[0], max[0] + 1, min[2] + 0.5 - w2, min[2] + 0.5 + w2, min[3] + 0.5 - w2, min[3] + 0.5 + w2);
        if (a == 2) return cube(min[0] + 0.5 - w2, min[0] + 0.5 + w2, min[2], max[2] + 1, min[3] + 0.5 - w2, min[3] + 0.5 + w2);
        return cube(min[0] + 0.5 - w2, min[0] + 0.5 + w2, min[2] + 0.5 - w2, min[2] + 0.5 + w2, min[3], max[3] + 1);
    }

    public static double getBendDistance(double w2, double m)
    {
        double x = 0.5 + w2;
        double r = x - m;
        double d = r * (2 - Math.Sqrt(2));
        return x - d;
    }

    public static Geom.Shape thinBend2(int pos0, int pos2, int fromDir, int toDir, double w2, double m, bool round, int arcn)
    {

        // the dirs are ordered, so fromDir is /pm x and toDir is /pm z,
        // but change that into x and y since we're building a 2D shape.
        toDir -= 2;

        double x = 0.5 + w2;
        double r = x - m;
        double d = r * (2 - Math.Sqrt(2));

        Arc arc = round ? Arc.curve(arcn, true, m, 0.5 + w2) : null;
        // can't store this on the track
        // since it depends on the platform width, which is per platform

        double[][] vertex = new double[round ? arc.n + 2 : 6][];
        for (int i = 0; i < vertex.Length; i++) vertex[i] = new double[2];
        int n = 0;
        double[] pen = new double[] { pos0 + 0.5, pos2 + 0.5 };
        double[] save = (double[])pen.Clone();

        Dir.apply(toDir, pen, 0.5);
        Dir.apply(fromDir, pen, -w2);
        Vec.copy(vertex[n++], pen);

        Dir.apply(fromDir, pen, 2 * w2);
        Vec.copy(vertex[n++], pen);

        Vec.copy(pen, save);
        Dir.apply(fromDir, pen, 0.5);
        Dir.apply(toDir, pen, w2);
        Vec.copy(vertex[n++], pen);

        Dir.apply(toDir, pen, -2 * w2);
        Vec.copy(vertex[n++], pen);

        if (round)
        {
            for (int i = 1; i < arc.n - 1; i++)
            { // skip end vertices
                Vec.copy(pen, save);
                Dir.apply(toDir, pen, 0.5 - arc.u[i]);
                Dir.apply(fromDir, pen, 0.5 - arc.v[i]);
                Vec.copy(vertex[n++], pen);
            }
        }
        else
        {
            Dir.apply(fromDir, pen, d - x);
            Vec.copy(vertex[n++], pen);

            Dir.apply(fromDir, pen, -d);
            Dir.apply(toDir, pen, d);
            Vec.copy(vertex[n++], pen);
        }

        return GeomUtil.genpoly(vertex);
    }

    public static Geom.Shape thinBend3(int[] pos, int fromDir, int toDir, double w2, double m, bool round, int arcn)
    {

        int a1 = Dir.getAxis(fromDir);
        int a2 = Dir.getAxis(toDir);
        int a3 = 5 - a1 - a2;
        int ap = (a3 == 0) ? 0 : a3 - 1; // prism axis

        if (a3 == 0) fromDir -= 4;
        if (a3 != 3) toDir -= 2;

        return GeomUtil.prism(thinBend2(pos[a1], pos[a2], fromDir, toDir, w2, m, round, arcn), ap, pos[a3] + 0.5 - w2, pos[a3] + 0.5 + w2);
    }

    private readonly int[][] dirTable = new int[][] { new int[] { 2,4,3,5 },
                                                         new int[] { 4,0,5,1 },
                                                         new int[] { 0,2,1,3 }  };

    public Geom.Shape thinJunction2(int[] pos, int nDir, bool[] conn, double w2)
    {

        // go around and add the vertices in circular order

        double[][] vertex = new double[nDir * 2][];
        for (int i = 0; i < vertex.Length; i++) vertex[i] = new double[2];
        int n = 0;

        for (int idir = 0; idir < 4; idir++)
        {
            int dir = dirTable[2][idir];
            if (!conn[dir]) continue;

            vertex[n][0] = pos[0] + 0.5;
            vertex[n][1] = pos[2] + 0.5;
            Dir.apply(dir, vertex[n], 0.5);

            Vec.copy(vertex[n + 1], vertex[n]);

            int forward = dirTable[2][(idir + 1) % 4];
            Dir.apply(forward, vertex[n], -w2);
            Dir.apply(forward, vertex[n + 1], w2);

            n += 2;
        }

        return GeomUtil.genpoly(vertex);
    }

    public Geom.Shape thinJunction3(int[] pos, int nDir, int nStr, bool[] conn, double w2, double m)
    {

        bool tripod = (nDir == 3 && nStr == 0); // nDir == 3 is redundant

        double[][] vertex = new double[nDir * 4 + (tripod ? 3 : 0)][];
        for (int i = 0; i < vertex.Length; i++) vertex[i] = new double[3];
        int n = 0;

        // vertices have a large offset (0.5) in one direction
        // and small offsets (w2) in all other directions.
        // first array index is primary direction, others are secondary.
        // add each vertex twice so order of secondaries doesn't matter.
        //
        int[][][] vindex = new int[6][][];
        for (int i = 0; i < vindex.Length; i++) {
          vindex[i] = new int[6][];
          for (int j = 0; j < vindex[i].Length; j++) {
            vindex[i][j] = new int[6];
          }
        }

        Geom.Builder builder = new Geom.Builder(false, true, true, 8); // octagon is max

        // first build square faces

        for (int dir = 0; dir < conn.Length; dir++)
        {
            if (!conn[dir]) continue;
            n = buildSquareFace(dir, vertex, n, vindex, builder, pos, w2);
        }

        // next build diagonal and triangular faces

        for (int da = 0; da < conn.Length; da++)
        {
            if (!conn[da]) continue;
            int a = Dir.getAxis(da);

            for (int db = Dir.forAxis(a + 1); db < conn.Length; db++)
            {
                if (!conn[db]) continue;
                int b = Dir.getAxis(db);

                buildDiagonalFace(da, db, 3 - a - b, vindex, builder);

                for (int dc = Dir.forAxis(b + 1); dc < conn.Length; dc++)
                {
                    if (!conn[dc]) continue;

                    buildTriangularFace(da, db, dc, vindex, builder);

                    // we've got all the directions, so go ahead and do this.
                    // we could move 3 of the 4 faces into buildBigFace, but
                    // it seems simpler to keep it all in one place.
                    if (tripod)
                    {
                        buildTripodFaces(new int[] { da, db, dc }, vertex, n, vindex, builder, getBendDistance(w2, m));
                    }
                }
            }
        }

        // last build big faces, one per unconnected direction as it turns out

        if (!tripod)
        {
            for (int db = 0; db < conn.Length; db++)
            {
                if (conn[db]) continue;
                buildBigFace(db, conn, vindex, builder);
            }
        }

        // put it all together

        return new Geom.Shape(builder.toCellArray(), builder.toFaceArray(), builder.toEdgeArray(), vertex);
    }

    public static int buildSquareFace(int dir, double[][] vertex, int n, int[][][] vindex,
                                      Geom.Builder builder, int[] pos, double w2)
    {

        // this function does extra work since it
        // also puts together the actual vertices

        // first we need to figure out the other two axes

        int a = Dir.getAxis(dir);
        int b = (a == 1) ? 0 : 1;
        int c = (a == 2) ? 0 : 2;

        int dbp = Dir.forAxis(b, false);
        int dbm = Dir.forAxis(b, true);
        int dcp = Dir.forAxis(c, false);
        int dcm = Dir.forAxis(c, true);

        // now we can make the vertices

        vertex[n][0] = pos[0] + 0.5;
        vertex[n][1] = pos[2] + 0.5;
        vertex[n][2] = pos[3] + 0.5;
        Dir.apply(dir, vertex[n], 0.5);

        Vec.copy(vertex[n + 1], vertex[n]);

        Dir.apply(dbm, vertex[n], w2);
        Dir.apply(dbp, vertex[n + 1], w2);

        Vec.copy(vertex[n + 2], vertex[n]);
        Vec.copy(vertex[n + 3], vertex[n + 1]);

        Dir.apply(dcm, vertex[n], w2);
        Dir.apply(dcm, vertex[n + 1], w2);
        Dir.apply(dcp, vertex[n + 2], w2);
        Dir.apply(dcp, vertex[n + 3], w2);

        // and index them

        vindex[dir][dbm][dcm] = n;
        vindex[dir][dbp][dcm] = n + 1;
        vindex[dir][dbm][dcp] = n + 2;
        vindex[dir][dbp][dcp] = n + 3;

        vindex[dir][dcm][dbm] = n;
        vindex[dir][dcm][dbp] = n + 1;
        vindex[dir][dcp][dbm] = n + 2;
        vindex[dir][dcp][dbp] = n + 3;

        // make face and edges

        int[] iv = new int[] { n, n + 1, n + 3, n + 2 };

        Geom.Cell cell = builder.makeCell(iv);

        Dir.apply(dir, cell.normal, 1);

        return n + 4;
    }

    public static void buildDiagonalFace(int da, int db, int c, int[][][] vindex, Geom.Builder builder)
    {

        int dcp = Dir.forAxis(c, false);
        int dcm = Dir.forAxis(c, true);

        int[] iv = new int[] { vindex[da][db][dcm],
                             vindex[da][db][dcp],
                             vindex[db][da][dcp],
                             vindex[db][da][dcm]  };

        Geom.Cell cell = builder.makeCell(iv);

        Dir.apply(da, cell.normal, 0.707);
        Dir.apply(db, cell.normal, 0.707);
    }

    public static void buildTriangularFace(int da, int db, int dc, int[][][] vindex, Geom.Builder builder)
    {

        int[] iv = new int[] { vindex[da][db][dc],
                             vindex[db][dc][da],
                             vindex[dc][da][db]  };

        Geom.Cell cell = builder.makeCell(iv);

        Dir.apply(da, cell.normal, 0.577);
        Dir.apply(db, cell.normal, 0.577);
        Dir.apply(dc, cell.normal, 0.577);
    }

    public static void buildTripodFaces(int[] dir, double[][] vertex, int n, int[][][] vindex,
                                        Geom.Builder builder, double bendDistance)
    {

        // tabulate opposite directions (i means idir here)

        int[] opp = new int[3];
        for (int i = 0; i < 3; i++) opp[i] = Dir.getOpposite(dir[i]);

        // build extra vertices (indices are n+i)

        for (int i = 0; i < 3; i++)
        {
            int j = (i + 1) % 3;
            int k = (i + 2) % 3;

            double[] v = vertex[n + i];
            Vec.copy(v, vertex[vindex[dir[i]][opp[j]][opp[k]]]);
            Dir.apply(opp[i], v, bendDistance);

            // build big faces (for direction db = opp[i])

            // can do this in same loop, face doesn't depend on
            // vertices having been computed yet

            int[] iv = new int[] { n+j,
                                vindex[dir[j]][opp[i]][opp[k]],
                                vindex[dir[j]][opp[i]][dir[k]],
                                vindex[dir[k]][opp[i]][dir[j]],
                                vindex[dir[k]][opp[i]][opp[j]],
                                n+k };

            Geom.Cell cell = builder.makeCell(iv);

            Dir.apply(opp[i], cell.normal, 1);
        }

        // build extra triangular face

        // block for parallelism
        {
            int[] iv = new int[] { n, n + 1, n + 2 };

            Geom.Cell cell = builder.makeCell(iv);

            for (int i = 0; i < 3; i++) Dir.apply(opp[i], cell.normal, 0.577);
        }

        // don't bother returning incremented n, this is the last step
    }

    public void buildBigFace(int db, bool[] conn, int[][][] vindex, Geom.Builder builder)
    {

        // cf. thinJunction2

        int b = Dir.getAxis(db);

        for (int ida = 0; ida < 4; ida++)
        {
            int da = dirTable[b][ida];
            if (!conn[da]) continue;

            int dc = dirTable[b][(ida + 1) % 4];
            builder.addFaceVertex(vindex[da][db][Dir.getOpposite(dc)]);
            builder.addFaceVertex(vindex[da][db][dc]);
        }

        Geom.Cell cell = builder.makeFace();

        Dir.apply(db, cell.normal, 1);
    }

    // --- ramps ---

    public static double[] pair(double x, double y)
    {
        double[] d = new double[2];
        d[0] = x;
        d[1] = y;
        return d;
    }

    public static Geom.ShapeInterface createRamp(Track track, int[] pos) //throws Exception
    {

        int YP = Dir.forAxis(1, false);
        int YM = Dir.forAxis(1, true);

        int dir; // direction from bottom to top
        int[]
      top;
        int[]
      bot;
        Track.Tile tileTop;
        Track.Tile tileBot;

        Track.Tile tile = track.findTile(pos);
        if (tile == null || tile.isEmpty()) throw new Exception("No tile at that location.");

        Track.Seg seg = (Track.Seg)tile.segs.First(); // should be only one

        // be nice and handle all cases
        if (seg.toDir == YP)
        {
            dir = Dir.getOpposite(seg.fromDir);
            tileTop = tile.neighbor[YP];
            tileBot = tile;
        }
        else if (seg.toDir == YM)
        {
            dir = seg.fromDir;
            tileTop = tile;
            tileBot = tile.neighbor[YM];
        }
        else if (seg.fromDir == YP)
        {
            dir = Dir.getOpposite(seg.toDir);
            tileTop = tile.neighbor[YP];
            tileBot = tile;
        }
        else if (seg.fromDir == YM)
        {
            dir = seg.toDir;
            tileTop = tile;
            tileBot = tile.neighbor[YM];
        }
        else
        {
            throw new Exception("No ramp at that location.");
        }

        if (tileTop.attached || tileBot.attached) throw new Exception("Tile is already on a ramp.");
        tileTop.attached = true;
        tileBot.attached = true;

        top = tileTop.pos;
        bot = tileBot.pos;

        // note, ramp doesn't make sense for anything except TM_ROTATE, so don't
        // worry about ramp shape of TM_SQUARE.
        // also, we couldn't even if we wanted to, since train mode is per train!

        double vscale = track.getVScale();
        double vtheta = track.getVTheta();

        double tan = Math.Tan(Math.PI * vtheta / 180);
        double adj = vscale / (2 * tan); // same as in RoundPathDetail

        double thickness = track.getPlatformThickness();

        List<Geom.Shape> shapes = new List<Geom.Shape>();
        List<Geom.Texture> textures = new List<Geom.Texture>();
        if (bot[1] == 0)
        {

            double t2 = vscale;
            double b2 = t2 - thickness;
            double c2 = (b2 > 0) ? b2 : 0; // corrected b

            double[] v0 = pair(1 - adj, 0);
            double[] v5 = pair(1 + adj, t2);
            double[] v6a = pair(1 + adj, c2);
            double[] v6b = pair(1 + adj, 0);
            double[] v7 = pair(2, t2);
            double[] v8 = pair(2, c2);

            shapes.Add(GeomUtil.genpoly(new double[][] { v5, v7, v8, v6a }));
            // sharing vertices is naughty, but prism operation will fix it
            //
            if (b2 > 0 && !track.getRampTriangle())
            { // the normal case
                shapes.Add(GeomUtil.genpoly(new double[][] { v0, v5, v6a, pair(1 + adj - b2 / tan, 0) }));
            }
            else
            { // all the way to the ground
                shapes.Add(GeomUtil.genpoly(new double[][] { v0, v5, v6b }));
            }

            textures.Add(lineTexture(v5, v7));
            textures.Add(lineTexture(v0, v5));

        }
        else
        {

            double t1 = bot[1] * vscale;
            double t2 = top[1] * vscale;
            double b1 = t1 - thickness;
            double b2 = t2 - thickness;
            double c1 = (b1 > 0) ? b1 : 0; // corrected b
            double c2 = (b2 > 0) ? b2 : 0;

            double[] v1 = pair(0, t1);
            double[] v2 = pair(0, c1);
            double[] v3 = pair(1 - adj, t1);
            double[] v4 = pair(1 - adj, c1);
            double[] v5 = pair(1 + adj, t2);
            double[] v6 = pair(1 + adj, c2);
            double[] v7 = pair(2, t2);
            double[] v8 = pair(2, c2);

            shapes.Add(GeomUtil.genpoly(new double[][] { v5, v7, v8, v6 }));
            //
            if (b1 >= 0)
            { // the normal case
                shapes.Add(GeomUtil.genpoly(new double[][] { v3, v5, v6, v4 }));
            }
            else if (b2 > 0)
            {
                shapes.Add(GeomUtil.genpoly(new double[][] { v3, v5, v6, pair(1 + adj - b2 / tan, 0), v4 }));
            }
            else
            { // all the way to the ground
                shapes.Add(GeomUtil.genpoly(new double[][] { v3, v5, v6, v4 }));
            }
            //
            shapes.Add(GeomUtil.genpoly(new double[][] { v1, v3, v4, v2 }));

            textures.Add(lineTexture(v5, v7));
            textures.Add(lineTexture(v3, v5));
            textures.Add(lineTexture(v1, v3));
        }

        int dim = track.getDimension();

        double pw2 = (isThin(track.getPlatformStyle()) ? track.getPlatformWidth() : 1) / 2;
        double mid = 0.5;
        double pmin = mid - pw2;
        double pmax = mid + pw2;

        Geom.ShapeInterface ramp = new Geom.CompositeShape((Geom.ShapeInterface[])shapes.ToArray());
        ramp = ramp.prism(2, pmin, pmax);
        if (dim == 4) ramp = ramp.prism(3, pmin, pmax);

        double w2 = track.getWidth() / 2;
        double min = mid - w2;
        double max = mid + w2;
        for (int i = 0; i < textures.Count; i++)
        { // not normal iteration, but it's a short list and this is convenient

            Geom.Texture t = (Geom.Texture)textures[i];
            Geom.Texture tn = GeomUtil.noSplit(t, 2, mid);
            Geom.Texture ts = GeomUtil.split(t, 2, min, max);

            if (dim == 4)
            {
                tn = GeomUtil.noSplit(tn, 3, mid);
                ts = GeomUtil.split(ts, 3, min, max);
            }

            Geom.Shape shape = (Geom.Shape)((Geom.CompositeShape)ramp).component[i];
            shape.cell[0].customTexture = new Track.RampTexture(tn.union(ts), track);
        }

        // block to keep variables contained
        {
            Geom.CompositeShape cs = (Geom.CompositeShape)ramp;
            Geom.Shape rampTop = (Geom.Shape)cs.component[0];
            Geom.Shape rampMid = (Geom.Shape)cs.component[1];
            Geom.Shape rampBot = (cs.component.Length == 3) ? (Geom.Shape)cs.component[2] : null;
            tileTop.hint = new RampTopHint(rampTop, rampMid);
            tileBot.hint = new RampBotHint(rampMid, rampBot);
        }

        // now we just have to put it in the right place

        double[] temp = new double[dim];
        for (int i = 0; i < dim; i++) temp[i] = 0.5; // Grid.fromCell(0)

        // first turn positive x to point in the right direction
        // cf. Car.placeCentered
        if (dir == 1)
        { // negative x, only 180 degree case
            ramp.rotate(0, 4, 180, temp);
        }
        else
        {
            ramp.rotate(0, dir, 90, temp);
        }

        for (int i = 0; i < dim; i++) temp[i] = bot[i]; // Grid.fromCell(bot) - Grid.fromCell(0)
        temp[1] = 0; // y is already set correctly

        ramp.translate(temp);

        ramp.setNoUserMove();
        if (track.getPlatformColor() != null) ramp.setShapeColor(track.getPlatformColor());
        return ramp;
    }

    public static Geom.Texture lineTexture(double[] v1, double[] v2)
    {
        return new Geom.Texture(new Geom.Edge[] { new Geom.Edge(0, 1) }, new double[][] { v1, v2 });
    }

    // it's hard to say where these hints came from,
    // I just had to think about the geometry a lot.

    // the root problem is that if you want smooth motion
    // of cars mounted at single points with their
    // bottom faces flush to the ground, there are going to be
    // ground collisions at the tops and bottoms of ramps.
    // hints are the best way I could find to deal with that.

    // even without that, you have to worry about separation
    // failure caused by FP errors on the diagonal ramp face.

    public class RampTopHint : Geom.HintInterface
    {

        private Geom.Shape rampTop;
        private Geom.Shape rampMid;

        public RampTopHint(Geom.Shape rampTop, Geom.Shape rampMid)
        {
            this.rampTop = rampTop;
            this.rampMid = rampMid;
        }

        public Geom.Separator getHint(Geom.Shape owner, Geom.Shape target, int invert)
        {
            if (target == rampTop
                 || target == rampMid)
            {
                return new Geom.NormalSeparator(owner.bottomFace, invert);
            }
            return null;
        }
    }

    public class RampBotHint : Geom.HintInterface
    {

        private Geom.Shape rampMid;
        private Geom.Shape rampBot; // null on ground level ramps

        public RampBotHint(Geom.Shape rampMid, Geom.Shape rampBot)
        {
            this.rampMid = rampMid;
            this.rampBot = rampBot;
        }

        public Geom.Separator getHint(Geom.Shape owner, Geom.Shape target, int invert)
        {
            if (target == rampMid)
            {
                return new Geom.NormalSeparator(rampMid.cell[0], -invert);
            }
            if (target == rampBot)
            {
                return new Geom.NormalSeparator(rampBot.cell[0], -invert);
            }
            return null;
        }
    }

    // --- pylons ---

    public static Geom.ShapeInterface createPylon(Track track, double[] dpos, int base_)
    {
        int dim = track.getDimension();

        // calc top

        int y = (int)dpos[1];
        double top = y * track.getVScale() - track.getPlatformThickness();

        // calc bottom

        if (base_ != -1)
        {
            y = base_;
        }
        else
        {

            int[] ipos = new int[dim];
            int[] temp = new int[dim];

            dpos[1] = 0.5; // avoid getting two cells from y direction
            Grid.toCell(ipos, temp, dpos); // could be two cells, but we just take the first

            while (--y > 0)
            {
                ipos[1] = y;
                // just check if there's a tile, don't worry about the exact platform shape
                Track.Tile tile = track.findTile(ipos);
                if (tile != null && !tile.isEmpty()) break;
            }
        }

        double bot = y * track.getVScale();

        // get outline

        Geom.Shape pylon;
        if (dim == 3)
        {
            int n = track.getPylonSides();
            double cos = Math.Cos(Math.PI / n);
            double f = ((n & 1) == 0) ? 2 * cos : 1 + cos; // width correction factor
            double r = track.getPylonWidth() / f;
            pylon = GeomUtil.polygon(dpos[0], dpos[2], r, n, track.getPylonOffset());
        }
        else
        {
            // in 4D we only support cubic pylons, track parameters are ignored
            double w2 = track.getPylonWidth() / 2;
            pylon = cube(dpos[0] - w2, dpos[0] + w2, dpos[2] - w2, dpos[2] + w2, dpos[3] - w2, dpos[3] + w2);
        }

        // finish up

        pylon = extend(pylon, top, top - bot);
        pylon.isNoUserMove = true;
        if (track.getPlatformColor() != null) pylon.setShapeColor(track.getPlatformColor());
        return pylon;
    }

}

