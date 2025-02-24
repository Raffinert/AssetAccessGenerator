﻿using DotNet.Globbing.Evaluation;

namespace DotNet.Globbing
{
    internal class EvaluationOptions
    {
        public EvaluationOptions()
        {
            CaseInsensitive = false;
        }

        public bool CaseInsensitive { get; set; }

        public virtual IGlobTokenEvaluatorFactory GetTokenEvaluatorFactory()
        {
            return new GlobTokenEvaluatorFactory(this);
        }
    }

}