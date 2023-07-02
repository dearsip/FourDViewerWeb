/*
 * Track.java
 */

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

//import java.awt.Color;
//import java.util.Iterator;
//import java.util.LinkedList;
//import java.util.ListIterator;
//import java.util.Vector;

/**
 * Data structures and functions related to train tracks.
 */

public class Track : SceneryBase, IDimension
{ // added specially but otherwise it's normal scenery

    private int dim;
  private double carLen;
  private double carScale;
  private double width;
  private double margin;
  private int arcn;
  private Color color;
  private Color colorSel;
  private bool expand; // expand from monorail to multirail?
  private double velStep;

  // parameters that for backward compatibility have defaults
  // and are set via set functions
  private double vscale;
  private double vtheta;
  private double vradius;
  //
  private int platformStyle;
  private double platformThickness;
  private double platformWidth; // for thin platform style only
  private Color platformColor;
  private bool platformCorner; // for 4D round platform only
                               //
  private bool rampTriangle;
  //
  private double pylonWidth;
  private int pylonSides;
  private double pylonOffset;

  private double adjacentSide;

  private Arc curve1; // monorail
  private Arc curve2a; // two rail (also four)
  private Arc curve2b;
  private Arc helix;
  private Arc rounded; // for platforms

  private List<Tile> tiles;

  private Color segColor;

  private int[] reg3;
  private int[] reg4;
  private int[] reg5;

  // I don't like having defaults, but we need them for backward compatibility
  //
  private static double DEFAULT_VSCALE = 0.4;
  private static double DEFAULT_VTHETA = 18.5;
  private static double DEFAULT_VRADIUS = 1.228;
  //
  private static int DEFAULT_PSTYLE = Platform.PS_THIN_ROUND;
  private static double DEFAULT_PTHICKNESS = 0.1;
  private static double DEFAULT_PWIDTH = 0.3;
  private static Color DEFAULT_PCOLOR = new Color(0, 160, 0); // dark green
  private static bool DEFAULT_PCORNER = true;
  //
  private static bool DEFAULT_RAMPTRIANGLE = false;
  //
  private static double DEFAULT_PYWIDTH = 0.1;
  private static int DEFAULT_PYSIDES = 4;
  private static double DEFAULT_PYOFFSET = GeomUtil.OFFSET_ALT;

  /**
   * @param arcn Number of arc segments to show for 90 degree curves.
   */
  public Track(int dim, double carLen, double carScale, double width, double margin, int arcn, Color color, Color colorSel, bool expand, double velStep)
          : this(dim, carLen, carScale, width, margin, arcn, color, colorSel, expand, velStep,
             DEFAULT_VSCALE, DEFAULT_VTHETA, DEFAULT_VRADIUS) //throws Exception
  {

  }
  public Track(int dim, double carLen, double carScale, double width, double margin, int arcn, Color color, Color colorSel, bool expand, double velStep,
               double vscale, double vtheta, double vradius) : base(dim) //throws Exception
  {

    this.dim = dim;
    this.carLen = carLen;
    this.carScale = carScale;
    this.width = width;
    this.margin = margin;
    this.arcn = arcn;
    this.color = color;
    this.colorSel = colorSel;
    this.expand = expand;
    this.velStep = velStep;

    this.vscale = vscale;
    this.vtheta = vtheta;
    this.vradius = vradius;
    //
    platformStyle = DEFAULT_PSTYLE;
    platformThickness = DEFAULT_PTHICKNESS;
    platformWidth = DEFAULT_PWIDTH;
    platformColor = DEFAULT_PCOLOR;
    platformCorner = DEFAULT_PCORNER;
    //
    rampTriangle = DEFAULT_RAMPTRIANGLE;
    //
    pylonWidth = DEFAULT_PYWIDTH;
    pylonSides = DEFAULT_PYSIDES;
    pylonOffset = DEFAULT_PYOFFSET;

    RoundPathDetail.validate(vscale, vtheta, vradius);
    adjacentSide = RoundPathDetail.getAdjacentSide(vscale, vtheta);

    curve1 = Arc.curve(arcn, true, margin, 0.5);
    curve2a = Arc.curve(arcn, true, margin, 0.5 - width / 2);
    curve2b = Arc.curve(arcn, true, margin, 0.5 + width / 2);
    if (dim == 4) helix = Arc.helix(arcn, width);
    rounded = Arc.curve((dim == 4) ? 3 : arcn, false, margin, 1);
    // limit arcn in 4D case to simplify corner shapes

    tiles = new List<Tile>();

    reg3 = new int[dim];
    reg4 = new int[dim];
    reg5 = new int[2 * dim];
  }

  public int getDimension()
  {
    return dim;
  }

  public double getCarLen()
  {
    return carLen;
  }

  public double getCarScale()
  {
    return carScale;
  }

  public double getWidth()
  {
    return width;
  }

  public double getMargin()
  {
    return margin;
  }

  public int getArcn()
  {
    return arcn;
  }

  public double getVelStep()
  {
    return velStep;
  }

  public double getVScale()
  {
    return vscale;
  }

  public double getVTheta()
  {
    return vtheta;
  }

  public double getVRadius()
  {
    return vradius;
  }

  public int getPlatformStyle()
  {
    return platformStyle;
  }

  public double getPlatformThickness()
  {
    return platformThickness;
  }

  public double getPlatformWidth()
  {
    return platformWidth;
  }

  public Color getPlatformColor()
  {
    return platformColor;
  }

  public bool getPlatformCorner()
  {
    return platformCorner;
  }

  public bool getRampTriangle()
  {
    return rampTriangle;
  }

  public double getPylonWidth()
  {
    return pylonWidth;
  }

  public int getPylonSides()
  {
    return pylonSides;
  }

  public double getPylonOffset()
  {
    return pylonOffset;
  }

  public void setPlatformStyle(double platformStyle)
  { // double so we can use Command.Set
    this.platformStyle = (int)platformStyle;
  }

  public void setPlatformThickness(double platformThickness)
  {
    this.platformThickness = platformThickness;
  }

  public void setPlatformWidth(double platformWidth)
  {
    this.platformWidth = platformWidth;
  }

  public void setPlatformColor(Color platformColor)
  {
    this.platformColor = platformColor;
  }

  public void setPlatformCorner(bool platformCorner)
  {
    this.platformCorner = platformCorner;
  }

  public void setRampTriangle(bool rampTriangle)
  {
    this.rampTriangle = rampTriangle;
  }

  public void setPylonWidth(double pylonWidth)
  {
    this.pylonWidth = pylonWidth;
  }

  public void setPylonSides(double pylonSides)
  { // double so we can use Command.Set
    this.pylonSides = (int)pylonSides;
  }

  public void setPylonOffset(double pylonOffset)
  {
    this.pylonOffset = pylonOffset;
  }

  public void setPylonOffset(bool alt)
  {
    this.pylonOffset = alt ? GeomUtil.OFFSET_ALT : GeomUtil.OFFSET_REG;
  }

  public Arc getRoundedArc()
  {
    return rounded;
  }

  // --- semi-implementation of IKeysNew ---

  public void toggleTrack()
  {
    expand = !expand;
  }

  public int[] projectToPlane(double[] origin, double[] viewAxis, double height)
  {

    if (!Clip.projectToPlane(reg1, origin, viewAxis, height)) return null;

    reg1[1] = reg1[1] / vscale + 0.5; // add 0.5 to avoid FP error at plane boundary

    Grid.toCell(reg3, reg4, reg1); // could be two cells, but we just take the first
    return reg3;
  }

  public void click(double[] origin, double[] viewAxis)
  {
    // we could use the leftover origin value
    // from the last draw call , but this is cleaner

    // unlike other pointing that I may add later on,
    // this doesn't depend on what's in front of what,
    // we just go to y = 0 and see what cell we're in.

    int[] pos = projectToPlane(origin, viewAxis,/* height = */ 0);
    if (pos == null) return;
    // pos is reg3, but that doesn't matter

    Tile tile = findTile(pos);
    if (tile == null) return; // not pointing at a tile

    clicked(tile);
  }

  public void click(double[] origin, double[] viewAxis, TileTexture tt)
  {

    int[] pos = projectToPlane(origin, viewAxis, tt.height);
    if (pos == null) return;

    Tile tile = findTile(tt.blockTiles, pos);
    if (tile == null) return;
    // checking blockTiles instead of tiles has a tiny validation effect,
    // but it's mostly just a nice optimization.  as far as I know,
    // the only way a relevant tile can be missing from blockTiles is if
    // you've added track since the platform was constructed.

    clicked(tile);
  }

  private void clicked(Tile tile)
  {

    if (tile.switchSegs == null) calcSwitchSegs(tile);
    int n = tile.switchSegs.Count;
    if (n == 0) return; // not switchable

    if (++tile.iSel == n)
    {
      tile.iSel = -1;
      tile.segSel = null;
    }
    else
    {
      tile.segSel = (Seg)tile.switchSegs[tile.iSel]; // this is why it's a vector
    }
  }

  public void calcSwitchSegs(Tile tile)
  {
    tile.switchSegs = new List<Seg>();

    // do we have two segs with the same direction?
    // crossed straights (e.g.) are not switchable.
    // actually it's worse that that ... in 4D you can have
    // things like an x-axis straight and a z-w switch.
    // in 3D, if any count > 1, all directions are involved.

    for (int i = 0; i < reg5.Length; i++) reg5[i] = 0;

    foreach (Seg seg in tile.segs)
    {
      reg5[seg.fromDir]++;
      reg5[seg.toDir]++;
    }

    foreach (Seg seg in tile.segs)
    {
      if (reg5[seg.fromDir] > 1
           || reg5[seg.toDir] > 1)
      {
        tile.switchSegs.Add(seg);
      }
    }
  }

  // --- building ---

  public static Tile findTile(List<Tile> useTiles, int[] pos)
  {
    foreach (Tile tile in useTiles)
    {
      if (Grid.equals(tile.pos, pos)) return tile;
    }
    return null;
  }

  public Tile findTile(int[] pos)
  {
    return findTile(tiles, pos);
  }

  public List<Tile> findAllTiles(int[] min, int[] max)
  {
    List<Tile> list = new List<Tile>();
    foreach (Tile tile in tiles)
    {
      if (Grid.bounds(min, tile.pos) && Grid.bounds(tile.pos, max)) list.Add(tile);
    }
    return list;
  }

  public List<Tile> findAllTiles(int y)
  {
    List<Tile> list = new List<Tile>();
    foreach (Tile tile in tiles)
    {
      if (tile.pos[1] == y) list.Add(tile);
    }
    return list;
  }

  public Tile findOrCreateTile(int[] pos)
  {
    Tile tile = findTile(pos);
    if (tile == null)
    {
      tile = new Tile(pos);
      tiles.Add(tile);
    }
    return tile;
  }

  public Tile advance(Tile tile, int[] pos, int dir1, int opp1)
  {

    // find tile
    Dir.apply(dir1, pos, 1);
    // possible it's already in the neighbor table,
    // but not a common case, don't worry about it
    Tile toTile = findOrCreateTile(pos);

    // link them up, just an optimization but it does 90% of the work
    tile.neighbor[dir1] = toTile;
    toTile.neighbor[opp1] = tile;

    return toTile;
  }

  // probably some better way to do this, I'm not super-familiar
  // with the maze code any more
  private static readonly int[][] turnLeft = new int[][] {
      new int[] { 9,9,9,9,6,7,5,4 }, // in = +x
      new int[] { 9,9,9,9,7,6,4,5 }, // in = -x
      new int[] { 9,9,9,9,9,9,9,9 }, // in = +y (n/a)
      new int[] { 9,9,9,9,9,9,9,9 }, // in = -y (n/a)
      new int[] { 7,6,9,9,9,9,0,1 }, // in = +z
      new int[] { 6,7,9,9,9,9,1,0 }, // in = -z
      new int[] { 4,5,9,9,1,0,9,9 }, // in = +w
      new int[] { 5,4,9,9,0,1,9,9 }  // in = -w
   };

  public void build(int[] pos, int dir1, int dir2, string s) //throws Exception
  {

    Tile tile = findOrCreateTile(pos);
    // now we're at our base state, having passed through tile at pos, exiting in direction dir1.
    // dir2 defines the in direction in 4D, so in 3D we can just set it to one of the w axes and
    // get everything to work the same way.

    for (int i = 0; i < s.Length; i++)
    {

      int opp1 = Dir.getOpposite(dir1);

      int toDir1;
      int toDir2 = dir2;
      int helix = 0;
      bool shouldAdd = true;

      char c = s[i];
      switch (c)
      {

        case ' ':
          continue;

        case 's': // straight
          toDir1 = dir1;
          break;

        case 'l':
          toDir1 = turnLeft[dir2][dir1];
          break;
        case 'r':
          toDir1 = Dir.getOpposite(turnLeft[dir2][dir1]);
          break;
        case 'i':
          toDir1 = dir2;
          toDir2 = opp1;
          break;
        case 'o':
          toDir1 = Dir.getOpposite(dir2);
          toDir2 = dir1;
          break;

        case 't': // turn around
          toDir1 = opp1;
          shouldAdd = false;
          break;

        case 'g': // the letter next to 'h' on the negative side
          toDir1 = dir1;
          helix = -1;
          break;
        case 'h': // helix
          toDir1 = dir1;
          helix = +1;
          break;
        // note about g and h: to get the same behavior as a train
        // riding over the tracks, we ought to rotate dir2 so that
        // the left and in directions cycle around.  but, it's a pain
        // to implement, and not helpful for building track either.

        case 'u':
          toDir1 = 2;
          break;
        case 'd':
          toDir1 = 3;
          break;

        default:
          throw new Exception("Invalid track command '" + c + "'.");
      }

      Tile toTile = advance(tile, pos, dir1, opp1);

      // add the segment
      if (shouldAdd) addSeg(toTile, new Seg(opp1, toDir1, helix));
      // segment directions are relative to center of the cell,
      // so we have to take the opposite of the from direction.

      if (Dir.getAxis(toDir1) == 1)
      { // vertical adds two tiles at once
        int toOpp1 = Dir.getOpposite(toDir1);
        Dir.apply(dir1, pos, 1); // diagonal neighbor linkage
        toTile = advance(toTile, pos, toDir1, toOpp1);
        addSeg(toTile, new Seg(toOpp1, dir1, helix));
        toDir1 = dir1; // don't change the final direction
      }

      // advance the state
      tile = toTile;
      // pos already advanced
      dir1 = toDir1;
      dir2 = toDir2;
    }

    // create a last link, that way all neighbors are prelinked, even
    // if you build a loop in two pieces from the same starting point.
    // to put it another way, since we link out from the loose ends
    // of what we build, every segment will have neighbors at both ends.
    advance(tile, pos, dir1, Dir.getOpposite(dir1));
  }

  public static Seg findSeg(Tile tile, int fromDir, int toDir)
  {
    foreach (Seg seg in tile.segs)
    {
      if (seg.isSamePathAs(fromDir, toDir)) return seg;
    }
    return null;
  }

  public static void addSeg(Tile tile, Seg segNew) //throws Exception
  {

    Seg seg = findSeg(tile, segNew.fromDir, segNew.toDir);
    if (seg == null)
    {

      if (tile.isVertical()
           || (segNew.isVertical() && !tile.isEmpty())) throw new Exception("Incompatible vertical track segment.");

      tile.segs.Add(segNew);

    }
    else if (segNew.isCompatibleWith(seg))
    {
      // already got it
    }
    else
    {
      throw new Exception("Incompatible straight track segment.");
    }
  }

  // --- tile texture ---

  public class TileTexture : Geom.CustomTexture
  {

    public List<Tile> blockTiles;
    public double height; // convenience for click function
		private Track track;
    protected List<double[]> tList;
    protected List<Color> cList;

    public TileTexture(List<Tile> blockTiles, double height, Track track)
    {
      this.blockTiles = blockTiles;
      this.height = height;
			this.track = track;
      tList = new List<double[]>();
      cList = new List<Color>();
    }

    public void draw(out double[][] texture, out Color[] textureColor, Geom.Cell cell, double[] origin)
    {
			tList.Clear();
			cList.Clear();
      // same set-up as SceneryBase.draw

			foreach (Tile tile in blockTiles)
			{
				track.drawTile(tile);
			}
			texture = tList.ToArray();
			textureColor = cList.ToArray();
    }
  }

  // --- ramp texture ---

  public class RampTexture : Geom.Texture
  {

		private Track track;
    public RampTexture(Geom.Texture t, Track track) : base(t.edge, t.vertex)
    {
			this.track = track;
    }

    public override void draw(out double[][] texture, out Color[] textureColor, Geom.Cell cell, double[] origin)
    {
			track.tList.Clear();
			track.cList.Clear();
      int iMin, iMax;
      if (track.expand)
      {
        iMin = 1;
        iMax = edge.Length - 1;
      }
      else
      {
        iMin = 0;
        iMax = 0;
      }
      for (int i = iMin; i <= iMax; i++)
      {
        Geom.Edge e = edge[i];
        track.drawLine(vertex[e.iv1], vertex[e.iv2], track.color);
        // never colorSel, ramps are not switchable
      }
			texture = track.tList.ToArray();
			textureColor = track.cList.ToArray();
    }
  }

  // --- drawing ---

  protected override void draw(out double[][] texture, out Color[] textureColor)
  {
		tList.Clear();
		cList.Clear();

    // segColor set per segment

    foreach (Tile tile in tiles)
    {
      if (tile.pos[1] == 0) drawTile(tile);
      // else it's off the ground
      // and you need TileTexture on a block
    }
		texture = tList.ToArray();
		textureColor = cList.ToArray();
  }

  public void drawTile(Tile tile)
  {
    Grid.fromCell(reg0, tile.pos);
    reg0[1] = vscale * (reg0[1] - 0.5);

    foreach (Seg seg in tile.segs)
    {
      segColor = (seg == tile.segSel) ? colorSel : color;
      drawSeg(seg);
    }
  }

  public void drawSeg(Seg seg)
  {
    bool vertical = seg.isVertical();
    if (!vertical && !Dir.isOpposite(seg.fromDir, seg.toDir))
    { // curve

      if (dim == 4 && expand)
      {
        int a = 5 - Dir.getAxis(seg.fromDir) - Dir.getAxis(seg.toDir);
        reg0[a] += width / 2;
        drawSegCurve(seg);
        reg0[a] -= width;
        drawSegCurve(seg);
        reg0[a] += width / 2;
      }
      else
      {
        drawSegCurve(seg);
      }

    }
    else
    { // straight or helix or vertical

      int dir = seg.fromDir;
      int axis = Dir.getAxis(dir);
      if (axis == 1)
      {
        dir = seg.toDir;
        axis = Dir.getAxis(dir);
      }

      double d1 = -0.5;
      double d2 = 0.5;

      if (vertical)
      {
        if (Dir.isPositive(dir)) d1 += adjacentSide;
        else d2 -= adjacentSide;
      }

      bool helix = (seg.helix != 0);
      if (dim == 4 && expand)
      {
        if (helix && margin == 0)
        {
          // there are no straight pieces in this case
        }
        else
        {
          int a = (axis == 3) ? 0 : 3;
          reg0[a] += width / 2;
          drawSegStraight(axis, helix, d1, d2);
          reg0[a] -= width;
          drawSegStraight(axis, helix, d1, d2);
          reg0[a] += width / 2;
        }
      }
      else
      {
        drawSegStraight(axis, false, d1, d2); // (*)
      }
      // (*) use "false" not "helix" so that a non-expanded helix
      // will show a straight line through the center in addition
      // to the four helix segments.

      if (helix)
      {
        drawSegHelix(seg);
      }
      // expand has no effect on this; I couldn't think of
      // any more compact sign to show the helix direction
    }
  }

  public void drawSegStraight(int axis, bool helix, double d1, double d2)
  {
    if (expand)
    {
      int a = (axis == 2) ? 0 : 2;
      reg0[a] += width / 2;
      drawSegStraight2(axis, helix, d1, d2);
      reg0[a] -= width;
      drawSegStraight2(axis, helix, d1, d2);
      reg0[a] += width / 2;
    }
    else
    {
      drawSegStraight2(axis, helix, d1, d2);
    }
  }

  public void drawSegStraight2(int axis, bool helix, double d1, double d2)
  {
    Vec.copy(reg1, reg0);
    Vec.copy(reg2, reg0);
    int a = axis;

    // doesn't matter which way is which

    if (helix)
    {
      reg1[a] -= 0.5;
      reg2[a] = reg1[a] + margin;
      drawLine(reg1, reg2, segColor);
      reg1[a] += 1;
      reg2[a] = reg1[a] - margin;
      drawLine(reg1, reg2, segColor);
    }
    else
    {
      reg1[a] += d1;
      reg2[a] += d2;
      drawLine(reg1, reg2, segColor);
    }
  }

  public void drawSegCurve(Seg seg)
  {
    if (expand)
    {
      drawSegArc(seg, curve2a);
      drawSegArc(seg, curve2b);
    }
    else
    {
      drawSegArc(seg, curve1);
    }
  }

  public void drawSegArc(Seg seg, Arc arc)
  {
    for (int i = 0; i < arc.n; i++)
    {

      // new point into reg2
      Vec.copy(reg2, reg0);
      Dir.apply(seg.fromDir, reg2, 0.5 - arc.u[i]);
      Dir.apply(seg.toDir, reg2, 0.5 - arc.v[i]);

      if (i != 0) drawLine(reg1, reg2, segColor);
      Vec.copy(reg1, reg2);
    }
  }

  public void drawSegHelix(Seg seg)
  {

    int a = Dir.getAxis(seg.fromDir);
    int b = (a == 2) ? 0 : 2;
    int c = (a == 3) ? 0 : 3;

    // so now we have three orthogonal axes,
    // the only question is what's the correct-handed coordinate system.

    // first let's generate the same handedness
    // for all values of a
    if (a == 0) { int temp = b; b = c; c = temp; }

    // now it's just an empirical question,
    // and I investigated and made it work

    drawSegHelix2(a, b, c, 1, 1 * seg.helix);
    drawSegHelix2(a, c, b, 1, -1 * seg.helix);
    drawSegHelix2(a, b, c, -1, -1 * seg.helix);
    drawSegHelix2(a, c, b, -1, 1 * seg.helix);
  }

  public void drawSegHelix2(int a, int b, int c, int sb, int sc)
  {
    double len = 0.5 - margin;
    for (int i = 0; i < helix.n; i++)
    {

      // new point into reg2
      Vec.copy(reg2, reg0);
      double d = ((double)i) / (helix.n - 1); // 0-1
      reg2[a] += (2 * d - 1) * len; // 0 -> -len, 1 -> +len
      reg2[b] += sb * helix.u[i];
      reg2[c] += sc * helix.v[i];

      if (i != 0) drawLine(reg1, reg2, segColor);
      Vec.copy(reg1, reg2);
    }
  }

  // --- helper structures ---

  public class Tile
  {

    public int[] pos; // dimension is same as space dimension for convenience, but y is usually zero
    public List<Seg> segs;
    public Tile[] neighbor;
    public List<Seg> switchSegs; // computed and then cached
    public int iSel;
    public Seg segSel;
    public bool attached; // extra flag for elevated tiles
    public Geom.HintInterface hint;

    public Tile(int[] pos)
    {
      this.pos = (int[])pos.Clone();
      segs = new List<Seg>();
      neighbor = new Tile[pos.Length * 2];
      switchSegs = null;
      iSel = -1;
      segSel = null;
      // attached stays false
      // hint stays null
    }

    public bool isEmpty() { return (segs.Count == 0); }

    public int getStraightAxis()
    {
      return (segs.Count == 1) ? segs.First<Seg>().getStraightAxis() : -1;
    }

    public bool isVertical()
    {
      return (segs.Count == 1) ? segs.First<Seg>().isVertical() : false;
    }

    public bool isRampTop()
    {
      return (segs.Count == 1) ? segs.First<Seg>().isRampTop() : false;
    }

    public bool isRampBottom()
    {
      return (segs.Count == 1) ? segs.First<Seg>().isRampBottom() : false;
    }
  }

  public class Seg
  {

    public int fromDir;
    public int toDir;
    public int helix; // -1, 0, +1

    public Seg(int fromDir, int toDir) { this.fromDir = fromDir; this.toDir = toDir; }
    public Seg(int fromDir, int toDir, int helix) { this.fromDir = fromDir; this.toDir = toDir; this.helix = helix; }

    public bool isStraight()
    {
      return Dir.isOpposite(fromDir, toDir);
    }

    public int getStraightAxis()
    {
      int a1 = Dir.getAxis(fromDir);
      int a2 = Dir.getAxis(toDir);
      return (a1 == a2) ? a1 : -1;
    }

    public bool isVertical()
    {
      return Dir.getAxis(fromDir) == 1
             || Dir.getAxis(toDir) == 1;
    }

    public bool isRampTop()
    {
      return (fromDir == 3 || toDir == 3);
    }

    public bool isRampBottom()
    {
      return (fromDir == 2 || toDir == 2);
    }

    public bool isSamePathAs(int fromDir, int toDir)
    {
      return (this.fromDir == fromDir && this.toDir == toDir)
             || (this.fromDir == toDir && this.toDir == fromDir);
    }

    public bool isSamePathAs(Seg seg)
    {
      return (fromDir == seg.fromDir && toDir == seg.toDir)
             || (fromDir == seg.toDir && toDir == seg.fromDir);
    }

    // if same path, then you can call this
    public bool isCompatibleWith(Seg seg)
    {
      if (isStraight())
      {
        return (helix == seg.helix); // you might think the helix
                                     // would change sign if the direction is reversed, but no
      }
      else
      {
        return true;
      }
    }

    public bool hasDir(int dir)
    {
      return (fromDir == dir || toDir == dir);
    }
  }

  // --- paths ---

  public class Path
  {

    public List<PathSeg> psegs;

    public Path()
    {
      psegs = new List<PathSeg>();
    }
  }

  public class PathSeg
  {

    public Tile tile;
    public int fromDir;
    public int toDir;
    public int hbase; // accumulated helix from both helix and helixTable
    public int helix; // amount of helix from this segment

    public PathSeg(Tile tile, Seg seg, int fromDir)
    {
      this.tile = tile;
      this.fromDir = seg.fromDir;
      this.toDir = seg.toDir;
      // hbase not known at this moment
      this.helix = seg.helix;
      if (this.fromDir != fromDir) flip();
    }

    public void flip()
    {
      int d = fromDir;
      fromDir = toDir;
      toDir = d;
      // helix doesn't change under flip
    }
    // it's easier to copy the directions from seg
    // and flip them around than to manage a bool

    public bool isStraight()
    {
      return Dir.isOpposite(fromDir, toDir);
    }

    public bool isVertical()
    {
      return Dir.getAxis(fromDir) == 1
             || Dir.getAxis(toDir) == 1;
    }
  }

  // connected to default rotations in Car.placeCentered,
  // probably that should all go together somewhere.
  // mathematically, we have standard rotations std, and here's what we're trying to figure out:
  // std(+x,a) then rot(a,b) = std(+x,b) times how much helix?
  private static readonly int[][] helixTable = new int[][] {
      new int[] { 9,0,0,0,0,0,2,2 }, // -x    // no helix in xz plane because of how +x to -x is done
      new int[] { 0,9,0,0,0,0,0,0 }, // +x    // no helix here because +x is the standard orientation
      new int[] { 0,0,9,9,0,0,0,0 }, // -y
      new int[] { 0,0,9,9,0,0,0,0 }, // +y
      new int[] { 0,0,0,0,9,0,1,3 }, // -z
      new int[] { 0,0,0,0,0,9,3,1 }, // +z
      new int[] { 0,2,0,0,3,1,9,0 }, // -w
      new int[] { 0,2,0,0,1,3,0,9 }  // +w
   };

  private static int mod4(int helix)
  {
    return (helix % 4);
    // doesn't matter if it's positive or negative, they both work;
    // all we have to do is keep it bounded.
    // even that isn't a big deal, no way we'll reach int overflow.
  }

  public static bool growHead(Path path, bool random)
  {
    PathSeg pseg = path.psegs.First();
    Tile tileNew = pseg.tile.neighbor[pseg.toDir];
    PathSeg psegNew = pickSeg(tileNew, Dir.getOpposite(pseg.toDir), random);
    if (psegNew != null)
    {
      psegNew.hbase = mod4(pseg.hbase + pseg.helix + helixTable[pseg.fromDir][pseg.toDir]);
      path.psegs.Insert(0, psegNew);
      return true;
    }
    else
    {
      return false; // dead end
    }
  }

  public static bool growTail(Path path, bool random)
  {
    PathSeg pseg = (PathSeg)path.psegs.Last();
    Tile tileNew = pseg.tile.neighbor[pseg.fromDir];
    PathSeg psegNew = pickSeg(tileNew, Dir.getOpposite(pseg.fromDir), random);
    if (psegNew != null)
    {
      psegNew.flip();
      psegNew.hbase = mod4(pseg.hbase - psegNew.helix - helixTable[psegNew.fromDir][psegNew.toDir]);
      path.psegs.Add(psegNew);
      return true;
    }
    else
    {
      return false; // dead end
    }
  }

  /**
   * Pick a segment on the tile that connects to dir,
   * either the first one or a random one.
   */
  public static PathSeg pickSeg(Tile tile, int dir, bool random)
  {
    if (random)
    {
      if (tile.segSel != null && tile.segSel.hasDir(dir)) return new PathSeg(tile, tile.segSel, dir);
      int count = countSeg(tile, dir);
      if (count == 0) return null; // no such segment
      int index = (count == 1) ? 0 // very common case, let's not run the random number generator for nothing
                               : (int)(count * (new System.Random()).NextDouble()); // ideally we'd control the seed here
      return pickSeg(tile, dir, index);
    }
    else
    {
      return pickSeg(tile, dir, 0);
    }
  }

  /**
   * Pick a segment on the tile that connects to dir,
   * the one with the given index.
   */
  public static PathSeg pickSeg(Tile tile, int dir, int index)
  {
    foreach (Seg seg in tile.segs)
    {
      if (seg.hasDir(dir))
      {
        if (index-- == 0) return new PathSeg(tile, seg, dir);
      }
    }
    return null; // no such segment
  }

  public static int countSeg(Tile tile, int dir)
  {
    int count = 0;
    foreach (Seg seg in tile.segs)
    {
      if (seg.hasDir(dir)) count++;
    }
    return count;
  }

  public class PathInfo
  {

    public double[] pos;
    public int fromDir; // not required unless angle is nonzero
    public int toDir;
    public double angle; // 0-1 for 90 degree curve
    public double helix; // 0-h for a helix segment
    public Geom.HintInterface hint;

    public PathInfo(int dim) { pos = new double[dim]; }
  }

  public interface PathDetail
  {
    double getLen(PathSeg pseg);
    void getPos(PathSeg pseg, double d, PathInfo pi);
  }

  public class SquarePathDetail : PathDetail
  {

    private double diag;
    private double dlen;

    private double vscale;

    public SquarePathDetail(double vscale)
    {

      diag = Math.Sqrt(1 + vscale * vscale) / 2;
      dlen = 0.5 + diag;

      this.vscale = vscale;
    }

    public double getLen(PathSeg pseg)
    {
      return pseg.isVertical() ? dlen : 1;
    }

    public void getPos(PathSeg pseg, double d, PathInfo pi)
    {

      Grid.fromCell(pi.pos, pseg.tile.pos);
      pi.pos[1] = vscale * (pi.pos[1] - 0.5);

      if (Dir.getAxis(pseg.toDir) == 1)
      {
        getPosVertical(pseg.fromDir, pseg.toDir, d, pi);
      }
      else if (Dir.getAxis(pseg.fromDir) == 1)
      {
        getPosVertical(pseg.toDir, pseg.fromDir, dlen - d, pi);
      }
      else
      {
        if (d < 0.5)
        {
          Dir.apply(pseg.fromDir, pi.pos, 0.5 - d);
        }
        else
        {
          Dir.apply(pseg.toDir, pi.pos, d - 0.5); // 0.5-(1-d) is the right way to think of it, see round case
        }
        // same for straight and curved!
      }

      // square path doesn't support rotation,
      // any angle info would just be ignored.
      // hints also don't apply.
    }

    /**
     * @param toDir The vertical direction.
     */
    private void getPosVertical(int fromDir, int toDir, double d, PathInfo pi)
    {
      if (d < 0.5)
      {
        Dir.apply(fromDir, pi.pos, 0.5 - d);
      }
      else
      {
        d = 0.5 * (d - 0.5) / diag;
        Dir.apply(toDir, pi.pos, d * vscale);
        Dir.apply(fromDir, pi.pos, -d);
      }
    }
  }

  public class RoundPathDetail : PathDetail
  {

    private double margin;
    private double r;
    private double len1;
    private double len2;
    private double arclen;

    private double vscale;
    private double vtheta; // angle in radians, for the trigonometric functions
    private double vangle; // angle in units where 1 = 90 degrees, for PathInfo
    private double vradius;
    private double vlen1;
    private double vlen2;
    private double vlen3;
    private double vlen;

    public RoundPathDetail(double margin, double vscale, double a_vtheta, double vradius)
    {
      this.margin = margin;
      r = (0.5 - margin);
      len1 = 1 - 2 * margin;
      len2 = r * Math.PI / 2;
      arclen = len2 + 2 * margin;

      this.vscale = vscale;
      vtheta = a_vtheta * Math.PI / 180;
      vangle = a_vtheta / 90;
      this.vradius = vradius;
      //
      // right triangle with opposite side vscale/2
      double adj = vscale / (2 * Math.Tan(vtheta));
      double hyp = vscale / (2 * Math.Sin(vtheta));
      //
      double delta = vradius * Math.Tan(vtheta / 2);
      vlen1 = (1 - adj) - delta;
      vlen2 = vradius * vtheta;
      vlen3 = hyp - delta;
      vlen = vlen1 + vlen2 + vlen3;
    }

    public static void validate(double vscale, double a_vtheta, double vradius) //throws Exception
    {

      // it's possible to make a mistake with margin, but that's more obvious

      double vtheta = a_vtheta * Math.PI / 180;

      double adj = vscale / (2 * Math.Tan(vtheta));
      double hyp = vscale / (2 * Math.Sin(vtheta));

      double delta = vradius * Math.Tan(vtheta / 2);

      if (adj >= 1) throw new Exception("Vertical angle is too small."); // better error message
      if (adj + delta >= 1) throw new Exception("Vertical radius of curvature is too large to fit horizontally.");

      if (delta >= hyp) throw new Exception("Vertical radius of curvature is too large to fit vertically.");

      // so, now we know all three vlen will be positive
    }

    /**
     * Ramps are per track, not per train, so they can't depend on the train mode.
     * Since RoundPathDetail is the preferred mode, I use that to construct ramps.
     * This is the necessary number.
     */
    public static double getAdjacentSide(double vscale, double a_vtheta)
    {
      return vscale / (2 * Math.Tan(a_vtheta * Math.PI / 180));
    }

    public double getLen(PathSeg pseg)
    {
      return pseg.isVertical() ? vlen : (pseg.isStraight() ? 1 : arclen);
    }

    public void getPos(PathSeg pseg, double d, PathInfo pi)
    {

      Grid.fromCell(pi.pos, pseg.tile.pos);
      pi.pos[1] = vscale * (pi.pos[1] - 0.5);

      pi.fromDir = pseg.fromDir;
      pi.toDir = pseg.toDir;

      if (Dir.getAxis(pseg.toDir) == 1)
      {
        getPosVertical(pseg.fromDir, pseg.toDir, d, pi);
      }
      else if (Dir.getAxis(pseg.fromDir) == 1)
      {
        getPosVertical(pseg.toDir, pseg.fromDir, vlen - d, pi);
        pi.fromDir = Dir.getOpposite(pseg.toDir);
        pi.toDir = Dir.getOpposite(pseg.fromDir);
        // toDir has to be the vertical one, see Car.placeCentered
      }
      else if (pseg.isStraight())
      {

        Dir.apply(pseg.fromDir, pi.pos, 0.5 - d);
        pi.angle = 0;
        if (pseg.helix == 0 || d < margin)
        { // avoid calculations in common case
          pi.helix = 0;
        }
        else if (1 - d < margin)
        {
          pi.helix = pseg.helix;
        }
        else
        {
          pi.helix = pseg.helix * (d - margin) / len1;
        }

      }
      else
      {

        if (d < margin)
        {
          Dir.apply(pseg.fromDir, pi.pos, 0.5 - d);
          pi.angle = 0;
        }
        else if (arclen - d < margin)
        {
          Dir.apply(pseg.toDir, pi.pos, 0.5 - (arclen - d));
          pi.angle = 1;
        }
        else
        {
          double theta = (d - margin) / r;
          Dir.apply(pseg.fromDir, pi.pos, r * (1 - Math.Sin(theta)));
          Dir.apply(pseg.toDir, pi.pos, r * (1 - Math.Cos(theta)));
          pi.angle = (d - margin) / len2;
        }
        pi.helix = 0;
      }

      pi.helix += pseg.hbase;
      pi.hint = pseg.tile.hint;
    }

    /**
     * @param toDir The vertical direction.
     */
    private void getPosVertical(int fromDir, int toDir, double d, PathInfo pi)
    {
      if (d < vlen1)
      {
        Dir.apply(fromDir, pi.pos, 0.5 - d);
        pi.angle = 0;
      }
      else if (vlen - d < vlen3)
      {
        d = vlen - d;
        Dir.apply(toDir, pi.pos, 0.5 * vscale - d * Math.Sin(vtheta));
        Dir.apply(fromDir, pi.pos, -0.5 + d * Math.Cos(vtheta));
        pi.angle = vangle;
      }
      else
      {
        double alpha = (d - vlen1) / vradius;
        Dir.apply(toDir, pi.pos, vradius - vradius * Math.Cos(alpha));
        Dir.apply(fromDir, pi.pos, 0.5 - vlen1 - vradius * Math.Sin(alpha));
        pi.angle = vangle * (d - vlen1) / vlen2;
      }
    }
  }

  public interface ContinuousPathIterator
  {
    void step(double d, PathInfo pi);
    void prune();
  }

  public class ContinuousPath
  {

    public Path path;
    public PathDetail detail;
    public double head;
    public double tail;
    // head is the head position within the first seg,
    // tail is the tail position within the last seg.

    public ContinuousPath(Track track, int[] pos, int fromDir, int toDir, double d0, bool round) //throws Exception
    {

      Tile tile = track.findTile(pos);
      if (tile == null) throw new Exception("Tile not found.");
      Seg seg = findSeg(tile, fromDir, toDir);
      if (seg == null) throw new Exception("Segment not found.");

      PathSeg pseg = new PathSeg(tile, seg, fromDir);
      // initial PathSeg has hbase = 0,
      // I don't let you control that tiny detail

      path = new Path();
      path.psegs.Add(pseg);

      detail = round ? new RoundPathDetail(track.getMargin(), track.getVScale(), track.getVTheta(), track.getVRadius())
                    : (PathDetail)new SquarePathDetail(track.getVScale());

      double min = 0;
      double max = detail.getLen(pseg);

      if (d0 < min) d0 = min;
      if (d0 > max) d0 = max;

      head = d0;
      tail = d0;
      // move with random = false to create initial extent
    }

    // because of the possibility of hitting a dead end,
    // we have to use this differently in forward and reverse.
    // in forward we move forward and then iterate in reverse,
    // in reverse we do the opposite.  trailing-end segments
    // are only pruned at the end of iteration.

    // if/when I add curve distance corrections (so that a car of
    // length L takes up slightly more than L distance on a curve)
    // then moving the head by d won't necessarily also move the
    // tail by d.  so, in the model we're forced into by dead ends,
    // it's like the train has engines at both head and tail.

    /**
     * @return False if we ran into a dead end.
     */
    public bool moveForward(double d, bool random)
    {
      head += d;
      while (true)
      {
        PathSeg pseg = (PathSeg)path.psegs.First();
        double len = detail.getLen(pseg);
        if (head <= len) break; // we're on this tile
        if (!growHead(path, random)) { head = len; return false; } // bonk
        head -= len;
      }
      return true;
    }

    /**
     * @return False if we ran into a dead end.
     */
    public bool moveReverse(double d, bool random)
    {
      tail -= d;
      while (true)
      {
        if (tail >= 0) break; // we're on this tile
        if (!growTail(path, random)) { tail = 0; return false; } // bonk
        PathSeg pseg = (PathSeg)path.psegs.Last();
        double len = detail.getLen(pseg);
        tail += len;
      }
      return true;
    }

    public ContinuousPathIterator initialIterator(ContinuousPath path) { return new InitialIterator(path); }
    public ContinuousPathIterator headToTailIterator(ContinuousPath path) { return new HeadToTailIterator(path); }
    public ContinuousPathIterator tailToHeadIterator(ContinuousPath path) { return new TailToHeadIterator(path); }
    // avoid the weird syntax for constructing inner classes from outside

    public class InitialIterator : ContinuousPathIterator
    {
			ContinuousPath path;
			public InitialIterator(ContinuousPath path) {
				this.path = path;
			}

      public void step(double d, PathInfo pi)
      {

        if (!path.moveReverse(d,/* random = */ false))
        {
          throw new Exception("Initial train placement failed.");
        }
        path.detail.getPos(path.path.psegs.Last(), path.tail, pi);
      }

      public void prune()
      {
      }
    }

    public class HeadToTailIterator : ContinuousPathIterator
    {

      private int li;
      private PathSeg pseg;
      private List<PathSeg> psegs;
      private double cur;
			private ContinuousPath path;

      public HeadToTailIterator(ContinuousPath path)
      {
        psegs = path.path.psegs;
        li = 0;
        pseg = (PathSeg)psegs[li];
        cur = path.head;
				this.path = path;
      }

      public void step(double d, PathInfo pi)
      { // like moveReverse except no growing
        cur -= d;
        while (true)
        {
          if (cur >= 0) break; // we're on this tile
          if (++li >= psegs.Count) throw new Exception("Ran out of tail segments.");
          pseg = (PathSeg)psegs[li];
          double len = path.detail.getLen(pseg);
          cur += len;
        }
        path.detail.getPos(pseg, cur, pi);
      }

      public void prune()
      { // remove unneeded tail psegs
        path.tail = cur;
        li++;
        while (psegs.Count > li) { psegs.RemoveAt(li); }
      }
    }

    public class TailToHeadIterator : ContinuousPathIterator
    {

      private int li;
      private PathSeg pseg;
      private List<PathSeg> psegs;
      private double cur;
			private ContinuousPath path;

      public TailToHeadIterator(ContinuousPath path)
      {
        psegs = path.path.psegs;
        li = psegs.Count-1;
        pseg = (PathSeg)psegs[li];
        cur = path.tail;
				this.path = path;

      }

      public void step(double d, PathInfo pi)
      { // like moveForward except no growing
        cur += d;
        while (true)
        {
          double len = path.detail.getLen(pseg);
          if (cur <= len) break; // we're on this tile
          if (--li < 0) throw new Exception("Ran out of head segments.");
          pseg = psegs[li];
          cur -= len; // yes, len from that other pseg
        }
        path.detail.getPos(pseg, cur, pi);
      }

      public void prune()
      { // remove unneeded head psegs
        path.head = cur;
        while (--li >= 0) { psegs.RemoveAt(li); }
      }
    }
  }

}

