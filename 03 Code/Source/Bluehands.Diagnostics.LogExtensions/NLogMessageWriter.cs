﻿using System;
using System.Diagnostics;
using NLog;

namespace Bluehands.Diagnostics.LogExtensions
{
    internal class NLogMessageWriter : LogMessageWriterBase
    {
        private readonly Logger m_NLogLog;


        public NLogMessageWriter(string messageCreatorFullName) : base(messageCreatorFullName)
        {
            try
            {
                m_NLogLog = LogManager.GetLogger(messageCreatorFullName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public NLogMessageWriter(Type messageCreator) : base(messageCreator)
        {
            try
            {
                m_NLogLog = LogManager.GetLogger(messageCreator.FullName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public override bool IsFatalEnabled
        {
            get
            {
                try
                {
                    return m_NLogLog.IsFatalEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override bool IsErrorEnabled
        {
            get
            {
                try
                {
                    return m_NLogLog.IsErrorEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override bool IsWarningEnabled
        {
            get
            {
                try
                {
                    return m_NLogLog.IsWarnEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override bool IsInfoEnabled
        {
            get
            {
                try
                {
                    return m_NLogLog.IsInfoEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override bool IsTraceEnabled
        {
            get
            {
                try
                {
                    return m_NLogLog.IsTraceEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override bool IsDebugEnabled
        {
            get
            {
                try
                {
                    return m_NLogLog.IsDebugEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override bool IsDisabled
        {
            get
            {
                try
                {
                    return m_NLogLog.IsEnabled(NLog.LogLevel.Off);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override void WriteLogEntry(LogEventInfo logEventInfo, Exception ex)
        {
            try
            {
                var nlogEventInfo = logEventInfo.ToNLog(ex);
                m_NLogLog.Log(nlogEventInfo);
            }
            catch (Exception exx)
            {
                Debug.WriteLine(exx);
            }
        }

    }

    internal static class LogEventInfoConversionExtension
    {
        public static NLog.LogEventInfo ToNLog(this LogEventInfo logEventInfo, Exception ex)
        {
            var nlogEventInfo = new NLog.LogEventInfo
            {
                Message = logEventInfo.MessageFactory(),
                Level = GetNLogLevel(logEventInfo.Level),
                LoggerName = logEventInfo.TypeName,
                Exception = ex
            };
            nlogEventInfo.Properties["Type"] = logEventInfo.TypeName;
            nlogEventInfo.Properties["Class"] = logEventInfo.ClassName;
            nlogEventInfo.Properties["Method"] = logEventInfo.MethodName;
            nlogEventInfo.Properties["CallContext"] = logEventInfo.CallContext;
            nlogEventInfo.Properties["Correlation"] = logEventInfo.Correlation;

            foreach (var customProperty in logEventInfo.CustomProperties)
            {
                nlogEventInfo.Properties[customProperty.Key] = customProperty.Value;
            }

            return nlogEventInfo;
        }

        private static NLog.LogLevel GetNLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Fatal:
                    return NLog.LogLevel.Fatal;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                case LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case LogLevel.Info:
                    return NLog.LogLevel.Info;
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                default:
                    return NLog.LogLevel.Trace;
            }
        }
    }
}