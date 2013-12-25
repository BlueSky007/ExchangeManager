using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ManagerConsole.Helper;
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;

namespace ManagerConsole.ViewModel
{
    public class VmBase : PropertyChangedNotifier
    {
        public static bool IsValid(DependencyObject obj)
        {
            return !Validation.GetHasError(obj) && LogicalTreeHelper.GetChildren(obj).OfType<DependencyObject>().All(child => VmBase.IsValid(child));
        }
        public static void GetUpdates(Type type, object oldObject, object newObject, Dictionary<string, object> updates)
        {
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Name != FieldSR.Id)
                {
                    object oldValue = type.GetProperty(propertyInfo.Name).GetValue(oldObject, null);
                    object newValue = type.GetProperty(propertyInfo.Name).GetValue(newObject, null);
                    if (newValue != null && !newValue.Equals(oldValue) || newValue == null && oldValue != null)
                    {
                        updates.Add(propertyInfo.Name, newValue);
                    }
                }
            }
        }

        private IMetadataObject _MetadataObject;

        public VmBase(IMetadataObject metadataObject)
        {
            this._MetadataObject = metadataObject;
        }
        public void ApplyChangeToUI(Dictionary<string, object> fieldAndValues)
        {
            this._MetadataObject.Update(fieldAndValues);
            foreach (string key in fieldAndValues.Keys)
            {
                this.OnPropertyChanged(key);
            }
        }

        public void ApplyChangeToUI(string field, object value)
        {
            this._MetadataObject.Update(field, value);
            this.OnPropertyChanged(field);
        }

        public void SubmitChange(MetadataType metadataType, string filed, object value)
        {
            ConsoleClient.Instance.UpdateMetadataObjectField(metadataType, this._MetadataObject.Id, filed, value, delegate(bool success)
            {
                if (success)
                {
                    this.ApplyChangeToUI(filed, value);
                }
                else
                {
                    base.OnPropertyChanged(filed);
                }
            });
        }
    }
}
