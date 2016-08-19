using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetWebIDE.Web
{

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            pipelines.OnError += ErrorHandler;
            StaticConfiguration.DisableErrorTraces = false;
        }


        private Response ErrorHandler(NancyContext ctx, Exception ex)
        {
            return null;
        }
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("easyui"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("JavaScript"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("images"));
        }
    }
}