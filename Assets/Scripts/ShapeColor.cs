/*
 * ShapeColor.java
 */

//import java.awt.Color;
//import java.io.IOException;
//import java.util.Arrays;
//import java.util.Comparator;
//import java.util.HashMap;
//import java.util.Iterator;
//import java.util.LinkedList;
//import java.util.Map;
using UnityEngine;
using System.Collections.Generic;

/**
 * The shape color calculations for Core.writeModel
 * were far too complicated, so I brought them here.
 */

public class ShapeColor {

// --- color states ---

   public interface State {
      Color faceColor(int i);
      Color edgeColor(int i);
   }

   public class NullState : State { // could use ColorState for this

      public Color faceColor(int i) { return Color.clear; }
      public Color edgeColor(int i) { return Color.clear; }
   }

   public class ColorState : State {

      private Color color;
      public ColorState(Color color) { this.color = color; }

      public Color faceColor(int i) { return color; }
      public Color edgeColor(int i) { return color; }
   }

   public class ShapeState : State {

      private Geom.Shape shape;
      public ShapeState(Geom.Shape shape) { this.shape = shape; }

      public Color faceColor(int i) { return shape.cell[i].color; }
      public Color edgeColor(int i) { return shape.edge[i].color; }
   }

// --- helpers ---

   public static void increment(Dictionary<Color, object> map, Color c) {
      object o = map[c];
      int count = (o == null) ? 0 : (int) o;
      map.Add(c,++count);
   }

   public static Color getDominantColor(Geom.Shape shape) {
      Dictionary<Color, object> map = new Dictionary<Color, object>(); // Color -> Integer

      for (int i=0; i<shape.cell.Length; i++) {
         increment(map,shape.cell[i].color);
      }

      int max = 0; // entries will always be at least 1
      Color c = Color.clear;

      foreach (KeyValuePair<Color, object> me in map) {
         int count = (int) me.Value;
         if (count > max) {
            max = count;
            c = (Color) me.Key; // can be null
         }
      }

      return c;
   }

   public static bool equals(Color c1, Color c2) {
      return (c1.a > 0) ? c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a : c2.a == 0;
   }

   public static int countFaceChanges(State base_, Geom.Shape shape) {
      int count = 0;
      for (int i=0; i<shape.cell.Length; i++) {
         if ( ! equals(shape.cell[i].color,base_.faceColor(i)) ) count++;
      }
      return count;
   }

   public static double getWeight(Geom.Shape shape, int cell) {
      int[] ie = shape.cell[cell].ie;
      if (ie.Length == 0) return 0; // unlikely but possible
      Color c = shape.cell[cell].color;
      int same = 0;
      for (int i=0; i<ie.Length; i++) {
         if (equals(shape.edge[ie[i]].color,c)) same++;
      }
      return same / (double) ie.Length;
   }

   public class Record {
      public int face;
      public Color color; // not strictly necessary, but I like it
      public double weight; // 0-1 for fraction of edges that have the face color
   }

   //public static class RecordComparator : Comparator {
      //public int compare(Object o1, Object o2) {
         //double d = ((Record) o1).weight - ((Record) o2).weight;
         //if (d == 0) return 0;
         //return (d > 0) ? 1 : -1;
      //}
   //}

   //public static RecordComparator recordComparator = new RecordComparator();

//// --- generate ---

   //public static void writeColors(IToken t, State base, Geom.Shape shape, HashMap colorNames, boolean first) throws IOException {

   //// decide whether to use shapecolor

      //Color shapeColor = getDominantColor(shape);
      //ColorState cs = new ColorState(shapeColor);

      //int nb = countFaceChanges(base,shape);
      //int nc = countFaceChanges(cs,  shape);

      //if (nc+1 < nb) { // only use if it *reduces* the line count

         //if (first) { t.newLine(); first = false; }
         //t.putWord(Core.format(shapeColor,colorNames)).putWord("shapecolor").newLine();

         //base = cs;
      //}

      //// actually this may not reduce the line count, because the
      //// wrong choice will generate a bunch of edgecolor commands.
      //// not sure what we can do about that.  but, for the
      //// single-colored shapes that we usually see, it works fine.

      //// Q: is it possible that a non-dominant color would work better?
      //// A: no.  when a color occupies k faces out of n, the other
      ////    n-k faces are a different color, so we need n-k+1 commands.

   //// generate facecolor commands

      //// it seems like the "same edge color" relation turns the faces
      //// into a graph (a DAG), so we could use the graph structure to
      //// compute the right command order, but ...
      //// 1. graph calculations are hard
      //// 2. in 4D, the intersection of two faces isn't a single edge
      //// 3. the cedge command can make cyclic graphs
      //// 4. same for the new edgecolor command that I added for save

      //LinkedList list = new LinkedList();

      //for (int i=0; i<shape.face.length; i++) {
         //if ( ! equals(shape.face[i].color,base.faceColor(i)) ) {

            //Record r = new Record();
            //r.face = i;
            //r.color = shape.face[i].color;
            //r.weight = getWeight(shape,i);

            //list.add(r);
         //}
      //}

      //Record[] array = (Record[]) list.toArray(new Record[list.size()]);
      //Arrays.sort(array,recordComparator);

      //for (int i=0; i<array.length; i++) {
         //Record r = array[i];

         //if (first) { t.newLine(); first = false; }
         //t.putInteger(r.face).putWord(Core.format(r.color,colorNames)).putWord("facecolor").newLine();
      //}

   //// generate edgecolor commands

      //Color[] ec = new Color[shape.edge.length];

      //for (int i=0; i<shape.edge.length; i++) {
         //ec[i] = base.edgeColor(i);
      //}

      //for (int i=0; i<array.length; i++) {
         //Record r = array[i];

         //// apply facecolor to edges, as in Geom.Shape.updateEdgeColor
         //int[] ie = shape.face[r.face].ie;
         //for (int j=0; j<ie.length; j++) ec[ie[j]] = r.color;
      //}

      //// here ec has the new base state, but it's not worth
      //// setting that up, we can just generate the commands

      //for (int i=0; i<shape.edge.length; i++) {
         //if ( ! equals(shape.edge[i].color,ec[i]) ) {

            //if (first) { t.newLine(); first = false; }
            //t.putInteger(i).putWord(Core.format(shape.edge[i].color,colorNames)).putWord("edgecolor").newLine();
         //}
      //}
   //}

}

