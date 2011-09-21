using System.IO;
using Machine.Specifications;

namespace Get.Specs
{
    public class when_called_with_an_outputdir_switch
    {
        Cleanup after = () =>
            {
                File.Delete("dependencies.txt");
                Directory.Delete("OutputDir", true);
            };

        Establish context = () =>
            {
                using (var sr = new StreamWriter("dependencies.txt"))
                {
                    sr.WriteLine("NHibernate 3.2.0.4000");
                }
            };

        Because of = () => new Engine().Process(new[] {"dependencies.txt", "-o", "OutputDir"});
        
        It should_install_packages_to_the_specified_output = () => Directory.Exists("OutputDir").ShouldBeTrue();
    }

    public class when_called_with_an_excludeversion_switch
    {
        Cleanup after = () =>
        {
            File.Delete("dependencies.txt");
            Directory.Delete("NHibernate", true);
        };

        Establish context = () =>
        {
            using (var sr = new StreamWriter("dependencies.txt"))
            {
                sr.WriteLine("NHibernate 3.2.0.4000");
            }
        };

        Because of = () => new Engine().Process(new[] { "dependencies.txt", "-ExcludeVersion" });

        It should_not_include_version_in_the_output_folder_name = () => Directory.Exists("NHibernate").ShouldBeTrue();
    }
}