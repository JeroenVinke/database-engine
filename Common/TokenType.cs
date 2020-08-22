﻿namespace Compiler.Common
{
    public enum TokenType
    {
        Identifier,
        Integer,
        ParenthesisOpen,
        ParenthesisClose,
        Nothing,
        EndOfFile,
        Plus,
        Minus,
        Division,
        Multiplication,
        EmptyString,
        EndMarker,
        Boolean,
        String,
        Or,
        And,
        RelOp,
        Hash,
        Comma,
        Select,
        From,
        Where
    }
}
