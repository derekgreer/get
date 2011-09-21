using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Get
{
    public class Engine
    {
        string _nuGetPath;
        Options _options;

        public int Process(string[] args)
        {
            _options = new Options(args.Skip(1));

            if (args.Length == 0)
            {
                _options.ShowHelp();
                return -1;
            }

            string dependencyFile = args[0];

            _nuGetPath = GetNuGetPath();

            if (_nuGetPath == null)
            {
                Console.WriteLine("Could not locate NuGet.exe.");
                return -1;
            }

            _options = new Options(args.Skip(1));
            ProcessPackagesFile(dependencyFile);
            return 0;
        }

        string GetNuGetPath()
        {
            return GetFullPath(Path.Combine(Environment.GetEnvironmentVariable("NUGET_PATH") ?? "", "NuGet.exe"));
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            string values = Environment.GetEnvironmentVariable("PATH");
            foreach (string path in values.Split(';'))
            {
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        void ProcessPackagesFile(string dependencyFile)
        {
            var packages = new List<Package>(GetPackages(dependencyFile));

            foreach (Package package in packages)
            {
                string path = Path.Combine(_options.OutputDirectory ?? "", package.Id);
                
                if(!_options.ExcludeVersion)
                {
                    path = path + "." + package.Version;
                }

                if (!Directory.Exists(path))
                {
                    RetrievePackage(package);
                }
            }
        }

        void RetrievePackage(Package package)
        {
            foreach (string source in _options.Sources)
            {
                RetrievePackage(package, source);
            }
        }

        void RetrievePackage(Package package, string source)
        {
            var p = new Process();

            var sb = new StringBuilder();
            sb.Append(string.Format("install {0} ", package.Id));
            if (!string.IsNullOrEmpty(package.Version))
            {
                sb.Append(string.Format("-Version {0} ", package.Version));
            }
            sb.Append(string.Format("-Source {0} ", source));
            if (!string.IsNullOrEmpty(_options.OutputDirectory))
            {
                sb.Append(string.Format("-OutputDirectory {0} ", _options.OutputDirectory));
            }
            if(_options.ExcludeVersion)
            {
                sb.Append("-ExcludeVersion ");
            }


            var startInfo =
                new ProcessStartInfo(_nuGetPath, sb.ToString());
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            
            p.StartInfo = startInfo;
            p.Start();

            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            Console.Write(stdout);
            Console.Write(stderr);
        }

        IEnumerable<Package> GetPackages(string dependencyFile)
        {
            var dependencies = new List<Package>();

            using (
                StreamReader sr = File.OpenText(dependencyFile)
                )
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fields = Regex.Split(line, @"\s+");

                    if (fields.Length < 2)
                    {
                        throw new Exception("An error occurred processing the file.");
                    }

                    dependencies.Add(new Package(fields[0], fields[1]));
                }
            }
            return
                dependencies;
        }
    }
}