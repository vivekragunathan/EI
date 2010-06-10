using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EasyInstallIDE
{
   static class Extensions
   {
      public static childItem FindVisualChild<childItem>(this DependencyObject obj)
                                     where childItem : DependencyObject
      {
         for ( int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++ )
         {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);
            if ( child != null && child is childItem )
               return (childItem)child;
            else
            {
               childItem childOfChild = FindVisualChild<childItem>(child);
               if ( childOfChild != null )
                  return childOfChild;
            }
         }
         return null;
      }

      public static TParent FindVisualParent<TParent>(this DependencyObject obj)
                                     where TParent : DependencyObject
      {
         DependencyObject depObj = obj;
         do
         {
            depObj = VisualTreeHelper.GetParent(depObj);

            if ( depObj is TParent )
               return (TParent)depObj;
         }
         while ( depObj != null );

         return null;
      }
   }
}
