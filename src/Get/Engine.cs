using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet;

namespace Get
{
    public class Engine
    {
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

           
            _options = new Options(args.Skip(1));
            ProcessPackagesFile(dependencyFile);
            return 0;
        }

        void ProcessPackagesFile(string dependencyFile)
        {
            var packages = new List<Package>(GetPackages(dependencyFile));

            foreach (Package package in packages)
            {
                if (!PackageExists(package))
                    RetrievePackage(package);
            }
        }

        bool PackageExists(Package package)
        {
            string path = Path.Combine(_options.OutputDirectory ?? "", package.Id);

            if (!_options.ExcludeVersion)
            {
                path = path + "." + package.Version;
            }

            return Directory.Exists(path);
        }

        void RetrievePackage(Package package)
        {
            foreach (string source in _options.Sources)
            {
                PackageManager packageManager = GetPackageManager(source);

                packageManager.InstallPackage(package.Id, new Version(package.Version));

                if (PackageExists(package))
                {
                    break;
                }
            }
        }

        IDictionary<string, PackageManager> _packageManagers = new Dictionary<string, PackageManager>();

        PackageManager GetPackageManager(string source)
        {
            if(_packageManagers.ContainsKey(source))
            {
                return _packageManagers[source];
            }

            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(source);
            var packageManager = new PackageManager(packageRepository,
                                                    new DefaultPackagePathResolver(_options.OutputDirectory, !_options.ExcludeVersion),
                                                    new PhysicalFileSystem(_options.OutputDirectory));

            packageManager.Logger = new ConsoleLogger();
            
            _packageManagers.Add(source, packageManager);

            return packageManager;
        }

        IEnumerable<Package> GetPackages(string dependencyFile)
        {
            var dependencies = new List<Package>();

            using (StreamReader sr = File.OpenText(dependencyFile))
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
            return dependencies;
        }
    }
}