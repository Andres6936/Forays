namespace Forays
{
    public struct ColorString
    {
        // Member Variable

        /// <summary>
        /// Return the length of text.
        /// </summary>
        public readonly int Length => Text.Length;

        /// <summary>
        /// The text that content this object.
        /// </summary>
        public string Text;

        /// <summary>
        /// Foreground color of text.
        /// </summary>
        public Color Foreground;

        /// <summary>
        /// Background color of text.
        /// </summary>
        public Color Background;

        // Constructs

        public ColorString(string text, Color foreground, Color background = Color.Black)
        {
            Foreground = foreground;
            Background = background;
            Text = text;
        }

        // Indexers

        /// <summary>
        /// Return the character that is store in the index.
        /// </summary>
        /// <param name="index">Index of character.</param>
        public readonly char this[int index] => Text[index];

        // Overload Operators

        public static ColorBufferString operator +(ColorString one, ColorString two)
        {
            return new ColorBufferString(one, two);
        }

        // Implicit Operators

        public static implicit operator ColorString(string s)
        {
            return new ColorString(s, Color.Gray);
        }

        public static implicit operator ColorBufferString(ColorString c)
        {
            return new ColorBufferString(c);
        }
    }
}