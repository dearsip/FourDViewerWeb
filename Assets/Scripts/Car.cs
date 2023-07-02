using System;
using System.Collections.Generic;

public class Car {

   public Geom.Shape shape;
   public double carLenOverride;
   public double len;
   public double[] anchorCenter;

   /**
    * Turn a shape in standard position into a railcar.
    * Bottom face stays on the bottom, the head points
    * to the right, the tail to the left.
    */
   public Car(Geom.Shape shape, double carLenOverride) {

      this.shape = shape;
      this.carLenOverride = carLenOverride;

      // rest has to wait until we know the scale
   }

   public void init(double carLen, double[] dScale) {

      double save = dScale[0];
      dScale[0] *= (carLenOverride != 0) ? carLenOverride : carLen;
      shape.scale(dScale);
      dScale[0] = save;

      shape.systemMove = true;
      //shape.noUserMove = true;

      int i = shape.findFace(/* axis = */ 1,/* sign = */ -1);
      if (i == -1) throw new Exception("Unable to find bottom face of railcar.");
      Geom.Cell cell = shape.bottomFace = shape.cell[i];

      bool[] b = shape.getCellVertices(cell);
      double vmin = shape.vmin(b,/* axis = */ 0);
      double vmax = shape.vmax(b,/* axis = */ 0);
      len = vmax-vmin;

      anchorCenter = Geom.clone1(cell.center);
      anchorCenter[0] = (vmax+vmin) / 2; // so length is centered
   }

   public void getShapes(List<Geom.Shape> list) {
      list.Add(shape);
   }

   public void placeCentered(Track.PathInfo pi, bool rotate) {

      shape.reset();
      // why not use a place operation?  there are a couple of reasons.
      // one, I think this might be faster.  two, place is relative to
      // aligncenter, and since that's not on the bottom face, I think
      // we might get FP error on the bottom plane.  aligncenter could
      // be set to anchorCenter, then maybe everything would work, but
      // I don't want to mess with it.

      if (rotate) {
         // helix first, otherwise sign gets mixed up
         if (pi.helix != 0) {
            shape.rotate(6,4,pi.helix*90,anchorCenter);
         }
         // first turn positive x to point at this direction
         int dir = Dir.getOpposite(pi.fromDir);
         if (dir == 1) { // negative x, only 180 degree case
            shape.rotate(0,4,180,anchorCenter);
         } else {
            shape.rotate(0,dir,90,anchorCenter);
         }
         if (pi.angle != 0) {
            shape.rotate(dir,pi.toDir,pi.angle*90,anchorCenter);
         }
         // angle and helix are mutually exclusive,
         // so we get two rotations max .. not bad.

         // hints are designed for rotating trains,
         // so only pass them along in rotate case.
         shape.hint = pi.hint;
      }

      Vec.sub(pi.pos,pi.pos,anchorCenter);
      shape.translate(pi.pos);
   }

}

