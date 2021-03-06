﻿namespace AjCat.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AjCat.Expressions;

    public class Compiler
    {
        private Lexer parser;
        private ExpressionEnvironment environment;

        public Compiler(Lexer parser)
            : this(parser, new TopExpressionEnvironment())
        {
        }

        public Compiler(string text)
            : this(new Lexer(text), new TopExpressionEnvironment())
        {
        }

        public Compiler(TextReader reader)
            : this(new Lexer(reader), new TopExpressionEnvironment())
        {
        }

        public Compiler(Lexer parser, ExpressionEnvironment environment)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            this.parser = parser;
            this.environment = environment;
        }

        public Compiler(string text, ExpressionEnvironment environment)
            : this(new Lexer(text), environment)
        {
        }

        public Compiler(TextReader reader, ExpressionEnvironment environment)
            : this(new Lexer(reader), environment)
        {
        }

        public Expression CompileExpression()
        {
            List<Expression> list = this.CompileList();

            Token token = this.parser.NextToken();

            if (token != null)
            {
                throw new UnexpectedTokenException(token);
            }

            if (list == null || list.Count == 0)
            {
                return null;
            }

            if (list.Count == 1)
            {
                return list[0];
            }

            return new CompositeExpression(list);
        }

        private Expression CompileQuotation()
        {
            List<Expression> list = this.CompileList();

            this.CompileToken("]");

            return new QuotationExpression(list);
        }

        private void CompileToken(string value)
        {
            Token token = this.parser.NextToken();

            if (token == null)
            {
                throw new ExpectedTokenException(value);
            }

            if (token.Value != value)
            {
                throw new UnexpectedTokenException(token);
            }
        }

        private Token CompileName()
        {
            Token token = this.parser.NextToken();

            if (token == null)
            {
                throw new NameExpectedException();
            }

            if (token.TokenType != TokenType.Name)
            {
                throw new UnexpectedTokenException(token);
            }

            return token;
        }

        private List<Expression> CompileList()
        {
            Token token = this.parser.NextToken();

            if (token == null)
            {
                return null;
            }

            List<Expression> list = new List<Expression>();

            this.parser.PushToken(token);

            while (!this.IsClosingToken(token))
            {
                Expression expression = this.CompileSingleExpression();
                list.Add(expression);
                token = this.parser.NextToken();
                if (token != null)
                {
                    this.parser.PushToken(token);
                }
            }

            return list;
        }

        private bool IsClosingToken(Token token)
        {
            if (token == null)
            {
                return true;
            }

            if (token.Value == "]" || token.Value == "}")
            {
                return true;
            }

            return false;
        }

        private Expression CompileDefineExpression()
        {
            Token nametoken = this.CompileName();
            CompositeExpression composite = new CompositeExpression(new List<Expression>());
            this.environment.DefineExpression(nametoken.Value, composite);

            this.CompileToken("{");

            List<Expression> list = this.CompileList();

            this.CompileToken("}");

            foreach (Expression expr in list)
            {
                composite.Expressions.Add(expr);
            }

            return new DefineExpression(nametoken.Value, list);
        }

        private Expression CompileSingleExpression()
        {
            Token token = this.parser.NextToken();

            if (token == null) 
            {
                return null;
            }

            switch (token.TokenType) 
            {
                case TokenType.Integer:
                    return new IntegerExpression(Convert.ToInt32(token.Value, CultureInfo.CurrentCulture));
                case TokenType.Double:
                    return new DoubleExpression(Convert.ToDouble(token.Value, CultureInfo.CurrentCulture));
                case TokenType.String:
                    return new StringExpression(token.Value);
                case TokenType.Name:
                    if (token.Value == "define")
                    {
                        return this.CompileDefineExpression();
                    }

                    return this.environment.GetByName(token.Value);
                case TokenType.Separator:
                    if (token.Value == "[")
                    {
                        return this.CompileQuotation();
                    }

                    break;
            }

            throw new UnexpectedTokenException(token);
        }
    }
}
