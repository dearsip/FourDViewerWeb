using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

// additional code for this project
namespace SimpleFileBrowser
{
	public class FileItem
	{
    public string name;
    public bool isDirectory;
		public int Position { get; set; }
    public FileItem parent;
    public List<FileItem> children;
    public string path { get { return parent == null ? name : Combine(parent.path, name); } }
    public static FileItem root;

    private FileItem(string name, FileItem parent)
    {
      this.name = name;
      this.parent = parent;
    }

    public void Directorize()
    {
      isDirectory = true;
      children = new List<FileItem>();
    }

    public static IEnumerator Build()
    {
      string directory;
#if UNITY_EDITOR
        yield return directory = File.ReadAllText(Combine(Application.streamingAssetsPath, directory_));
#else
        using (UnityWebRequest www = UnityWebRequest.Get(Combine(Application.streamingAssetsPath, directory_)))
        {
          yield return www.SendWebRequest();
          if (www.result == UnityWebRequest.Result.Success) directory = www.downloadHandler.text;
          else { Debug.Log(www.error); yield break; }
        }
#endif
        root = CreateRoot();
        FileItem current = root;
        foreach (string s in directory.Split(new string[] { "\r\n" }, StringSplitOptions.None))
        {
          if (s == "/") { current = current.children.Last(); current.Directorize(); continue; }
          if (s == "..") { current = current.parent; continue; }
          current.children.Add(new FileItem(s, current));
        }
    }

    private static FileItem CreateRoot()
    {
      root = new FileItem("", null);
      root.Directorize();
      return root; 
    }
    
    public static bool Exists(string filename)
    {
      FileItem current = root;
      foreach (string name in filename.Split('/'))
      {
        current = current.children.Find(x => x.name == name);
        if (current == null) return false;
      }
      return !current.isDirectory;
    }

    public static bool ExistsDirectory(string filename)
    {
      if (filename == root.path) return true;
      FileItem current = root;
      foreach (string name in filename.Split('/'))
      {
        current = current.children.Find(x => x.name == name);
        if (current == null) return false;
      }
      return current.isDirectory;
    }

    public static FileItem Find(string filename)
    {
      if (filename == root.path) return root;
      FileItem current = root;
      foreach (string name in filename.Split('/'))
      {
        current = current.children.Find(x => x.name == name);
        if (current == null) return null;
      }
      return current;
    }

    public static string Combine(string path1, string path2)
    {
      return Path.Combine(path1, path2).Replace('\\', '/');
    }

    public static string GetParent(string filename)
    {
      int i = filename.LastIndexOf('/');
      return i == -1 ? "" : filename.Substring(0, i);
    }
    
    public static string GetFileName(string filename)
    {
      int i = filename.LastIndexOf('/');
      return i == -1 ? filename : filename.Substring(i + 1);
    }

    public static string Retrieve(string filename)
    {
      return filename.Substring(Application.streamingAssetsPath.Length + 1);
    }

    private static readonly string directory_ = "directory.txt";
	}
}