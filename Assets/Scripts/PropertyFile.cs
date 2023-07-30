using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using SimpleFileBrowser;
using UnityEngine.Networking;
using Ruccho.BlobIO;

public class PropertyFile
{
    public enum SaveType { EXPORT_MAZE, EXPORT_PROPERTIES, SAVE_PROPERTIES }
    public delegate void Loader(IStore store);
    public delegate void Saver(IStore store);
    public static bool isMaze;
    private static IEnumerator testProperties(string file, bool path) //throws IOException 
    {
        string text;
        if (!path) text = file;
        else {
#if UNITY_EDITOR
            yield return text = File.ReadAllText(Path.IsPathRooted(file) ? file : FileItem.Combine(Application.streamingAssetsPath, file));
#else
            using (UnityWebRequest www = UnityWebRequest.Get(file.Substring(0,4) == "http" ? file : FileItem.Combine(Application.streamingAssetsPath, file)))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success) text = www.downloadHandler.text;
                else { Debug.Log(www.error); yield break; }
            }
#endif
        }
        isMaze = text[0] == '#';
    }

    // https://stackoverflow.com/questions/485659/can-net-load-and-parse-a-properties-file-equivalent-to-java-properties-class
    // modified for maze properties
    private static IEnumerator loadProperties(string file, Dictionary<string, string> dict){
        string text;
#if UNITY_EDITOR
        yield return text = File.ReadAllText(Path.IsPathRooted(file) ? file : FileItem.Combine(Application.streamingAssetsPath, file));
#else
        using (UnityWebRequest www = UnityWebRequest.Get(file.Substring(0,4) == "http" ? file : FileItem.Combine(Application.streamingAssetsPath, file)))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) text = www.downloadHandler.text;
            else { Debug.Log(www.error); yield break; }
        }
#endif
        foreach (string line in text.Replace("\r\n","\n").Split(new[]{'\n','\r'}))
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

    private static void loadImmidiate(string text, Dictionary<string, string> dict) {
        foreach (string line in text.Replace("\r\n","\n").Split(new[]{'\n','\r'}))
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

    private static void storeProperties(Dictionary<string, string> p, SaveType saveType) {
        string file = string.Empty;
        try {
            file += "#"+DateTime.Now.ToString("ddd MMM dd HH:mm:ss K yyyy")+"\r\n";
            foreach(KeyValuePair<string, string> c in p) file += c.Key+"="+c.Value+"\r\n";
        }
        catch (Exception t)
        {
          Debug.LogException(t);
        }
        switch (saveType)
        {
            case SaveType.EXPORT_MAZE:
                BlobIO.MakeDownloadText(file, "maze_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                break;
            case SaveType.EXPORT_PROPERTIES:
                BlobIO.MakeDownloadText(file, "properties_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                break;
            case SaveType.SAVE_PROPERTIES:
                UnityEngine.PlayerPrefs.SetString(Core.fileCurrent, file);
                break;
        }
    }

    public static IEnumerator test(string file, bool path) {
        yield return testProperties(file, path);
    }

    public static IEnumerator load(string file, Loader storable) {
        Dictionary<string, string> p = new Dictionary<string, string>();
        yield return loadProperties(file, p);
        try {
            PropertyStore store = new PropertyStore(p);
            storable(store);
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public static void loadImmidiate(string text, Loader storable) {
        Dictionary<string, string> p = new Dictionary<string, string>();
        loadImmidiate(text, p);
        try {
            PropertyStore store = new PropertyStore(p);
            storable(store);
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public static void save(Saver storable, SaveType saveType) {
        Dictionary<string, string> p = new Dictionary<string, string>();

        try {
            PropertyStore store = new PropertyStore(p);
            storable(store);
        } catch (Exception e) {
            Debug.LogException(e);
        }

        try {
            storeProperties(p, saveType);
        } catch (IOException e) {
            Debug.LogException(e);
        }
    }

    public static readonly string default_ = @"
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
opt.od.size=1
opt.od.useEdgeColor=True
opt.od.hidesel=False
opt.od.invertNormals=False
opt.od.toggleSkyBox=False
opt.od.separate=True
opt.od.map=False
opt.od.glass=True
opt.od.focus=False
opt.od.mapDistance=3
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
opt.oo.showInput=True
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
opt.of.fisheye=False
opt.of.adjust=True
opt.of.rainbow=False
opt.of.width=0.75
opt.of.flare=0.33
opt.of.rainbowGap=0.5
opt.of.threeDMazeIn3DScene=False
opt.oh.allowDiagonalMovement=True
opt.oh.alternativeControlIn3D=False
opt.oh.leftTouchToggleMode=False
opt.oh.rightTouchToggleMode=False
opt.oh.showController=True
opt.oh.showHint=False
opt.oh.horizontalInputFollowing=False
opt.oh.stereo=False
opt.oh.cross=True
opt.oh.invertX=False
opt.oh.invertY=False
opt.oh.iPD=0.064
opt.oh.fovscale=1
opt.oh.cameraDistanceScale=1
tab=7
";
}