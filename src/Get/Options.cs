using System;
using System.Collections.Generic;
using NDesk.Options;

namespace Get
{
    public class Options
    {
        public Options(IEnumerable<string> args)
        {
            Sources = new List<string>();
            bool showHelp = false;

            SetDefaults();

            var optionSet = new OptionSet
                                {
                                    {"o|OutputDirectory=", "The destination folder.", x => OutputDirectory = x},
                                    {"s|source=", "A repository source.", x => Sources.Add(x)},
                                    {"x|ExcludeVersion", "Omit version number from destination folders." , x => ExcludeVersion = x != null },
                                    {"h|Help", "Show this message and exit.", x => showHelp = x != null},
                                };

            Extra = optionSet.Parse(args);

            ShowHelp = () =>
                {
                    Console.WriteLine("Get.exe [dependency file] [options]");
                    Console.WriteLine("Options:");
                    optionSet.WriteOptionDescriptions(Console.Out);
                };

            if (Sources.Count == 0)
            {
                Sources.Add("http://packages.nuget.org/v1/FeedService.svc");
            }
        }

        void SetDefaults()
        {
            OutputDirectory = ".";
        }

        public bool ExcludeVersion { get; set; }

        public Action ShowHelp { get; set; }

        public string OutputDirectory { get; set; }

        public List<string> Extra { get; set; }

        public IList<string> Sources { get; set; }
    }
}