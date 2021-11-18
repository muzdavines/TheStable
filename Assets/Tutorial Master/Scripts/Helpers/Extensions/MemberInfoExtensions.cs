using System;
using System.Reflection;

namespace HardCodeLab.TutorialMaster
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Determines whether a specified member has an attribute of type
        /// </summary>
        /// <param name="member">The member which will be checked.</param>
        /// <returns>
        /// Returns true if a specified member has a given attribute
        /// </returns>
        public static bool HasAttribute<TAttribute>(this MemberInfo member)
            where TAttribute : Attribute
        {
            return member.GetCustomAttributes(typeof(TAttribute), false).Length > 0;
        }
    }
}