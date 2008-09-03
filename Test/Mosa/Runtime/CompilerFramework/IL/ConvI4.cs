﻿using System;
using System.Collections.Generic;
using System.Text;
using MbUnit.Framework;
using System.Reflection.Emit;

namespace Test.Mosa.Runtime.CompilerFramework.IL
{
    [TestFixture]
    public class ConvI4 : MosaCompilerTestRunner
    {
        delegate bool Native_ConvI4_I1(int expect, sbyte a);
        [Column(0, 1, 2, sbyte.MinValue, sbyte.MaxValue)]
        [Test, Author("alyman", "mail.alex.lyman@gmail.com")]
        public void ConvI4_I1(sbyte a)
        {
            CodeSource = "static class Test { static bool ConvI4_I1(int expect, sbyte a) { return expect == ((int)a); } }";
            Assert.IsTrue((bool)Run<Native_ConvI4_I1>("", "Test", "ConvI4_I1", ((int)a), a));
        }

        delegate bool Native_ConvI4_I2(int expect, short a);
        [Column(0, 1, 2, short.MinValue, short.MaxValue)]
        [Test, Author("alyman", "mail.alex.lyman@gmail.com")]
        public void ConvI4_I2(short a)
        {
            CodeSource = "static class Test { static bool ConvI4_I2(int expect, short a) { return expect == ((int)a); } }";
            Assert.IsTrue((bool)Run<Native_ConvI4_I2>("", "Test", "ConvI4_I2", ((int)a), a));
        }

        delegate bool Native_ConvI4_I4(int expect, int a);
        [Column(0, 1, 2, int.MinValue, int.MaxValue)]
        [Test, Author("alyman", "mail.alex.lyman@gmail.com")]
        public void ConvI4_I4(int a)
        {
            CodeSource = "static class Test { static bool ConvI4_I4(int expect, int a) { return expect == ((int)a); } }";
            Assert.IsTrue((bool)Run<Native_ConvI4_I4>("", "Test", "ConvI4_I4", ((int)a), a));
        }

        delegate bool Native_ConvI4_I8(int expect, long a);
        [Column(0, 1, 2, int.MinValue, int.MaxValue)]
        [Test, Author("alyman", "mail.alex.lyman@gmail.com")]
        public void ConvI4_I8(long a)
        {
            CodeSource = "static class Test { static bool ConvI4_I8(int expect, long a) { return expect == ((int)a); } }";
            Assert.IsTrue((bool)Run<Native_ConvI4_I4>("", "Test", "ConvI4_I8", ((int)a), a));
        }

        delegate bool Native_ConvI4_R4(int expect, float a);
        [Column(0, 1, 2, int.MinValue, int.MaxValue)]
        [Test, Author("alyman", "mail.alex.lyman@gmail.com")]
        public void ConvI4_R4(float a)
        {
            CodeSource = "static class Test { static bool ConvI1_R4(int expect, float a) { return expect == ((int)a); } }";
            Assert.IsTrue((bool)Run<Native_ConvI4_I4>("", "Test", "ConvI1_R4", ((int)a), a));
        }

        delegate bool Native_ConvI4_R8(int expect, double a);
        [Column(0, 1, 2, int.MinValue, int.MaxValue)]
        [Test, Author("alyman", "mail.alex.lyman@gmail.com")]
        public void ConvI4_R8(double a)
        {
            CodeSource = "static class Test { static bool ConvI1_R8(int expect, double a) { return expect == ((int)a); } }";
            Assert.IsTrue((bool)Run<Native_ConvI4_R8>("", "Test", "ConvI1_R8", ((int)a), a));
        }
    }
}