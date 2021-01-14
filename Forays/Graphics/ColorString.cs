namespace Forays
{
    public struct ColorString
    {
        // Member Variables

        public Color Foreground;
        public Color Background;
        public string Text;

        // Constructs

        public ColorString(string text, Color foreground, Color background = Color.Black)
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