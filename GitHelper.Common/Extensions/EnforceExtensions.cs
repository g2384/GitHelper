using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace GitHelper.Common.Extensions
{
    public static class EnforceExtensions
    {
        [DebuggerNonUserCode, ContractAnnotation("argument:null => halt")]
        public static T ArgumentNullCheck<T>(this T argument, [InvokerParameterName] string argName) where T : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argName);
            }

            return argument;
        }

        [DebuggerNonUserCode, ContractAnnotation("argument:null => halt")]
        public static string ArgumentNullOrEmptyCheck(this string argument, [InvokerParameterName] string argName)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(argName);
            }

            return argument;
        }

        [DebuggerNonUserCode, ContractAnnotation("reference:null => halt")]
        public static T NullCheck<T>(this T reference, [NotNull] string message) where T : class
        {
            if (reference == null)
            {
                throw new NullReferenceException(message);
            }
            return reference;
        }

        [DebuggerNonUserCode, ContractAnnotation("argument:null => halt")]
        public static string ArgumentNullOrWhitespaceCheck(this string argument, [InvokerParameterName]string argName, string exceptionMessage = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw string.IsNullOrWhiteSpace(exceptionMessage) ? new ArgumentNullException(argName) :
                    new ArgumentNullException(argName, exceptionMessage);
            }

            return argument;
        }

        [DebuggerNonUserCode, ContractAnnotation("argument:null => halt")]
        public static string ArgumentNullOrWhitespaceCheck(this string argument, Func<Exception> createException)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw createException();
            }

            return argument;
        }
    }
}
