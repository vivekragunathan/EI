using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using EasyInstall;
using EasyInstall.Interfaces;

namespace SetupPackager
{
   class Setup
   {
      public static void Main(string[] args)
      {
         try
         {
            Assembly a = Assembly.GetExecutingAssembly();

            string[] names = a.GetManifestResourceNames();
            foreach ( string name in names )
               System.Console.WriteLine(name);

            Stream pkgStream = a.GetManifestResourceStream("MyPackage");

            IPackage package = Package.Load(pkgStream);

            package.Deploy();
         }
         catch ( Exception e )
         {
            Console.WriteLine(e.ToString());
         }
      }
   }
}
