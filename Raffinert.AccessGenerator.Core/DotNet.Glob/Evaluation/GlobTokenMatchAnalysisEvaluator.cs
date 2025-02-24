using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNet.Globbing.Token;

namespace DotNet.Globbing.Evaluation
{
    internal class GlobTokenMatchAnalysisEvaluator : IGlobTokenVisitor
    {
        private IGlobToken[] _Tokens;
        private GlobStringReader _Reader;
        public Queue<IGlobToken> _TokenQueue;
        public string _text;

        public List<GlobTokenMatch> MatchedTokens { get; set; }

        public GlobTokenMatchAnalysisEvaluator(IGlobToken[] tokens)
        {
            _Tokens = tokens;
            var tokenCount = tokens.Length;
            _TokenQueue = new Queue<IGlobToken>(tokenCount);
            MatchedTokens = new List<GlobTokenMatch>(tokens.Length);
        }

        private void EnqueueTokens(IGlobToken[] tokens)
        {
            MatchedTokens.Clear();
            _TokenQueue.Clear();
            foreach (var token in tokens)
            {
                _TokenQueue.Enqueue(token);
            }
        }

        public MatchInfo Evaluate(string text)
        {
            EnqueueTokens(_Tokens);
            _text = text;
            IGlobToken token = null;
            using (_Reader = new GlobStringReader(text))
            {
                while (_TokenQueue.Any())
                {
                    token = _TokenQueue.Dequeue();
                    token.Accept(this);
                    if (!Success)
                    {
                        return FailedResult(token);
                    }
                }

                // if all tokens matched but still more text then fail!
                if (_Reader.Peek() != -1)
                {
                    return FailedResult(null);
                }

                // Success.
                return SuccessfulResult();
            }
        }
        private MatchInfo SuccessfulResult()
        {
            return new MatchInfo()
            {
                Matches = MatchedTokens.ToArray(),
                Missed = null,
                Success = true,
                UnmatchedText = _Reader.ReadToEnd()
            };
        }
        private MatchInfo FailedResult(IGlobToken token)
        {
            return new MatchInfo()
            {
                Matches = MatchedTokens.ToArray(),
                Missed = token,
                Success = false,
                UnmatchedText = _Reader.ReadToEnd()
            };
        }
        public void Visit(PathSeparatorToken token)
        {
            Success = false;
            var read = _Reader.Read();
            if (read == -1)
            {
                return;
            }

            var currentChar = (char)read;
            if (!GlobStringReader.IsPathSeparator(currentChar))
            {
                return;
            }

            AddMatch(new GlobTokenMatch() { Token = token, Value = currentChar.ToString() });
        }
        public void Visit(LiteralToken token)
        {
            Success = false;
            foreach (var literalChar in token.Value)
            {
                var read = _Reader.Read();
                if (read == -1)
                {
                    return;
                }
                var currentChar = (char)read;
                if (currentChar != literalChar)
                {
                    return;
                }
            }

            AddMatch(new GlobTokenMatch() { Token = token });

        }
        public void Visit(AnyCharacterToken token)
        {
            Success = false;
            var read = _Reader.Read();
            if (read == -1)
            {
                return;
            }
            var currentChar = (char)read;
            if (GlobStringReader.IsPathSeparator(currentChar))
            {
                return;
            }

            AddMatch(new GlobTokenMatch() { Token = token, Value = currentChar.ToString() });
        }
        public void Visit(WildcardToken token)
        {
            // When * encountered,
            // Dequees all remaining tokens and passes them to a nested Evaluator.
            // Keeps seing if the nested evaluator will match, and if it doesn't then
            // will consume / match against one character, and retry.
            // Exits when match successful, or when the end of the current path segment is reached.
            GlobTokenMatch match = null;

            match = new GlobTokenMatch() { Token = token };
            AddMatch(match);


            var remainingText = _Reader.ReadToEnd();
            int endOfSegmentPos;
            using (var pathReader = new GlobStringReader(remainingText))
            {
                var thisPath = pathReader.ReadPathSegment();
                endOfSegmentPos = pathReader.CurrentIndex;
            }

            var remaining = _TokenQueue.ToArray();
            // if no more tokens remaining then just return as * matches the rest of the segment.
            if (remaining.Length == 0)
            {
                this.Success = true;

                match.Value = remainingText;


                return;
            }

            // we have to attempt to match the remaining tokens, and if they dont all match,
            // then consume a character, until we have matched the entirity of this segment.
            var matchedText = new StringBuilder(endOfSegmentPos);
            var nestedEval = new GlobTokenMatchAnalysisEvaluator(remaining);
            var pathSegments = new List<string>();

            // we keep a record of text that this wildcard matched in order to satisfy the
            // greatest number of child token matches.
            var bestMatchText = new StringBuilder(endOfSegmentPos);
            IList<GlobTokenMatch> bestMatches = null; // the most tokens that were matched.

            for (int i = 0; i <= endOfSegmentPos; i++)
            {
                var matchInfo = nestedEval.Evaluate(remainingText);
                if (matchInfo.Success)
                {
                    break;
                }

                // match a single character
                matchedText.Append(remainingText[0]);
                // re-attempt matching of child tokens against this remaining string.
                remainingText = remainingText.Substring(1);
                // If we have come closer to matching, record our best results.
                if ((bestMatches == null && matchInfo.Matches.Any()) || (bestMatches != null && bestMatches.Count < matchInfo.Matches.Length))
                {
                    bestMatches = matchInfo.Matches.ToArray();
                    bestMatchText.Clear();
                    bestMatchText.Append(matchedText.ToString());
                }

            }

            this.Success = nestedEval.Success;
            if (nestedEval.Success)
            {
                // add all child matches.
                this.MatchedTokens.AddRange(nestedEval.MatchedTokens);

            }
            else
            {
                // add the most tokens we managed to match.
                if (bestMatches != null && bestMatches.Any())
                {
                    this.MatchedTokens.AddRange(bestMatches);
                }
            }

            match.Value = matchedText.ToString();
            _TokenQueue.Clear();
        }

        public void Visit(WildcardDirectoryToken token)
        {
            // When * encountered,
            // Dequees all remaining tokens and passes them to a nested Evaluator.
            // Keeps seing if the nested evaluator will match, and if it doesn't then
            // will consume / match against one character, and retry.
            // Exits when match successful, or when the end of the current path segment is reached.
            GlobTokenMatch match = null;

            match = new GlobTokenMatch() { Token = token };
            AddMatch(match);


            var remainingText = _Reader.ReadToEnd();
            int endOfSegmentPos = remainingText.Length; //TODO: improve this.


            var remaining = _TokenQueue.ToArray();
            // if no more tokens remaining then just return as * matches the rest of the segment.
            if (remaining.Length == 0)
            {
                this.Success = true;
                match.Value = remainingText;
                return;
            }

            // we have to attempt to match the remaining tokens, and if they dont all match,
            // then consume a character, until we have matched the entirity of this segment.
            var matchedText = new StringBuilder(endOfSegmentPos);
            var nestedEval = new GlobTokenMatchAnalysisEvaluator(remaining);
            var pathSegments = new List<string>();

            // we keep a record of text that this wildcard matched in order to satisfy the
            // greatest number of child token matches.
            var bestMatchText = new StringBuilder(endOfSegmentPos);
            IList<GlobTokenMatch> bestMatches = null; // the most tokens that were matched.

            for (int i = 0; i < endOfSegmentPos; i++)
            {
                var matchInfo = nestedEval.Evaluate(remainingText);
                if (matchInfo.Success)
                {
                    break;
                }

                // match a single character
                matchedText.Append(remainingText[0]);
                // re-attempt matching of child tokens against this remaining string.
                remainingText = remainingText.Substring(1);
                // If we have come closer to matching, record our best results.
                if ((bestMatches == null && matchInfo.Matches.Any()) || (bestMatches != null && bestMatches.Count < matchInfo.Matches.Length))
                {
                    bestMatches = matchInfo.Matches.ToArray();
                    bestMatchText.Clear();
                    bestMatchText.Append(matchedText.ToString());
                }

            }

            this.Success = nestedEval.Success;
            if (nestedEval.Success)
            {
                // add all child matches.
                this.MatchedTokens.AddRange(nestedEval.MatchedTokens);

            }
            else
            {
                // add the most tokens we managed to match.
                if (bestMatches != null && bestMatches.Any())
                {
                    this.MatchedTokens.AddRange(bestMatches);
                }
            }

            match.Value = matchedText.ToString();
            _TokenQueue.Clear();
        }
        public void Visit(LetterRangeToken token)
        {
            Success = false;
            var read = _Reader.Read();
            if (read == -1)
            {
                return;
            }
            var currentChar = (char)read;
            if (currentChar >= token.Start && currentChar <= token.End)
            {
                if (token.IsNegated)
                {
                    return; // failed to match
                }
            }
            else
            {
                if (!token.IsNegated)
                {
                    return; // failed to match
                }
            }

            AddMatch(new GlobTokenMatch() { Token = token, Value = currentChar.ToString() });



        }
        public void Visit(NumberRangeToken token)
        {
            Success = false;
            var read = _Reader.Read();
            if (read == -1)
            {
                return;
            }
            var currentChar = (char)read;
            if (currentChar >= token.Start && currentChar <= token.End)
            {
                if (token.IsNegated)
                {
                    return; // failed to match
                }
            }
            else
            {
                if (!token.IsNegated)
                {
                    return; // failed to match
                }
            }


            AddMatch(new GlobTokenMatch() { Token = token, Value = currentChar.ToString() });


        }
        public void Visit(CharacterListToken token)
        {
            Success = false;
            var read = _Reader.Read();
            if (read == -1)
            {
                return;
            }
            var currentChar = (char)read;
            var contains = token.Characters.Contains(currentChar);

            if (token.IsNegated)
            {
                if (contains)
                {
                    return;
                }
            }
            else
            {
                if (!contains)
                {
                    return;
                }
            }


            AddMatch(new GlobTokenMatch() { Token = token, Value = currentChar.ToString() });


        }
        public bool Success { get; set; }
        private void AddMatch(GlobTokenMatch match)
        {
            this.MatchedTokens.Add(match);
            this.Success = true;
        }

    }
}