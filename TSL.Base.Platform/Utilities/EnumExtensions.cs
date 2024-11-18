using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace TSL.Base.Platform.Utilities
{
    /// <summary>
    /// 擴充Enum方法
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Enum 擴充方法, 取得 Enum Value 並轉成字串
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>Enum Value</returns>
        public static string ToNumberString(this Enum enumType)
        {
            return Convert.ToInt32(enumType, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Enum 擴充方法, 取得 Enum Value 並轉成字串, 字串長度不足時, 左補0
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <param name="length">回傳字串長度</param>
        /// <returns>Enum Value</returns>
        public static string ToNumberString(this Enum enumType, int length)
        {
            string ret = Convert.ToInt32(enumType, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
            string strzero = "";
            for (int i = 0; i < length; i++)
            {
                strzero += "0";
            }

            return strzero.Substring(0, length - ret.Length) + ret;
        }

        /// <summary>
        /// Enum 擴充方法, 取得 Enum Value 並轉成單一文字
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>Enum Value</returns>
        public static char ToChar(this Enum enumType)
        {
            return Convert.ToChar(enumType, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Enum 擴充方法, 取得 Enum Value 並轉成字串
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>Enum Value</returns>
        public static string ToCharString(this Enum enumType)
        {
            return Convert.ToChar(enumType, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Enum 擴充方法, 取得 Enum Value 並轉成數字
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>Enum Value</returns>
        public static int ToNumberValue(this Enum enumType)
        {
            return Convert.ToInt32(enumType, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Enum 擴充方法, 取得 Enum Value 並轉成布林值
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>Enum Value</returns>
        public static bool ToBooleanValue(this Enum enumType)
        {
            return Convert.ToBoolean(enumType.ToNumberValue());
        }

        /// <summary>
        /// Enum 擴充方法, 取得 Enum Value 並轉成多個文字字串
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>Enum Value</returns>
        public static string ToNumberMutiString(this Enum enumType)
        {
            return Convert.ToInt32(enumType, CultureInfo.CurrentCulture).ToString("D3", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Enum 擴充方法，GetDisplayName
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>Enum DisplayName</returns>
        public static string GetEnumDisplayName(this Enum enumType)
        {
            var i = enumType.GetType().GetMember(enumType.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>();

            return i?.Name ?? string.Empty;
        }

        /// <summary>
        /// Enum 擴充方法。Get enum key name.
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>Enum DisplayName</returns>
        public static string GetEnumKeyName(this Enum enumType)
        {
            return Enum.GetName(enumType.GetType(), enumType) ?? string.Empty;
        }

        /// <summary>
        /// Enum 擴充方法，取得 EnumMemberAttribute 的資料值
        /// </summary>
        /// <typeparam name="T">回傳形態</typeparam>
        /// <param name="enumType">Enum</param>
        /// <returns>EnumMemberAttribute 資料值</returns>
        public static T MemberValue<T>(this Enum enumType)
            where T : struct, IConvertible
        {
            var i = enumType.GetType().GetMember(enumType.ToString())
                .First()
                .GetCustomAttribute<EnumMemberAttribute>();

            return (T)Convert.ChangeType(i.Value, typeof(T), CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Enum 擴充方法，取得 EnumMemberAttribute 的資料值
        /// </summary>
        /// <param name="enumType">Enum</param>
        /// <returns>EnumMemberAttribute 資料值</returns>
        public static string MemberValue(this Enum enumType)
        {
            return enumType.GetType().GetMember(enumType.ToString())
                .First()
                .GetCustomAttribute<EnumMemberAttribute>().Value;
        }

        /// <summary>
        /// Enum 轉為 List
        /// </summary>
        /// <typeparam name="T">Enum</typeparam>
        /// <returns>List of Enum</returns>
        public static List<T> ToList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList<T>();
        }

        /// <summary>
        /// Enum 轉為 GetSelectList
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns>List of Enum</returns>
        public static List<SelectListItem> GetSelectList<T>()
            where T : struct
        {
            List<SelectListItem> itemList = new List<SelectListItem>();

            foreach (T eVal in Enum.GetValues(typeof(T)))
            {
                FieldInfo? fi = eVal.GetType().GetField(eVal.ToString());
                DisplayAttribute[] attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);
                itemList.Add(new SelectListItem { Value = Convert.ToInt32(eVal, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture), Text = attributes[0].Name });
            }

            return itemList;
        }

        /// <summary>
        /// Enum 轉為 Dynamic KeyValuepair
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns></returns>
        public static List<dynamic> GetKeyValue<T>()
            where T : struct
        {
            List<dynamic> itemList = new List<dynamic>();

            foreach (T eVal in Enum.GetValues(typeof(T)))
            {
                FieldInfo fi = eVal.GetType().GetField(eVal.ToString());
                DisplayAttribute[] attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);
                itemList.Add(new { Key = attributes[0].Name, Value = Convert.ToInt32(eVal, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture) });
            }

            return itemList;
        }

        /// <summary>
        /// Enum 轉為 Dictionary
        /// </summary>
        /// <typeparam name="T">Enum</typeparam>
        /// <returns></returns>
        public static Dictionary<int, string> GetDictionary<T>()
            where T : struct
        {
            var itemDict = new Dictionary<int, string>();

            foreach (T eVal in Enum.GetValues(typeof(T)))
            {
                FieldInfo fi = eVal.GetType().GetField(eVal.ToString());
                DisplayAttribute[] attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);
                itemDict.Add(Convert.ToInt32(eVal, CultureInfo.CurrentCulture), attributes[0].Name);
            }

            return itemDict;
        }
    }
}
