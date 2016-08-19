using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNetWebIDE.SolutionResolve.Model
{
    public class ProjectPropertyModel
    {
        /// <summary>
        /// 工程类型ID
        /// </summary>
        public string ProjectTypeID { get; set; }

        /// <summary>
        /// 工程名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 工程相对路径
        /// </summary>
        public string ProjectRelativePath { get; set; }

        /// <summary>
        /// 工程绝对路径
        /// </summary>
        [JsonIgnore]
        public string ProjectAbsolutePath { get; set; }

        /// <summary>
        /// 工程唯一标识
        /// </summary>
        public string ProjectID { get; set; }
        [JsonIgnore]
        public Project Project { get; set; }

        public void Builder()
        {

            var log = new Log();
            this.Project.Build(log);
        }

        #region 项目引用
        private Dictionary<String, ProjectItem> ProjectReferenceDict = new Dictionary<string, ProjectItem>();
        /// <summary>
        /// 项目引用
        /// </summary>
        public List<String> ProjectReference
        {
            get; private set;
        } = new List<string>();


        public ProjectFolderModel ProjectFiles { get; set; }
        private void AddReferenceToDict(String key, ProjectItem projectItem)
        {
            if (String.IsNullOrEmpty(key))
            {
                return;
            }
            if (key.IndexOf(",") > -1)
            {
                key = key.Substring(0, key.IndexOf(","));
            }
            ProjectReference.Add(key);
            key = key.ToLower();
            if (!ProjectReferenceDict.ContainsKey(key))
            {
                ProjectReferenceDict.Add(key, projectItem);
            }

        }
        #endregion
        /// <summary>
        /// 解析项目文件
        /// </summary>
        public void Resolve()
        {
            #region 引用文件
            var _projectReference = this.Project.Items.Where(x => x.ItemType == "Reference" || x.ItemType == "ProjectReference");
            foreach (ProjectItem projectItem in _projectReference)
            {
                switch (projectItem.ItemType)
                {
                    case "Reference":
                        {
                            AddReferenceToDict(projectItem.EvaluatedInclude, projectItem);
                        }
                        break;
                    case "ProjectReference":
                        {
                            var _referenceProject = projectItem.DirectMetadata.Where(x => x.Name == "Name").FirstOrDefault();
                            AddReferenceToDict(_referenceProject.EvaluatedValue, projectItem);
                        }
                        break;
                }
            }
            #endregion 引用文件

            #region 编译的文件


            #region 先初始化文件夹

            var _ProjectPath = Path.GetDirectoryName(this.ProjectAbsolutePath);
            Dictionary<String, ProjectFolderModel> tempDict = new Dictionary<string, ProjectFolderModel>();
            ProjectFiles = new ProjectFolderModel();
            ProjectFiles.FolderPath = "";
            _projectReference = this.Project.Items.Where(x => x.ItemType == "Folder").OrderBy(x => x.EvaluatedInclude.Split('\\').Length);
            foreach (ProjectItem projectItem in _projectReference)
            {
                var filePath = projectItem.EvaluatedInclude;
                if (tempDict.ContainsKey(filePath))
                {
                    continue;
                }
                var pathSeparator = filePath.Split(Path.DirectorySeparatorChar);
                var current = ProjectFiles;
                var currentPath = Path.GetDirectoryName(this.ProjectAbsolutePath);
                var currentRelativePath = ProjectFiles.FolderPath;
                if (pathSeparator.Length > 1)
                {
                    for (int i = 0; i < pathSeparator.Length - 1; i++)
                    {
                        var folderPath = Path.Combine(currentPath, pathSeparator[i]);
                        if (tempDict.ContainsKey(folderPath))
                        {
                            currentPath = folderPath;
                            current = tempDict[folderPath];
                            continue;
                        }
                        var _folderModel = new ProjectFolderModel();
                        _folderModel.FolderPath = Path.Combine(currentRelativePath, pathSeparator[i]);
                        if (current.Folders == null)
                        {
                            current.Folders = new List<ProjectFolderModel>();
                        }
                        current.Folders.Add(_folderModel);

                        current = _folderModel;
                        currentPath = folderPath;

                        tempDict.Add(folderPath, current);
                    }
                }
                tempDict.Add(filePath, current);
            }
            #endregion 先初始化文件夹

            _projectReference = this.Project.Items.Where(x => x.ItemType == "Compile" || x.ItemType == "Content").OrderBy(x => x.EvaluatedInclude.Split('\\').Length);
            foreach (ProjectItem projectItem in _projectReference)
            {
                var filePath = projectItem.EvaluatedInclude;
                if (tempDict.ContainsKey(filePath))
                {
                    continue;
                }
                var pathSeparator = filePath.Split(Path.DirectorySeparatorChar);
                var current = ProjectFiles;
                var currentPath = Path.GetDirectoryName(this.ProjectAbsolutePath);
                var currentRelativePath = ProjectFiles.FolderPath;
                if (pathSeparator.Length > 1)
                {
                    for (int i = 0; i < pathSeparator.Length - 1; i++)
                    {
                        var folderPath = Path.Combine(currentPath, pathSeparator[i]);
                        if (tempDict.ContainsKey(folderPath))
                        {
                            currentPath = folderPath;
                            current = tempDict[folderPath];
                            currentRelativePath = current.FolderPath;
                            continue;
                        }
                        var _folderModel = new ProjectFolderModel();
                        _folderModel.FolderPath = Path.Combine(currentRelativePath, pathSeparator[i]);
                        if (current.Folders == null)
                        {
                            current.Folders = new List<ProjectFolderModel>();
                        }
                        current.Folders.Add(_folderModel);

                        current = _folderModel;
                        currentPath = folderPath;

                        tempDict.Add(folderPath, current);
                    }
                }
                #region 判断是否未文件
                if (File.Exists(Path.Combine(_ProjectPath, filePath)))
                {
                    ///是文件
                    var _fileModel = new ProjectFileModel();
                    _fileModel.FilePath = filePath;
                    _fileModel.FileName = Path.GetFileName(filePath);
                    if (current.Files == null)
                    {
                        current.Files = new List<ProjectFileModel>();
                    }
                    current.Files.Add(_fileModel);
                }
                if (pathSeparator.Length != 1)
                {
                    tempDict.Add(filePath, current);
                }
                #endregion
            }
            #endregion 编译的文件

            #region 配置文件 
            _projectReference = this.Project.Items.Where(x => x.ItemType == "None").OrderBy(x => x.EvaluatedInclude.Split('\\').Length);
            foreach (ProjectItem projectItem in _projectReference)
            {
                var filePath = projectItem.EvaluatedInclude;
                if (tempDict.ContainsKey(filePath))
                {
                    continue;
                }
                var pathSeparator = filePath.Split(Path.DirectorySeparatorChar);
                var current = ProjectFiles;
                var currentPath = Path.GetDirectoryName(this.ProjectAbsolutePath);
                var currentRelativePath = ProjectFiles.FolderPath;
                if (pathSeparator.Length > 1)
                {
                    for (int i = 0; i < pathSeparator.Length - 1; i++)
                    {
                        var folderPath = Path.Combine(currentPath, pathSeparator[i]);
                        if (tempDict.ContainsKey(folderPath))
                        {
                            currentPath = folderPath;
                            current = tempDict[folderPath];
                            currentRelativePath = current.FolderPath;
                            continue;
                        }
                        var _folderModel = new ProjectFolderModel();
                        _folderModel.FolderPath = Path.Combine(currentRelativePath, pathSeparator[i]);
                        if (current.Folders == null)
                        {
                            current.Folders = new List<ProjectFolderModel>();
                        }
                        current.Folders.Add(_folderModel);

                        current = _folderModel;
                        currentPath = folderPath;

                        tempDict.Add(folderPath, current);
                    }
                }
                #region 判断是否未文件
                if (File.Exists(Path.Combine(_ProjectPath, filePath)))
                {
                    ///是文件
                    var _fileModel = new ProjectFileModel();
                    _fileModel.FilePath = filePath;
                    _fileModel.FileName = Path.GetFileName(filePath);
                    if (current.Files == null)
                    {
                        current.Files = new List<ProjectFileModel>();
                    }
                    current.Files.Add(_fileModel);
                }
                if (pathSeparator.Length != 1)
                {
                    tempDict.Add(filePath, current);
                }
                #endregion
            }
            #endregion
            tempDict = null;
        }
        public override string ToString()
        {
            return String.Join("<br />", ProjectReference);
            //  return base.ToString();
        }

    }

    public class Log : ILogger
    {
        public Log()
        {
            this.Verbosity = new LoggerVerbosity();

        }
        public string Parameters
        {
            get; set;
        }

        public LoggerVerbosity Verbosity
        {
            get; set;
        }

        public void Initialize(IEventSource eventSource)
        {
            eventSource.MessageRaised += EventSource_MessageRaised;
        }

        private void EventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            Trace.WriteLine(e.Message);
        }

        public void Shutdown()
        {
        }
    }

}
