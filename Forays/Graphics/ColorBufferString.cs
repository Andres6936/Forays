using System;
using System.Collections.Generic;

namespace Forays
{
    public class ColorBufferString
    {
        public List<ColorString> strings = new List<ColorString>();

        public int Length()
        {
            int total = 0;
            foreach (ColorString s in strings)
            {
                total += s.Text.Length;
            }

            return total;
        }

        public ColorChar this[int index]
        {
            get
            {
                int cstr_idx = 0;
                while (index >= strings[cstr_idx].Text.Length)
                {
                    index -= strings[cstr_idx].Text.Length;
                    ++cstr_idx;
                }

                return new ColorChar(strings[cstr_idx].Text[index], strings[cstr_idx].Foreground,
                    strings[cstr_idx].Background);
            }
        }

        public static implicit operator ColorBufferString(string s)
        {
            return new ColorBufferString(s);
        }

        public ColorBufferString(ColorBufferString other)
        {
            foreach (ColorString cs in other.strings)
            {
                strings.Add(cs);
            }
        }

        public ColorBufferString(ColorString cs)
        {
            strings.Add(cs);
        }

        public ColorBufferString(string s)
        {
            strings.Add(new ColorString(s, Color.Gray));
        }

        public ColorBufferString(string s1, Color c1)
        {
            strings.Add(new ColorString(s1, c1));
        }

        public ColorBufferString(string s1, Color c1, Color bg1)
        {
            strings.Add(new ColorString(s1, c1, bg1));
        }

        public ColorBufferString(string s1, Color c1, string s2, Color c2)
        {
            strings.Add(new ColorString(s1, c1));
            strings.Add(new ColorString(s2, c2));
        }

        public ColorBufferString(string s1, Color c1, string s2, Color c2, string s3, Color c3)
        {
            strings.Add(new ColorString(s1, c1));
            strings.Add(new ColorString(s2, c2));
            strings.Add(new ColorString(s3, c3));
        }

        public ColorBufferString(string s1, Color c1, string s2, Color c2, string s3, Color c3, string s4, Color c4)
        {
            strings.Add(new ColorString(s1, c1));
            strings.Add(new ColorString(s2, c2));
            strings.Add(new ColorString(s3, c3));
            strings.Add(new ColorString(s4, c4));
        }

        public ColorBufferString(string s1, Color c1, string s2, Color c2, string s3, Color c3, string s4, Color c4,
            string s5, Color c5)
        {
            strings.Add(new ColorString(s1, c1));
            strings.Add(new ColorString(s2, c2));
            strings.Add(new ColorString(s3, c3));
            strings.Add(new ColorString(s4, c4));
            strings.Add(new ColorString(s5, c5));
        }

        public ColorBufferString(string s1, Color c1, string s2, Color c2, string s3, Color c3, string s4, Color c4,
            string s5, Color c5, string s6, Color c6)
        {
            strings.Add(new ColorString(s1, c1));
            strings.Add(new ColorString(s2, c2));
            strings.Add(new ColorString(s3, c3));
            strings.Add(new ColorString(s4, c4));
            strings.Add(new ColorString(s5, c5));
            strings.Add(new ColorString(s6, c6));
        }

        public ColorBufferString(params object[] objs)
        {
            if (objs != null)
            {
                foreach (object o in objs)
                {
                    if (o is string)
                    {
                        strings.Add(new ColorString(o as string, Color.Gray));
                    }
                    else
                    {
                        if (o is ColorString)
                        {
                            strings.Add((ColorString) o);
                        }
                        else
                        {
                            if (o is ColorBufferString)
                            {
                                foreach (ColorString cs in (o as ColorBufferString).strings)
                                {
                                    strings.Add(cs);
                                }
                            }
                            else
                            {
                                throw new ArgumentException("Arguments must be string, cstr, or colorstring.");
                            }
                        }
                    }
                }
            }
        }

        public void Add(ColorString cs)
        {
            strings.Add(cs);
        }

        public void Add(string s)
        {
            strings.Add(new ColorString(s, Color.Gray));
        }

        public void Add(string s1, Color c1)
        {
            strings.Add(new ColorString(s1, c1));
        }

        public void Add(string s1, Color c1, Color bg1)
        {
            strings.Add(new ColorString(s1, c1, bg1));
        }

        public ColorBufferString PadLeft(int totalWidth)
        {
            return PadLeft(totalWidth, new ColorChar(' ', Color.Gray, Color.Black));
        }

        public ColorBufferString PadLeft(int totalWidth, ColorChar paddingChar)
        {
            int diff = totalWidth - Length();
            if (diff <= 0) return new ColorBufferString(this);
            return new ColorString("".PadRight(diff, paddingChar.c), paddingChar.color, paddingChar.bgcolor) + this;
        }

        public ColorBufferString PadRight(int totalWidth)
        {
            return PadRight(totalWidth, new ColorChar(' ', Color.Gray, Color.Black));
        }

        public ColorBufferString PadRight(int totalWidth, ColorChar paddingChar)
        {
            int diff = totalWidth - Length();
            if (diff <= 0) return new ColorBufferString(this);
            return this + new ColorString("".PadRight(diff, paddingChar.c), paddingChar.color, paddingChar.bgcolor);
        }

        public ColorBufferString PadOuter(int totalWidth)
        {
            return PadOuter(totalWidth, new ColorChar(' ', Color.Gray, Color.Black));
        }

        public ColorBufferString PadOuter(int totalWidth, ColorChar paddingChar)
        {
            int diff = totalWidth - Length();
            if (diff <= 0) return new ColorBufferString(this);
            return new ColorString("".PadRight(diff / 2, paddingChar.c), paddingChar.color, paddingChar.bgcolor) +
                   this +
                   new ColorString("".PadRight((diff + 1) / 2, paddingChar.c), paddingChar.color, paddingChar.bgcolor);
        }

        public static ColorBufferString operator +(ColorBufferString one, ColorBufferString two)
        {
            ColorBufferString result = new ColorBufferString();
            foreach (ColorString s in one.strings)
            {
                result.strings.Add(s);
            }

            foreach (ColorString s in two.strings)
            {
                result.strings.Add(s);
            }

            return result;
        }

        public static ColorBufferString operator +(ColorString one, ColorBufferString two)
        {
            //todo: whoops, forgot colorchar in this section.
            ColorBufferString result = new ColorBufferString();
            result.strings.Add(one);
            foreach (ColorString s in two.strings)
            {
                result.strings.Add(s);
            }

            return result;
        }

        public static ColorBufferString operator +(ColorBufferString one, ColorString two)
        {
            ColorBufferString result = new ColorBufferString();
            foreach (ColorString s in one.strings)
            {
                result.strings.Add(s);
            }

            result.strings.Add(two);
            return result;
        }

        public static ColorBufferString operator +(string one, ColorBufferString two)
        {
            return new ColorString(one, Color.Gray) + two;
        }

        public static ColorBufferString operator +(ColorBufferString one, string two)
        {
            ColorBufferString result = new ColorBufferString();
            foreach (ColorString s in one.strings)
            {
                result.strings.Add(s);
            }

            result.strings.Add(new ColorString(two, Color.Gray));
            return result;
        }

        public ColorBufferString[] SplitAt(int idx, bool remove_at_split_idx = false)
        {
            if (idx < 0)
            {
                throw new ArgumentOutOfRangeException("idx argument " + idx.ToString() + " can't be negative.");
            }

            if (idx >= Length())
            {
                throw new ArgumentOutOfRangeException(
                    "idx argument " + idx.ToString() + " can't be outside the string.");
            }

            ColorBufferString[] result = new ColorBufferString[2];
            result[0] = new ColorBufferString();
            result[1] = new ColorBufferString();
            foreach (ColorString s in strings)
            {
                int len_0 = result[0].Length();
                if (len_0 < idx)
                {
                    if (len_0 + s.Text.Length > idx)
                    {
                        result[0].strings
                            .Add(new ColorString(s.Text.Substring(0, idx - len_0), s.Foreground, s.Background));
                        int second_start = idx - len_0;
                        if (remove_at_split_idx)
                        {
                            ++second_start;
                        }

                        if (second_start < s.Text.Length)
                        {
                            result[1].strings.Add(new ColorString(s.Text.Substring(idx - len_0), s.Foreground,
                                s.Background));
                        }
                    }
                    else
                    {
                        result[0].strings.Add(s);
                    }
                }
                else
                {
                    result[1].strings.Add(s);
                }
            }

            return result;
        }
    }
}