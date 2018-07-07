using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using GitHelper.Helpers;

namespace GitHelper.UserControls
{
    public static class AssemblyHelper { 

        public static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
            where TAttribute : Attribute
        {
            try
            {
                return from a in AppDomain.CurrentDomain.GetAssemblies()
                    from t in a.GetTypes()
                    where t.IsDefined(typeof(TAttribute), inherit)
                    select t;
            }
            catch (Exception e)
            {
                if (e is ReflectionTypeLoadException typeLoadException)
                {
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    var errorInfo = string.Join(Environment.NewLine, loaderExceptions.Select(ex => ex.Message));
                    MessageBox.Show(string.Format("Cannot load assembly.\n{0}", errorInfo));
                }
            }

            return new List<Type>();
        }

        public static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit, Assembly assembly)
            where TAttribute : Attribute
        {
            return from t in assembly.GetLoadableTypes()
                where t.IsDefined(typeof(TAttribute), inherit)
                select t;
        }
    }
}