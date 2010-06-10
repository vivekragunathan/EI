using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using EasyInstall.Interfaces;
using System.Xml.Linq;

namespace EasyInstall
{
   [Serializable]
   [DataContract]
   public class Program
   {
      static void Main(string[] args)
      {
         try
         {
            IPackage p = Package.Create("MyPackage");

            IEntity folder = p.CreateChild(EntityTypes.FOLDERENTITY, "MyFolder");

            folder.SetAttribute("SourceFolder", @"C:\chess board");
            folder.SetAttribute("DestinationFolder", @"C:\Users\Sanju\TestSetup");

            /*ISetupPackager sp = new SetupPackager();
            sp.SetupInfo.OutputFileName = @"C:\Setup.exe";*/
            using (FileStream fs = new FileStream(@"C:\Users\Sanju\Data.EPK", FileMode.Create, FileAccess.ReadWrite))
            {
               p.Build(fs);
            }
            p.Deploy();

            Console.Write(p.ID);
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error: " + ex.Message);
         }
      }

      public Program()
      {
      }
   }
}
