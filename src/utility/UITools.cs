using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AncientTools.Utility
{
    public static class UITools
    {
        public static string SpacedUIString(string[] stringArray, int characters)
        {
            int arrayCharactersCount = 0;

            foreach (string word in stringArray)
                arrayCharactersCount += word.Length;

            if (arrayCharactersCount >= characters)
                return stringArray.ToString();

            int numSpaces = characters - arrayCharactersCount;

            if(stringArray.Length > 2)
            {
                numSpaces /= stringArray.Length;
            }

            string spaces = "";
            for (int i = 0; i < numSpaces; i++)
                spaces += " ";

            string final = "";
            for(int i = 0; i < stringArray.Length; i++)
            {
                final += stringArray[i];
                if (i != stringArray.Length - 1)
                    final += spaces;
            }

            return final;
        }
        public static string SpacedUIStringNL(string[] stringArray, int characters)
        {
            return SpacedUIString(stringArray, characters) + "\n";
        }
    }
}
