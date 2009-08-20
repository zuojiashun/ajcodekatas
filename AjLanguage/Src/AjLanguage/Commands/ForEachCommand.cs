﻿namespace AjLanguage.Commands
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;

    using AjLanguage.Expressions;

    public class ForEachCommand : ICommand
    {
        private string name;
        private IExpression expression;
        private ICommand command;

        public ForEachCommand(string name, IExpression expression, ICommand command)
        {
            this.name = name;
            this.expression = expression;
            this.command = command;
        }

        public void Execute(BindingEnvironment environment)
        {
            BindingEnvironment newenv = new BindingEnvironment(environment);

            foreach (object result in (IEnumerable) this.expression.Evaluate(environment))
            {
                newenv.SetValue(this.name, result);
                this.command.Execute(newenv);
            }
        }
    }
}
