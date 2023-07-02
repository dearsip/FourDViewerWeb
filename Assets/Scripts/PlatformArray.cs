/*
 * PlatformArray.java
 */

/**
 * Another utility class for manipulating arrays with variable numbers of dimensions.
 * There are many similarities to {@link DynamicArray}, but also several differences.
 */

public class PlatformArray {

// --- fields ---

   private int dim;
   private int[] min;
   private int[] max;
   private object[][][] data; // array of user objects

// --- construction ---

   public PlatformArray(int[] min, int[] max) {
      //dim = min.Length;
      if (min.Length == 2) throw new System.Exception("The system does not support 3D scene");
      this.min = min;
      this.max = max;
      //if (dim == 2) { // no y dimension here
         //data = new object[max[0]-min[0]+1][max[1]-min[1]+1];
      //} else {
         data = new object[max[0]-min[0]+1][][];
         for (int i = 0; i < data.Length; i++) {
            data[i] = new object[max[1]-min[1]+1][];
            for (int j = 0; j < data[i].Length; j++)
                data[i][j] = new object[max[2]-min[2]+1];
         }
      //}
   }

// --- accessors ---

   public object get(int[] p) {
      //if (dim == 2) {
         //return ((object[][]) data)[p[0]-min[0]][p[1]-min[1]];
      //} else {
         return ((object[][][]) data)[p[0]-min[0]][p[1]-min[1]][p[2]-min[2]];
      //}
   }

   public void set(int[] p, object o) {
      //if (dim == 2) {
         //((object[][]) data)[p[0]-min[0]][p[1]-min[1]] = o;
      //} else {
         ((object[][][]) data)[p[0]-min[0]][p[1]-min[1]][p[2]-min[2]] = o;
      //}
   }

// --- iterator ---

   public class Iterator {

   // --- fields ---

      private int[] pmin;
      private int[] pmax;
      private int[] i;
      private bool done;

   // --- construction ---

      public Iterator(int[] pmin, int[] pmax) {
         this.pmin = pmin;
         this.pmax = pmax;
         i = (int[]) pmin.Clone();
         done = false;
      }

   // --- methods ---

      public bool hasCurrent() {
         return ( ! done );
      }

      public int[] current() {
         return i; // caller shouldn't modify
      }

      public void increment() {
         for (int j=0; j<i.Length; j++) {
            if (++i[j] <= pmax[j]) return; // no carry
            i[j] = pmin[j];
         }
         done = true;
      }
   }

// --- methods ---

   public void clear(int[] pmin, int[] pmax) {
      Iterator i = new Iterator(pmin,pmax);
      while (i.hasCurrent()) {
         set(i.current(),null);
         i.increment();
      }
   }

   public void clearAll() {
      clear(min,max);
   }

}


