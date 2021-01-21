namespace Forays
{
    public class VertexAttributes
    {
        public readonly int[] Size;
        public readonly int TotalSize;

        public VertexAttributes(params int[] counts)
        {
            //makes zeroed arrays in the given counts.
            int count = counts.GetLength(0);
            float[][] defaults = new float[count][];
            Size = new int[count];
            TotalSize = 0;
            int idx = 0;
            foreach (int i in counts)
            {
                // This method needs a note:  which attributes are assumed to be
                // here already? if you Create(2), is that texcoords? and what?.
                defaults[idx] = new float[i];
                Size[idx] = i;
                TotalSize += i;
                ++idx;
            }
        }
    }
}