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
using EasyInstall.Interfaces;
using System.IO;
using EasyInstall.ViewModels;

namespace EasyInstall
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      SetupSequencer sequencer;

      public MainWindow()
      {
         InitializeComponent();
         IPackage p = Package.Create("MyPackage");

         IEntity folder = p.CreateChild(EntityTypes.FOLDERENTITY, "MyFolder");

         folder.SetAttribute("SourceFolder", @"C:\Chess board");
         folder.SetAttribute("DestinationFolder", @"C:\Users\Sanju\TestInstallFolder");

         MemoryStream ms = new MemoryStream();
         p.Build(ms);

         PackageViewModel pvm = new PackageViewModel(p);

         sequencer = new SetupSequencer(pvm);

         DataContext = sequencer.Context;
      }

      private void btnNext_Click(object sender, RoutedEventArgs e)
      {
         sequencer.Next();
      }

      private void btnBack_Click(object sender, RoutedEventArgs e)
      {
         sequencer.Previous();
      }

      private void btnCancel_Click(object sender, RoutedEventArgs e)
      {
         sequencer.Cancel();
      }
   }
}
