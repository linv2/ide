using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNetWebIDE.SolutionResolve.Model
{
    public class ProjectFolderModel
    {
        /// <summary>
        /// 相对路径
        /// </summary>
        public String FolderPath { get; set; }

        private String _folderName = String.Empty;
        /// <summary>
        /// 文件夹名字
        /// </summary>
        public String FolderName
        {
            get
            {

                if (!String.IsNullOrEmpty(FolderPath) && String.IsNullOrEmpty(_folderName))
                {
                    _folderName = new DirectoryInfo(FolderPath).Name;
                }
                return _folderName;
            }
        }
        public List<ProjectFileModel> Files { get; set; }
        public List<ProjectFolderModel> Folders { get; set; }

    }
}
