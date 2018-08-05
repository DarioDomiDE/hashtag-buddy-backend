﻿namespace Instaq.BlacklistImport.Tests
{
    using Xunit;

    public class TextHandler_WhenInsertText
    {
        private readonly TextHandler textHandler;

        public TextHandler_WhenInsertText()
        {
            this.textHandler = new TextHandler();
        }

        [Theory]
        [InlineData("Istanbul", "istanbul")]
        [InlineData("LONDON", "london")]
        [InlineData("kinshasA", "kinshasa")]
        [InlineData("johannesBurg", "johannesburg")]
        public void ThenAnyChar_ShouldBeLowercase(string input, string expected)
        {
            var result = this.textHandler.ToLower(input);
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("Istanbul (Asiatischer Teil)", "Istanbul  ")]
        [InlineData("Zhōngguó(Zhōnghuá Rénmín Gònghéguó) 中国(中华人民共和国)", "Zhōngguó  中国 ")]
        [InlineData("Anchorage[1337]", "Anchorage ")]
        [InlineData("[19]Anchorage", " Anchorage")]
        [InlineData("Anchorage[q] Test", "Anchorage  Test")]
        [InlineData("test (a) test2 [b] test3", "test   test2   test3")]
        [InlineData("test (b", "test (b")]
        [InlineData("test]b", "test]b")]
        public void ThenTextBetweenBracketsAndBrackets_ShouldBeRemoved(string input, string expected)
        {
            var result = this.textHandler.RemoveTextBetweenBracketsAndBrackets(input);
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("Washington D.C.", "Washington D C ")]
        [InlineData("Test 123 456 7890", "Test             ")]
        [InlineData("Bla-Blubb", "Bla Blubb")]
        [InlineData("Bla, Blubb", "Bla  Blubb")]
        [InlineData("x'y", "x y")]
        [InlineData("x\"y", "x y")]
        [InlineData("x/y", "x y")]
        public void ThenSpecialChars_ShouldBeReplacedWithSpace(string input, string expected)
        {
            var result = this.textHandler.ReplaceSpecialCharsWithSpace(input);
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("a b", new string[] { "a", "b"})]
        [InlineData(" a  b    c   ", new string[] { "a", "b", "c"})]
        public void ThenTextWithSpaces_ShouldSplittedAtSpaces(string input, string[] expected)
        {
            var result = this.textHandler.SplitAtSpaces(input);
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("ab")]
        public void ThenTooShortWords_ShouldBeIdentify(string input)
        {
            var result = this.textHandler.IsTooShort(input);
            Assert.True(result);
        }

        [Theory]
        [InlineData("abc")]
        public void ThenTooShortWords_ShouldNotBeIdentify(string input)
        {
            var result = this.textHandler.IsTooShort(input);
            Assert.False(result);
        }

        [Theory]
        [InlineData(new string[] { "a", "ab", "abc", "→", "test", "x"}, new string[] { "abc", "test" })]
        public void ThenTooShortWords_ShouldBeRemoved(string[] input, string[] expected)
        {
            var result = this.textHandler.RemoveTooShortWords(input);
            Assert.Equal(result, expected);
        }
    }
}