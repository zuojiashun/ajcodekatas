﻿namespace AjCat.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class NotExpression : Expression
    {
        private static NotExpression instance = new NotExpression();

        private NotExpression()
        {
        }

        public static NotExpression Instance
        {
            get
            {
                return instance;
            }
        }

        public override void Evaluate(Machine machine)
        {
            bool op = (bool)machine.Pop();

            machine.Push(!op);
        }

        public override string ToString()
        {
            return "not";
        }
    }
}
