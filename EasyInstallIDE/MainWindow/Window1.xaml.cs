using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using WPF.Themes;
using EasyInstallIDE.Properties;
using EasyInstall.ViewModels;
using EasyInstall.Interfaces;
using EasyInstall;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace EasyInstallIDE
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      const string TITLE = "EasyInstall";

      public static ObservableCollection<PackageViewModel> CurrentPackage
      {
         get;
         set;
      }

      static Window1()
      {
         CurrentPackage = new ObservableCollection<PackageViewModel>();
         CurrentPackage.Add(PackageManager.CreateNewPackage("TestPackage"));
      }

      string _currentFileName;
      string CurrentFileName
      {
         get { return _currentFileName; }
         set 
         {
            _currentFileName = value;

            // Update the Title
            this.Title = string.Format("{0} - {1}", TITLE, value);
         }
      }
      public Window1()
      {
         InitializeComponent();
      }

      private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         CurrentPackage[0] = PackageManager.CreateNewPackage("TestPackage");

         this.DataContext = null;
         this.DataContext = CurrentPackage;
      }

      

      private void Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         Settings.Default.Theme = e.AddedItems[0].ToString();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         Themes.ItemsSource = ThemeManager.GetThemes();

         Themes.SelectedItem = Settings.Default.Theme;
      }

      
      private void TreeWorkArea_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
      {

      }
   }
}
