
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Temp
{
   class Program
   {
      static void Main()
      {
         Foo foo = new Foo
         {
            Values =
                  {
                        { "abc", "def" },
                        { "ghi", "jkl" }
                  }
         };

         XmlSerializer ser = new XmlSerializer(typeof(Foo));
         StringWriter writer = new StringWriter();
         ser.Serialize(writer, foo);
         string xml = writer.ToString();

         StringReader reader = new StringReader(xml);
         Foo newFoo = (Foo)ser.Deserialize(reader);
         foreach (KeyValuePair<string, string> pair in newFoo.Values)
         {
            Console.WriteLine(pair.Key + ": " + pair.Value);
         }
      }
   }

   [Serializable]
   public class Foo
   {
      private readonly Dictionary<string, string> values
         = new Dictionary<string, string>();

      [XmlIgnore]
      public Dictionary<string, string> Values
      {
         get
         {
            return values;
         }
      }

      [EditorBrowsable(EditorBrowsableState.Never)]
      [XmlArray(ElementName = "Values")]
      [XmlArrayItem(ElementName = "Add", Type = typeof(StringPair))]
      public StringPairList ValuesProxy
      {
         get { return new StringPairList(values); }
      }
   }

   [Serializable]
   public class StringPairList : IList<StringPair>
   {
      private readonly IDictionary<string, string> parent;
      public StringPairList(IDictionary<string, string> parent)
      {
         if (parent == null) throw new ArgumentNullException("parent");
         this.parent = parent;
      }

      #region IList<StringPair> Members

      public int IndexOf(StringPair item)
      {
         throw new NotImplementedException();
      }

      public void Insert(int index, StringPair item)
      {
         throw new NotImplementedException();
      }

      public void RemoveAt(int index)
      {
         throw new NotImplementedException();
      }

      public StringPair this[int index]
      {
         get { throw new NotImplementedException(); }
         set { throw new NotImplementedException(); }
      }

      #endregion

      #region ICollection<StringPair> Members

      public void Add(StringPair item)
      {
         parent.Add(item.Key, item.Value);
      }

      public void Clear()
      {
         parent.Clear();
      }

      public bool Contains(StringPair item)
      {
         throw new NotImplementedException();
      }

      public void CopyTo(StringPair[] array, int arrayIndex)
      {
         throw new NotImplementedException();
      }

      public int Count
      {
         get { return parent.Count; }
      }

      public bool IsReadOnly
      {
         get { return false; }
      }

      public bool Remove(StringPair item)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region IEnumerable<StringPair> Members

      public IEnumerator<StringPair> GetEnumerator()
      {
         foreach (var pair in parent)
         {
            yield return new StringPair
            {
               Key = pair.Key,
               Value = pair.Value
            };
         }
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator
      System.Collections.IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      #endregion
   }

   [Serializable]
   public class StringPair
   {
      [XmlAttribute]
      public string Key { get; set; }
      [XmlAttribute]
      public string Value { get; set; }
   }
}