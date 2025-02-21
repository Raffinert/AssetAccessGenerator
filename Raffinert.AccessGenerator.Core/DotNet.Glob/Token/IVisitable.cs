namespace DotNet.Globbing.Token
{
    internal interface IVisitable<T>
    {
        void Accept(T Visitor);
    }
}
