using System.Collections.Generic;

namespace AncientTools.Utility
{
    static class ParticleColor
    {
        private static Dictionary<string, int> colorDict = new Dictionary<string, int>();

        public static void InitColours()
        {
            colorDict.Add("bone", ColorFromRgba(255, 255, 255, 100));

            colorDict.Add("amaranth", ColorFromRgba(189, 117, 65, 100));
            colorDict.Add("flax", ColorFromRgba(137, 46, 2, 100));
            colorDict.Add("rice", ColorFromRgba(232, 224, 185, 100));
            colorDict.Add("rye", ColorFromRgba(177, 138, 81, 100));
            colorDict.Add("spelt", ColorFromRgba(189, 145, 106, 100));
            colorDict.Add("sunflower", ColorFromRgba(39, 33, 26, 100));

            colorDict.Add("chalk", ColorFromRgba(190, 188, 179, 100));
            colorDict.Add("limestone", ColorFromRgba(184, 170, 136, 100));
            colorDict.Add("halite", ColorFromRgba(219, 191, 179, 100));

            colorDict.Add("borax", ColorFromRgba(231, 230, 226, 100));
            colorDict.Add("sulfur", ColorFromRgba(255, 234, 171, 100));
            colorDict.Add("sylvite", ColorFromRgba(175, 69, 33, 100));

            colorDict.Add("rawcassava", ColorFromRgba(225, 216, 208, 100));
        }
        public static void Destroy()
        {
            colorDict.Clear();
        }
        
        public static int GetColour(string colour)
        {
            int colourInt;

            if (colorDict.TryGetValue(colour, out colourInt))
                return colourInt;
            else
                return 0;
        }
        public static int GetColour(string colour1, string colour2)
        {
            int colourInt;

            if (colorDict.TryGetValue(colour1, out colourInt))
                return colourInt;
            else if (colorDict.TryGetValue(colour2, out colourInt))
                return colourInt;
            else
                return 0;
        }
        public static int ColorFromRgba(int r, int g, int b, int a)
        {
            return (a << 24) | (r << 16) | (g << 8) | (b);
        }
    }
}
