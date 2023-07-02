using System;

/*
 * ValidationException.java
 */

/**
 * An exception subclass used for validation in the user interface and in I/O routines.
 */

public class ValidationException : Exception
{

   public ValidationException(string message) : base(message)
{
}

}

