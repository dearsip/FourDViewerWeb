/*
 * Context.java
 */

using System.Collections;
using System.Collections.Generic;
//import java.util.HashMap;
//import java.util.HashSet;
//import java.util.LinkedList;
//import java.util.Stack;

/**
 * The data context for geometry-definition commands.
 */

public class Context
{

    public Stack stack;
    public Dictionary<string, object> dict; // String -> Object
    public List<string> libDirs; // File
    public Stack<string> dirStack; // File
    public HashSet<string> included; // File in canonical form
    public List<string> topLevelInclude; // String
    public HashSet<string> topLevelDef; // String

    public Context()
    {
        stack = new Stack();
        dict = new Dictionary<string, object>();
        libDirs = new List<string>();
        dirStack = new Stack<string>();
        included = new HashSet<string>();
        topLevelInclude = new List<string>();
        topLevelDef = new HashSet<string>();
    }

    public bool isTopLevel()
    {
        return (dirStack.Count == 1);
    }

}

