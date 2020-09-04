using Compiler.Common;
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
            lexLanguage.Add("(select|SELECT)#", (string value) => { return new WordToken { Type = TokenType.Select }; });
            lexLanguage.Add("(where|WHERE)#", (string value) => { return new WordToken { Type = TokenType.Where }; });
            lexLanguage.Add("(from|FROM)#", (string value) => { return new WordToken { Type = TokenType.From }; });
            lexLanguage.Add("(join|JOIN)#", (string value) => { return new WordToken { Type = TokenType.Join }; });
            lexLanguage.Add("(insert|INSERT)#", (string value) => { return new WordToken { Type = TokenType.Insert }; });
            lexLanguage.Add("(into|INTO)#", (string value) => { return new WordToken { Type = TokenType.Into }; });
            lexLanguage.Add("(values|VALUES)#", (string value) => { return new WordToken { Type = TokenType.Values }; });
            lexLanguage.Add("(on|ON)#", (string value) => { return new WordToken { Type = TokenType.On }; });
            lexLanguage.Add("(top|TOP)#", (string value) => { return new WordToken { Type = TokenType.Top }; });
            lexLanguage.Add("(<|>|<=|>=|=|!=)#", (string value) => { return new WordToken { Type = TokenType.RelOp }; });
            lexLanguage.Add("\\|\\|#", (string value) => { return new WordToken { Type = TokenType.Or }; });
            lexLanguage.Add("&&#", (string value) => { return new WordToken { Type = TokenType.And }; });
            lexLanguage.Add(",#", (string value) => { return new WordToken { Type = TokenType.Comma }; });
            lexLanguage.Add("\\*#", (string value) =>
            {
                return new WordToken { Type = TokenType.Identifier, Lexeme = value };
            });
            lexLanguage.Add("\\/#", (string value) => { return new WordToken { Type = TokenType.Division }; });
            lexLanguage.Add("(true|false)#", (string value) => { return new WordToken { Type = TokenType.Boolean }; });
            lexLanguage.Add("\"([a-zA-Z])*\"#", (string value) =>
            {
                return new WordToken { Type = TokenType.String, Lexeme = value.Replace("\"", "") };
            });
            lexLanguage.Add("\\(#", (string value) => { return new WordToken { Type = TokenType.ParenthesisOpen }; });
            lexLanguage.Add("\\)#", (string value) => { return new WordToken { Type = TokenType.ParenthesisClose }; });
            lexLanguage.Add("([a-zA-Z])*.([a-zA-Z])*.#", (string value) =>
            {
                return new WordToken { Type = TokenType.Identifier, Lexeme = value };
            });
            lexLanguage.Add("([a-zA-Z])+([a-zA-Z0-9])*#", (string value) =>
            {
                return new WordToken { Type = TokenType.Identifier, Lexeme = value };
            });
            lexLanguage.Add("([0-9])*#", (string value) => { return new WordToken { Type = TokenType.Integer }; });
            return lexLanguage;
        }
    }
}
