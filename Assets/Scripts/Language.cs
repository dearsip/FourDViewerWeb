/*
 * Language.java
 */
using System;
using UnityEngine;
using System.IO;
//import java.awt.Color;
//import java.io.File;
//import java.io.FileReader;
//import java.io.StreamTokenizer;
//import java.util.Iterator;

/**
 * The heart of the scene language interpreter.
 */

public class Language
{

    public static void include(Context c, string filename) //throws Exception
    {
        if (include_(c, resolve(c, filename)))
        {
            if (c.isTopLevel()) c.topLevelInclude.Add(filename);
        }
    }

    public static bool include_(Context c, string file) //throws Exception
    {

        file = Path.GetFullPath(file);
        if (!c.included.Add(file)) return false;

        StreamReader fr = new StreamReader(file);
        try
        {
            StreamTokenizer st = createTokenizer(fr);

            c.dirStack.Push(Path.GetDirectoryName(file));
            try
            {
                doFile(c, st);
            }
            catch (Exception t)
            {
                throw (t is LanguageException) ? t : new LanguageException(t, file, st.ToString());
            }
            finally
            {
                c.dirStack.Pop();
            }

        }
        finally
        {
            fr.Close();
        }

        return true;
    }

    public static string resolve(Context c, string filename) //throws Exception
    {

        string file = filename;
        if (Path.IsPathRooted(file)) return file;

        file = Path.Combine(c.dirStack.Peek(), filename);
        if (File.Exists(file)) return file;
        
        foreach (string dir in c.libDirs)
        {
            file = Path.Combine(dir, filename);
            if (File.Exists(file)) return file;
        }

        throw new Exception("Unable to resolve filename '" + filename + "'.");
    }

    public static StreamTokenizer createTokenizer(StreamReader fr)
    {
        StreamTokenizer st = new StreamTokenizer(fr);

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

    public static void doFile(Context c, StreamTokenizer st) //throws Exception
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

