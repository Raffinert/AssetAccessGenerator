using DotNet.Globbing.Token;

namespace DotNet.Globbing
{
    internal class GlobTokenMatch
    {
        public IGlobToken Token { get; set; }
        public string Value { get; set; }
    }
}