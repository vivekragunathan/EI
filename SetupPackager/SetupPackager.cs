using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Resources;
using ILMerging;
using EasyInstall.Interfaces;

namespace EasyInstall
{
   public class SetupPackager : ISetupPackager, ISetupInfo
   {
      static string DefaultConfigXML = "Config.XML";
      static string DefaulPackageFile = "Data.EPK";
      public SetupPackager(string packageFile, string configXml)
      {
         FrameWorkAssemblyReferences = new List<string>();
         ModuleReferences = new List<string>();
         Sources = new List<string>();
         LibPath = new List<string>();
         ExternalAssemblyReferences = new List<string>();
         EmbeddedResources = new List<string>();

         OutputDirectory = string.Empty;

         PackageFile = packageFile;

         ReadConfiguration(configXml);
      }

      public SetupPackager(string packageFile)
         :this(packageFile, DefaultConfigXML)
      {
      }

      public SetupPackager()
         : this(DefaulPackageFile, DefaultConfigXML)
      {
      }

      void ReadConfiguration(string configXml)
      {
         if (!string.IsNullOrEmpty(configXml))
         {
            if (!File.Exists(configXml))
            {
               System.Diagnostics.Trace.Write(string.Format("SetupPackager: Configuration {0} file does not exist!", configXml));

               configXml = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Config.Xml");
            }
         }
         
         if (!File.Exists(configXml))
         {
            System.Diagnostics.Trace.Write(string.Format("SetupPackager: Default configuration {0} file does not exist!", configXml));

            return;
         }
         

         XDocument xd = XDocument.Load(configXml);

         FrameWorkAssemblyReferences = (from a in xd.Root.Element("FrameworkAssemblies").Elements("Assembly")
                        select a.Value).ToList();

         ExternalAssemblyReferences = (from a in xd.Root.Element("ExtAssemblies").Elements("Assembly")
                             select a.Value).ToList();

         ModuleReferences = (from a in xd.Root.Element("Modules").Elements("Module")
                        select a.Value).ToList();

         Sources = (from a in xd.Root.Element("Sources").Elements("Source")
                        select a.Value).ToList();

         EmbeddedResources = (from a in xd.Root.Element("EmbeddedResources").Elements("Resource")
                              select a.Value).ToList();

         LibPath = (from a in xd.Root.Element("LibPaths").Elements("Path")
                     select a.Value).ToList();
      }

      public string PackageFile
      {
         get;
         set;
      }

      class Arguments
      {
         public string PackageFile { get; set; }
      }

      static Arguments ParseArguments(string[] args)
      {
         Arguments arguments = new Arguments();
         if (args.Length > 0)
         {
            arguments.PackageFile = args[0];
         }

         return arguments;
      }

      public void Pack()
      {
         if (string.IsNullOrEmpty(PackageFile))
         {
            throw new ArgumentException("PackageFile is not assigned");
         }

         if (string.IsNullOrEmpty(OutputFileName))
         {
            throw new ArgumentException("OutputFileName is not assigned");
         }

         var providerOptions = new Dictionary<string, string>();
         providerOptions.Add("CompilerVersion", "v3.5");

         string AsmPath = Path.Combine(OutputDirectory, OutputFileName);

         CSharpCodeProvider cp = new CSharpCodeProvider(providerOptions);
         CompilerParameters cparams = new CompilerParameters();
         cparams.GenerateExecutable = true;
         cparams.OutputAssembly = AsmPath;
         cparams.IncludeDebugInformation = true;
         cparams.ReferencedAssemblies.AddRange(FrameWorkAssemblyReferences.Concat(ExternalAssemblyReferences).ToArray());

         cparams.EmbeddedResources.Add(PackageFile);

         foreach (var resource in EmbeddedResources)
         {
            cparams.EmbeddedResources.Add(resource);
         }

         string libpathstr = string.Empty;
         foreach (var path in LibPath)
         {
            libpathstr += "\"" + path + "\" ";
         }
         if (LibPath.Count() > 0)
         {
            cparams.CompilerOptions = String.Format("/lib:{0}",
               libpathstr);
         }

         // Add modules
         if (ModuleReferences.Count() > 0)
         {
            StringBuilder sb = new StringBuilder();

            sb.Append("/addmodule:");
            foreach (var mr in ModuleReferences)
            {
               sb.Append(mr + " ");
            }
            cparams.CompilerOptions += sb.ToString();
         }

         CompilerResults cr = cp.CompileAssemblyFromFile(cparams, Sources.ToArray());

         cr.Errors.OfType<CompilerError>().ToList()
            .ForEach((e) => Console.WriteLine((e.IsWarning ? "Warning : " : "Error : ") + e.ErrorText));

         foreach (CompilerError r in cr.Errors)
         {
            if (!r.IsWarning)
            {
               throw new Exception("Error Building Package!");
            }
         }

         // Copy the external dependencies to the OutputDirectory
         string destFolder = Path.GetDirectoryName(AsmPath);
         foreach ( var file in ExternalAssemblyReferences )
         {
            File.Copy(file, Path.Combine(destFolder, Path.GetFileName(file)), true);
         }

         // Merge the IL to create a single assembly
         //ILMerge merger = new ILMerge();

         //merger.OutputFile = @"D:\Setup.exe";//Path.Combine(OutputDirectory, OutputFileName);
         //string[] primaryAssembly = new string[] { tempAsmPath };
         //string[] assemblies = primaryAssembly.Concat(ExternalAssemblyReferences).ToArray();

         //merger.SetInputAssemblies(assemblies);
         //merger.DebugInfo = true;

         //merger.Merge();
      }

      static void Main(string[] args)
      {
         SetupPackager sp = new SetupPackager(ParseArguments(args).PackageFile, null);

         sp.Pack();
      }

      public ISetupInfo SetupInfo
      {
         get
         {
            return this;
         }
      }

      #region ISetupPackager Members


      public string OutputFileName
      {
         get;
         set;
      }

      public string OutputDirectory
      {
         get;
         set;
      }

      public IList<string> SourcesDirectories
      {
         get;
         private set;
      }

      public IList<string> Sources
      {
         get;
         private set;
      }

      public IList<string> ExternalAssemblyReferences
      {
         get;
         private set;
      }

      public IList<string> FrameWorkAssemblyReferences
      {
         get;
         private set;
      }

      public IList<string> LibPath
      {
         get;
         private set;
      }

      public IList<string> ModuleReferences
      {
         get;
         private set;
      }

      public IList<string> EmbeddedResources
      {
         get;
         private set;
      }

      #endregion
   }
}
