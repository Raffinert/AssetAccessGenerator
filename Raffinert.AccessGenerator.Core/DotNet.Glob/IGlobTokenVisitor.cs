﻿using DotNet.Globbing.Token;

namespace DotNet.Globbing
{
    public interface IGlobTokenVisitor
    {
        void Visit(WildcardToken token);
        void Visit(WildcardDirectoryToken wildcardDirectoryToken);
        void Visit(AnyCharacterToken token);
        void Visit(LetterRangeToken token);
        void Visit(NumberRangeToken token);
        void Visit(CharacterListToken token);
        void Visit(LiteralToken token);
        void Visit(PathSeparatorToken token);

    }
}