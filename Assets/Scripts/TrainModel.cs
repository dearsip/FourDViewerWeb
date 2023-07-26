/*
 * TrainModel.java
 */

//import java.util.Iterator;
//import java.util.LinkedList;
using System.Collections.Generic;

/**
 * A model that lets the user move around geometric shapes.
 */

public class TrainModel : GeomModel {

   private Track track;
   private Train[] trains;
   private int velNumber;

   public TrainModel(int dim, Geom.Shape[] shapes, Struct.DrawInfo drawInfo, Struct.ViewInfo viewInfo,
                     Track track, Train[] trains) : base(dim,join(shapes,getShapes(trains)),drawInfo,viewInfo) {

      this.track = track;
      this.trains = trains;
      this.velNumber = 0; // initial state is stopped
   }

   public static void init(Track track, Train[] trains) {
      for (int i=0; i<trains.Length; i++) {
         trains[i].init(track);
      }
      // split this out, it has to happen before GeomModel constructor
   }

   public static Geom.Shape[] join(Geom.Shape[] shapes, List<Geom.Shape> list) {
      Geom.Shape[] result = new Geom.Shape[shapes.Length + list.Count];
      int n = 0;

      for (int i=0; i<shapes.Length; i++) {
         result[n++] = shapes[i];
      }

      foreach (Geom.Shape s in list)
      {
         result[n++] = s;
      }

      return result;
   }

   public static List<Geom.Shape> getShapes(Train[] trains) {
      List<Geom.Shape> list = new List<Geom.Shape>();
      for (int i=0; i<trains.Length; i++) {
         trains[i].getShapes(list);
      }
      return list;
   }

    public override void setOptions(OptionsColor oc, int seed, int depth, bool arrow, bool[] texture, OptionsDisplay od)
    {
        setTexture(texture);
        setTransparency(od.transparency);
        usePolygon = od.usePolygon;
        useEdgeColor = od.useEdgeColor;
        hideSel = od.hidesel;
        invertNormals = od.invertNormals;
        useSeparation = od.separate;
        cameraDistance = od.cameraDistance;
        glide = od.glide;
        velNumber = od.trainSpeed;
    }

   public override bool isAnimated() {
      return true;
   }

   public override int getSaveType() {
      return IModel.SAVE_NONE;
   }

   public override void animate(double delta) {
      double d = velNumber*delta*30*track.getVelStep();
      if (d == 0) return;
      for (int i=0; i<trains.Length; i++) {
         bool ok = (d > 0) ? trains[i].moveForward(d) : trains[i].moveReverse(-d);
         // ignore the result; if we bonk at a dead end that's OK.
         // the trouble is, when there are multiple trains,
         // we don't want to stop them all if one hits a dead end.
      }
   }

// --- implementation of IKeysNew ---

   public override void adjustSpeed(int dv) {
      if (dv == 0) velNumber = 0; // not really dv in this case
      else velNumber += dv;
   }

   public override void toggleTrack() {
      track.toggleTrack();
   }

   // GeomModel handles toggleEdgeColor

   protected override void clickNoShape(double[] origin, double[] viewAxis) {
      track.click(origin,viewAxis);
   }

   protected override void clickNoUserMove(Geom.Shape shape, double[] origin, double[] viewAxis) {
      Track.TileTexture tt = Platform.getTileTexture(shape);
      if (tt != null) track.click(origin,viewAxis,tt);
   }

}

