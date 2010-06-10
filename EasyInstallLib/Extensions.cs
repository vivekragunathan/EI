using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace EasyInstall
{
   public static class Extensions
   {
      public static void CopyTo(this Stream src, Stream dest)
      {
         int size = (src.CanSeek) ? Math.Min((int)(src.Length - src.Position), 0x2000) : 0x2000;
         byte[] buffer = new byte[size];
         int n;
         do
         {
            n = src.Read(buffer, 0, buffer.Length);
            dest.Write(buffer, 0, n);
         } while (n != 0);
      }

      public static void CopyTo(this MemoryStream src, Stream dest)
      {
         dest.Write(src.GetBuffer(), (int)src.Position, (int)(src.Length - src.Position));
      }

      public static void CopyTo(this Stream src, MemoryStream dest)
      {
         if (src.CanSeek)
         {
            int pos = (int)dest.Position;
            int length = (int)(src.Length - src.Position) + pos;
            dest.SetLength(length);

            while (pos < length)
               pos += src.Read(dest.GetBuffer(), pos, length - pos);
         }
         else
            src.CopyTo((Stream)dest);
      }

      public static void CopyStream(Stream input, Stream output)
      {
         using (StreamReader reader = new StreamReader(input))
         using (StreamWriter writer = new StreamWriter(output))
         {
            writer.Write(reader.ReadToEnd());
         }
      }

      public static void DisposeAndRemove<T>(this ICollection<T> collection, T item)
         where T: class
      {
         if ( collection.Contains(item) )
         {
            IDisposable disposable = item as IDisposable;
            if ( null != disposable )
            {
               disposable.Dispose();
            }

            collection.Remove(item);
         }
      }

      public static void DisposeAndClear<T>(this ICollection<T> collection)
         where T : class
      {
         IEnumerator<T> erator = collection.GetEnumerator();
         while ( erator.MoveNext() )
         {
            IDisposable disposable = erator.Current as IDisposable;

            if ( null != disposable )
            {
               disposable.Dispose();
            }
         }
         collection.Clear();
      }
   }
}
