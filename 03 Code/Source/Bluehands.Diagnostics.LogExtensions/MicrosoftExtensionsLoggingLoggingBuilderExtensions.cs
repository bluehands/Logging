﻿using Bluehands.Diagnostics.LogExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    public static class MicrosoftExtensionsLoggingLoggingBuilderExtensions
    {
        public static ILoggingBuilder AddLogEnhancementWithNLog(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LogProvider>());
            return builder;
        }
    }
}