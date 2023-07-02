/*
 * DefaultContext.java
 */
using UnityEngine;
//import java.awt.Color;

/**
 * The default data context for geometry-definition commands.
 */

public class DefaultContext
{

    // if you change these names, you may also need to change
    // some duplicates in the save code.  but, for backward
    // compatibility you should really avoid changing anything.

    public static Context create()
    {
        Context c = new Context();

        c.dict.Add("dup", new Command.Dup());
        c.dict.Add("exch", new Command.Exch());
        c.dict.Add("pop", new Command.Pop());
        c.dict.Add("index", new Command.Index());
        c.dict.Add("def", new Command.Def());

        c.dict.Add("add", new Command.Add());
        c.dict.Add("sub", new Command.Sub());
        c.dict.Add("mul", new Command.Mul());
        c.dict.Add("div", new Command.Div());
        c.dict.Add("neg", new Command.Neg());

        c.dict.Add("[", new Command.ArrayStart());
        c.dict.Add("]", new Command.ArrayEnd());

        c.dict.Add("include", new Command.Include());

        c.dict.Add("true", true);
        c.dict.Add("false", false);

        c.dict.Add("viewinfo", new Command.ViewInfo());
        c.dict.Add("drawinfo", new Command.DrawInfo());
        //
        c.dict.Add("noblock3", new Struct.DimensionMarker(3));
        c.dict.Add("noblock4", new Struct.DimensionMarker(4));

        c.dict.Add("null", new Command.Null()); // can't just put constant null, that would look like undefined
        c.dict.Add("?", new Command.Null());
        c.dict.Add("edge", new Command.Edge());
        c.dict.Add("cedge", new Command.ColorEdge());
        c.dict.Add("face", new Command.Face());
        c.dict.Add("shape", new Command.Shape());
        c.dict.Add("texture", new Command.Texture());
        c.dict.Add("shapetexture", new Command.ShapeTexture());
        c.dict.Add("glue", new Command.Glue());
        c.dict.Add("unglue", new Command.Unglue());

        c.dict.Add("nomove", new Command.NoUserMove());
        c.dict.Add("idealize", new Command.Idealize());
        c.dict.Add("copy", new Command.Copy());
        c.dict.Add("place", new Command.Place());
        c.dict.Add("translate", new Command.Translate());
        c.dict.Add("scale", new Command.Scale());
        c.dict.Add("aligncenter", new Command.AlignCenter());
        c.dict.Add("rotate", new Command.Rotate(false));
        c.dict.Add("altrot", new Command.Rotate(true));
        c.dict.Add("glass", new Command.Glass());
        c.dict.Add("shapecolor", new Command.ShapeColor());
        c.dict.Add("facecolor", new Command.FaceColor());
        c.dict.Add("edgecolor", new Command.EdgeColor());
        c.dict.Add("facetexture", new Command.FaceTexture());
        c.dict.Add("genpoly", new Command.GeneralPolygon());
        c.dict.Add("etr", new Command.EdgeToRadius());
        c.dict.Add("eth", new Command.EdgeToHeight());
        c.dict.Add("polygon", new Command.Polygon(GeomUtil.OFFSET_REG));
        c.dict.Add("altpoly", new Command.Polygon(GeomUtil.OFFSET_ALT));
        c.dict.Add("product", new Command.Product());
        c.dict.Add("rect", new Command.Rect());
        c.dict.Add("prism", new Command.Prism());
        c.dict.Add("frustum", new Command.Frustum());
        c.dict.Add("cone", new Command.Cone());
        c.dict.Add("polygon-antiprism", new Command.Antiprism(GeomUtil.OFFSET_REG));
        c.dict.Add("altpoly-antiprism", new Command.Antiprism(GeomUtil.OFFSET_ALT));
        c.dict.Add("trainpoly", new Command.TrainPoly());

        c.dict.Add("texturecolor", new Command.TextureColor());
        c.dict.Add("union", new Command.Union());
        c.dict.Add("merge", new Command.Merge());
        c.dict.Add("normalize", new Command.Normalize());
        c.dict.Add("lift", new Command.Lift());
        c.dict.Add("project", new Command.Project());

        c.dict.Add("newtrack", new Command.NewTrack());
        c.dict.Add("newtrack2", new Command.NewTrack2());
        c.dict.Add("platformstyle", new Command.Set(typeof(Track),"setPlatformStyle"));
      c.dict.Add("platformthickness",new Command.Set(typeof(Track),"setPlatformThickness"));
      c.dict.Add("platformwidth",new Command.Set(typeof(Track),"setPlatformWidth"));
      c.dict.Add("platformcolor",new Command.Set(typeof(Track),"setPlatformColor",typeof(Color)));
      c.dict.Add("platformcorner",new Command.Set(typeof(Track),"setPlatformCorner",typeof(bool)));
      c.dict.Add("ramptriangle",new Command.Set(typeof(Track),"setRampTriangle",typeof(bool)));
      c.dict.Add("pylonwidth",new Command.Set(typeof(Track),"setPylonWidth"));
      c.dict.Add("pylonsides",new Command.Set(typeof(Track),"setPylonSides"));
      c.dict.Add("pylonoffset",new Command.Set(typeof(Track),"setPylonOffset",typeof(bool)));
      c.dict.Add("track",new Command.AddTrack());
      c.dict.Add("platforms",new Command.AddPlatforms());
      c.dict.Add("platform", new Command.AddPlatform(false));
      c.dict.Add("rplatform",new Command.AddPlatform(true));
      c.dict.Add("roundshape",new Command.RoundShape());
      // maybe add thinshape some day
      c.dict.Add("ramp",new Command.AddRamp());
      c.dict.Add("pylon", new Command.AddPylon(false));
      c.dict.Add("bpylon",new Command.AddPylon(true));
      c.dict.Add("train",new Command.TrainCtor());
      c.dict.Add("car",   new Command.CarCtor(false));
      c.dict.Add("lencar",new Command.CarCtor(true));

      c.dict.Add("mat3",new Command.ConstructSetColor(typeof(Mat.Mat3)));
      c.dict.Add("mat4",new Command.ConstructSetColor(typeof(Mat.Mat4)));
      c.dict.Add("oldmat4",new Command.ConstructSetColor(typeof(Mat.OldMat4)));
      c.dict.Add("nomat",new Command.Construct(typeof(Mat.NoMat)));
      //
      c.dict.Add("meshring3",new Command.MeshRing3());
      c.dict.Add("meshring4",new Command.MeshRing4());
      c.dict.Add("meshsphere4",new Command.MeshSphere4());
      c.dict.Add("meshframe4",new Command.MeshFrame4());
      c.dict.Add("meshringframe4",new Command.MeshRingFrame4());
      c.dict.Add("meshcube4",new Command.MeshCube4());
      c.dict.Add("groundcube3",new Command.GroundCube3());
      c.dict.Add("groundcube4",new Command.GroundCube4());
      //
      c.dict.Add("heightconst",new Command.HeightConst());
      c.dict.Add("heightpower",new Command.HeightPower());
      c.dict.Add("mountain",new Command.HeightMountain());
      c.dict.Add("heightmax",new Command.HeightMaxN());
      c.dict.Add("colorconst",new Command.ColorConst());
      c.dict.Add("colordir",new Command.ColorDir());
      c.dict.Add("colorblend",new Command.ColorBlend());
      //
      c.dict.Add("compass3",new Command.Compass(3));
      c.dict.Add("compass4",new Command.Compass(4));
      c.dict.Add("grid3",new Command.Grid(3));
      c.dict.Add("grid4",new Command.Grid(4));
      c.dict.Add("ground",new Command.Ground());
      c.dict.Add("groundtexture",new Command.GroundTexture());
      c.dict.Add("monolith",new Command.Monolith());
      c.dict.Add("horizon",new Command.Horizon());
      c.dict.Add("sky",new Command.Sky());
      c.dict.Add("sun",new Command.Sun());

      c.dict.Add("TM_SQUARE",(double)Train.TM_SQUARE);
      c.dict.Add("TM_ROUND", (double)Train.TM_ROUND );
      c.dict.Add("TM_ROTATE",(double)Train.TM_ROTATE);

      c.dict.Add("PS_SQUARE",    (double)Platform.PS_SQUARE    );
      c.dict.Add("PS_ROUND",     (double)Platform.PS_ROUND     );
      c.dict.Add("PS_ROUND_MORE",(double)Platform.PS_ROUND_MORE);
      c.dict.Add("PS_THIN",      (double)Platform.PS_THIN      );
      c.dict.Add("PS_THIN_ROUND",(double)Platform.PS_THIN_ROUND);

      c.dict.Add("PROJ_NONE",   (double)Vec.PROJ_NONE   );
      c.dict.Add("PROJ_NORMAL", (double)Vec.PROJ_NORMAL );
      c.dict.Add("PROJ_ORTHO",  (double)Vec.PROJ_ORTHO  );
      c.dict.Add("PROJ_PERSPEC",(double)Vec.PROJ_PERSPEC);

      c.dict.Add("x",0.0);
      c.dict.Add("y",1.0);
      c.dict.Add("z",2.0);
      c.dict.Add("w",3.0);

      c.dict.Add("x+",0.0);
      c.dict.Add("x-",1.0);
      c.dict.Add("y+",2.0);
      c.dict.Add("y-",3.0);
      c.dict.Add("z+",4.0);
      c.dict.Add("z-",5.0);
      c.dict.Add("w+",6.0);
      c.dict.Add("w-",7.0);

      c.dict.Add("STAND",       (double)Enemy.STAND      );
      c.dict.Add("STAND_SHOOT", (double)Enemy.STAND_SHOOT);
      c.dict.Add("WALK",        (double)Enemy.WALK       );
      c.dict.Add("WALK_SHOOT",  (double)Enemy.WALK_SHOOT );

      c.dict.Add("FOOT",         (double)Struct.FOOT        );
      c.dict.Add("COMPASS",      (double)Struct.COMPASS     );
      c.dict.Add("FOOT_COMPASS", (double)Struct.FOOT_COMPASS);

      // have to be a little careful because these are 4D,
      // but they'll work in the situations I use them in.
      //
      c.dict.Add("X+",new double[] { 1, 0, 0, 0 });
      c.dict.Add("X-",new double[] {-1, 0, 0, 0 });
      c.dict.Add("Y+",new double[] { 0, 1, 0, 0 });
      c.dict.Add("Y-",new double[] { 0,-1, 0, 0 });
      c.dict.Add("Z+",new double[] { 0, 0, 1, 0 });
      c.dict.Add("Z-",new double[] { 0, 0,-1, 0 });
      c.dict.Add("W+",new double[] { 0, 0, 0, 1 });
      c.dict.Add("W-",new double[] { 0, 0, 0,-1 });

      c.dict.Add("red",Color.red);
      c.dict.Add("green",Color.green);
      c.dict.Add("blue",Color.blue);
      c.dict.Add("cyan",Color.cyan);
      c.dict.Add("magenta",Color.magenta);
      c.dict.Add("yellow",Color.yellow);
      c.dict.Add("orange",new Color(255,128,0)); // Color.orange is (255,200,0)
      c.dict.Add("gray",Color.gray);
      c.dict.Add("white",Color.white);
      c.dict.Add("brown",new Color(128,96,0));

      c.dict.Add("finishinfo",new Command.FinishInfo());
      c.dict.Add("footinfo",new Command.FootInfo());
      c.dict.Add("blockinfo",new Command.BlockInfo());

      c.dict.Add("enemy",new Command.NewEnemy());

      return c;
   }

}
