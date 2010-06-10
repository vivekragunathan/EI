using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyInstall.ViewModels;
using EasyInstall.Controls;
using System.Collections.Generic;
using System.Linq;
using EasyInstall.Interfaces;
using System.Windows.Media;
using EasyInstall;
using System.IO;

namespace EasyInstallIDE
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class Window1 : Window
   {
      private void TreeWorkArea_KeyDown(object sender, KeyEventArgs e)
      {
         if ( e.Key == Key.F2 )
         {
            SetCurrentItemInEditMode(true);
         }
      }

      private void SetCurrentItemInEditMode(bool editMode)
      {
         TreeViewItem tvItem = TreeWorkArea.Tag as TreeViewItem;

         EditableTextBlock etb = tvItem.FindVisualChild<EditableTextBlock>();

         etb.IsInEditMode = editMode;
      }

      EntityViewModel GetCurrentEntity(object parameter)
      {
         if ( parameter == null )
         {
            return CurrentEntity;
         }
         return parameter as EntityViewModel;
      }

      EntityViewModel CurrentEntity; // Keeps track of the currently selected entity
      private void TreeWorkArea_Selected(object sender, RoutedEventArgs e)
      {
         TreeWorkArea.Tag = e.OriginalSource;
         CurrentEntity = (e.OriginalSource as TreeViewItem).Header as EntityViewModel;
      }

      #region DRAG N DROP

      IList<string> draggedEntites;

      private void TreeWorkArea_GiveFeedback(object sender, GiveFeedbackEventArgs e)
      {
         System.Diagnostics.Debug.WriteLine(e.Source.ToString());

      }

      private void TreeWorkArea_DragOver(object sender, DragEventArgs e)
      {
         e.Effects = DragDropEffects.None;

         EntityViewModel evm;
         if ( CheckDropTarget(e, out evm) )
         {
            e.Effects = DragDropEffects.Link;
         }

         e.Handled = true;
      }

      private void TreeWorkArea_DragEnter(object sender, DragEventArgs e)
      {
         draggedEntites = new List<string>();

         string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

         foreach ( var item in files )
         {
            if ( Directory.Exists(item) )
            {
               draggedEntites.Add(Constants.FOLDERENTITY);
               continue;
            }

            if ( File.Exists(item) )
            {
               draggedEntites.Add(Constants.FILEENTITY);
            }
         }
      }

      private void TreeWorkArea_Drop(object sender, DragEventArgs e)
      {
         try
         {
            EntityViewModel evm;

            if ( CheckDropTarget(e, out evm) )
            {
               if ( evm != null )
               {
                  string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

                  foreach ( var item in files )
                  {
                     if ( Directory.Exists(item) )
                     {
                        IEntity subEntity = evm.Entity.CreateChild(Constants.FOLDERENTITY, Path.GetFileName(item));
                        subEntity.SetAttribute("SourceFolder", item);
                        continue;
                     }

                     if ( File.Exists(item) )
                     {
                        IEntity subEntity = evm.Entity.CreateChild(Constants.FILEENTITY, Path.GetFileName(item));
                        subEntity.SetAttribute("SourcePath", item);
                     }
                  }
               }
            }
         }
         finally
         {
            draggedEntites = null;
         }
      }

      // Check what kind of entities can be created using the dragsource data
      private bool CheckDropTarget(DragEventArgs e, out EntityViewModel entityVM)
      {
         EntityViewModel evm = null;
         entityVM = null;

         if (draggedEntites == null)
            return false;

         HitTestResult htr = VisualTreeHelper.HitTest(TreeWorkArea, e.GetPosition(TreeWorkArea));

         TreeViewItem tvItem = htr.VisualHit.FindVisualParent<TreeViewItem>();

         if ( null == tvItem )
            return false;

         evm = tvItem.Header as EntityViewModel;

         if ( null == evm )
            return false;

         entityVM = evm;

         return draggedEntites.All((en) => evm.Entity.Schema.SupportedEntityTypes.Contains(en));
      }

      #endregion
   }
}