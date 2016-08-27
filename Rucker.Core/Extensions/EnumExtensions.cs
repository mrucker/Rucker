using System;
using System.ComponentModel;

namespace Rucker.Core
{
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum @enum)
        {
            var fieldInfo = @enum.GetType().GetField(@enum.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            return (descriptionAttributes == null) ? "" : (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Description : @enum.ToString();
        }
    }
}