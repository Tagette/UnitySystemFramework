using System;
using System.Text;

namespace UnitySystemFramework.Utility
{
    public static class StringUtil
    {
        public static string PascalToTitleCase(this string pascalCase)
        {
            if (pascalCase == null)
                return null;

            var builder = new StringBuilder();
            for (int i = 0; i < pascalCase.Length; i++)
            {
                if (i > 0 && char.IsUpper(pascalCase[i]))
                    builder.Append(' ');
                builder.Append(pascalCase[i]);
            }

            return builder.ToString();
        }

        public static int IndexOfSkip(this string str, char value, int skip)
        {
            return IndexOfSkip(str, value, 0, str.Length, skip);
        }

        public static int IndexOfSkip(this string str, char value, int startIndex, int length, int skip)
        {
            if (str.Length - startIndex < length)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive and startIndex must refer to a location within the string.");

            int count = 0;
            int index = -1;
            int offset = 0;
            while (count <= skip && str.IndexOf(value, startIndex + offset, length - offset) > 0)
            {
                count++;
                index = str.IndexOf(value, startIndex + offset, length - offset);
                offset += (index + 1) - startIndex;
            }

            return index;
        }
    }
}
