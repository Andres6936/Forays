namespace Forays
{
    public class VertexAttributes
    {
        public float[][] Defaults;
        public int[] Size;
        public int TotalSize;

        public VertexAttributes(params int[] counts)
        {
            //makes zeroed arrays in the given counts.
            int count = counts.GetLength(0);
            Defaults = new float[count][];
            Size = new int[count];
            TotalSize = 0;
            int idx = 0;
            foreach (int i in counts)
            {
                // This method needs a note:  which attributes are assumed to be
                // here already? if you Create(2), is that texcoords? and what?.
                Defaults[idx] = new float[i];
                Size[idx] = i;
                TotalSize += i;
                ++idx;
            }
        }
    }
}