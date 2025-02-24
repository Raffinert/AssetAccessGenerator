﻿using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Globbing.Token;

namespace DotNet.Globbing
{
    internal class GlobTokeniser
    {
        private readonly StringBuilder _currentBufferText;

        public GlobTokeniser()
        {
            _currentBufferText = new StringBuilder();
        }

        public IList<IGlobToken> Tokenise(string globText)
        {
            var tokens = new List<IGlobToken>();
            using (var reader = new GlobStringReader(globText))
            {
                while (reader.ReadChar())
                {
                    if (reader.IsBeginningOfRangeOrList)
                    {
                        tokens.Add(ReadRangeOrListToken(reader));
                    }
                    else if (reader.IsSingleCharacterMatch)
                    {
                        tokens.Add(ReadSingleCharacterMatchToken());
                    }
                    else if (reader.IsWildcardCharacterMatch)
                    {
                        tokens.Add(ReadWildcardToken());
                    }
                    else if (reader.IsPathSeparator())
                    {
                        var sepToken = ReadPathSeparatorToken(reader);
                        tokens.Add(sepToken);
                    }
                    else if (reader.IsBeginningOfDirectoryWildcard)
                    {
                        if (tokens.Count > 0)
                        {
                            if (tokens[tokens.Count - 1] is PathSeparatorToken lastToken)
                            {
                                tokens.Remove(lastToken);
                                tokens.Add(ReadDirectoryWildcardToken(reader, lastToken));
                                continue;
                            }
                        }

                        tokens.Add(ReadDirectoryWildcardToken(reader, null));
                    }
                    else
                    {
                        tokens.Add(ReadLiteralToken(reader));
                    }
                }
            }

            _currentBufferText.Clear();

            return tokens;
        }

        private IGlobToken ReadDirectoryWildcardToken(GlobStringReader reader, PathSeparatorToken leadingPathSeparatorToken)
        {
            reader.ReadChar();

            if (GlobStringReader.IsPathSeparator(reader.PeekChar()))
            {
                reader.ReadChar();
                var trailingSeparator = ReadPathSeparatorToken(reader);
                return new WildcardDirectoryToken(leadingPathSeparatorToken, trailingSeparator);
            }

            return new WildcardDirectoryToken(leadingPathSeparatorToken, null); // this shouldn't happen unless a pattern ends with ** which is weird. **sometext is not legal.
        }

        private IGlobToken ReadLiteralToken(GlobStringReader reader)
        {
            AcceptCurrentChar(reader);

            while (!reader.HasReachedEnd)
            {
                var peekChar = reader.PeekChar();
                var isValid = GlobStringReader.IsNotStartOfToken(peekChar) && !GlobStringReader.IsPathSeparator(peekChar);

                if (isValid)
                {
                    if (reader.ReadChar())
                    {
                        AcceptCurrentChar(reader);
                    }
                    else
                    {
                        // potentially hit end of string.
                        break;
                    }
                }
                else
                {
                    // we have hit a character that may not be a valid literal (could be unsupported, or start of a token for instance).
                    break;
                }
            }

            return new LiteralToken(GetBufferAndReset());
        }

        /// <summary>
        /// Parses a token for a range or list globbing expression.
        /// </summary>
        private IGlobToken ReadRangeOrListToken(GlobStringReader reader)
        {
            bool isNegated = false;
            bool isNumberRange = false;
            bool isLetterRange = false;
            bool isCharList = false;

            if (reader.PeekChar() == GlobStringReader.ExclamationMarkChar)
            {
                isNegated = true;
                reader.Read();
            }

            var nextChar = reader.PeekChar();
            if (Char.IsLetterOrDigit(nextChar))
            {
                reader.Read();
                nextChar = reader.PeekChar();
                if (nextChar == GlobStringReader.DashChar)
                {
                    if (Char.IsLetter(reader.CurrentChar))
                    {
                        isLetterRange = true;
                    }
                    else
                    {
                        isNumberRange = true;
                    }
                }
                else
                {
                    isCharList = true;
                }

                AcceptCurrentChar(reader);
            }
            else
            {
                isCharList = true;
                reader.Read();
                AcceptCurrentChar(reader);
            }

            if (isLetterRange || isNumberRange)
            {
                // skip over the dash char
                reader.ReadChar();
            }

            while (reader.ReadChar())
            {
                if (reader.IsEndOfRangeOrList)
                {
                    var peekChar = reader.PeekChar();
                    // Close brackets within brackets are escaped with another
                    // Close bracket. e.g. [a]] matches a[
                    if (peekChar == GlobStringReader.CloseBracketChar)
                    {
                        AcceptCurrentChar(reader);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    AcceptCurrentChar(reader);
                }
            }

            // construct token
            IGlobToken result = null;
            var value = GetBufferAndReset();
            if (isCharList)
            {
                result = new CharacterListToken(value.ToCharArray(), isNegated);
            }
            else if (isLetterRange)
            {
                var start = value[0];
                var end = value[1];
                result = new LetterRangeToken(start, end, isNegated);
            }
            else if (isNumberRange)
            {
                var start = value[0]; // int.Parse(value[0].ToString());
                var end = value[1]; // int.Parse(value[1].ToString());
                result = new NumberRangeToken(start, end, isNegated);
            }

            return result;
        }

        private PathSeparatorToken ReadPathSeparatorToken(GlobStringReader reader)
        {
            return new PathSeparatorToken(reader.CurrentChar);
        }

        private IGlobToken ReadWildcardToken()
        {
            return new WildcardToken();
        }

        private IGlobToken ReadSingleCharacterMatchToken()
        {
            return new AnyCharacterToken();
        }

        private void AcceptChar(char character)
        {
            _currentBufferText.Append(character);
        }

        private void AcceptCurrentChar(GlobStringReader reader)
        {
            _currentBufferText.Append(reader.CurrentChar);
        }

        private string GetBufferAndReset()
        {
            var text = _currentBufferText.ToString();
            _currentBufferText.Clear();
            return text;
        }
    }
}
