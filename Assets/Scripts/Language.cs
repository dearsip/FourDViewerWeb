/*
 * Language.java
 */
using System;
using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using SimpleFileBrowser;

/**
 * The heart of the scene language interpreter.
 */

public class Language
{

    public static IEnumerator include(Context c, string filename, bool path)
    {
        if (c.included.Add(path ? filename = resolve(c, filename) : "local"))
        {
            yield return include_(c, filename, path);
            if (c.isTopLevel()) c.topLevelInclude.Add(filename);
        }
    }

    public static IEnumerator include_(Context c, string file, bool path)
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
        StreamTokenizer st = createTokenizer(text);

        if (path) c.dirStack.Push(FileItem.GetParent(file));
        yield return doFile(c, st);
        if (path) c.dirStack.Pop();

    }

    public static string resolve(Context c, string filename) //throws Exception
    {
        if (FileItem.Exists(filename)) return filename;

        string file = filename;
#if UNITY_EDITOR
        if (Path.IsPathRooted(file)) return file;
#else
        if (file.Substring(0,4) == "http") return file;
#endif

        foreach (string dir in c.libDirs)
        {
            file = FileItem.Combine(dir, filename);
            if (FileItem.Exists(file)) return file;
        }

        file = FileItem.Combine(c.dirStack.Peek(), filename);
        if (FileItem.Exists(file)) return file;
        
        throw new Exception("Unable to resolve filename '" + filename + "'.");
    }

    public static StreamTokenizer createTokenizer(string text)
    {
        StreamTokenizer st = new StreamTokenizer(text);

        // customize tokenizer
        st.WordChars('#', '#');
        st.WordChars('%', '%');
        st.WordChars('+', '+');
        st.WordChars('-', '-');
        st.WordChars('_', '_');
        st.SlashSlashComments = true;
        st.SlashStarComments = true;

        return st;
    }

    public static string includeFile;
    public static IEnumerator doFile(Context c, StreamTokenizer st) //throws Exception
    {
        while (true)
        {
            int t = st.NextToken();
            if (t == StreamTokenizer.TT_EOF) break;
            switch (t)
            {
                case StreamTokenizer.TT_NUMBER:
                    c.stack.Push(st.NumberValue);
                    break;
                case '\'':
                case '"':
                    c.stack.Push(st.StringValue);
                    break;
                case StreamTokenizer.TT_WORD:
                    doWord(c, st.StringValue);
                    break;
                default: // ordinary chars, treat as words of length 1
                    doWord(c, ((char)t).ToString());
                    break;
                case StreamTokenizer.TT_EOL:
                    throw new Exception("Unexpected token type.");
            }
            if (includeFile != null)
            {
                string file = includeFile;
                includeFile = null;
                yield return include(c, file, true);
            }
        }
    }

    public static void doWord(Context c, string s) //throws Exception
    {
        if (s[0] == '#')
        { // color literal
            Color color;
            ColorUtility.TryParseHtmlString(s, out color);
            c.stack.Push(color);
            return;
        }
        if (s[0] == '%')
        { // binary number
            c.stack.Push((double)(Convert.ToInt32(s.Substring(1), 2)));
            return;
        }
        object o = c.dict[s];
        if (o == null) throw new Exception("Undefined token '" + s + "'.");
        if (o is ICommand)
        {
            ((ICommand)o).exec(c);
        }
        else
        {
            c.stack.Push(tryCopy(o));
            // the normal plan is, you include some files that define shapes,
            // then you use modified forms of those shapes to set up a scene.
            // so, to avoid messing up the original, copy shapes when they come
            // out of the dictionary.
        }
    }

    public static object tryCopy(object o)
    {
        if (o is Geom.ShapeInterface)
        {
            return ((Geom.ShapeInterface)o).copySI();
        }
        else if (o is Geom.Texture)
        {
            return ((Geom.Texture)o).copy();
        }
        else
        {
            return o;
        }
    }

}

