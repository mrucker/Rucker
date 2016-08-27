using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;

namespace Rucker.Core
{
    public static class Reflect<T>
    {
        public static string Display<TReturn>(Expression<Func<T, TReturn>> expression)
        {
            return Reflect.Display(PropertyInfo(expression));
        }

        public static string Display(string property)
        {
            return Reflect.Display(typeof(T), property);
        }

        public static string DisplayOrDefault<TReturn>(Expression<Func<T, TReturn>> expression)
        {
            try
            {
                return Display(expression);
            }
            catch
            {
                return null;
            }
        }

        public static string DisplayOrDefault(string property)
        {
            try
            {
                return Display(property);
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
        public static object ValueOrNull(object obj, string property)
        {
            return obj?.GetType().GetProperty(property)?.GetValue(obj);
        }

        public static object Value(object obj, string property)
        {
            return obj.GetType().GetProperty(property).GetValue(obj);
        }

        public static string Display(object obj, string property)
        {
            return Display(obj.GetType(), property);            
        }

        public static string Display(Type type, string property)
        {
            return Display(type.GetProperty(property));
        }

        public static string Display(PropertyInfo propertyInfo)
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
