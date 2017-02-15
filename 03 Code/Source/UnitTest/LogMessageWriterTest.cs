﻿using System;
using System.IO;
using Bluehands.Repository.Diagnostics.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogLevel = Bluehands.Repository.Diagnostics.Log.LogLevel;

namespace UnitTest
{
    [TestClass]
    public class LogMessageWriterTest
    {
        private readonly LogMessageWriter m_Sut = new LogMessageWriter(typeof(LogMessageWriterTest));
        
        [TestMethod]
        public void PossitivTestWriteLogEntryWithoutException()
        {
            //Arange
            var autoTrace = new AutoTrace(m_Sut, "");
            var indent = autoTrace.Indent;
            File.Delete("logTest.txt");
            m_Sut.WriteLogEntry(LogLevel.Fatal, "logg emol ebbes anres", indent);


            //Act
            var completeLogText = File.ReadAllText("logTest.txt");

            string[] stringSeparator = { "," };
            var splitedLogTextArray = completeLogText.Split(stringSeparator, StringSplitOptions.None);


            //Assert
            Assert.AreEqual(LogLevel.Fatal.ToString(), splitedLogTextArray[0]);
            Assert.AreEqual("UnitTest.LogMessageWriterTest", splitedLogTextArray[1]);
            Assert.AreEqual("LogMessageWriterTest", splitedLogTextArray[2]);
            Assert.AreEqual("PossitivTestWriteLogEntryWithoutException", splitedLogTextArray[3]);
            Assert.AreEqual("", splitedLogTextArray[4]);
            Assert.AreEqual(" ", splitedLogTextArray[5]);
            Assert.AreEqual("logg emol ebbes anres", splitedLogTextArray[6]);
            Assert.AreEqual("UnitTest.LogMessageWriterTest", splitedLogTextArray[7]);
        }

        [TestMethod]
        public void PossitivTestWriteLogEntryWithException()
        {
            //Arange
            var autoTrace = new AutoTrace(m_Sut, "AutoTrace von PossitivTestWriteLogEntryWithException");
            var indent = autoTrace.Indent;

            File.Delete("logTest.txt");
            var exception = new NotImplementedException();
            m_Sut.WriteLogEntry(LogLevel.Fatal, "logg emol ebbes anres", indent, exception);

            //Act
            var completLogText = File.ReadAllText("logTest.txt");

            string[] stringSeparator = { "," };
            var splitedLogTextArray = completLogText.Split(stringSeparator, StringSplitOptions.None);

            foreach (var textPart in splitedLogTextArray)
            {
                Console.WriteLine(textPart);
            }

            //Assert
            Assert.AreEqual(LogLevel.Fatal.ToString(), splitedLogTextArray[0]);
            Assert.AreEqual("UnitTest.LogMessageWriterTest", splitedLogTextArray[1]);
            Assert.AreEqual("LogMessageWriterTest", splitedLogTextArray[2]);
            Assert.AreEqual("PossitivTestWriteLogEntryWithException", splitedLogTextArray[3]);
            Assert.AreEqual("Die Methode oder der Vorgang ist nicht implementiert.", splitedLogTextArray[4]);
            Assert.AreEqual(" ", splitedLogTextArray[5]);
            Assert.AreEqual("logg emol ebbes anres", splitedLogTextArray[6]);
            Assert.AreEqual("UnitTest.LogMessageWriterTest", splitedLogTextArray[7]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckCtor()
        {
            //Arrange
            var logMessageWriter = new LogMessageWriter(null);
        }
    }
}
