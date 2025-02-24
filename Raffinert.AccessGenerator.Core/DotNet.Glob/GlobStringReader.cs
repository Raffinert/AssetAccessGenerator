﻿using System.IO;
using System.Linq;
using System.Text;

namespace DotNet.Globbing
{
    internal class GlobStringReader : StringReader
    {
        private readonly string _text;
        private int _currentIndex;
        public const int FailedRead = -1;
        public const char NullChar = (char)0;

        public const char ExclamationMarkChar = '!';
        public const char StarChar = '*';
        public const char OpenBracketChar = '[';
        public const char CloseBracketChar = ']';
        public const char DashChar = '-';
        public const char QuestionMarkChar = '?';

        /// <summary>
        /// Tokens can start with the following characters.
        /// </summary>
        public static char[] BeginningOfTokenCharacters = { StarChar, OpenBracketChar, QuestionMarkChar };

        public static char[] AllowedNonAlphaNumericChars = { '.', ' ', '!', '#', '-', ';', '=', '@', '~', '_', ':' };

        /// <summary>
        /// The current delimiters
        /// </summary>
        private static readonly char[] PathSeparators = { '/', '\\' };

        public GlobStringReader(string text) : base(text)
        {
            _text = text;
            _currentIndex = -1;
        }

        /// <summary>
        /// The index of the current character
        /// </summary>
        public int CurrentIndex
        {
            get { return _currentIndex; }
            private set
            {
                _currentIndex = value;
                LastChar = _text[_currentIndex - 1];
                CurrentChar = _text[_currentIndex];
            }
        }

        public bool ReadChar()
        {
            return Read() != FailedRead;
        }

        public override int Read()
        {
            var result = base.Read();
            if (result != FailedRead)
            {
                _currentIndex++;
                LastChar = CurrentChar;
                CurrentChar = (char)result;
                return result;
            }

            return result;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            var read = base.Read(buffer, index, count);
            CurrentIndex += read;
            CurrentChar = _text[CurrentIndex];
            return read;
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            var read = base.ReadBlock(buffer, index, count);
            CurrentIndex += read;
            return read;
        }

        public override string ReadLine()
        {
            var readLine = base.ReadLine();
            if (readLine != null)
                CurrentIndex += readLine.Length;
            return readLine;
        }

        public string ReadPathSegment()
        {
            var segmentBuilder = new StringBuilder();
            while (ReadChar())
            {
                if (!IsPathSeparator(CurrentChar))
                {
                    segmentBuilder.Append(CurrentChar);
                }
                else
                {
                    break;
                }
            }
            return segmentBuilder.ToString();
        }

        public override string ReadToEnd()
        {
            CurrentIndex = _text.Length - 1;
            return base.ReadToEnd();
        }

        /// <summary>
        /// The previous character
        /// </summary>
        public char LastChar { get; private set; }

        /// <summary>
        /// The current character
        /// </summary>
        public char CurrentChar { get; private set; }

        /// <summary>
        /// Has the Command Reader reached the end of the file
        /// </summary>
        public bool HasReachedEnd
        {
            get { return Peek() == -1; }
        }

        /// <summary>
        /// Is current character WhiteSpace
        /// </summary>
        public bool IsWhiteSpace
        {
            get { return char.IsWhiteSpace(CurrentChar); }
        }

        /// <summary>
        /// Peek at the next character
        /// </summary>
        public char PeekChar()
        {
            if (HasReachedEnd)
            {
                return NullChar;
            }
            return (char)Peek();
        }

        public bool IsBeginningOfRangeOrList
        {
            get { return CurrentChar == OpenBracketChar; }
        }

        public bool IsEndOfRangeOrList
        {
            get { return CurrentChar == CloseBracketChar; }
        }

        public bool IsPathSeparator()
        {
            return IsPathSeparator(CurrentChar);
        }

        public static bool IsPathSeparator(char character)
        {

            var isCurrentCharacterStartOfDelimiter = character == PathSeparators[0] ||
                                                     character == PathSeparators[1];

            return isCurrentCharacterStartOfDelimiter;

        }

        public bool IsSingleCharacterMatch
        {
            get { return CurrentChar == QuestionMarkChar; }
        }

        public bool IsWildcardCharacterMatch
        {
            get { return CurrentChar == StarChar && PeekChar() != StarChar; }
        }

        public bool IsBeginningOfDirectoryWildcard
        {
            get { return CurrentChar == StarChar && PeekChar() == StarChar; }
        }

        public static bool IsNotStartOfToken(char character)
        {
            return !BeginningOfTokenCharacters.Contains(character);
        }
    }
}
