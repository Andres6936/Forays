using System.Collections.Generic;

namespace Forays
{
    public class SurfaceDefaults
    {
        public int fill_count = -1;
        public int single_layout = -1;
        public int single_sprite = -1;
        public int single_position = -1;
        public int single_sprite_type = -1;

        public List<int> positions;
        public List<int> layouts;
        public List<int> sprites;
        public List<int> sprite_types;
        public List<float>[] other_data;
        public List<float>[] single_other_data;

        public SurfaceDefaults()
        {
        }

        public SurfaceDefaults(SurfaceDefaults other)
        {
            if (other.positions != null)
            {
                positions = new List<int>(other.positions);
            }

            if (other.layouts != null)
            {
                layouts = new List<int>(other.layouts);
            }

            if (other.sprites != null)
            {
                sprites = new List<int>(other.sprites);
            }

            if (other.sprite_types != null)
            {
                sprite_types = new List<int>(other.sprite_types);
            }

            if (other.other_data != null)
            {
                other_data = new List<float>[other.other_data.GetLength(0)];
                int idx = 0;
                foreach (List<float> l in other.other_data)
                {
                    other_data[idx] = new List<float>(l);
                    ++idx;
                }
            }

            single_position = other.single_position;
            single_layout = other.single_layout;
            single_sprite = other.single_sprite;
            single_sprite_type = other.single_sprite_type;
            single_other_data = other.single_other_data;
            fill_count = other.fill_count;
        }

        public void FillValues(bool fill_positions, bool fill_other_data)
        {
            if (fill_count <= 0)
            {
                return;
            }

            if (fill_positions)
            {
                if (single_position != -1)
                {
                    //if this value is -1, nothing can be added anyway.
                    if (positions == null)
                    {
                        positions = new List<int>();
                    }

                    while (positions.Count < fill_count)
                    {
                        positions.Add(single_position);
                    }
                }

                if (single_layout != -1)
                {
                    if (layouts == null)
                    {
                        layouts = new List<int>();
                    }

                    while (layouts.Count < fill_count)
                    {
                        layouts.Add(single_layout);
                    }
                }
            }

            if (fill_other_data)
            {
                if (single_sprite != -1)
                {
                    if (sprites == null)
                    {
                        sprites = new List<int>();
                    }

                    while (sprites.Count < fill_count)
                    {
                        sprites.Add(single_sprite);
                    }
                }

                if (single_sprite_type != -1)
                {
                    if (sprite_types == null)
                    {
                        sprite_types = new List<int>();
                    }

                    while (sprite_types.Count < fill_count)
                    {
                        sprite_types.Add(single_sprite_type);
                    }
                }

                if (single_other_data != null)
                {
                    if (other_data == null)
                    {
                        other_data = new List<float>[single_other_data.GetLength(0)];
                        for (int i = 0; i < other_data.GetLength(0); ++i)
                        {
                            other_data[i] = new List<float>();
                        }
                    }

                    int idx = 0;
                    foreach (List<float> l in other_data)
                    {
                        while (l.Count < fill_count * single_other_data[idx].Count)
                        {
                            foreach (float f in single_other_data[idx])
                            {
                                l.Add(f);
                            }
                        }

                        ++idx;
                    }
                }
            }
        }
    }
}