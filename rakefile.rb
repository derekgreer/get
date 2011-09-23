require 'rubygems'
require 'albacore'
require 'configatron'

FRAMEWORK_PATH = "C:/Windows/Microsoft.NET/Framework/v4.0.30319/"
BUILD_PATH = File.expand_path('build')
MERGE_PATH = "#{BUILD_PATH}/merge"
TOOLS_PATH = File.expand_path('tools')
ARTIFACTS_PATH = File.expand_path('artifacts')
LIB_PATH = File.expand_path('lib')
SOLUTION = "src/Get.sln"
COMPILE_TARGET = "Release"
FEEDS = ["https://go.microsoft.com/fwlink/?LinkID=206669" ]
load "VERSION.txt"
require './packages.rb'

task :default => ["build:all"]

namespace :build do

	task :all => [:clean, :dependencies, :compile, :tests, :merge, :package]

	assemblyinfo :versioning do |asm|
  		asm.output_file = "src/CommonAssemblyInfo.cs"
  		asm.version = "#{BUILD_VERSION}"
	end

	task :clean do
		rm_rf "#{BUILD_PATH}"
		rm_rf "#{ARTIFACTS_PATH}"
	end

	task :compile => [:versioning] do

		mkdir "#{BUILD_PATH}"
		sh "#{FRAMEWORK_PATH}msbuild.exe /p:Configuration=#{COMPILE_TARGET} #{SOLUTION}"
		copyOutputFiles "src/Get/bin/#{COMPILE_TARGET}", "*.{exe,dll,pdb}", "#{BUILD_PATH}"
	end

	task :merge do
        mkdir_p "#{MERGE_PATH}"
		assemblies = FileList.new("#{BUILD_PATH}/*.dll")
        sh "#{TOOLS_PATH}/ILMerge/ILMerge.exe /targetplatform:v4 /lib:#{FRAMEWORK_PATH}WPF /out:#{MERGE_PATH}\\Get.exe #{BUILD_PATH}\\Get.exe #{assemblies}"
    end

	task :tests do
		specs = FileList.new("src/Get.Specs/bin/#{COMPILE_TARGET}/*.Specs.dll")
		sh "lib/Machine.Specifications/tools/mspec-x86-clr4.exe -x integration #{specs}"
	end

	task :package do
		mkdir_p "#{ARTIFACTS_PATH}"
		rm Dir.glob("#{ARTIFACTS_PATH}/*.nupkg")
		FileList["packaging/nuget/*.nuspec"].each do |spec|
		sh "nuget pack #{spec} -o #{ARTIFACTS_PATH} -Version #{BUILD_VERSION} -BasePath ."
		end
	end

	task :dependencies do
		configatron.packages.each do | package |
			FEEDS.each do | feed | 
				!(File.exists?("#{LIB_PATH}/#{package[0]}")) and
					sh "nuget Install #{package[0]} -Version #{package[1]} -o #{LIB_PATH} -Source #{feed} -ExcludeVersion" do | cmd, results | cmd  end
			end
		end
	end

	def copyOutputFiles(fromDir, filePattern, outDir)
		Dir.glob(File.join(fromDir, filePattern)){|file| 		
			copy(file, outDir) if File.file?(file)
  		} 
	end
end
