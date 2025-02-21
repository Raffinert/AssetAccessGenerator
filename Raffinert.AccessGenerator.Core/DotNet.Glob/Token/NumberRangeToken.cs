namespace DotNet.Globbing.Token
{
    internal class NumberRangeToken : RangeToken<char>
    {
        public NumberRangeToken(char start, char end, bool isNegated) : base(start, end, isNegated)
        {
        }

        public override void Accept(IGlobTokenVisitor Visitor)
        {
            Visitor.Visit(this);
        }

    }
}