﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AjIo.Compiler;
using AjIo.Language;
using AjIo.Methods;

namespace AjIo.Tests
{
    [TestClass]
    public class EvaluationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            this.machine = new Machine();
        }

        [TestMethod]
        public void EvaluateInteger()
        {
            object result = this.Evaluate("12");
            Assert.AreEqual(12, result);
        }

        [TestMethod]
        public void EvaluateIdentifier()
        {
            this.machine.SetSlot("foo", "bar");
            object result = this.Evaluate("foo");
            Assert.AreEqual("bar", result);
        }

        [TestMethod]
        public void EvaluateObject()
        {
            object result = this.Evaluate("Object");
            Assert.IsInstanceOfType(result, typeof(TopObject));
        }

        [TestMethod]
        public void EvaluateObjectClone()
        {
            object result = this.Evaluate("Object clone");
            Assert.IsInstanceOfType(result, typeof(DerivedObject));
        }

        [TestMethod]
        public void EvaluateDefineDog()
        {
            this.Evaluate("setSlot(\"Dog\", Object clone)");

            object result = this.machine.GetSlot("Dog");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DerivedObject));
        }

        [TestMethod]
        public void EvaluateDefineDogAndSetName()
        {
            this.Evaluate("setSlot(\"Dog\", Object clone)");
            this.Evaluate("Dog setSlot(\"name\", \"Fido\")");

            object result = this.machine.GetSlot("Dog");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IObject));

            result = ((IObject)result).GetSlot("name");

            Assert.IsNotNull(result);
            Assert.AreEqual("Fido", result);
        }

        [TestMethod]
        public void EvaluateDefineDogWithMethod()
        {
            this.Evaluate("setSlot(\"Dog\", Object clone)");
            this.Evaluate("Dog setSlot(\"name\", \"Fido\")");
            this.Evaluate("Dog setSlot(\"getName\", method(name))");

            object result = this.Evaluate("Dog getName");

            Assert.IsNotNull(result);
            Assert.AreEqual("Fido", result);
        }

        [TestMethod]
        public void EvaluateDefineDogUsingAssigment()
        {
            this.Evaluate("Dog := Object clone");

            object result = this.machine.GetSlot("Dog");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DerivedObject));
        }

        [TestMethod]
        public void EvaluateDefineDogUsingAssigmentWithExpression()
        {
            this.Evaluate("Dog ::= Object clone clone");

            object result = this.machine.GetSlot("Dog");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DerivedObject));
        }

        [TestMethod]
        public void EvaluateDefineDogAndSetSlot()
        {
            this.Evaluate("Dog ::= Object clone clone");
            this.Evaluate("Dog name ::= \"Fido\"");

            object result = this.machine.GetSlot("Dog");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DerivedObject));

            Assert.AreEqual("Fido", ((IObject)result).GetSlot("name"));
        }

        [TestMethod]
        public void EvaluateManyDefineDogAndSetSlot()
        {
            this.EvaluateMany("Dog ::= Object clone clone; Dog name ::= \"Fido\"");

            object result = this.machine.GetSlot("Dog");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DerivedObject));

            Assert.AreEqual("Fido", ((IObject)result).GetSlot("name"));
        }

        [TestMethod]
        public void EvaluateAddTwoIntegers()
        {
            object result = this.Evaluate("1 + 2");

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void EvaluateSubtractTwoIntegers()
        {
            object result = this.Evaluate("3 - 2");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void EvaluateReal()
        {
            object result = this.Evaluate("3.14");

            Assert.IsNotNull(result);
            Assert.AreEqual(3.14, result);
        }

        [TestMethod]
        public void EvaluateOperateTwoReals()
        {
            object result = this.Evaluate("3.14 + 1.23");

            Assert.IsNotNull(result);
            Assert.AreEqual(3.14 + 1.23, result);
            
            result = this.Evaluate("3.14 - 1.23");

            Assert.IsNotNull(result);
            Assert.AreEqual(3.14 - 1.23, result);

            result = this.Evaluate("3.14 * 1.23");

            Assert.IsNotNull(result);
            Assert.AreEqual(3.14 * 1.23, result);

            result = this.Evaluate("3.14 / 1.23");

            Assert.IsNotNull(result);
            Assert.AreEqual(3.14 / 1.23, result);
        }

        [TestMethod]
        public void EvaluateMethod()
        {
            object result = this.Evaluate("method(a)");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IMethod));
            Assert.IsInstanceOfType(result, typeof(Method));
        }

        [TestMethod]
        public void EvaluateAndExecuteMethodWithMultipleMessages()
        {
            object result = this.Evaluate("method(a:=1;b:=2)");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IMethod));
            Assert.IsInstanceOfType(result, typeof(Method));

            Method method = (Method)result;

            method.Execute(this.machine, this.machine, null);

            Assert.AreEqual(1, this.machine.GetSlot("a"));
            Assert.AreEqual(2, this.machine.GetSlot("b"));
        }

        [TestMethod]
        public void EvaluateAndExecuteMethodWithMultipleMessagesInLines()
        {
            object result = this.Evaluate("method(a:=1\r\nb:=2\r\nc:=3)");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IMethod));
            Assert.IsInstanceOfType(result, typeof(Method));

            Method method = (Method)result;

            method.Execute(this.machine, this.machine, null);

            Assert.AreEqual(1, this.machine.GetSlot("a"));
            Assert.AreEqual(2, this.machine.GetSlot("b"));
            Assert.AreEqual(3, this.machine.GetSlot("c"));
        }

        [TestMethod]
        public void EvaluateMultiplyTwoIntegers()
        {
            object result = this.Evaluate("2 * 3");

            Assert.IsNotNull(result);
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void EvaluateDivideTwoIntegers()
        {
            object result = this.Evaluate("6 / 3");

            Assert.IsNotNull(result);
            Assert.AreEqual(2.0, result);
        }

        [TestMethod]
        public void EvaluateSimpleArithmeticExpression()
        {
            object result = this.Evaluate("6 / 3 * 2 + 1");

            Assert.IsNotNull(result);
            Assert.AreEqual(5.0, result);
        }

        [TestMethod]
        public void EvaluateArithmeticExpressionWithParenthesis()
        {
            object result = this.Evaluate("6 + (3 * 2) + 1");

            Assert.IsNotNull(result);
            Assert.AreEqual(13, result);
        }

        [TestMethod]
        public void EvaluateArithmeticExpressionWithStartingParenthesis()
        {
            object result = this.Evaluate("(3 * 2) + 1");

            Assert.IsNotNull(result);
            Assert.AreEqual(7, result);
        }

        [TestMethod]
        public void EvaluateNativeStringLength()
        {
            object result = this.Evaluate("\"Foo\" length");

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void EvaluateNativeIntegerToString()
        {
            object result = this.Evaluate("123 toString");

            Assert.IsNotNull(result);
            Assert.AreEqual("123", result);
        }

        [TestMethod]
        public void EvaluateNewNativeObject()
        {
            object result = this.Evaluate("System.IO.DirectoryInfo new(\".\")");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(System.IO.DirectoryInfo));
        }

        [TestMethod]
        public void EvaluateNativeEquals()
        {
            Assert.IsTrue((bool) this.Evaluate("2 == 2"));
            Assert.IsTrue((bool) this.Evaluate("\"Foo\" == \"Foo\""));
            Assert.IsFalse((bool)this.Evaluate("2 == 3"));
            Assert.IsFalse((bool)this.Evaluate("\"Foo\" == \"Bar\""));
        }

        [TestMethod]
        public void EvaluateNativeNotEquals()
        {
            Assert.IsFalse((bool)this.Evaluate("2 != 2"));
            Assert.IsFalse((bool)this.Evaluate("\"Foo\" != \"Foo\""));
            Assert.IsTrue((bool)this.Evaluate("2 != 3"));
            Assert.IsTrue((bool)this.Evaluate("\"Foo\" != \"Bar\""));
        }

        [TestMethod]
        public void EvaluateIf()
        {
            Assert.AreEqual(2, this.Evaluate("if(1==1,2,3)"));
            Assert.AreEqual(3, this.Evaluate("if(1==2,2,3)"));
            this.Evaluate("if(1==1, a:= 2)");
            Assert.AreEqual(2, this.machine.GetSlot("a"));
            this.Evaluate("if(1==2, b:= 1, b:=3)");
            Assert.AreEqual(3, this.machine.GetSlot("b"));
        }

        [TestMethod]
        public void EvaluateListClone()
        {
            object result = this.Evaluate("List clone");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ListObject));
        }

        [TestMethod]
        public void EvaluateAppendToList()
        {
            this.Evaluate("aList := List clone");
            Assert.AreEqual(0, this.Evaluate("aList size"));
            this.Evaluate("aList append(10)");
            Assert.AreEqual(1, this.Evaluate("aList size"));
            this.Evaluate("aList append(20)");
            Assert.AreEqual(2, this.Evaluate("aList size"));
            Assert.AreEqual(10, this.Evaluate("aList at(0)"));
            Assert.AreEqual(20, this.Evaluate("aList at(1)"));
        }

        [TestMethod]
        public void EvaluateCloneCloneList()
        {
            this.Evaluate("aList := List clone");
            Assert.AreEqual(0, this.Evaluate("aList size"));
            this.Evaluate("aList append(10)");
            this.Evaluate("aList append(20)");
            this.Evaluate("anotherList := aList clone");
            Assert.AreEqual(2, this.Evaluate("anotherList size"));
            Assert.AreEqual(10, this.Evaluate("anotherList at(0)"));
            Assert.AreEqual(20, this.Evaluate("anotherList at(1)"));

            this.Evaluate("aList append(30)");
            Assert.AreEqual(3, this.Evaluate("aList size"));
            Assert.AreEqual(2, this.Evaluate("anotherList size"));
        }

        [TestMethod]
        public void EvaluateChainedAddList()
        {
            this.Evaluate("aList := List clone");
            Assert.AreEqual(0, this.Evaluate("aList size"));
            this.Evaluate("aList append(10) append(20) append(30)");
            Assert.AreEqual(3, this.Evaluate("aList size"));
            Assert.AreEqual(10, this.Evaluate("aList at(0)"));
            Assert.AreEqual(20, this.Evaluate("aList at(1)"));
            Assert.AreEqual(30, this.Evaluate("aList at(2)"));
        }

        [TestMethod]
        public void EvaluateListMethod()
        {
            object result = this.Evaluate("list(1, 2, 3)");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ListObject));

            ListObject list = (ListObject)result;
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [TestMethod]
        public void EvaluateListAndAtPut()
        {
            object result = this.Evaluate("list(1, 2, 3) atPut(1, 4)");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ListObject));

            ListObject list = (ListObject)result;
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(4, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [TestMethod]
        public void EvaluateListForEach()
        {
            this.Evaluate("a := 0");
            this.Evaluate("list(1, 2, 3) foreach(n,v, a := a + v)");

            object result = this.Evaluate("a");
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void EvaluateListSelect()
        {
            this.Evaluate("a := 0");
            object result = this.Evaluate("list(1, 2, 3) select(x, x != 2)");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ListObject));

            ListObject list = (ListObject)result;

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(3, list[1]);
        }

        [TestMethod]
        public void EvaluateListMap()
        {
            this.Evaluate("a := 0");
            object result = this.Evaluate("list(1, 2, 3) map(i, x, x+i)");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ListObject));

            ListObject list = (ListObject)result;

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(3, list[1]);
            Assert.AreEqual(5, list[2]);
        }

        [TestMethod]
        public void EvaluateBooleansAndNil()
        {
            object result = this.Evaluate("true");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsTrue((bool)result);

            result = this.Evaluate("false");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse((bool)result);

            result = this.Evaluate("nil");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void EvaluateAddTwoVariables()
        {
            this.Evaluate("a := 1");
            this.Evaluate("b := 2");
            Assert.AreEqual(3, this.Evaluate("a + b"));
        }

        private object Evaluate(string text)
        {
            Parser parser = new Parser(text);

            object expression = parser.ParseExpression();

            Assert.IsNull(parser.ParseExpression());

            return machine.Evaluate(expression);
        }

        private void EvaluateMany(string text)
        {
            Parser parser = new Parser(text);

            for (object expression = parser.ParseExpression(); expression != null; expression = parser.ParseExpression())
                this.machine.Evaluate(expression);
        }
    }
}
