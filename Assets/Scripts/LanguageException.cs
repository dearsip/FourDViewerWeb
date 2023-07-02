/*
 * LanguageException.java
 */
using System;
//import java.io.File;

/**
 * A wrapper exception that carries extra information
 * from the scene language interpreter up to the user.
 */

public class LanguageException : Exception
{

    private string file;
    private string detail; // includes line number

    public LanguageException(Exception t, string file, string detail) : base(t.ToString())
    {
        this.file = file;
        this.detail = detail;
    }

    public string getFile() { return file; }
    public string getDetail() { return detail; }

}

