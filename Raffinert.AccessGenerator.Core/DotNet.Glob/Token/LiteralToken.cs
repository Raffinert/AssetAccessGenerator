﻿namespace DotNet.Globbing.Token
{
    internal class LiteralToken : IGlobToken
    {
        public LiteralToken(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public void Accept(IGlobTokenVisitor Visitor)
        {
            Visitor.Visit(this);
        }
    }
}