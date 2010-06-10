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

namespace SetupPackager
{
   class Program
   {
      static string[] FrwkAssemblyRefs { get; set; }
      static string[] ModuleRefs { get; set; }
      static string[] Sources { get; set; }
      static string[] ExtAssemblyRefs { get; set; }

      static string[] LibPaths { get; set; }

      static Program()
      {
         FrwkAssemblyRefs = new string[] { };
         ModuleRefs = new string[] { };
         Sources = new string[] { };
         LibPaths = new string[] { };
         ExtAssemblyRefs = new string[] { };

         PackageFile = string.Empty;
      }

      static void ReadConfiguration()
      {
         if (!File.Exists("Config.xml"))
            return;
         XDocument xd = XDocument.Load("Config.xml");

         FrwkAssemblyRefs = (from a in xd.Root.Element("FrameworkAssemblies").Elements("Assembly")
                        select a.Value).ToArray();

         ExtAssemblyRefs = (from a in xd.Root.Element("ExtAssemblies").Elements("Assembly")
                             select a.Value).ToArray();

         ModuleRefs = (from a in xd.Root.Element("Modules").Elements("Module")
                        select a.Value).ToArray();

         Sources = (from a in xd.Root.Element("Sources").Elements("Source")
                        select a.Value).ToArray();

         LibPaths = (from a in xd.Root.Element("LibPaths").Elements("Path")
                     select a.Value).ToArray();
      }

      static string PackageFile
      {
         get;
         set;
      }

      static string Output
      {
         get;
         set;
      }

      static void ParseArguments(string[] args)
      {
         //args.Fi
         PackageFile = args[0];
      }

      static void Main(string[] args)
      {
         ReadConfiguration();

         ParseArguments(args);

         var providerOptions = new Dictionary<string, string>();
         providerOptions.Add("CompilerVersion", "v3.5");

         CSharpCodeProvider cp = new CSharpCodeProvider(providerOptions);
         CompilerParameters cparams = new CompilerParameters();
         cparams.GenerateExecutable = true;
         cparams.OutputAssembly = "Setup_Temp.exe";
         cparams.ReferencedAssemblies.AddRange(FrwkAssemblyRefs.Concat(ExtAssemblyRefs).ToArray());

         cparams.EmbeddedResources.Add(PackageFile);

         string libpathstr = string.Empty;
         foreach (var path in LibPaths)
         {
            libpathstr += "\"" + path + "\" ";
         }
         if (LibPaths.Length > 0)
         {
            cparams.CompilerOptions = String.Format("/lib:{0}",
               libpathstr); 
         }

         // Add modules
         if (ModuleRefs.Length > 0)
         {
            StringBuilder sb = new StringBuilder();

            sb.Append("/addmodule:");
            foreach ( var mr in ModuleRefs )
            {
               sb.Append(mr + " ");
            }
            cparams.CompilerOptions += sb.ToString();
         }
         
         CompilerResults cr = cp.CompileAssemblyFromFile(cparams, Sources);

         cr.Errors.OfType<CompilerError>().ToList()
            .ForEach((e) => Console.WriteLine((e.IsWarning ? "Warning : " : "Error : ") + e.ErrorText));

         foreach ( CompilerError r in cr.Errors )
         {
            if (!r.IsWarning)
            {
               return;
            }
         }

         // Merge the IL to create a single assembly
         ILMerge merger = new ILMerge();

         merger.OutputFile = "Setup.exe";
         string[] primaryAssembly = new string[] { "Setup_Temp.exe" };
         string[] assemblies = primaryAssembly.Concat(ExtAssemblyRefs).ToArray();
         
         merger.SetInputAssemblies(assemblies);

         merger.Merge();

         File.Delete("Setup_Temp.exe");
      }
   }
}
