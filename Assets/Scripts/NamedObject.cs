/*
 * NamedObject.java
 */

using System;
using System.Collections.Generic;
//import java.util.Map;

/**
 * A helper structure for ISelectShape.
 */

public class NamedObject<T> : IComparable
{

    public string name;
    public T obj;

   public NamedObject(String name, T obj)
    {
        this.name = name;
        this.obj = obj;
    }

    public NamedObject(KeyValuePair<string,T> entry)
    {
        name = entry.Key;
        obj = entry.Value;
    }

    public String toString()
    {
        return name;
    }

    public int CompareTo(Object o)
    {
        NamedObject<T> that = (NamedObject<T>)o;
        return name.CompareTo(that.name);
    }

}

