/*
 * TokenFile.java
 */

/**
 * A utility class for writing tokens into a file.
 */

public class TokenFile : IToken {

// --- setup ---

   public string w { get; private set; }
   private string decimalFormat;
   private bool needSpace;

   public TokenFile() {
      w = string.Empty;

      decimalFormat = "0.000000000000";
      // the point is to remove annoying FP effects.
      // you could use up to 15 digits if you wanted,
      // but I think 12 is plenty.  it's a lot more
      // precise than any of the epsilons in the code.

      needSpace = false;
   }

// --- implementation of IToken ---

   private void spaceIfNeeded() {
      if (needSpace) w += ' ';
   }

   public IToken putBoolean(bool b) {
      spaceIfNeeded();
      w += b ? "true" : "false";
      needSpace = true;
      return this;
   }

   public IToken putInteger(int i) {
      spaceIfNeeded();
      w += i;
      needSpace = true;
      return this;
   }

   public IToken putDouble(double d) {
      spaceIfNeeded();
      w += d.ToString(decimalFormat);
      needSpace = true;
      return this;
   }

   public IToken putWord(string s) {
      spaceIfNeeded();
      w += s;
      needSpace = true;
      return this;
   }

   public IToken putSymbol(string s) {
      w += s;
      needSpace = false;
      return this;
   }

   public IToken putString(string s) {
      spaceIfNeeded();
      w += '\"';
      w += s.Replace("\"","\\\\\"");
      w +='\"';
      needSpace = true;
      return this;
   }

   public IToken space() {
      w += ' ';
      needSpace = false;
      return this;
   }

   public IToken newLine() {
      w += lineSeparator;
      needSpace = false;
      return this;
   }

	private static string lineSeparator = "\r\n";

}

