using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyInstall.Interfaces;

namespace EasyInstall
{
   internal class PackageManager
   {
      private static IList<IPackage> Packages = new List<IPackage>();

      public static IPackage GetPackage(Guid id)
      {
         return Packages.First((p) => p.ID == id);
      }

      public static void AddPackage(IPackage package)
      {
         if (null == package)
            throw new ArgumentNullException("package");

         Packages.Add(package);
      }
   }
}
