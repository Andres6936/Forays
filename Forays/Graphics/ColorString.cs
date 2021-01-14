namespace Forays
{
    public struct ColorString
    {
        // Member Variables

        public Color Foreground;
        public Color Background;
        public string Text;

        public static implicit operator ColorString(string s)
        {
            return new ColorString(s, Color.Gray);
        }

        public static implicit operator ColorBufferString(ColorString c)
        {
            return new ColorBufferString(c);
        }

        // Constructs

        public ColorString(string text, Color foreground) : this(text, foreground, Color.Black)
        {
            // Delegate the construction of object to another construct.
        }

        public ColorString(string text, Color foreground, Color background)
        {
            Foreground = foreground;
            Background = background;
            Text = text;
        }

        // Overload Operators

        public static ColorBufferString operator +(ColorString one, ColorString two)
        {
            return new ColorBufferString(one, two);
        }
    }
}