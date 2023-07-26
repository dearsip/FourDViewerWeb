/*
 * ActionModel.java
 */

//import java.awt.Color;
using UnityEngine;

/**
 * A model that lets the user walk around geometric shapes.
 */

public class ActionModel : GeomModel {

   protected double[] reg3;
   protected Engine engine;
   private int[] finish;
   private Geom.Texture foot;
   protected bool bfoot;
   protected bool bcompass;
   protected Color[] compassColor;

   public ActionModel(int dim, Geom.Shape[] shapes, Struct.DrawInfo drawInfo, Struct.ViewInfo viewInfo, Struct.FootInfo footInfo, Struct.FinishInfo finishInfo) : base(dim,shapes,drawInfo,viewInfo) {
      if (finishInfo != null) this.finish = finishInfo.finish;
      if (footInfo != null) {
         this.bfoot = footInfo.foot;
         this.bcompass = footInfo.compass;
      }
      reg3 = new double[dim];

      if (bfoot || bcompass) foot = (dim==3) ? setFoot3() : setFoot4();
      if (bcompass) compassColor = (dim==3) ? cColor3 : cColor4;

      invertNormals = false;
      useSeparation = true;
      glide = true;
   }

   public void setEngine(Engine engine) {
      this.engine = engine;
   }

   public int[] retrieveFinish() {
      return finish;
   }

   public int retrieveFoot() {
      int i=0;
      if (bfoot) i+=1;
      if (bcompass) i+=2;
      return i;
   }

   // --- implementation of IKeysNew ---

   public override void scramble(bool alignMode, double[] origin) {}
   public override void toggleSeparation() {}
   public override void addShapes(int quantity, bool alignMode, double[] origin, double[] viewAxis) {}
   public override void removeShape(double[] origin, double[] viewAxis) {}
   public override void toggleNormals() {}
   public override void toggleHideSel() {}
   public override void paint(double[] origin, double[] viewAxis) {}

   public override bool canAddShapes() { return false; }
   public override bool canPaint() { return false; }

   public override void jump() {
      engine.jump();
   }

   // --- implementation of IModel ---

    public override void setOptions(OptionsColor oc, int seed, int depth, bool arrow, bool[] texture, OptionsDisplay od)
    {
        setTexture(texture);
        setTransparency(od.transparency);
        usePolygon = od.usePolygon;
        useEdgeColor = od.useEdgeColor;
        hideSel = od.hidesel;
        cameraDistance = od.cameraDistance;
    }


   public override bool getAlignMode(bool defaultAlignMode) {
      return false;
   }

   public override bool isAnimated() {
      return true;
   }

   public override int getSaveType() {
      return IModel.SAVE_ACTION;
   }

   public override bool atFinish(double[] origin, int[] reg1, int[] reg2) {
      int dir = Grid.toCell(reg1, reg2, origin);
      return (                            Grid.equals(reg1,finish)
            || (dir != Dir.DIR_NONE && Grid.equals(reg2,finish) ) );
   }

   public override void animate(double delta) {
      engine.Fall(delta);
   }

   public override void render(double[] origin, double[][] axis, bool viewClip) {
      this.viewClip = viewClip;
      Vec.unitVector(reg1,1);
      Vec.addScaled(reg3,origin,reg1,0.5);
      Vec.addScaled(reg3,reg3,axis[dim-1],-cameraDistance);
      renderer(reg3, axis);

        currentDraw = buf;
      if (bfoot) { drawPoint(origin,Color.green,0.02); }
      if (bcompass) {
         origin[1] += 0.5;
         for (int i=0; i<2*(dim-1); i++) {
            int j = (i<2) ? i : i+2;
            Dir.apply(j,origin,0.5);
            drawPoint(origin,compassColor[i],0.05);
            Dir.apply(j,origin,-0.5);
         }
         origin[1] -= 0.5;
      }
   }

   private void drawPoint(double[] center, Color c, double s) {
      for (int i=0; i<foot.edge.Length; i++) {
         Geom.Edge edge = foot.edge[i];

         Vec.addScaled(reg1,center,foot.vertex[edge.iv1],s);
         Vec.addScaled(reg2,center,foot.vertex[edge.iv2],s);

         drawLine(reg1,reg2,c,reg3);
      }
   }


   public Clip.Result getResult() {
      return clipResult;
   }

   private Geom.Texture setFoot3() {
      Geom.Edge[] edge = new Geom.Edge[fedge3.Length];
      for (int i=0;i<edge.Length;i++) edge[i] = new Geom.Edge(fedge3[i][0],fedge3[i][1]);
      return new Geom.Texture(edge,fvertex3);
   }

   private Geom.Texture setFoot4() {
      Geom.Edge[] edge = new Geom.Edge[fedge4.Length];
      for (int i=0;i<edge.Length;i++) edge[i] = new Geom.Edge(fedge4[i][0],fedge4[i][1]);
      return new Geom.Texture(edge,fvertex4);
   }

   private Color[] cColor3 = { Color.magenta, new Color(128,0,128), Color.cyan, new Color(0,128,128) };
   private Color[] cColor4 = { Color.magenta, new Color(128,0,128), Color.yellow, new Color(128,128,0), Color.cyan, new Color(0,128,128) };
   private int[][] fedge3 = { new int[] { 0,1 },new int[] { 1,2 },new int[] { 2,3 },new int[] { 3,0 },new int[] { 0,4 },new int[] { 1,4 },new int[] { 2,4 },new int[] { 3,4 },new int[] { 0,5 },new int[] { 1,5 },new int[] { 2,5 },new int[] { 3,5 } };
   private int[][] fedge4 = { new int []{ 0,1 },new int []{ 1,2 },new int []{ 2,3 },new int []{ 3,0 },new int []{ 0,4 },new int []{ 1,4 },new int []{ 2,4 },new int []{ 3,4 },new int []{ 0,5 },new int []{ 1,5 },new int []{ 2,5 },new int []{ 3,5 },new int []{ 0,6 },new int []{ 1,6 },new int []{ 2,6 },new int []{ 3,6 },new int []{ 4,6 },new int []{ 5,6 },new int []{ 0,7 },new int []{ 1,7 },new int []{ 2,7 },new int []{ 3,7 },new int []{ 4,7 },new int []{ 5,7 } };
   private double[][] fvertex3 = { new double[] { 1,0,0 },new double[] { 0,0,1 },new double[] { -1,0,0 },new double[] { 0,0,-1 },new double[] { 0,1,0 },new double[] { 0,-1,0 } };
   private double[][] fvertex4 = { new double[] { 1,0,0,0 },new double[] { 0,0,1,0 },new double[] { -1,0,0,0 },new double[] { 0,0,-1,0 },new double[] { 0,0,0,1 },new double[] { 0,0,0,-1 },new double[] { 0,1,0,0 },new double[] { 0,-1,0,0 } };
}
