namespace DotNet.Globbing.Token
{
    internal class WildcardToken : IGlobToken
    {
        public void Accept(IGlobTokenVisitor Visitor)
        {
            Visitor.Visit(this);
        }
    }
}