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
      const string RESOURCEPACKAGENAME = "Data.EPK";
      public static void Main(string[] args)
      {
         try
         {
            Assembly a = Assembly.GetExecutingAssembly();

            string[] names = a.GetManifestResourceNames();
            foreach ( string name in names )
               System.Console.WriteLine(name);

            Stream pkgStream = a.GetManifestResourceStream(RESOURCEPACKAGENAME);

            if ( pkgStream == null )
            {
               throw new Exception(string.Format("Unable to find resource {0}", RESOURCEPACKAGENAME));
            }

            IPackage package = Package.Load(pkgStream);

            package.Deploy();
         }
         catch ( Exception e )
         {
            Console.WriteLine(e.ToString());
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
         }
      }
   }
}
