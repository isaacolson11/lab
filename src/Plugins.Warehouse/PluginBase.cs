using Microsoft.Xrm.Sdk;
using System;

namespace Plugins.Warehouse
{
    public interface ILocalPluginContext
    {
        IPluginExecutionContext PluginExecutionContext { get; }
        IOrganizationServiceFactory OrgSvcFactory { get; }
        ITracingService TracingService { get; }
    }

    public sealed class LocalPluginContext : ILocalPluginContext
    {
        public LocalPluginContext(IPluginExecutionContext pluginExecutionContext, IOrganizationServiceFactory orgSvcFactory, ITracingService tracingService)
        {
            PluginExecutionContext = pluginExecutionContext ?? throw new ArgumentNullException(nameof(pluginExecutionContext));
            OrgSvcFactory = orgSvcFactory ?? throw new ArgumentNullException(nameof(orgSvcFactory));
            TracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
        }

        public IPluginExecutionContext PluginExecutionContext { get; }
        public IOrganizationServiceFactory OrgSvcFactory { get; }
        public ITracingService TracingService { get; }
    }

    public abstract class PluginBase : IPlugin
    {
        protected PluginBase(Type pluginType)
        {
            PluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
        }

        protected Type PluginType { get; }

        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (pluginExecutionContext == null)
            {
                throw new InvalidPluginExecutionException("Plugin execution context is not available.");
            }

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            if (serviceFactory == null)
            {
                throw new InvalidPluginExecutionException("Organization service factory is not available.");
            }

            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Tracing service is not available.");
            }

            ExecuteDataversePlugin(new LocalPluginContext(pluginExecutionContext, serviceFactory, tracingService));
        }

        protected abstract void ExecuteDataversePlugin(ILocalPluginContext localPluginContext);
    }
}
