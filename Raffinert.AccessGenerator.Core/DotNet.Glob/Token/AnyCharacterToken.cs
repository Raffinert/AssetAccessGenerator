namespace DotNet.Globbing.Token
{
    internal class AnyCharacterToken : IGlobToken
    {
        public void Accept(IGlobTokenVisitor Visitor)
        {
            Visitor.Visit(this);
        }
    }
}