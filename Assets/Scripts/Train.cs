/*
 * Train.java
 */

//import java.util.LinkedList;
using System.Collections.Generic;

/**
 * Data structure for trains.
 */

public class Train : IDimensionMultiSrc {

   public const int TM_SQUARE = 0;
   public const int TM_ROUND  = 1;
   public const int TM_ROTATE = 2;

   public Car[] cars;
   public double gap;

   // one-time use variables for deferred construction of ContinuousPath
   public int[] pos;
   public int fromDir;
   public int toDir;
   public double d0;
   public int trainMode; // this is not entirely one-time

   public Track.ContinuousPath path;
   public Track.PathInfo pi;

   public Train(Car[] cars, double gap, int[] pos, int fromDir, int toDir, double d0, int trainMode) {

      this.cars = cars;
      this.gap = gap;

      this.pos = pos;
      this.fromDir = fromDir;
      this.toDir = toDir;
      this.d0 = d0;
      this.trainMode = trainMode;
   }

   public void init(Track track) {

      int dim = track.getDimension();

      double[] dScale = new double[dim];
      for (int i=0; i<dim; i++) dScale[i] = track.getCarScale();

      for (int i=0; i<cars.Length; i++) {
         cars[i].init(track.getCarLen(),dScale);
      }

      path = new Track.ContinuousPath(track,pos,fromDir,toDir,d0,/* round = */ (trainMode >= TM_ROUND));
      pi = new Track.PathInfo(dim);

      placeCenteredHeadToTail(path.initialIterator(path));
   }

   public void getDimension(IDimensionMultiDest dest) {
      for (int i=0; i<cars.Length; i++) {
         dest.putDimension(cars[i].shape.getDimension());
      }
   }

   public void getShapes(List<Geom.Shape> list) {
      for (int i=0; i<cars.Length; i++) {
         cars[i].getShapes(list);
      }
   }

   public bool moveForward(double d) {
      bool ok = path.moveForward(d,/* random = */ true);
      placeCenteredHeadToTail(path.headToTailIterator(path));
      return ok;
   }

   public bool moveReverse(double d) {
      bool ok = path.moveReverse(d,/* random = */ true);
      placeCenteredTailToHead(path.tailToHeadIterator(path));
      return ok;
   }

   public void placeCenteredHeadToTail(Track.ContinuousPathIterator cpi) {
      for (int i=0; i<cars.Length; i++) {
         cpi.step((i == 0) ? 0 : (cars[i-1].len/2 + gap + cars[i].len/2),pi);
         cars[i].placeCentered(pi,/* rotate = */ (trainMode >= TM_ROTATE));
      }
      cpi.prune();
   }

   public void placeCenteredTailToHead(Track.ContinuousPathIterator cpi) {
      for (int i=cars.Length-1; i>=0; i--) {
         cpi.step((i == cars.Length-1) ? 0 : (cars[i].len/2 + gap + cars[i+1].len/2),pi);
         cars[i].placeCentered(pi,/* rotate = */ (trainMode >= TM_ROTATE));
      }
      cpi.prune();
   }

}

