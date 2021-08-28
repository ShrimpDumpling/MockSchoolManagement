using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using MockSchoolManagement.Models.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace MockSchoolManagement.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取特性DisplayAttribute的Name的扩展方法
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string GetDisplayName(this System.Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo!=null&&memInfo.Length>0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(
                    typeof(DisplayAttribute), true);
                if (attrs!=null && attrs.Length>0)
                {
                    return ((DisplayAttribute)attrs[0]).Name;
                }
            }
            return en.ToString();
        }
    }
}
