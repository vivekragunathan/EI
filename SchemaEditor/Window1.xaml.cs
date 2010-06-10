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
using EasyInstall;
using System.IO;
using EasyInstall.ViewModels;
using Microsoft.Win32;
using WPF.Themes;
using SchemaEditor.Properties;

namespace SchemaEditor
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      EntitySchemaCollection coll;
      public Window1()
      {
         InitializeComponent();
         
         if (File.Exists("Schemas.xml"))
         {
            EntitySchema.LoadSchemas("Schemas.xml");
         }

         coll = this.FindResource("Schemas") as EntitySchemaCollection;

         CurrentFileName = string.Empty;
      }

      private void Schema_RowEditEnding(object sender, Microsoft.Windows.Controls.DataGridRowEditEndingEventArgs e)
      {
         
      }

      private void Schema_CellEditEnding(object sender, Microsoft.Windows.Controls.DataGridCellEditEndingEventArgs e)
      {
         
      }

      private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         OpenFileDialog fd = new OpenFileDialog();
         fd.Filter = "Schema Files|*.xml";
         if ( fd.ShowDialog() == true )
         {
            coll.LoadSchemas(fd.FileName);

            CurrentFileName = fd.FileName;
         }
      }

      private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         coll.SaveChanges(CurrentFileName);
      }

      private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         CurrentFileName = string.Empty;
      }

      string _currentFileName;
      string CurrentFileName
      {
         get
         {
            return _currentFileName;
         }

         set
         {
            _currentFileName = value;

            if ( string.IsNullOrEmpty(value) )
            {
               this.Title = "Schema Editor - [New schema]";
            }
            else
               this.Title = "Schema Editor - " + value;
         }
      }

      private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         SaveFileDialog sd = new SaveFileDialog();

         if ( sd.ShowDialog() == true )
         {
            coll.SaveChanges(sd.FileName);

            CurrentFileName = sd.FileName;
         }
      }

      private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = (!string.IsNullOrEmpty(CurrentFileName));
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         Themes.ItemsSource = ThemeManager.GetThemes();

         Themes.SelectedItem = Settings.Default.Theme;

      }

      private void Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         Settings.Default.Theme = e.AddedItems[0].ToString();
      }
   }
}
