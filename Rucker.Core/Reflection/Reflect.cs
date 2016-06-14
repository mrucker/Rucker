using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;

using Rucker.Convert;

namespace Rucker.Reflection
{
    public static class Reflect<T>
    {        
        public static string DisplayName<TReturn>(Expression<Func<T, TReturn>> expression)
        {
            return Reflect.DisplayName(PropertyInfo(expression));
        }

        public static string DisplayName(string property)
        {
            return Reflect.DisplayName(typeof(T), property);
        }

        public static string DisplayNameSafe<TReturn>(Expression<Func<T, TReturn>> expression)
        {
            try
            {
                return DisplayName(expression);
            }
            catch
            {
                return null;
            }
        }

        public static string DisplayNameSafe(string property)
        {
            try
            {
                return DisplayName(property);
            }
            catch
            {
                return null;
            }
        }

        private static PropertyInfo PropertyInfo<TReturn>(Expression<Func<T, TReturn>> expression)
        {
            //Unary check handles the case where an expression comes in as Convert(originalexpression)
            var propertyInfo = expression.Body.ToOrDefault<MemberExpression>()?.Member.ToOrDefault<PropertyInfo>() ?? expression.Body.ToOrDefault<UnaryExpression>()?.Operand.ToOrDefault<MemberExpression>()?.Member.ToOrDefault<PropertyInfo>();

            if (propertyInfo != null)
            {
                return propertyInfo;
            }

            throw new ArgumentException("Couldn't determine a property from the given expression");
        }
    }

    public static class Reflect
    {
        public static object TryValue(object obj, string property)
        {
            return obj?.GetType().GetProperty(property)?.GetValue(obj);
        }

        public static object Value(object obj, string property)
        {
            return obj.GetType().GetProperty(property).GetValue(obj);
        }

        public static string DisplayName(object obj, string property)
        {
            return DisplayName(obj.GetType(), property);            
        }

        public static string DisplayName(Type type, string property)
        {
            return DisplayName(type.GetProperty(property));
        }

        public static string DisplayName(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().SingleOrDefault()?.Name
                ?? propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().SingleOrDefault()?.DisplayName;
        }

        public static object Default(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }

    
}
