using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyInstall
{
   /// <summary>
   /// Interaction logic for DeployScreen.xaml
   /// </summary>
   public partial class DeployScreen : UserControl
   {
      public DeployScreen()
      {
         InitializeComponent();

         this.ProgressCounter.ValueChanged += new RoutedPropertyChangedEventHandler<double>(ProgressCounter_ValueChanged);
      }

      void ProgressCounter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         System.Diagnostics.Trace.WriteLine(string.Format("Progress Value changed {0}", e.NewValue));
      }
   }
}
