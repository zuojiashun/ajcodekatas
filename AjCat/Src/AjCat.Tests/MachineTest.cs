﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using AjCat;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AjCat.Tests
{
    [TestClass]
    public class MachineTest
    {
        [TestMethod]
        public void CreateWithoutParameters()
        {
            Machine machine = new Machine();

            Assert.IsNotNull(machine);
            Assert.AreEqual(0, machine.StackCount);
        }

        [TestMethod]
        public void PushAndPopAValue()
        {
            Machine machine = new Machine();

            machine.Push("foo");

            Assert.AreEqual(1, machine.StackCount);
            Assert.AreEqual("foo", machine.Pop());
            Assert.AreEqual(0, machine.StackCount);
        }

        [TestMethod]
        public void PushPeekAndPopAValue()
        {
            Machine machine = new Machine();

            machine.Push("foo");

            Assert.AreEqual("foo", machine.Top());
            Assert.AreEqual("foo", machine.Pop());
            Assert.AreEqual(0, machine.StackCount);
        }

        [TestMethod]
        public void PushAndPopTwoValues()
        {
            Machine machine = new Machine();

            machine.Push("foo");
            machine.Push("bar");

            Assert.AreEqual(2, machine.StackCount);
            Assert.AreEqual("bar", machine.Pop());
            Assert.AreEqual("foo", machine.Pop());
            Assert.AreEqual(0, machine.StackCount);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RaiseIfPopAndStackIsEmpty()
        {
            Machine machine = new Machine();

            machine.Pop();
        }

        [TestMethod]
        public void GetStackContent()
        {
            Machine machine = new Machine();

            machine.Push(1);
            machine.Push(2);
            machine.Push("foo");
            machine.Push("bar");

            object[] content = machine.StackContent;

            Assert.IsNotNull(content);
            Assert.AreEqual(4, content.Length);
            Assert.AreEqual(1, content[3]);
            Assert.AreEqual(2, content[2]);
            Assert.AreEqual("foo", content[1]);
            Assert.AreEqual("bar", content[0]);
        }
    }
}
