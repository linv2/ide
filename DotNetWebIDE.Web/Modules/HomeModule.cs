using DotNetWebIDE.SolutionResolve;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebIDE.Web.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = _ => Index();
            Get["/Setting"] = _ => Setting();
        }
        public dynamic Index()
        {
            return View["Index"];
        }
        public dynamic Setting()
        {
            return View["Setting"];
        }
    }
}

