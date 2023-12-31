﻿/*
 * StandEnemy.java
 */

//import java.util.Random;
using System;

/**
 * Standard enemy objects.
 */

public class StandEnemy : Enemy {

   protected bool shoot;
   protected bool isMove;
   protected Random random;
   protected double wshoot;
   protected double wmove;
   protected double[] walk;
   protected double[] reg1;
   protected double[] reg2;
   protected double[] reg3;

   public StandEnemy(Geom.Shape shape, bool shoot, bool move) : base(shape) {
      this.shoot = shoot;
      this.isMove = move;
      random = new Random();
      wshoot = 1;
      wmove = 0;
      if (move) shape.setNoUserMove();
      int dim = shape.getDimension();
      walk = new double[dim];
      reg1 = new double[dim];
      reg2 = new double[dim];
      reg3 = new double[dim-1];
   }

   public override void move(double delta) {
      if (shoot) {
         if (wshoot < 0) {
            Vec.sub(reg1,model.getOrigin(reg1),shape.aligncenter);
            model.addBullet(shape.aligncenter,reg1,4);
            wshoot = 1 + random.NextDouble();
         }
         wshoot -= delta;
      }
      if (isMove) {
         if (wmove < 0) {
            Vec.randomNormalized(reg3,random);
            Vec.scale(reg3,reg3,0.6);
            for (int i=0; i<reg3.Length; i++) {
               walk[(i+2)%walk.Length] = reg3[i];
            }
            wmove = 1.5 + random.NextDouble() * 2;
         }
         Vec.scale(reg1,walk,delta);
         shape.translate(reg1);
         if (!model.isSeparated(shape,model.getOrigin(reg1))) {
            // Vec.scale(reg1,walk,-1);
            // shape.translate(reg1);
            Vec.scale(walk,walk,-1);
            wmove = 1.5 + random.NextDouble() * 2;
         }
         wmove -= delta;
      }
   }

   public override bool hit() { return true; }

}
