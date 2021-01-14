namespace Forays
{
    public struct ColorChar
    {
        public Color color;
        public Color bgcolor;
        public char c;

        public ColorChar(char c_, Color color_, Color bgcolor_)
        {
            color = color_;
            bgcolor = bgcolor_;
            c = c_;
        }

        public ColorChar(char c_, Color color_)
        {
            color = color_;
            bgcolor = Color.Black;
            c = c_;
        }

        public ColorChar(Color color_, Color bgcolor_, char c_)
        {
            color = color_;
            bgcolor = bgcolor_;
            c = c_;
        }

        public ColorChar(Color color_, char c_)
        {
            color = color_;
            bgcolor = Color.Black;
            c = c_;
        }

        public static implicit operator ColorChar(char c)
        {
            return new ColorChar(c, Color.Gray, Color.Black);
        }
    }
}