using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;

public class PropertyFile
{
    public delegate void Loader(IStore store);
    public delegate void Saver(IStore store);
    public static bool isMaze;
    private static IEnumerator testProperties(string file) //throws IOException 
    {
        string text;
#if UNITY_EDITOR
        yield return text = File.ReadAllText(file);
#else
        using (UnityWebRequest www = UnityWebRequest.Get(file))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) text = www.downloadHandler.text;
            else { Debug.Log(www.error); yield break; }
        }
#endif
        isMaze = text[0] == '#';
    }

    // https://stackoverflow.com/questions/485659/can-net-load-and-parse-a-properties-file-equivalent-to-java-properties-class
    // modified for maze properties
    private static IEnumerator loadProperties(string file, Dictionary<string, string> dict){
        if (file == "default.properties") loadDefault(dict);
        else
        {
            string text;
#if UNITY_EDITOR
            yield return text = File.ReadAllText(file);
#else
            using (UnityWebRequest www = UnityWebRequest.Get(file))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success) text = www.downloadHandler.text;
                else { Debug.Log(www.error); yield break; }
            }
#endif
            foreach (string line in text.Replace("\r\n","\n").Split(new[]{'\n','\r'}))
        // string str = "";
        // switch (file) {
            // case "default.properties":
                // str = default_; break;
        // }
        // foreach (string line in str.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                if ((!String.IsNullOrEmpty(line)) &&
                    (!line.StartsWith("#")) &&
                    (line.Contains("=")))
                {
                    int index = line.IndexOf('=');
                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();
                    dict.Add(key, value);
                }
            }
        }
    }

    private static void loadDefault(Dictionary<string, string> dict) {
        foreach (string line in default_.Replace("\r\n","\n").Split(new[]{'\n','\r'}))
        {
            if ((!String.IsNullOrEmpty(line)) &&
                (!line.StartsWith("#")) &&
                (line.Contains("=")))
            {
                int index = line.IndexOf('=');
                string key = line.Substring(0, index).Trim();
                string value = line.Substring(index + 1).Trim();
                dict.Add(key, value);
            }
        }
    }

    private static void storeProperties(string file, Dictionary<string, string> p) {
        try {
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.WriteLine("#"+DateTime.Now.ToString("ddd MMM dd HH:mm:ss K yyyy"));
                foreach(KeyValuePair<string, string> c in p) sw.WriteLine(c.Key+"="+c.Value);
            }
        }
        catch (Exception t)
        {
          Debug.Log(t);
        }
    }

    public static IEnumerator test(string file) {
        yield return testProperties(file);
    }

    public static IEnumerator load(string file, Loader storable) {
        Dictionary<string, string> p = new Dictionary<string, string>();
        yield return loadProperties(file, p);
        try {
            PropertyStore store = new PropertyStore(p);
            storable(store);
        } catch (Exception e) {
            Debug.Log(e);
            //throw App.getException("PropertyFile.e2",new Object[] { file.getName(), e.getMessage() });
        }
    }

    public static void loadDefault(Loader storable) {
        Dictionary<string, string> p = new Dictionary<string, string>();
        loadDefault(p);
        try {
            PropertyStore store = new PropertyStore(p);
            storable(store);
        } catch (Exception e) {
            Debug.LogError(e);
        }
    }

    public static void save(string file, Saver storable) {
        Dictionary<string, string> p = new Dictionary<string, string>();

        try {
            PropertyStore store = new PropertyStore(p);
            storable(store);
        } catch (Exception e) {
            Debug.Log(e);
        }

        try {
            storeProperties(file,p);
        } catch (IOException e) {
            Debug.Log(e);
        }
    }

    private static readonly string default_ = @"
#土 7 30 18:12:22 +09:00 2022
opt.om4.dimMap=4
opt.om4.size[0]=3
opt.om4.size[1]=3
opt.om4.size[2]=3
opt.om4.size[3]=3
opt.om4.density=1
opt.om4.twistProbability=0.4
opt.om4.branchProbability=0.2
opt.om4.allowLoops=False
opt.om4.loopCrossProbability=0.7
opt.om4.allowReservedPaths=True
opt.oc4.colorMode=1
opt.oc4.dimSameParallel=0
opt.oc4.dimSamePerpendicular=0
opt.oc4.enable[0]=True
opt.oc4.enable[1]=True
opt.oc4.enable[2]=True
opt.oc4.enable[3]=True
opt.oc4.enable[4]=True
opt.oc4.enable[5]=False
opt.oc4.enable[6]=True
opt.oc4.enable[7]=True
opt.oc4.enable[8]=False
opt.oc4.enable[9]=False
opt.oc4.enable[10]=True
opt.oc4.enable[11]=False
opt.ov4.depth=5
opt.ov4.arrow=False
opt.ov4.texture[0]=False
opt.ov4.texture[1]=False
opt.ov4.texture[2]=False
opt.ov4.texture[3]=False
opt.ov4.texture[4]=False
opt.ov4.texture[5]=False
opt.ov4.texture[6]=False
opt.ov4.texture[7]=False
opt.ov4.texture[8]=False
opt.ov4.texture[9]=True
opt.ov4.retina=1.8
opt.ov4.scale=0.6
opt.od.transparency=0.1
opt.od.lineThickness=0.005
opt.od.usePolygon=True
opt.od.border=1
opt.od.useEdgeColor=True
opt.od.hidesel=False
opt.od.invertNormals=False
opt.od.separate=True
opt.od.map=False
opt.od.cameraDistance=0
opt.od.trainSpeed=0
opt.od.glide=True
opt.oo.inputTypeLeftAndRight=1
opt.oo.inputTypeForward=1
opt.oo.inputTypeYawAndPitch=1
opt.oo.inputTypeRoll=1
opt.oo.invertLeftAndRight=False
opt.oo.invertForward=False
opt.oo.invertYawAndPitch=False
opt.oo.invertRoll=False
opt.oo.sliceMode=False
opt.oo.limit3D=False
opt.oo.keepUpAndDown=False
opt.oo.baseTransparency=0.2
opt.oo.sliceTransparency=1
opt.oo.sliceDir=0
opt.ot4.frameRate=70
opt.ot4.timeMove=1
opt.ot4.timeRotate=1
opt.ot4.timeAlignMove=2
opt.ot4.timeAlignRotate=2
opt.ot4.paintWithAddButton=False
opt.oh.allowDiagonalMovement=True
opt.oh.alternativeControlIn3D=False
opt.oh.leftTouchToggleMode=False
opt.oh.rightTouchToggleMode=False
opt.oh.showController=True
opt.oh.showHint=False
opt.oh.horizontalInputFollowing=False
opt.oh.stereo=False
opt.oh.cross=True
opt.oh.iPD=0.064
opt.oh.fovscale=1
opt.oh.cameraDistanceScale=1
dim=4
";
}