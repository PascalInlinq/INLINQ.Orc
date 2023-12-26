using INLINQ.Orc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INLINQ.Orc.Helpers
{
    public static class FastActivator<T> where T : new()
    {
        //public static T CreateInstance<T>() where T : new()
        //{
        //    return FastActivatorImpl<T>.Create();
        //}

        //private static class FastActivatorImpl<T> where T : new()
        //{
        //    public static readonly Func<T> Create =
        //        DynamicModuleLambdaCompiler.GenerateFactory<T>();
        //}

        /// <summary>
        /// Extremely fast generic factory method that returns an instance
        /// of the type <typeparam name="T"/>.
        /// </summary>
        public static readonly Func<T> Create =
            DynamicModuleLambdaCompiler.GenerateFactory<T>();
    }

    namespace System
    {
        /// <summary>
        /// Dirty hack that allows using a fast implementation
        /// of the activator.
        /// </summary>
        public static class Activator
        {
            public static T CreateInstance<T>() where T : new()
            {
#if DEBUG
        Console.WriteLine("Fast Activator was called");
#endif
                return ActivatorImpl<T>.Create();
            }

            private static class ActivatorImpl<T> where T : new()
            {
                public static readonly Func<T> Create =
                    DynamicModuleLambdaCompiler.GenerateFactory<T>();
            }
        }
    }
}
