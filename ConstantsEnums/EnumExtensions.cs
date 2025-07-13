using System;
using System.ComponentModel;
using System.Reflection;

namespace P3D_Scenario_Generator.ConstantsEnums
{
    /// <summary>
    /// Provides extension methods for Enum types.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the Description attribute value for an enum member.
        /// If no Description attribute is found, the enum member's name (as a string) is returned.
        /// </summary>
        /// <param name="enumValue">The enum member to get the description for.</param>
        /// <returns>The string value of the Description attribute, or the enum member's name.</returns>
        public static string GetDescription(this Enum enumValue)
        {
            FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
            }
            return enumValue.ToString(); // Fallback to string representation if no description
        }
    }
}
