﻿using DotNet.Globbing.Token;

namespace DotNet.Globbing.Evaluation
{
    public class GlobTokenEvaluatorFactory : IGlobTokenEvaluatorFactory
    {
        private readonly EvaluationOptions _options;

        public GlobTokenEvaluatorFactory(EvaluationOptions options)
        {
            _options = options;
        }

        public IGlobTokenEvaluator CreateTokenEvaluator(AnyCharacterToken token)
        {
            return new AnyCharacterTokenEvaluator(token);
        }

        public IGlobTokenEvaluator CreateTokenEvaluator(CharacterListToken token)
        {
            if (_options.CaseInsensitive)
            {
                return new CharacterListTokenEvaluatorCaseInsensitive(token);
            }

            return new CharacterListTokenEvaluator(token);
        }

        public IGlobTokenEvaluator CreateTokenEvaluator(LetterRangeToken token)
        {
            if (_options.CaseInsensitive)
            {
                return new LetterRangeTokenEvaluatorCaseInsensitive(token);
            }
            return new LetterRangeTokenEvaluator(token);
        }

        public IGlobTokenEvaluator CreateTokenEvaluator(LiteralToken token)
        {
            if (_options.CaseInsensitive)
            {
                return new LiteralTokenEvaluatorCaseInsensitive(token);
            }

            return new LiteralTokenEvaluator(token);

        }

        public IGlobTokenEvaluator CreateTokenEvaluator(NumberRangeToken token)
        {
            return new NumberRangeTokenEvaluator(token);
        }

        public IGlobTokenEvaluator CreateTokenEvaluator(PathSeparatorToken token)
        {
            return new PathSeparatorTokenEvaluator(token);
        }

        public IGlobTokenEvaluator CreateTokenEvaluator(WildcardDirectoryToken token, CompositeTokenEvaluator nestedCompositeTokenEvaluator)
        {
            return new WildcardDirectoryTokenEvaluator(token, nestedCompositeTokenEvaluator);
        }

        public IGlobTokenEvaluator CreateTokenEvaluator(WildcardToken token, CompositeTokenEvaluator nestedCompositeTokenEvaluator)
        {
            return new WildcardTokenEvaluator(token, nestedCompositeTokenEvaluator);
        }
    }
}