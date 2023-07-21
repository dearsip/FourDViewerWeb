/*
 * ShootModel.java
 */

//import java.awt.Color;
//import java.util.LinkedList;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShootModel : ActionModel {

   private int count;
   private Enemy[] enemies;
   private Bullet[] bullets;
   private int[] reg4;
   private bool isDead;

   public ShootModel(int dim, Geom.Shape[] shapes, Struct.DrawInfo drawInfo, Struct.ViewInfo viewInfo, Struct.FootInfo footInfo, Enemy[] enemies)
      : base(dim, TrainModel.join(shapes,getShapes(enemies)), drawInfo, viewInfo, footInfo, null) {
      this.enemies = enemies;
      for (int i=0; i<enemies.Length; i++) {
         enemies[i].setModel(this);
      }
      bullets = new Bullet[16];
      count = enemies.Length;
      reg4 = new int[dim];
      isDead = false;
   }

   public static List<Geom.Shape> getShapes(Enemy[] enemies) {
      List<Geom.Shape> list = new List<Geom.Shape>();
      for (int i=0; i<enemies.Length; i++) {
         list.Add(enemies[i].getShape());
      }
      return list;
   }

   public double[] getOrigin(double[] dest) {
      Vec.unitVector(dest,1);
      Vec.addScaled(dest,engine.getOrigin(),dest,0.5);
      return dest;
   }

   // --- implementation of IKeysNew ---

   public override bool canAddShapes() { return true; }

   public override void addShapes(int quantity, bool alignMode, double[] origin, double[] viewAxis) {
      Vec.unitVector(reg3,1);
      Vec.addScaled(reg3,origin,reg3,0.5);
      addBullet(reg3,engine.getAxisArray(),6);
   }

   // --- implementation of IModel ---

   public override int getSaveType() {
      return IModel.SAVE_SHOOT;
   }

   public override bool atFinish(double[] origin, int[] reg1, int[] reg2) {
      return (count == 0);
   }

   public override bool dead() { return isDead; }

   public override void animate(double delta) {
      engine.Fall(delta);
      for (int i = 0; i < enemies.Length; i++) {
         if (enemies[i] != null) enemies[i].move(delta);
      }
      for (int i = 0; i < bullets.Length; i++) {
         if (bullets[i] != null && bulletMove(bullets[i],delta)) deleteBullet(i);
      }
   }

   // --- bullet ---

   public void addBullet(double[] center, double[] dir, double speed) {
      double[][] axis = new double[dim][];
      for (int i = 0; i < axis.Length; i++) axis[i] = new double[dim];
      Vec.normalize(dir,dir);

      Vec.copy(axis[dim-1],dir);

      if (dim == 4) {
         axis[0][0]=-dir[1]; axis[0][1]=dir[0]; axis[0][2]=-dir[3]; axis[0][3]=dir[2];
         axis[1][0]=-dir[2]; axis[1][1]=dir[3]; axis[1][2]=dir[0]; axis[1][3]=-dir[1];
         axis[2][0]=-dir[3]; axis[2][1]=-dir[2]; axis[2][2]=dir[1]; axis[2][3]=dir[0];
      } else {
         double[] reg = new double[dim-1];
         for (int i=0; i<dim-1; i++) {
            reg[i] = dir[(2+i)%dim];
         }
         if (dir[1] < 1) {
            axis[1][1] = Math.Sqrt(1-dir[1]*dir[1]);
            Vec.scale(reg,reg,-dir[1]/axis[1][1]);
            for (int i=0; i<dim-1; i++) {
               axis[1][(2+i)%dim] = reg[i];
            }
         } else Vec.unitVector(axis[1],0);
         for (int i=0; i<dim; i++) {
            int i1 = (i+1)%dim;
            int i2 = (i+2)%dim;
            axis[0][i] = axis[1][i1]*axis[2][i2] - axis[1][i2]*axis[2][i1];
         }
      }
      addBullet(center,axis,speed);
   }

   public void addBullet(double[] center, double[][] axis, double speed) {
      for (int i=0; i<bullets.Length; i++) {
         if (bullets[i] == null) {bullets[i] = new Bullet(center,axis,speed,reg3);
         addBullets(bullets[i].shape);
         return;
         }
      }
      Bullet[] bulletsNew = new Bullet[bullets.Length+16];
      Array.Copy(bullets,0,bulletsNew,0,bullets.Length);
      bulletsNew[bullets.Length] = new Bullet(center,axis,speed,reg3);
      addBullets(bulletsNew[bullets.Length].shape);
      bullets = bulletsNew;
   }

   private void addBullets(Geom.Shape shape) {
      if (countSlots() == 0) reallocate(shapes.Length+16);
      shapes[findSlot(0)] = shape;
   }

   private void deleteBullet(int ib) {
      int i = indexOf(bullets[ib].shape);
      shapes[i] = null;
      clipUnits[i].setBoundaries(null);
      clearSeparators(i);
      bullets[ib] = null;
   }

   public bool bulletMove(Bullet bullet, double delta) {
      if ((bullet.leave -= delta) == 0) return true;
      Geom.Shape shape = bullet.shape;
      Vec.scale(reg3,shape.axis[dim-1],bullet.speed*delta);
      Vec.add(reg3,shape.aligncenter,reg3);
      if (!canMove(shape.aligncenter,reg3,reg4,reg2,true)) {
         Geom.Shape hitShape = getHitShape();
         for (int i=0; i<enemies.Length; i++) {
            if (enemies[i] != null && hitShape == enemies[i].getShape()) {
               if (enemies[i].hit()) killEnemy(i);
               break;
            }
         }
         return true;
      }
      Vec.sub(reg2,reg3,getOrigin(reg1));
      if (Vec.norm(reg2) < 0.25) {
         isDead = true;
      }
      Vec.sub(reg3,reg3,shape.aligncenter);
      shape.translate(reg3);
      return false;
   }

   public class Bullet {

      public Geom.Shape shape;
      public double speed;
      public double leave;

      public Bullet(double[] center, double[][] axis, double speed, double[] reg3) {
         int dim = center.Length;
         try {
            if (dim == 3) {
               double[][] d = { new double[] { 0, 0.1 }, new double[] { 0, 0.1 }, new double[] { 0, 0.2 } };
               shape = GeomUtil.rect(d);
            } else {
               double[][] d = { new double[] { 0, 0.1 }, new double[] { 0, 0.1 }, new double[] { 0, 0.1 } , new double[] { 0, 0.2 } };
               shape = GeomUtil.rect(d);
            }
         } catch (Exception) {}
         shape.setNoUserMove();
         shape.setShapeColor(Color.white*OptionsColor.fixer);
         shape.place(center,axis);
         Vec.copy(reg3,axis[dim-1]);
         Vec.scale(reg3,reg3,0.5);
         shape.translate(reg3);

         this.speed = speed;
         leave = 3;
      }

   }

   private void killEnemy(int ie) {
      int i = indexOf(enemies[ie].getShape());
      shapes[i] = null;
      clipUnits[i].setBoundaries(null);
      clearSeparators(i);
      enemies[ie] = null;
      count--;
   }
}
