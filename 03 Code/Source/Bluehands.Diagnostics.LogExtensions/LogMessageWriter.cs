﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;

namespace Bluehands.Diagnostics.LogExtensions
{
    internal class LogMessageWriter : LogMessageWriterBase
    {
        private readonly Logger m_NLogLog;
        private readonly NLogMessageBuilder m_NLogMessageBuilder;

        public LogMessageWriter(Type messageCreator) : base(messageCreator)
        {
            try
            {
                m_NLogMessageBuilder = new NLogMessageBuilder(m_MessageCreator.FullName);
                m_NLogLog = LogManager.GetLogger(Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public override bool IsFatalEnabled => m_NLogLog.IsFatalEnabled;
        public override bool IsErrorEnabled => m_NLogLog.IsErrorEnabled;
        public override bool IsWarningEnabled => m_NLogLog.IsWarnEnabled;
        public override bool IsInfoEnabled => m_NLogLog.IsInfoEnabled;
        public override bool IsTraceEnabled => m_NLogLog.IsTraceEnabled;
        public override bool IsDebugEnabled => m_NLogLog.IsDebugEnabled;

        public override void WriteLogEntry(LogLevel logLevel, Func<string> messageFactory, string callerMethodName = null, Exception ex = null, params KeyValuePair<string, string>[] customProperties)
        {
            try
            {
                if (string.IsNullOrEmpty(callerMethodName)) { throw new ArgumentNullException(nameof(callerMethodName)); }
                if (messageFactory == null) { throw new ArgumentNullException(nameof(messageFactory)); }

                if (IsLogLevelEnabled(logLevel))
                {
                    var logEventInfo = GetLogEventInfo(logLevel, callerMethodName, messageFactory, ex, customProperties);
                    m_NLogLog.Log(logEventInfo);
                }
            }
            catch (Exception exx)
            {
                Debug.WriteLine(exx);
            }
        }

        protected override bool IsLogLevelEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Fatal:
                    return m_NLogLog.IsFatalEnabled;
                case LogLevel.Error:
                    return m_NLogLog.IsErrorEnabled;
                case LogLevel.Warning:
                    return m_NLogLog.IsWarnEnabled;
                case LogLevel.Info:
                    return m_NLogLog.IsInfoEnabled;
                case LogLevel.Debug:
                    return m_NLogLog.IsDebugEnabled;
                case LogLevel.Trace:
                    return m_NLogLog.IsTraceEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "The requested LogLevel is not supported by NLog!");
            }
        }

        private LogEventInfo GetLogEventInfo(LogLevel logLevel, string callerMethodName, Func<string> messageFactory, Exception ex, KeyValuePair<string, string>[] customProperties)
        {
            var callerInfo = new CallerInfo(m_MessageCreator.FullName, m_MessageCreatorFriendlyName, callerMethodName, TraceStack.CurrentStack(LogFormatters.ContextPartSeparator), TrackCorrelation.Correlation);
            var logEventInfo = m_NLogMessageBuilder.BuildLogEventInfo(logLevel, messageFactory(), ex, callerInfo, TraceStack.Indent, customProperties);
            return logEventInfo;
        }
    }
}