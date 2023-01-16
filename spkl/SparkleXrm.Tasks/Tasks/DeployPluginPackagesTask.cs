using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using SparkleXrm.Tasks.Config;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Linq;

namespace SparkleXrm.Tasks
{
    public class DeployPluginPackagesTask : BaseTask
    {
        public bool ExcludePluginSteps { get; set; }

        public DeployPluginPackagesTask(IOrganizationService service, ITrace trace) : base(service, trace)
        {
        }

        protected override void ExecuteInternal(string folder, OrganizationServiceContext ctx)
        {
            _trace.WriteLine("Searching for plugin packages config in '{0}'", folder);
            var configs = ServiceLocator.ConfigFileFactory.FindConfig(folder);

            foreach (var config in configs)
            {
                _trace.WriteLine("Using Config '{0}'", config.filePath);
                DeployPackages(ctx, config);
            }
            _trace.WriteLine("Processed {0} config(s)", configs.Count);
        }

        private void DeployPackages(OrganizationServiceContext ctx, ConfigFile config)
        {
            var packages = config.GetPackagesConfig(this.Profile);
            foreach (var package in packages)
            {
                List<string> nugets = config.GetNugets(package);

                var pluginRegistration = new PluginRegistraton(_service, ctx, _trace);

                if (!string.IsNullOrEmpty(package.solution))
                {
                    pluginRegistration.SolutionUniqueName = package.solution;
                }

                if (string.IsNullOrEmpty(package.packageprefix))
                {
                    throw new InvalidOperationException("Package prefix is not set"); 
                }

                foreach (var nugetFilePath in nugets)
                {
                    try
                    {
                        var excludePluginSteps = this.ExcludePluginSteps || package.excludePluginSteps;
                        pluginRegistration.RegisterPackage(nugetFilePath, package.packageprefix, excludePluginSteps);
                    }

                    catch (ReflectionTypeLoadException ex)
                    {
                        throw new Exception(ex.LoaderExceptions.First().Message);
                    }
                }
            }
        }
    }
}
