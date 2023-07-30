/*
 * IToken.java
 */

/**
 * An interface for writing tokens into a file.
 * Sort of like {@link IStore}.
 * The read side is handled by StreamTokenizer
 * in {@link Language}.
 */

public interface IToken {

   IToken putBoolean(bool b);
   IToken putInteger(int i);
   IToken putDouble(double d);

   IToken putWord(string s);
   IToken putSymbol(string s); // one-character word that doesn't require adjacent spaces
   IToken putString(string s);

   IToken space(); // for putting optional spaces between symbols
   IToken newLine();

}

