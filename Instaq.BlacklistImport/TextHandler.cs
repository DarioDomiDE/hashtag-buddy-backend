﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Instaq.BlacklistImport
{
    using System.Text.RegularExpressions;

    public class TextHandler
    {
        public string ChangeToLowercase(string input)
        {
            return input.ToLower();
        }

        public string RemoveTextInsideBrackets(string input)
        {
            // *\([^)]*\
            var output = Regex.Replace(input, @" ?\(.*?\)", string.Empty);
            return output;
        }

        public string ReplaceBracketsNumbersAndDotsWithSpace(string input)
        {
            var pattern = new Regex("[0123456789]|[ ]{2}");
            return pattern.Replace(input, "\n");
        }

        public string[] SplitAtSpaces(string input)
        {
            return input.Split(' ');
        }

    }
}
