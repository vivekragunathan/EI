using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using EasyInstallIDE.Properties;

namespace EasyInstallIDE
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private void Application_Exit(object sender, ExitEventArgs e)
      {
         Settings.Default.Save();
      }
   }
}
