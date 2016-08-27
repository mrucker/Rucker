using System;
using System.Diagnostics;
using System.ComponentModel;
using Rucker.Core;

namespace Rucker.Core
{    
    public static class Convert
    {
        #region Public Methods
        public static object To(this object value, Type type)
        {
            return SafeTo(type, value);
        }

        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        public static object ToOrDefault(this object value, Type type)
        {
            try { return UnsafeTo(type, value); } catch { return (type.IsValueType) ? Activator.CreateInstance(type) : null; }
        }

        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        public static bool Can(this object value, Type type)
        {
            try { UnsafeTo(type, value); } catch {  return false; } return true;
        }

        public static T To<T>(this object value)
        {
            return SafeTo<T>(value);
        }

        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        public static T ToOrDefault<T>(this object value)
        {
            try { return UnsafeTo<T>(value); } catch { return default(T); }
        }

        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        public static bool Can<T>(this object value)
        {
            try { UnsafeTo<T>(value); } catch { return false; } return true;
        }
        #endregion

        #region Private Methods
        /// <remarks>
        /// Maybe a little terrifying at first, and it may seem overcomplicated, but everyone of these if statements 
        /// was added to address a specific situation. It is suggested that great care is taken when modifying this code.
        /// </remarks>
        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        private static object UnsafeTo(Type type, object value)
        {
            //Checks for all known types where null isn't a valid return value
            if (!IsNullable(type) && value == null)
            {
                throw new Exception($"null is not a valid value for {type}");
            }

            //If the tye is nullable and the value is null return null
            if (IsNullable(type) && value == null)
            {
                return null;
            }
            
            //If the value's type equals the conversion type we can simply pass back the value.
            //If we don't handle this case explicitly both System.Convert.ChangeType() and TypeDescriptor.GetConverter(type).ConvertFrom() throw errors.
            if (value.GetType() == type || type.IsInstanceOfType(value))
            {
                return value;
            }

            //If the type is a generic type then we want to actually convert to the underlying type
            //e.g. if value is the string "10" and type == Nullable<int> then we want to convert value to the integer 10 and typecast that to a Nullable<int>
            if (IsNullableGeneric(type))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            //Do this check again for the same reason we did it above. We do this again incase type changed from a Nullable<> type to its underlying type.
            //e.g. if value is the integer 10 and type was passed in as Nullable<int> then the first value.GetType() == type would fail because Int32 != Nullable<Int32>
            //     however this time around type will be equal to simply Int32 because we pulled out the underlying type above. So this will pass because Int32 == Int32.
            if (value.GetType() == type)
            {
                return value;
            }

            //For some reason TypeDescriptor.GetConverter(type).ConvertFrom(value) returns DateTime.MinValue when the value is spaces so we need to manually handle
            if (IsDateTime(type) && value.ToString().IsSpaces())
            {
                throw new Exception($"'' is not a valid value for {type}");
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, value.ToString());
            }

            //if the type conversion wasn't handled anywhere above then we use .net's provided type conversion functionality to try and convert.
            //this isn't guaranteed to work (e.g. "a" simply can't be converted to an Int32), but all *known* valid cases that would fail here have been handled above.
            if (TypeDescriptor.GetConverter(type).CanConvertFrom(value.GetType()))
            {
                try { return TypeDescriptor.GetConverter(type).ConvertFrom(value); } catch { return System.Convert.ChangeType(value, type); }
            }
            else
            {
                return System.Convert.ChangeType(value, type);
            }            
        }
        #endregion

        #region Private Helpers
        private static object SafeTo(Type type, object value)
        {
            try
            {
                return UnsafeTo(type, value);
            }
            catch
            {
                throw new Exception($"{value?.ToString().Trim() ?? "null"} is not a valid value for {type}");
            }
        }

        private static T SafeTo<T>(object value)
        {
            var typeOfT  = typeof(T);
            var valueAsT = SafeTo(typeOfT, value);

            return (IsNullable(typeOfT) && valueAsT == null) ? default(T) : (T)valueAsT;
        }

        [DebuggerHidden, DebuggerStepThrough, DebuggerNonUserCode]
        private static T UnsafeTo<T>(object value)
        {
            var typeOfT  = typeof(T);
            var valueAsT = UnsafeTo(typeOfT, value);

            return (IsNullable(typeOfT) && valueAsT == null) ? default(T) : (T)valueAsT;
        }

        /// <summary>
        /// Returns true if the type is a DateTime
        /// </summary>
        private static bool IsDateTime(Type type)
        {
            return type == typeof(DateTime);
        }

        /// <summary>
        /// Returns true if a value of the given type can be assigned null
        /// </summary>
        private static bool IsNullable(Type type)
        {
            return !type.IsValueType || IsNullableGeneric(type);
        }

        /// <summary>
        /// Returns true if the given type == typeof(Nullable)
        /// </summary>
        private static bool IsNullableGeneric(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        #endregion
    }
}