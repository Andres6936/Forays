namespace Forays
{
    public class VertexAttributes
    {
        public float[][] Defaults;
        public int[] Size;
        public int TotalSize;

        public static VertexAttributes Create(params float[][] defaults)
        {
            VertexAttributes v = new VertexAttributes();
            int count = defaults.GetLength(0);
            v.Defaults = new float[count][];
            v.Size = new int[count];
            v.TotalSize = 0;
            int idx = 0;
            foreach (float[] f in defaults)
            {
                v.Defaults[idx] = f;
                v.Size[idx] = f.GetLength(0);
                v.TotalSize += v.Size[idx];
                ++idx;
            }

            return v;
        }

        public static VertexAttributes Create(params int[] counts)
        {
            //makes zeroed arrays in the given counts.
            VertexAttributes v = new VertexAttributes();
            int count = counts.GetLength(0);
            v.Defaults = new float[count][];
            v.Size = new int[count];
            v.TotalSize = 0;
            int idx = 0;
            foreach (int i in counts)
            {
                v.Defaults[idx] =
                    new float[i]; //todo: this method needs a note:  which attribs are assumed to be here already? if you Create(2), is that texcoords? and what?
                v.Size[idx] = i;
                v.TotalSize += i;
                ++idx;
            }

            return v;
        }
    }
}