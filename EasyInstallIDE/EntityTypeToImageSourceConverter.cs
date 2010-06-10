using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace EasyInstallIDE
{
   class EntityTypeToImageSourceConverter : IValueConverter
   {
      #region IValueConverter Members

      static IDictionary<string, BitmapImage> ImageMap = new Dictionary<string, BitmapImage>()
      {
         { EasyInstall.Constants.FOLDERENTITY, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/Folder.png")) },
         { EasyInstall.Constants.FILEENTITY, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/FileEntity.png")) },
         { EasyInstall.Constants.PACKAGEENTITY, new BitmapImage(new Uri("pack://application:,,,/EasyInstallResources;component/Images/Package.png")) }
      };

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (ImageMap.ContainsKey(value.ToString()))
            return ImageMap[value.ToString()];

         return null;
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}
