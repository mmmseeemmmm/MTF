using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace MTFCore.Services
{
    abstract class ServiceWithCallback<TCallback> : ServiceBase
    {
        private readonly List<TCallback> Callbacks = new List<TCallback>();

        protected void RememberClient()
        {
            var callback = OperationContext.Current.GetCallbackChannel<TCallback>();
            if (!Callbacks.Contains(callback))
            {
                Callbacks.Add(callback);
            }
        }

        protected void CallbackInvoke(Action<TCallback> action)
        {
            List<TCallback> callbacksToDelete = new List<TCallback>();
            foreach (var callback in Callbacks)
            {
                try
                {
                    action(callback);
                }
                catch
                {
                    callbacksToDelete.Add(callback);
                }
            }

            foreach (var callbackToDelete in callbacksToDelete)
            {
                Callbacks.Remove(callbackToDelete);
            }
        }
    }
}
