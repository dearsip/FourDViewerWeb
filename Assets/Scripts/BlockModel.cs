/*
 * BlockModel.java
 */

//import java.awt.Color;
using System;

/**
 * A model that lets the user build architecture with blocks.
 */

public class BlockModel : ActionModel {

   private int[] reg4;
   private int[] reg5;

   public BlockModel(int dim, Geom.Shape[] shapes, Struct.DrawInfo drawInfo, Struct.ViewInfo viewInfo, Struct.FootInfo footInfo)
      : base(dim, shapes, drawInfo, viewInfo, footInfo, null) {
      reg4 = new int[dim];
      reg5 = new int[dim];
      addShape = this.shapes[0].copy();
   }

   // --- implementation of IKeysNew ---

   private Geom.Shape createShape() {
      return addShape.copy();
   }

   public override bool canAddShapes() {
      return true;
   }

   public override void addShapes(int quantity, bool alignMode, double[] origin, double[] viewAxis) {
      Vec.unitVector(reg1,1);
      Vec.addScaled(reg3,origin,reg1,0.5);
      Geom.Shape colShape = findShape(reg3,viewAxis);
      if (colShape == null) {
         Vec.addScaled(reg2,reg3,viewAxis,100);
         if (reg2[1] < 0) {
            Vec.mid(reg2,reg3,reg2,reg3[1]/(reg3[1]-reg2[1])-0.000001);
            Grid.toCell(reg4,reg4,reg2);
         } else return;
      } else {
         Vec.copy(reg2,colShape.aligncenter);
         Grid.toCell(reg4,reg4,reg2);
         int d = calcDir(colShape.cell[faceNumber].normal);
         Dir.apply(d,reg4,1);
      }
      Grid.toCell(reg5,reg5,reg3);
      for (int i=0; i<dim; i++) {
         if (reg4[i] != reg5[i]) break;
         if (i==dim-1) return;
      }
      Grid.fromCell(reg2,reg4);

      if (countSlots() == 0) reallocate(shapes.Length+1);

      Geom.Shape shape = createShape();
      Vec.sub(reg2,reg2,shape.aligncenter);
      shape.translate(reg2);
      shapes[findSlot(0)] = shape;
   }

   private int calcDir(double[] n) {
      double max = Math.Abs(n[0]);
      int imax = 0;
      for (int i=1; i<n.Length; i++) {
         if (max < Math.Abs(n[i])) {
            max = Math.Abs(n[i]);
            imax = i;
         }
      }
      return 2*imax + ((n[imax] > 0) ? 0 : 1);
   }

   public override void removeShape(double[] origin, double[] viewAxis) {

      if (selectedShape != null) return;

      Vec.unitVector(reg1,1);
      Vec.addScaled(reg3,origin,reg1,0.5);
      Geom.Shape shape = findShape(reg3,viewAxis);
      if (shape == null) return;

      if (shape.isNoUserMove) return;

      addShape = shape;
      int i = indexOf(shape);
      shapes[i] = null;
      clipUnits[i].setBoundaries(null);
      clearSeparators(i);
   }

   // --- implementation of IModel ---

   public override int getSaveType() {
      return IModel.SAVE_BLOCK;
   }

   public override bool atFinish(double[] origin, int[] reg1, int[] reg2) {
      return false;
   }

   public override void animate(double delta) {
      engine.Fall(delta);
   }

    public override void setSelectedColor(UnityEngine.Color color)
    {
    }

    public override void setSelectedShape(Geom.Shape shape)
    {
    }

    public override void setPaintColor(UnityEngine.Color color)
    {
    }

}
