using DotNetWebIDE.SolutionResolve.Model;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace DotNetWebIDE.SolutionResolve
{
    public class Solution
    {
        private ProjectCollection _projectCollection = new ProjectCollection();
        public String FolderPath { get; private set; }
        public String SolutionName { get; private set; }
        public List<ProjectPropertyModel> ProjectProperties { get; set; } = new List<ProjectPropertyModel>();
        private Solution(String slnFolderPath, String slnSolutionName)
        {
            this.FolderPath = slnFolderPath;
            this.SolutionName = slnSolutionName;

        }
        /// <summary>
        /// 重新加载项目
        /// </summary>
        private void ReLoad()
        {
            try
            {
                _projectCollection.UnloadAllProjects();
                var fileContent = File.ReadAllText(Path.Combine(FolderPath, SolutionName));
                var projectReg = new Regex(projectRegexExp, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var projectContexts = projectReg.Matches(fileContent);
                foreach (var projectContext in projectContexts)
                {
                    var projectPropertyModel = new ProjectPropertyModel();
                    var projectContextArr = projectContext.ToString().Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    projectPropertyModel.ProjectTypeID = new Regex(RegexExpressionConst.GuidExp).Match(projectContextArr[0]).ToString().Trim();
                    var projectValueArr = projectContextArr[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    projectPropertyModel.ProjectName = projectValueArr[0].Replace("\"", "").Trim();
                    projectPropertyModel.ProjectRelativePath = projectValueArr[1].Replace("\"", "").Trim();
                    projectPropertyModel.ProjectID = projectValueArr[2].Replace("\"{", "").Replace("}\"", "").Trim();
                    projectPropertyModel.ProjectAbsolutePath = Path.Combine(FolderPath, projectPropertyModel.ProjectRelativePath.Replace("\\", "/"));
                    if (!File.Exists(projectPropertyModel.ProjectAbsolutePath))
                    {
                        continue;
                    }
                    try
                    {
                        var project = new Project(projectPropertyModel.ProjectAbsolutePath);
                        projectPropertyModel.Project = project;
                        projectPropertyModel.Resolve();

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    ProjectProperties.Add(projectPropertyModel);
                }
                ProjectProperties = ProjectProperties.OrderBy(x => x.ProjectName).ToList();
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        static readonly String projectRegexExp = string.Format("Project\\(\"{{{0}}}\"\\)\\s*=\\s*\"{1}\"\\s*,\\s*\"{2}\"\\s*,\\s*\"{{{3}}}\"",
        RegexExpressionConst.GuidExp, RegexExpressionConst.ProjectExt, RegexExpressionConst.RelativePathExt, RegexExpressionConst.GuidExp);
        public static Solution LoadSolution(String slnPath)
        {
            var fileInfo = new FileInfo(slnPath);
            if (!fileInfo.Exists)
            {
                return null;
            }

            var _solution = new Solution(fileInfo.DirectoryName, fileInfo.Name);
            try
            {
                _solution.ReLoad();
                return _solution;
            }
            catch (Exception ex)
            {
                throw ex;
                return null;
            }
        }
    }
}
