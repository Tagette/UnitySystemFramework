using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnitySystemFramework.Commands
{
    public class CommandReader
    {
        public int ArgIndex;
        public readonly string[] Args;
        public readonly List<string> Errors = new List<string>();

        public string CurrentArg => Args[ArgIndex];
        public int ArgsLeft => Args.Length - ArgIndex;

        public CommandReader(string[] args)
        {
            Args = args;
            ArgIndex = 1;
        }

        public bool ReadWord(out string value)
        {
            value = null;
            if (ArgIndex == Args.Length)
                return false;

            value = Args[ArgIndex++];
            return true;
        }

        public bool ReadSentence(out string sentence, int words = -1)
        {
            if (words < 0)
                words = Args.Length - 1;

            var builder = new StringBuilder();
            while (ReadWord(out var value) && words > 0)
            {
                words--;
                if (builder.Length > 0)
                    builder.Append(" ");
                builder.Append(value);
            }

            if (builder.Length == 0)
            {
                sentence = null;
                return false;
            }

            sentence = builder.ToString();
            return true;
        }

        public bool ReadNumber(out double value)
        {
            try
            {
                string text = Args[ArgIndex];
                value = (double)Convert.ChangeType(text, typeof(double));
            }
            catch
            {
                Debug.LogError($"Cannot cast from string to a number. (param {ArgIndex})");
                value = default;
                return false;
            }

            ArgIndex++;

            return true;
        }
    }
}
