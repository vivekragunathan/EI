using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using EasyInstall.Interfaces;

namespace EasyInstall
{
   class Setup
   {
      static void Main(string[] args)
      {
         Stream packageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Data.pkg");

         IPackage package = Package.Load(packageStream);

         // TODO: Before deploying create the UI and hooks
         package.Deploy();
      }
   }
}
