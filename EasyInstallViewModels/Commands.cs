using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EasyInstall.ViewModels
{
   public class Commands
   {
      static RoutedUICommand addFolderEntity;
      static RoutedUICommand addFileEntity;
      static RoutedUICommand removeEntity;
      static RoutedUICommand removeAllEntities;
      static RoutedUICommand renameEntity;
      
      static RoutedUICommand buildPackage;

      static Commands()
      {
         // Initialize the commands

         InputGestureCollection inputs = new InputGestureCollection();
         inputs.Add(new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+F"));
         addFolderEntity = new RoutedUICommand("Add Folder", "AddFolder", typeof(Commands), inputs);

         inputs = new InputGestureCollection();
         inputs.Add(new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+L"));
         addFileEntity = new RoutedUICommand("Add File", "AddFile", typeof(Commands), inputs);

         inputs = new InputGestureCollection();
         inputs.Add(new KeyGesture(Key.Delete, ModifierKeys.Control, "Ctrl+Del"));
         removeEntity = new RoutedUICommand("Remove Entity", "RemoveEntity", typeof(Commands), inputs);

         inputs = new InputGestureCollection();
         inputs.Add(new KeyGesture(Key.Delete, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+Del"));
         removeAllEntities = new RoutedUICommand("Remove All Entities", "RemoveAllEntities", typeof(Commands), inputs);

         inputs = new InputGestureCollection();
         inputs.Add(new KeyGesture(Key.F7, ModifierKeys.None, "F7"));
         buildPackage = new RoutedUICommand("Build Package", "BuildPackage", typeof(Commands), inputs);

         inputs = new InputGestureCollection();
         inputs.Add(new KeyGesture(Key.F2, ModifierKeys.None, "F2"));
         renameEntity = new RoutedUICommand("Rename Entity", "RenameEntity", typeof(Commands), inputs);
      }

      public static RoutedUICommand AddFolderEntity
      {
         get { return addFolderEntity; }
      }

      public static RoutedUICommand AddFileEntity
      {
         get { return addFileEntity; }
      }

      public static RoutedUICommand RemoveEntity
      {
         get { return removeEntity; }
      }

      public static RoutedUICommand RemoveAllEntities
      {
         get { return removeAllEntities; }
      }

      public static RoutedUICommand RenameEntity
      {
         get { return renameEntity; }
      }

      public static RoutedCommand BuildPackage
      {
         get { return buildPackage; }
      }

      public static RoutedUICommand GetRoutedCommandForEntity(string entity)
      {
         switch ( entity )
         {
            case EasyInstall.EntityTypes.FILEENTITY:
               return AddFileEntity;
            case EasyInstall.EntityTypes.FOLDERENTITY:
               return AddFolderEntity;
         }

         return null;
      }

      #region Command Binding Executed
      public void AddFileEntity_Executed(object sender, ExecutedRoutedEventArgs e)
      {

      }
      #endregion
   }
}
