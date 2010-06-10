using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Controls;

namespace EasyInstall.ViewModels
{
   public class ViewModelBase : INotifyPropertyChanged
   {
      public class PropertyChangingEventArgsEx : PropertyChangingEventArgs
      {
         public PropertyChangingEventArgsEx(string propertyName, object oldValue, object newValue)
            : base(propertyName)
         {
            this.OldValue = oldValue;
            this.NewValue = newValue;
         }

         public object OldValue { get; internal set; }
         public object NewValue { get; set; }
      }
      private const string SetPropertyPrefix = "set_";

      #region INotifyPropertyChanged Members

      public event PropertyChangedEventHandler PropertyChanged;

      public event PropertyChangingEventHandler PropertyChanging;

      #endregion

      // Call this method from within a set accessor of a property
      protected void NotifyCurrentPropertyChanged()
      {
         if ( PropertyChanged == null )
         {
            return;
         }

         StackTrace st = new StackTrace(1);
         StackFrame sf = st.GetFrame(0);
         MethodBase mb = sf.GetMethod();

         if ( mb.MemberType == MemberTypes.Method )
         {
            // if the calling method is not an accessor of a property, do nothing
            string methodName = mb.Name;
            if ( methodName.StartsWith(ViewModelBase.SetPropertyPrefix) )
            {
               // We are bothered only with set accessors
               string propertyName = methodName.Substring(ViewModelBase.SetPropertyPrefix.Length);
               NotifyPropertyChanged(propertyName);
            }
         }
      }

      // The conventional NotifyPropertyChanged
      protected void NotifyPropertyChanged(string propertyName)
      {
         if ( PropertyChanged != null )
         {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      // The conventional NotifyPropertyChanged
      protected void NotifyPropertyChanging(string propertyName, object oldValue, object newValue)
      {
         if ( PropertyChanging != null )
         {
            PropertyChanging(this, new PropertyChangingEventArgsEx(propertyName, oldValue, newValue));
         }
      }
   }

   /// <summary>
   /// A validation rule that makes use of tghe business objects IDataErrorInfo interface is applied at the group level
   /// </summary>
   public class RowDataInfoValidationRule : ValidationRule
   {
      public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
      {
         BindingGroup group = (BindingGroup)value;

         StringBuilder error = null;
         foreach ( var item in group.Items )
         {
            // aggregate errors
            IDataErrorInfo info = item as IDataErrorInfo;
            if ( info != null )
            {
               if ( !string.IsNullOrEmpty(info.Error) )
               {
                  if ( error == null )
                     error = new StringBuilder();
                  error.Append((error.Length != 0 ? ", " : "") + info.Error);
               }
            }
         }

         if ( error != null )
            return new ValidationResult(false, error.ToString());

         return ValidationResult.ValidResult;
      }
   }
}
