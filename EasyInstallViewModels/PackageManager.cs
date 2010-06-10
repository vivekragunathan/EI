using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyInstall.Interfaces;

namespace EasyInstall.ViewModels
{
   public class PackageManager
   {
      public static PackageViewModel CurrentPackage { get; set; }

      public static PackageViewModel CreateNewPackage(string name)
      {
         PackageViewModel package = new PackageViewModel(name);

         return package;
      }

      public static PackageViewModel LoadPackage(string filename)
      {
         IPackage package = Package.LoadFromTOC(filename);

         return new PackageViewModel(package);
      }
   }
}
