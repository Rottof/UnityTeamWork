using System.Collections.Generic;

namespace Unity.FPS.Hex
{
    public class HexBag
    {
        public static Dictionary<string, int> Bag = new Dictionary<string, int>();
        
        public static void AddHex(string effect, int count = 1)
        {
            if (Bag.ContainsKey(effect))
            {
                Bag[effect] += count;
            }
            else
            {
                Bag[effect] = count;
            }
        }

        public static void RemoveHex(string effect)
        {
            if (Bag.ContainsKey(effect) && Bag[effect] > 0)
            {
                Bag[effect]--;
            }
        }

        public static int GetHexCount(string effect)
        {
            if (Bag.ContainsKey(effect))
            {
                return Bag[effect];
            }
            return 0;
        }
    }
}