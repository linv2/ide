using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebIDE.SolutionResolve
{
    public class RegexExpressionConst
    {
        /// <summary>
        /// GUID的正则表达式,格式 FAE04EC0-301F-11D3-BF4B-00C04F79EFBC
        /// </summary>
        public const string GuidExp = @"\w{8}-(\w{4}-){3}\w{12}";

        /// <summary>
        /// 匹配[工程文件命名]
        /// </summary>
        public const string ProjectExt = @"[a-z][\s\.\-\w]+";

        /// <summary>
        /// 匹配[相对路径]
        /// </summary>
        public const string RelativePathExt = @"(\\?([a-z][\s\.\-\w]+))+";

    }
}
