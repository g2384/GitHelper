using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace DeepBlameLine
{
    public static class EnforceExtensions
    {
        /// <summary>
        /// Arguments the null check.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argument">The argument.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        [DebuggerNonUserCode, ContractAnnotation("argument:null => halt")]
        public static T ArgumentNullCheck<T>(this T argument) where T : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            return argument;
        }
    }
}
