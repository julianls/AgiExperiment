﻿using System.Reflection;
using AgiExperiment.AI.Cortex.Pipeline.Interceptors;

namespace AgiExperiment.AI.Cortex.Settings
{
    public class InterceptorRepository
    {
        private IServiceProvider _serviceProvider;

        public InterceptorRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IInterceptor> LoadAll()
        {
            var internalInterceptors = LoadInternal();


            var externalInterceptors = LoadExternal();
            return internalInterceptors.Concat(externalInterceptors).OrderBy(o => o.Name);
        }

        public IEnumerable<IInterceptor> LoadInternal()
        {
            Assembly assembly = Assembly.GetExecutingAssembly(); 

            var types = assembly.GetTypes().Where(type => type.GetInterface("IInterceptor") != null && type.Name != "InterceptorBase");

            List<IInterceptor> interceptors = new List<IInterceptor>();

            foreach (var type in types)
            {
                interceptors.Add((IInterceptor)Activator.CreateInstance(type, _serviceProvider));
            }
            return interceptors;
        }

        public IEnumerable<IInterceptor> LoadExternal()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

            if (!Directory.Exists(path))
                return [];

            var files = Directory.GetFiles(path, "*.dll");

            List<IInterceptor> interceptors = new List<IInterceptor>();
            foreach (var file in files)
            {
                Assembly assembly = Assembly.LoadFile(file);

                var types = assembly.GetTypes().Where(type => type.GetInterface("IInterceptor") != null && type.Name != "InterceptorBase");

                foreach (var type in types)
                {
                    interceptors.Add((IInterceptor)Activator.CreateInstance(type, _serviceProvider));
                }
            }
            return interceptors;
        }


    }
}
