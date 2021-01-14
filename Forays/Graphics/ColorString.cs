namespace Forays
{
    public struct ColorString
    {
        //todo: change this to a class eventually
        public Color color;
        public Color bgcolor;
        public string s;

        public static implicit operator ColorString(string s)
        {
            return new ColorString(s, Color.Gray);
        }

        public static implicit operator ColorBufferString(ColorString c)
        {
            return new ColorBufferString(c);
        }

        public ColorString(string s_, Color color_)
        {
            color = color_;
            bgcolor = Color.Black;
            s = s_;
        }

        public ColorString(string s_, Color color_, Color bgcolor_)
        {
            color = color_;
            bgcolor = bgcolor_;
            s = s_;
        }

        public ColorString(Color color_, string s_)
        {
            color = color_;
            bgcolor = Color.Black;
            s = s_;
        }

        public ColorString(Color color_, Color bgcolor_, string s_)
        {
            color = color_;
            bgcolor = bgcolor_;
            s = s_;
        }

        public static ColorBufferString operator +(ColorString one, ColorString two)
        {
            return new ColorBufferString(one, two);
        }
    }
}