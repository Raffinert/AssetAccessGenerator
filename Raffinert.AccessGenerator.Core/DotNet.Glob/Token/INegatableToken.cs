namespace DotNet.Globbing.Token
{
    internal interface INegatableToken : IGlobToken
    {
        bool IsNegated { get; set; }
    }
}