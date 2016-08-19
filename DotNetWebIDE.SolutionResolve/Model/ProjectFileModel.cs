using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNetWebIDE.SolutionResolve.Model
{
    public class ProjectFileModel
    {
        /// <summary>
        /// 相对路径
        /// </summary>
        [JsonIgnore]
        public String FilePath { get; set; }
        public String FileName { get; set; }
        public String _fileType = String.Empty;
        public String FileType
        {
            get
            {
                if (String.IsNullOrEmpty(_fileType))
                {
                    if (String.IsNullOrEmpty(FileName))
                    {
                        return "txt";
                    }
                    else {
                        _fileType= Path.GetExtension(FileName);
                    }
                    if (_fileType.StartsWith("."))
                    {
                        _fileType = _fileType.Substring(1, _fileType.Length - 1);
                    }
                }
                return _fileType;
            }
        }

    }
}
