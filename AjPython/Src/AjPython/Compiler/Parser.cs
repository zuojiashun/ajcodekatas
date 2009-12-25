﻿namespace AjPython.Compiler
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AjPython.Commands;
    using AjPython.Expressions;

    public class Parser
    {
        private static string[] opslevel1 = new string[] { "+", "-" };
        private static string[] opslevel2 = new string[] { "*", "/" };
        private static string[] opslevel3 = new string[] { "**" };

        private Lexer lexer;

        public Parser(Lexer lexer)
        {
            if (lexer == null)
                throw new System.ArgumentNullException("lexer");

            this.lexer = lexer;
        }

        public Parser(string text)
            : this(new Lexer(text))
        {
        }

        public Parser(TextReader reader)
            : this(new Lexer(reader))
        {
        }

        public IExpression CompileExpression()
        {
            IExpression expression = this.CompileBinaryLevel1Expression();

            return expression;
        }

        public IExpression CompileList()
        {
            ListExpression listExpression = new ListExpression();

            Token token = this.lexer.NextToken();

            while (token != null && token.Value != "]")
            {
                if (listExpression.Expressions.Count != 0)
                {
                    if (token.Value != ",")
                        throw new InvalidDataException(string.Format("Unexpected '{0}'", token.Value));
                }
                else
                    this.lexer.PushToken(token);

                IExpression expression = this.CompileExpression();
                listExpression.Add(expression);

                token = this.lexer.NextToken();
            }

            if (token != null)
                this.lexer.PushToken(token);

            return listExpression;
        }

        public IExpression CompileDictionary()
        {
            DictionaryExpression dictionaryExpression = new DictionaryExpression();

            Token token = this.lexer.NextToken();

            while (token != null && token.Value != "}")
            {
                if (dictionaryExpression.KeyExpressions.Count != 0)
                {
                    if (token.Value != ",")
                        throw new InvalidDataException(string.Format("Unexpected '{0}'", token.Value));
                }
                else
                    this.lexer.PushToken(token);

                IExpression keyExpression = this.CompileExpression();
                this.CompileExpectedToken(":");
                IExpression valueExpression = this.CompileExpression();
                dictionaryExpression.Add(keyExpression, valueExpression);

                token = this.lexer.NextToken();
            }

            if (token != null)
                this.lexer.PushToken(token);

            return dictionaryExpression;
        }

        public ICommand CompileCommand()
        {
            ICommand command = this.CompileSimpleCommand();

            if (command == null)
                return null;

            this.CompileEndOfCommand();

            return command;
        }

        private void CompileEndOfCommand()
        {
            Token token = this.lexer.NextToken();

            if (token == null)
                return;

            if (token.TokenType == TokenType.EndOfLine)
                return;

            this.lexer.PushToken(token);

            throw new UnexpectedTokenException(token);
        }

        private ICommand CompileSimpleCommand()
        {
            Token token = this.CompileName();

            if (token == null)
                return null;

            if (token.Value == "print")
            {
                IExpression expression = this.CompileExpression();
                return new PrintCommand(expression);
            }

            Token token2 = this.lexer.NextToken();

            if (token2 != null && token2.TokenType == TokenType.Operator && token2.Value == "=")
            {
                IExpression expression = this.CompileExpression();
                return new SimpleAssignmentCommand(token.Value, expression);
            }

            if (token2 == null)
                throw new UnexpectedEndOfInputException();

            throw new UnexpectedTokenException(token2);
        }

        private static Operator CompileOperator(string oper)
        {
            if (oper == "+")
                return Operator.Add;

            if (oper == "-")
                return Operator.Subtract;

            if (oper == "*")
                return Operator.Multiply;

            if (oper == "/")
                return Operator.Divide;

            if (oper == "**")
                return Operator.Power;

            throw new System.InvalidOperationException(string.Format("Unexpected {0}", oper));
        }

        private static bool IsLevel1Operator(Token token)
        {
            return token != null && token.TokenType == TokenType.Operator && opslevel1.Contains(token.Value);
        }

        private static bool IsLevel2Operator(Token token)
        {
            return token != null && token.TokenType == TokenType.Operator && opslevel2.Contains(token.Value);
        }

        private static bool IsLevel3Operator(Token token)
        {
            return token != null && token.TokenType == TokenType.Operator && opslevel3.Contains(token.Value);
        }

        private IExpression CompileBinaryLevel3Expression()
        {
            IExpression expression = this.CompileTerm();

            if (expression == null)
                return null;

            Token token = this.lexer.NextToken();

            while (IsLevel3Operator(token))
            {
                IExpression expression2 = this.CompileTerm();
                expression = new BinaryOperatorExpression(expression, expression2, CompileOperator(token.Value));
                token = this.lexer.NextToken();
            }

            if (token != null)
                this.lexer.PushToken(token);

            return expression;
        }

        private IExpression CompileBinaryLevel2Expression()
        {
            IExpression expression = this.CompileBinaryLevel3Expression();

            if (expression == null)
                return null;

            Token token = this.lexer.NextToken();

            while (IsLevel2Operator(token))
            {
                IExpression expression2 = this.CompileBinaryLevel3Expression();
                expression = new BinaryOperatorExpression(expression, expression2, CompileOperator(token.Value));
                token = this.lexer.NextToken();
            }

            if (token != null)
                this.lexer.PushToken(token);

            return expression;
        }

        private IExpression CompileBinaryLevel1Expression()
        {
            IExpression expression = this.CompileBinaryLevel2Expression();

            if (expression == null)
                return null;

            Token token = this.lexer.NextToken();

            while (IsLevel1Operator(token))
            {
                IExpression expression2 = this.CompileBinaryLevel2Expression();
                expression = new BinaryOperatorExpression(expression, expression2, CompileOperator(token.Value));
                token = this.lexer.NextToken();
            }

            if (token != null)
                this.lexer.PushToken(token);

            return expression;
        }

        private IExpression CompileTerm()
        {
            Token token = this.lexer.NextToken();

            if (token == null)
                return null;

            switch (token.TokenType)
            {
                case TokenType.String:
                    return new ConstantExpression(token.Value);
                case TokenType.Integer:
                    return new ConstantExpression(System.Convert.ToInt32(token.Value));
                case TokenType.Real:
                    return new ConstantExpression(System.Convert.ToDouble(token.Value));
                case TokenType.Boolean:
                    return new ConstantExpression(System.Convert.ToBoolean(token.Value));
                case TokenType.Name:
                    return new NameExpression(token.Value);
                case TokenType.Separator:
                    if (token.Value == "(")
                    {
                        IExpression expression = this.CompileExpression();
                        this.CompileExpectedToken(")");
                        return expression;
                    }

                    if (token.Value == "[")
                    {
                        IExpression expression = this.CompileList();
                        this.CompileExpectedToken("]");
                        return expression;
                    }

                    if (token.Value == "{")
                    {
                        IExpression expression = this.CompileDictionary();
                        this.CompileExpectedToken("}");
                        return expression;
                    }

                    break;
            }

            throw new InvalidDataException(string.Format("Unknown '{0}'", token.Value));
        }

        private void CompileExpectedToken(string value)
        {
            Token token = this.lexer.NextToken();

            if (token == null || token.Value != value)
                throw new InvalidDataException(string.Format("{0} expected", value));
        }

        private Token CompileName()
        {
            Token token = this.lexer.NextToken();

            if (token == null || token.TokenType != TokenType.Name)
                throw new NameExpectedException();

            return token;
        }
    }
}
