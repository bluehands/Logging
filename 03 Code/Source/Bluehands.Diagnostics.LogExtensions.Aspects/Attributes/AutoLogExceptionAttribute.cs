﻿using System;
using System.Diagnostics;
using System.Reflection;
using Bluehands.Diagnostics.LogExtensions;
using Bluehands.Repository.Diagnostics.Log.Aspects.LogFactory;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace Bluehands.Repository.Diagnostics.Log.Aspects.Attributes
{
	[Serializable]
	[DebuggerNonUserCode]
	[MulticastAttributeUsage(MulticastTargets.Method | MulticastTargets.InstanceConstructor | MulticastTargets.StaticConstructor, Inheritance = MulticastInheritance.None)]
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = true)]
	public class AutoLogExceptionAttribute : OnExceptionAspect
	{
		private LogFactoryBase m_Factory;
	    private string m_CallerName;

	    public sealed override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
		{
			base.CompileTimeInitialize(method, aspectInfo);
			m_Factory = LogFactoryBase.Create(method);
		    m_CallerName = method.Name;
		}

		public sealed override void OnException(MethodExecutionArgs args)
		{
			try
			{
				var log = GetLog(args.Instance, args.Arguments);
				if (log == null)
				{
					throw new ArgumentNullException(nameof(log));
				}
				log.Error("Unhandled exception.", args.Exception, m_CallerName);
			}
			catch (Exception ex)
			{
				var log = new Log<AutoLogExceptionAttribute>();
				log.Error("Unexpected error. Please contact your Administrator", ex);
			}
			args.FlowBehavior = FlowBehavior.RethrowException;
		}

		Bluehands.Diagnostics.LogExtensions.Log GetLog(object instance, Arguments args)
		{
		    return m_Factory.GetLog(instance);
		}
	}
}
