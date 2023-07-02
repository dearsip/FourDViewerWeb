/*
 * Enemy.java
 */

public abstract class Enemy {

   protected Geom.Shape shape;
   protected ShootModel model;

   public const int STAND = 0;
   public const int STAND_SHOOT = 1;
   public const int WALK = 2;
   public const int WALK_SHOOT = 3;

   public static Enemy createEnemy(Geom.Shape shape, int type) {
      Enemy enemy;
      switch (type) {
         default:
         case STAND:
            enemy = new StandEnemy(shape,false,false);
            break;
         case STAND_SHOOT:
            enemy = new StandEnemy(shape,true,false);
            break;
         case WALK:
            enemy = new StandEnemy(shape,false,true);
            break;
         case WALK_SHOOT:
            enemy = new StandEnemy(shape,true,true);
            break;
      }
      return enemy;
   }

   public Enemy(Geom.Shape shape) {
      this.shape = shape;
   }

   public void setModel(ShootModel model) { this.model = model; }

   public Geom.Shape getShape() { return shape; }

   public abstract bool hit();
   public abstract void move(double delta);
}
