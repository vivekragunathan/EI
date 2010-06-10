using System;

using System.Collections.Generic;

using System.Text;

using System.Windows.Media;

using System.Runtime.InteropServices;

using System.Media;

using System.Windows.Data;

using System.Windows.Media.Imaging;

namespace EasyInstallIDE
{
   public enum IconSize
   {

      Small,

      Large

   }

   /// <summary>

   ///

   /// Extracts the icon associated with any file on your system.

   /// Author: WidgetMan http://softwidgets.com

   ///

   /// </summary>

   /// <remarks>

   ///

   /// Class requires the IconSize enumeration that is implemented in this

   /// same file. For best results, draw an icon from within a control's Paint

   /// event via the e.Graphics.DrawIcon method.

   ///

   /// </remarks>

   public class ExtractIcon : IDisposable
   {

      private const uint SHGFI_ICON = 0x100;

      private const uint SHGFI_LARGEICON = 0x0;

      private const uint SHGFI_SMALLICON = 0x1;

      private const int FILE_ATTRIBUTE_NORMAL = 0x80;

      private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

      [StructLayout(LayoutKind.Sequential)]

      private struct SHFILEINFO
      {
         public IntPtr hIcon;

         public IntPtr iIcon;

         public uint dwAttributes;

         [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]

         public string szDisplayName;

         [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]

         public string szTypeName;

      };

      [DllImport("shell32.dll")]

      private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

      public ExtractIcon()
      {

      }

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      extern static bool DestroyIcon(IntPtr handle);

      IntPtr hIcon = IntPtr.Zero;

      public System.Drawing.Icon Extract(string File, IconSize Size)
      {
         if ( hIcon != IntPtr.Zero )
         {
            DestroyIcon(hIcon);
            hIcon = IntPtr.Zero;
         }

         SHFILEINFO shinfo = new SHFILEINFO();

         if ( Size == IconSize.Large )
         {
            hIcon = SHGetFileInfo(File, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_USEFILEATTRIBUTES);
         }

         else
         {
            hIcon = SHGetFileInfo(File, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);
         }

         return System.Drawing.Icon.FromHandle(shinfo.hIcon);
      }

      public System.Drawing.Icon Extract(string File)
      {
         return this.Extract(File, IconSize.Large);
      }

      #region IDisposable Members

      public void Dispose()
      {
         if ( hIcon != IntPtr.Zero )
         {
            DestroyIcon(hIcon);
            hIcon = IntPtr.Zero;
         }
      }

      #endregion
   }

}