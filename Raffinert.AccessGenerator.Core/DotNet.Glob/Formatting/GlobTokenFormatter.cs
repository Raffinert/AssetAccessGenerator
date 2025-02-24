﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet.Globbing.Token;

namespace DotNet.Globbing
{
    internal class GlobTokenFormatter : IGlobTokenVisitor
    {
        private StringBuilder _stringBuilder;

        public GlobTokenFormatter()
        {
            _stringBuilder = new StringBuilder();
        }

        public string Format(IEnumerable<IGlobToken> tokens)
        {
            _stringBuilder.Clear();
            foreach (var token in tokens)
            {
                token.Accept(this);
            }
            return _stringBuilder.ToString();
        }

        public void Visit(WildcardDirectoryToken wildcardDirectoryToken)
        {
            if (wildcardDirectoryToken.LeadingPathSeparator != null)
            {
                _stringBuilder.Append(wildcardDirectoryToken.LeadingPathSeparator.Value);
            }
            _stringBuilder.Append("**");
            if (wildcardDirectoryToken.TrailingPathSeparator != null)
            {
                _stringBuilder.Append(wildcardDirectoryToken.TrailingPathSeparator.Value);
            }
        }

        void IGlobTokenVisitor.Visit(CharacterListToken token)
        {
            _stringBuilder.Append('[');

            if (token.IsNegated)
            {
                _stringBuilder.Append('!');
            }

            foreach (var tokenChar in token.Characters)
            {
                _stringBuilder.Append(tokenChar);
            }
            _stringBuilder.Append(']');
        }

        void IGlobTokenVisitor.Visit(PathSeparatorToken token)
        {
            _stringBuilder.Append(token.Value);
        }

        void IGlobTokenVisitor.Visit(LiteralToken token)
        {
            _stringBuilder.Append(token.Value);
        }

        void IGlobTokenVisitor.Visit(LetterRangeToken token)
        {
            _stringBuilder.Append('[');

            if (token.IsNegated)
            {
                _stringBuilder.Append('!');
            }

            _stringBuilder.Append(token.Start);
            _stringBuilder.Append('-');
            _stringBuilder.Append(token.End);
            _stringBuilder.Append(']');
        }

        void IGlobTokenVisitor.Visit(NumberRangeToken token)
        {
            _stringBuilder.Append('[');

            if (token.IsNegated)
            {
                _stringBuilder.Append('!');
            }

            _stringBuilder.Append(token.Start);
            _stringBuilder.Append('-');
            _stringBuilder.Append(token.End);
            _stringBuilder.Append(']');
        }

        void IGlobTokenVisitor.Visit(AnyCharacterToken token)
        {
            _stringBuilder.Append('?');
        }

        void IGlobTokenVisitor.Visit(WildcardToken token)
        {
            _stringBuilder.Append('*');
        }
    }
}
