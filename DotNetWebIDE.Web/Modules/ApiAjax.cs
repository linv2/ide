using DotNetWebIDE.SolutionResolve;
using Nancy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DotNetWebIDE.Web.Modules
{
    public class ApiAjax : NancyModule
    {
        string slnFile = ConfigurationManager.AppSettings.Get("slnFile");
        public ApiAjax() : base("api")
        {
            Get["load"] = _ => LoadSolution();
        }
        public dynamic LoadSolution()
        {
            var solution = Solution.LoadSolution(slnFile);
            return Newtonsoft.Json.JsonConvert.SerializeObject(solution);
        }

    }
}