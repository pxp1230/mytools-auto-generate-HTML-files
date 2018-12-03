using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Program
    {
        static string pandoc_param_raw;
        static Process p;
        static void Md2Html(FileInfo file)
        {
            string raw_fullName = file.FullName.Substring(0, file.FullName.LastIndexOf("."));
            string pandoc_param = string.Format(pandoc_param_raw, raw_fullName);
            if (p == null)
            {
                p = new Process();
                p.StartInfo.FileName = "cmd";//必须用cmd启动pandoc
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            }
            p.StartInfo.Arguments = "/c pandoc " + pandoc_param;
            p.Start();
            p.WaitForExit();
        }

        static void Main(string[] args)
        {
            //示例：
#if DEBUG
            args = new string[1];
            args[0] = @"G:\coding.net\GhostCTO.coding.me\Project Management";
#endif
            if (args != null && args.Length == 1)
            {
                //header.txt必须是utf-8编码
                pandoc_param_raw = "\"{0}.md\" -s -f gfm+hard_line_breaks -H \"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "header.txt") + "\" -t html5 -o \"{0}.html\"";
                string path = args[0];
                DirectoryInfo rootDirectory = new DirectoryInfo(path);
                FileInfo curFile = new FileInfo(path);
                if (curFile.Exists)
                {
                    Md2Html(curFile);
                }
                else
                {
                    if (!FindGitFolder(rootDirectory))
                    {
                        gitFolderPath = rootDirectory.FullName;
                        gitFolderName = rootDirectory.Name;
                        Directory.CreateDirectory(gitFolderPath + "\\_git");
                        File.Create(gitFolderPath + "\\_git\\该文件夹用于标记笔记根目录，请勿删除！");
                    }
                    AutoMd2Html(rootDirectory);
                    GenIndexFile(rootDirectory);
                }
            }
            else
            {
                Console.WriteLine("失败：参数不正确，请输入需要处理的文件或文件夹");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
            Exit();
        }

        static void Exit()
        {
            if (p != null)
                p.Close();
            Process.GetCurrentProcess().Kill();
        }


        static string gitFolderPath;
        static string gitFolderName;
        static bool FindGitFolder(DirectoryInfo curDirectory)
        {
            if (curDirectory == null || !curDirectory.Exists)
                return false;

            DirectoryInfo[] dis = curDirectory.GetDirectories(".git", SearchOption.TopDirectoryOnly);
            DirectoryInfo[] dis2 = curDirectory.GetDirectories("_git", SearchOption.TopDirectoryOnly);
            if (dis.Length > 0 || dis2.Length > 0)
            {
                gitFolderPath = curDirectory.FullName;
                gitFolderName = curDirectory.Name;
                return true;
            }
            else
            {
                return FindGitFolder(curDirectory.Parent);
            }
        }
        /// <summary>
        /// 获取根目录相对于当前目录的路径，比如“../../”、“/”
        /// </summary>
        /// <param name="curDirectory">当前目录</param>
        /// <returns></returns>
        static string GetJumpToGitFolderPath(DirectoryInfo curDirectory)
        {
            string cut = curDirectory.FullName.Substring(gitFolderPath.Length);
            int count = Regex.Matches(cut, @"\\").Count;//使用逐字字符串符号@
            string ret = "";
            for (int i = 0; i < count; i++)
            {
                ret += "../";
            }
            return ret;
        }

        static List<DirectoryInfo> dirShouldGenIndexFile;
        static void GenIndexFile(DirectoryInfo rootDirectory)
        {
            if (dirShouldGenIndexFile == null)
            {
                dirShouldGenIndexFile = new List<DirectoryInfo>();
                if (TestDirIfShouldGenIndexFile(rootDirectory))
                {
                    foreach (DirectoryInfo path in dirShouldGenIndexFile)
                    {
                        //开始生产index.html
                        List<FileInfo> files = new List<FileInfo>(path.GetFiles("*.html"));
                        List<DirectoryInfo> folders = new List<DirectoryInfo>();
                        DirectoryInfo[] __folders = path.GetDirectories();
                        foreach (DirectoryInfo item in __folders)
                        {
                            if (dirShouldGenIndexFile.Exists((d) => { return d.FullName == item.FullName; }))
                            {
                                folders.Add(item);
                            }
                        }
                        files.Sort(_CompareFileInfoByDateTime);
                        FileInfo indexFile = new FileInfo(Path.Combine(path.FullName, "index.html"));
                        List<_TableEle> oldTableEles = new List<_TableEle>();
                        string old_right_path = "";
                        List<_TableEle> newTableEles = new List<_TableEle>();
                        if (indexFile.Exists)
                        {
                            StreamReader sr = new StreamReader(indexFile.FullName, Encoding.UTF8);
                            string str = sr.ReadToEnd();
                            sr.Close();
                            int from = str.IndexOf("<body>"), to = str.IndexOf("</body>") + 7;
                            XmlDocument xmlDoc = new XmlDocument();
                            try
                            {
                                xmlDoc.LoadXml(str.Substring(from, to - from));
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            XmlNodeList x = xmlDoc.DocumentElement.SelectNodes("//tr");
                            foreach (XmlNode y in x)
                            {
                                string str1 = y.ChildNodes[0].InnerText;
                                string str2 = y.ChildNodes[1].InnerText;
                                oldTableEles.Add(new _TableEle() { date = str1, file = str2 });
                            }
                            XmlNode right_path_node = xmlDoc.DocumentElement.SelectSingleNode("//div[@class='right'][3]");
                            if (right_path_node != null)
                                old_right_path = right_path_node.InnerText;
                        }
                        foreach (FileInfo file in files)
                        {
                            string fileNoExpandedName = file.Name.Substring(0, file.Name.LastIndexOf('.'));
                            if (!IsNeedToIgnore(file.Name) && !oldTableEles.Exists((tableEle) => { return tableEle.date != "[目录]" && fileNoExpandedName == tableEle.file; }))
                            {
                                newTableEles.Add(new _TableEle() { date = file.LastWriteTime.ToString("yyyy-M-d"), file = fileNoExpandedName });
                            }
                        }
                        foreach (_TableEle file in oldTableEles)
                        {
                            if (files.Exists((__file) => { string fileNoExpandedName = __file.Name.Substring(0, __file.Name.LastIndexOf('.')); return file.date != "[目录]" && fileNoExpandedName == file.file; }))
                            {
                                newTableEles.Add(file);
                            }
                        }
                        foreach (DirectoryInfo folder in folders)
                        {
                            if (!IsNeedToIgnore(folder.Name))
                                newTableEles.Add(new _TableEle() { date = "[目录]", file = folder.Name });
                        }
                        //测试：
                        //Console.WriteLine("=====================================");
                        //foreach (_TableEle item in newTableEles)
                        //{
                        //    Console.WriteLine("newTableEles：    " + item.date + "          " + item.file);
                        //}
                        bool needToCreateNewIndexFile = oldTableEles.Count != newTableEles.Count;
                        if (!needToCreateNewIndexFile)
                        {
                            for (int i = 0; i < oldTableEles.Count; i++)
                            {
                                if (oldTableEles[i].file != newTableEles[i].file)
                                {
                                    needToCreateNewIndexFile = true;
                                    break;
                                }
                            }
                        }
                        if (!needToCreateNewIndexFile)
                        {
                            //right_path：如"/2015/新建文件夹/"
                            string right_path = path.FullName.Substring(gitFolderPath.Length);
                            if (right_path != "")
                            {
                                right_path = right_path.Replace('\\', '/') + "/";
                            }
                            needToCreateNewIndexFile = right_path != old_right_path;
                        }
                        if (needToCreateNewIndexFile)
                        {
                            CreateNewIndexFile(path, newTableEles);
                        }
                    }
                }
            }
        }

        static bool IsNeedToIgnore(string fileOrFolderName)
        {
            return fileOrFolderName.EndsWith("_files") || fileOrFolderName.StartsWith("-") || fileOrFolderName.StartsWith("_") || fileOrFolderName.StartsWith(".") || fileOrFolderName == "index.html" || fileOrFolderName == "404.html" || fileOrFolderName == "README.html";
        }

        static bool is处 = true;
        static bool TestDirIfShouldGenIndexFile(DirectoryInfo curDirectory)
        {
            bool ret = false;
            if (is处 || !IsNeedToIgnore(curDirectory.Name))
            {
                is处 = false;
                DirectoryInfo[] dirs = curDirectory.GetDirectories();
                foreach (DirectoryInfo item in dirs)
                {
                    ret = TestDirIfShouldGenIndexFile(item) || ret;
                }
                if (!ret)
                {
                    FileInfo[] files = curDirectory.GetFiles();
                    foreach (FileInfo item in files)
                    {
                        if (item.Extension.ToLower() == ".html")
                        {
                            ret = true;
                            break;
                        }
                    }
                }
                if (ret)
                {
                    dirShouldGenIndexFile.Add(curDirectory);
                }
            }
            return ret;
        }


        class _File
        {
            public string date;
            public FileInfo file;
        }
        class _TableEle
        {
            public string date;
            public string file;
        }
        static int _CompareFileInfoByDateTime(FileInfo x, FileInfo y)
        {
            if (x.LastWriteTime < y.LastWriteTime)
                return 1;
            else if (x.LastWriteTime == y.LastWriteTime)
                return 0;
            else
                return -1;
        }
        static void CreateNewIndexFile(DirectoryInfo curDirectory, List<_TableEle> tableEles)
        {
            //cur_dir_name：如"新建文件夹"
            string cur_dir_name = curDirectory.Name;
            //right_path：如"/2015/新建文件夹/"
            string right_path = curDirectory.FullName.Substring(gitFolderPath.Length);
            if (right_path != "")
            {
                right_path = right_path.Replace('\\', '/') + "/";
            }
            FileStream file = new FileStream(Path.Combine(curDirectory.FullName, "index.html"), FileMode.Create);
            StringBuilder builder = new StringBuilder();
            builder.Append(@"<!DOCTYPE html>
<html>
<head>
<title>" + (right_path != "" ? cur_dir_name + " - " : "") + gitFolderName + "</title>");
            builder.Append(@"<meta charset='UTF-8'/>
<meta name='viewport' content='width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no'/>
<script type='text/javascript' src='file:///storage/emulated/0/pxp1230.github.io.js'></script>
<script type='text/javascript' src='file:///C:/pxp1230.github.io.js'></script>
<script type='text/javascript'>
if(window.location.protocol!='file:'){var d=document,s=d.createElement('script');s.src='/pxp1230.github.io.js';d.head.appendChild(s)}
</script>
<style type='text/css'>
body{margin:10px;font:12px 'Hiragino Sans GB','Microsoft YaHei','微软雅黑',tahoma,arial,simsun,'宋体';color:#1A1A1A;background:#1A1A1A;}a{color:#00CCFF;text-decoration:none;}a:hover{text-decoration:underline;}table,.bar{width:100%;max-width:700px;padding:0;margin:0 auto;border-style:none;border-spacing:10px 1px;}td{background:#454545;font-size:12px;padding:6px 12px;box-shadow:10px 10px 20px #000;}.right{float:right;margin:10px;}.right,.right a{font-weight:bold;color:#005266;}.clear{clear:both;}.time{width:70px;text-align:center;white-space:nowrap;}
</style>
</head>
<body>");
            if (right_path != "")
            {
                builder.Append(@"<div class='bar'>
<div class='right'><a href='../index.html'>[上一级]</a></div>
<div class='right'><a href='" + GetJumpToGitFolderPath(curDirectory) + @"index.html'>[根目录]</a></div>
<div class='right'>" + right_path + @"</div>
<div class='clear'></div>
</div>");
            }
            builder.Append("<table>");
            foreach (_TableEle item in tableEles)
            {
                if (item.date == "[目录]")
                {
                    builder.Append(@"<tr>
<td class='time'><a target='_blank' href='" + UrlEncode(item.file) + @"/index.html'>[目录]</a></td>
<td><a href='" + UrlEncode(item.file) + @"/index.html'>" + item.file + @"</a></td>
</tr>");
                }
                else
                {
                    builder.Append(@"<tr>
<td class='time'>" + item.date + @"</td>
<td><a target='_blank' href='" + UrlEncode(item.file) + @".html'>" + item.file + @"</a> </td>
</tr>");
                }
            }
            builder.Append(@"</table>
</body>
</html>");
            string content = builder.ToString();
            byte[] array = Encoding.UTF8.GetBytes(content);
            file.Write(array, 0, array.Length);
            file.Dispose();
        }

        static string UrlEncode(string url)
        {
            return System.Net.WebUtility.UrlEncode(url).Replace("+", "%20");
        }




        static List<FileInfo> mdShouldGenHtmlFile = new List<FileInfo>();
        /// <summary>
        /// 在目录及子目录下将未生成.html的.md转换成.html
        /// </summary>
        static void AutoMd2Html(DirectoryInfo curDirectory)
        {
            if (GetMdFileList(curDirectory))
            {
                foreach (FileInfo item in mdShouldGenHtmlFile)
                {
                    Md2Html(item);
                }
            }
        }
        static bool GetMdFileList(DirectoryInfo curDirectory)
        {
            bool ret = false;
            FileInfo[] files = curDirectory.GetFiles("*.md");
            foreach (FileInfo item in files)
            {
                if (!IsNeedToIgnore(item.Name))
                {
                    string html = item.FullName.Substring(0, item.FullName.LastIndexOf(".")) + ".html";
                    FileInfo htmlFile = new FileInfo(html);
                    if (!htmlFile.Exists || htmlFile.LastWriteTime.AddSeconds(4) < item.LastWriteTime)
                        mdShouldGenHtmlFile.Add(item);
                }
            }
            ret = mdShouldGenHtmlFile.Count > 0 || ret;
            DirectoryInfo[] dirs = curDirectory.GetDirectories();
            foreach (DirectoryInfo item in dirs)
            {
                if (!IsNeedToIgnore(item.Name))
                {
                    ret = GetMdFileList(item) || ret;
                }
            }
            return ret;
        }
    }
}
