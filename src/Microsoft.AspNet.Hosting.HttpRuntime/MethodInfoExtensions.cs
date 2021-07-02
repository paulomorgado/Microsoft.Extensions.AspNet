using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Microsoft.AspNet.Hosting
{
    internal static class MethodInfoExtensions
    {
        // This version of MethodInfo.Invoke removes TargetInvocationExceptions
        public static object InvokeWithoutWrappingExceptions(this MethodInfo methodInfo, object obj, object[] parameters)
        {
            // These are the default arguments passed when methodInfo.Invoke(obj, parameters) are called. We do the same
            // here but specify BindingFlags.DoNotWrapExceptions to avoid getting TAE (TargetInvocationException)
            // methodInfo.Invoke(obj, BindingFlags.Default, binder: null, parameters: parameters, culture: null)

            try
            {
                return methodInfo.Invoke(obj, BindingFlags.Default, binder: null, parameters: parameters, culture: null);
            }
            catch (TargetInvocationException ex)
            {
                Exception exception = ex;
                while (exception is TargetInvocationException)
                {
                    exception = exception.InnerException;
                }

                if (exception is null)
                {
                    throw;
                }

                ExceptionDispatchInfo.Capture(exception).Throw();
            }

#pragma warning disable CS0618 // Type or member is obsolete
            throw new ExecutionEngineException();
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
