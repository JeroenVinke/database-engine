﻿using Compiler.Common;
using System;
using System.Collections.Generic;

namespace Compiler.LexicalAnalyer
{
    public static class LexicalLanguage
    {
        public static Dictionary<string, Func<string, Token>> GetLanguage()
        {
            Dictionary<string, Func<string, Token>> lexLanguage = new Dictionary<string, Func<string, Token>>();
            lexLanguage.Add(" #", (string value) => { return new WordToken { Type = TokenType.Nothing }; });
            lexLanguage.Add("\r#", (string value) => { return new WordToken { Type = TokenType.Nothing }; });
            lexLanguage.Add("\n#", (string value) => { return new WordToken { Type = TokenType.Nothing }; });
            lexLanguage.Add("\t#", (string value) => { return new WordToken { Type = TokenType.Nothing }; });
            lexLanguage.Add("select#", (string value) => { return new WordToken { Type = TokenType.Select }; });
            lexLanguage.Add("where#", (string value) => { return new WordToken { Type = TokenType.Where }; });
            lexLanguage.Add("from#", (string value) => { return new WordToken { Type = TokenType.From }; });
            lexLanguage.Add("(<|>|<=|>=|==|!=)#", (string value) => { return new WordToken { Type = TokenType.RelOp }; });
            lexLanguage.Add("\\|\\|#", (string value) => { return new WordToken { Type = TokenType.Or }; });
            lexLanguage.Add("&&#", (string value) => { return new WordToken { Type = TokenType.And }; });
            lexLanguage.Add("\\+#", (string value) => { return new WordToken { Type = TokenType.Plus }; });
            lexLanguage.Add(",#", (string value) => { return new WordToken { Type = TokenType.Comma }; });
            lexLanguage.Add("\\-#", (string value) => { return new WordToken { Type = TokenType.Minus }; });
            lexLanguage.Add("\\*#", (string value) => { return new WordToken { Type = TokenType.Multiplication }; });
            lexLanguage.Add("\\/#", (string value) => { return new WordToken { Type = TokenType.Division }; });
            lexLanguage.Add("(true|false)#", (string value) => { return new WordToken { Type = TokenType.Boolean }; });
            lexLanguage.Add("\"([a-zA-Z])*\"#", (string value) => { return new WordToken { Type = TokenType.String }; });
            lexLanguage.Add("\\(#", (string value) => { return new WordToken { Type = TokenType.ParenthesisOpen }; });
            lexLanguage.Add("\\)#", (string value) => { return new WordToken { Type = TokenType.ParenthesisClose }; });
            lexLanguage.Add("([a-zA-Z])+([a-zA-Z0-9])*#", (string value) =>
            {

                return new WordToken { Type = TokenType.Identifier, Lexeme = value };
            });
            lexLanguage.Add("([0-9])*#", (string value) => { return new WordToken { Type = TokenType.Integer }; });
            return lexLanguage;
        }
    }
}