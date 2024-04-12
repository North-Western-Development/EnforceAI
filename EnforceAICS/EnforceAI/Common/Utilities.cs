using System.Text;
using CitizenFX.Core;

namespace EnforceAI.Common
{
    public static class Utilities
    {
        public static void Print(object item)
        {
            Debug.WriteLine(item.ToString());
        }

        public static void Print(params object[] items)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object item in items)
            {
                stringBuilder.Append(item);
            }
            Debug.WriteLine(stringBuilder.ToString());
        }
    }
}