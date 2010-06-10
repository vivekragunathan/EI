using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using EasyInstall.Interfaces;
using System.IO;
using System.Runtime.InteropServices;

namespace EasyInstallIDE
{
   class EntityTypeToImageSourceConverter : IValueConverter
   {
      #region IValueConverter Members

      static IDictionary<string, BitmapImage> ImageMap = new Dictionary<string, BitmapImage>()
      {
         { EasyInstall.EntityTypes.FOLDERENTITY, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/Folder.png")) },
         { EasyInstall.EntityTypes.FILEENTITY, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/FileEntity.png")) },
         { EasyInstall.EntityTypes.PACKAGEENTITY, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/Package.png")) },

         // Categories
         { EasyInstall.EntityCategories.MISCELLANEOUS, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/Miscellaneous.png")) },
         { EasyInstall.EntityCategories.FILESANDFOLDERS, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/FolderEntity.png")) },
         { EasyInstall.EntityCategories.REGISTRY, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/Registry.png")) }
      };

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      extern static bool DestroyIcon(IntPtr handle);

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         string entityType = string.Empty;

         if ( value is string )
         {
            entityType = value as string;
         }
         else
         {
            IEntity entity = value as IEntity;

            if ( null != entity )
            {
               entityType = entity.Schema.EntityType;

               if ( entityType == EasyInstall.EntityTypes.FILEENTITY )
               {
                  string sourcePath = entity.GetAttribute<string>("SourcePath");
                  System.Drawing.Icon fileIcon = null;
                  try
                  {
                     MemoryStream ms = new MemoryStream();
                     fileIcon = System.Drawing.Icon.ExtractAssociatedIcon(sourcePath);

                     fileIcon.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                     BitmapImage bImg = new BitmapImage();
                     bImg.BeginInit();
                     ms.Seek(0, SeekOrigin.Begin);
                     bImg.StreamSource = ms;

                     bImg.EndInit();
                     
                     return bImg;
                  }
                  finally
                  {
                     if ( fileIcon != null )
                     {
                        DestroyIcon(fileIcon.Handle);
                     }
                  }
               }
            }
         }

         if ( ImageMap.ContainsKey(entityType) )
            return ImageMap[entityType];

         return null;
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}
