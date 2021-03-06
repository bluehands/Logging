﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Bluehands.Diagnostics.LogExtensions;
using Bluehands.Repository.Diagnostics.Log.Aspects.Internal;
using Bluehands.Repository.Diagnostics.Log.Aspects.LogFactory;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Reflection;


namespace Bluehands.Repository.Diagnostics.Log.Aspects.Attributes
{
    [Serializable]
    [MulticastAttributeUsage(
        MulticastTargets.Method | MulticastTargets.InstanceConstructor | MulticastTargets.StaticConstructor,
        Inheritance = MulticastInheritance.None)]
    [AttributeUsage(
        AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method |
        AttributeTargets.Constructor, AllowMultiple = true)]
    public class AutoTraceAttribute : OnMethodBoundaryAspect
    {
        private LogFactoryBase m_Factory;
        private readonly string m_Message;
        private static readonly Stopwatch s_StopWatch = Stopwatch.StartNew();
        private string m_Caller;
        
        public AutoTraceAttribute()
        {
            ApplyToStateMachine = true;
        }

        public AutoTraceAttribute(string message) : this()
        {
            m_Message = message;
        }

        public sealed override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(method, aspectInfo);
            m_Factory = LogFactoryBase.Create(method);
        }

        public override bool CompileTimeValidate(MethodBase method)
        {
            if (method.MemberType == MemberTypes.Property)
            {
                return false;
            }

            string methodName = method.Name;
            if (methodName.StartsWith("get_") || methodName.StartsWith("set_"))
            {
                return false;
            }
            if (method.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0)
            {
                return false;
            }
            if (method.IsStatic)
            {
                if (method.DeclaringType != null)
                {
                    Message.Write(MessageLocation.Unknown, SeverityType.Warning, "TA001",
                        $"{method.DeclaringType.Name}:{method.Name} is excluded from tracing, because it is static. Please use [Trace(AttributeExclude = true)] or make the method non static.",
                        method);
                }
                return false;
            }

            var logMember = method.DeclaringType.GetFieldFromType<Bluehands.Diagnostics.LogExtensions.Log>();
            if (logMember != null)
            {
                var logLocationInfo = new LocationInfo(logMember);
                if (logLocationInfo.LocationType.ContainsGenericParameters)
                {
                    if (method.DeclaringType != null)
                    {
                        Message.Write(MessageLocation.Unknown, SeverityType.Warning, "TA002",
                            $"Type {method.DeclaringType.Name} contains a log from a type with generic parameters. This log cannot be reused, and the method is excluded from tracing.",
                            method);
                    }
                    return false;
                }
            }
            if (method.DeclaringType != null && method.DeclaringType.ContainsGenericParameters)
            {
                Message.Write(MessageLocation.Unknown, SeverityType.Warning, "TA003",
                    $"Type {method.DeclaringType.Name} contains a log from a type with generic parameters. This log cannot be reused, , and the method is excluded from tracing.",
                    method);
                return false;
            }

            return base.CompileTimeValidate(method);
        }

        class ExecutionTag
        {
            public TimeSpan Begin { get; set; }

            public IDisposable TraceStackHandle { get; set; }
        }

        public sealed override void OnEntry(MethodExecutionArgs args)
        {
            try
            {
                m_Caller = args.Method.Name;
                var log = GetLog(args.Instance, args.Arguments);
                if (log != null && log.IsTraceEnabled)
                {
                    var execTag = new ExecutionTag
                    {
                        Begin = s_StopWatch.Elapsed
                    };
                    args.MethodExecutionTag = execTag;
                    log.Trace(LogFormatters.TraceEnter(m_Message), m_Caller);
                    execTag.TraceStackHandle = TraceStack.Push(LogFormatters.ContextPart(m_Caller));
                }
            }
            catch (Exception ex)
            {
                var log = new Log<AutoTraceAttribute>();
                log.Error("Unexpected error. Please contact your Administrator", ex);
            }
        }

        public sealed override void OnExit(MethodExecutionArgs args)
        {
            try
            {
                var log = GetLog(args.Instance, args.Arguments);
                if (log != null && log.IsTraceEnabled)
                {
                    var tag = args.MethodExecutionTag;
                    if (tag != null)
                    {
                        var execTag = (ExecutionTag) tag;
                        var end = s_StopWatch.Elapsed - execTag.Begin;
                        execTag.TraceStackHandle.Dispose();
                        log.Trace(LogFormatters.TraceLeave(m_Message, end.TotalMilliseconds), m_Caller);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new Log<AutoTraceAttribute>();
                log.Error("Unexpected error. Please contact your Administrator", ex);
            }
        }

        Bluehands.Diagnostics.LogExtensions.Log GetLog(object instance, Arguments args)
        {
            return m_Factory.GetLog(instance);
        }
    }
}