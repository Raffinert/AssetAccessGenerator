﻿using System;
using System.Linq;
using DotNet.Globbing.Evaluation;
using DotNet.Globbing.Token;

namespace DotNet.Globbing
{
    internal class Glob
    {
        public IGlobToken[] Tokens { get; }
        private GlobTokenFormatter _Formatter;
        private string _pattern;
        private readonly GlobTokenEvaluator _isMatchEvaluator;
        private readonly GlobOptions _options;

        public Glob(params IGlobToken[] tokens) : this(GlobOptions.Default, tokens)
        {
        }

        public Glob(GlobOptions options = null, params IGlobToken[] tokens)
        {
            Tokens = tokens;
            _options = options ?? GlobOptions.Default;
            _Formatter = new GlobTokenFormatter();
            _isMatchEvaluator = new GlobTokenEvaluator(options.Evaluation, Tokens);
        }

        public static Glob Parse(string pattern)
        {
            var options = GlobOptions.Default;
            return Parse(pattern, options);
        }

        public static Glob Parse(string pattern, GlobOptions options)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new ArgumentNullException(pattern);
            }
            var tokeniser = new GlobTokeniser();
            var tokens = tokeniser.Tokenise(pattern);
            return new Glob(options, tokens.ToArray());
        }

        public bool IsMatch(string subject)
        {
            return _isMatchEvaluator.IsMatch(subject);
        }

#if SPAN
        public bool IsMatch(ReadOnlySpan<char> subject)
        {
            return _isMatchEvaluator.IsMatch(subject);
        }
#endif

        public override string ToString()
        {
            if (_pattern == null)
            {
                _pattern = _Formatter.Format(Tokens);
            }
            return _pattern;
        }
    }
}
