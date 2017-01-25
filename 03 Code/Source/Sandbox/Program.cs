﻿using System;
using Bluehands.Repository.Diagnostics.Log;

namespace Sandbox
{
    public class Program
    {
        private static readonly Log log = new Log(typeof(Program));

        public static void Main(string[] args)
        {
            Test();
        }

        private static void Test()
        {
            //log.Fatal("Log von Sandbox.Core.Test");

            var exeption = new NotImplementedException();

            log.Fatal(exeption, "Log von Sandbox.Core.Test");
        }

    }
}
