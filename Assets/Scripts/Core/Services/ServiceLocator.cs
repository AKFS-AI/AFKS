using System;
using System.Collections.Generic;

namespace AFKS.Core.Services
{
    /// <summary>
    /// Simple service locator for runtime singletons.
    /// Stores instances by their concrete or interface type for reuse across scenes.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> typeToInstance = new Dictionary<Type, object>();

        public static void Register<TService>(TService instance, bool overwriteExisting = false) where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var key = typeof(TService);
            if (typeToInstance.ContainsKey(key))
            {
                if (!overwriteExisting)
                {
                    throw new InvalidOperationException($"Service already registered: {key.FullName}");
                }

                typeToInstance[key] = instance;
                return;
            }

            typeToInstance.Add(key, instance);
        }

        public static bool TryGet<TService>(out TService service) where TService : class
        {
            if (typeToInstance.TryGetValue(typeof(TService), out var obj) && obj is TService cast)
            {
                service = cast;
                return true;
            }

            service = null;
            return false;
        }

        public static TService Get<TService>() where TService : class
        {
            if (TryGet<TService>(out var service))
            {
                return service;
            }

            throw new KeyNotFoundException($"Service not found: {typeof(TService).FullName}");
        }

        public static void Unregister<TService>() where TService : class
        {
            typeToInstance.Remove(typeof(TService));
        }

        public static void Clear()
        {
            typeToInstance.Clear();
        }
    }
}


