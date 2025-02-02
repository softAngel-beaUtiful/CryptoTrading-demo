﻿using System;
using System.Collections;
using System.Reflection;

namespace CryptoTrading
{
    /// <summary>
    /// 把枚举值按照指定的文本显示
    /// <remarks>
    /// 通过枚举值的ToString()可以得到变量的文本，
    /// 该特性可提供更充分的文本信息
    /// </remarks>
    /// <example>
    /// [EnumDescription("数字")]
    /// enum NumberEnum
    /// {
    ///		[EnumDescription("一")]
    /// 	One = 1,
    ///		[EnumDescription("二")]
    ///		Two
    /// }
    /// EnumDescription.GetEnumText(typeof(NumberEnum))=="数字"
    /// EnumDescription.GetFieldText(NumberEnum.Two)=="二"
    /// EnumDescription.GetFieldTexts(typeof(NumberEnum))==?
    /// </example>
    /// Author:rongy.huang@gmail.com
    /// Create Date:2015-12-17
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class EnumDescription : Attribute
    {
        private string enumDisplayText;
        private int enumRank;
        private FieldInfo fieldInfo;

        /// <summary>
        /// 描述枚举值
        /// </summary>
        /// <param name="enumDisplayText">描述内容</param>
        /// <param name="enumRank">排列顺序</param>
        public EnumDescription(string enumDisplayText, int enumRank)
        {
            this.enumDisplayText = enumDisplayText;
            this.enumRank = enumRank;
        }

        /// <summary>
        /// 描述枚举值，默认排序为5
        /// </summary>
        /// <param name="enumDisplayText">描述内容</param>
        public EnumDescription(string enumDisplayText)
            : this(enumDisplayText, 5)
        { }
        /// <summary>
        /// 描述枚举的文字
        /// </summary>
        public string EnumDisplayText
        {
            get { return this.enumDisplayText; }
        }

        public int EnumRank
        {
            get { return enumRank; }
        }
        /// <summary>
        /// 枚举成员值
        /// </summary>
        public int EnumValue
        {
            get { return (int)fieldInfo.GetValue(null); }
        }
        /// <summary>
        /// 枚举成员名
        /// </summary>
        public string FieldName
        {
            get { return fieldInfo.Name; }
        }

        #region  枚举描述属性解释的相关函数

        /// <summary>
        /// 排序类型
        /// </summary>
        public enum SortType
        {
            /// <summary>
            ///按枚举顺序默认排序
            /// </summary>
            Default,
            /// <summary>
            /// 按描述值排序
            /// </summary>
            DisplayText,
            /// <summary>
            /// 按排序熵
            /// </summary>
            Rank
        }

        private static System.Collections.Hashtable cachedEnum = new System.Collections.Hashtable();


        /// <summary>
        /// 得到对枚举的描述文本
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <returns></returns>
        public static string GetEnumText(Type enumType)
        {
            EnumDescription[] eds = (EnumDescription[])enumType.GetCustomAttributes(typeof(EnumDescription), false);
            if (eds.Length != 1) return string.Empty;
            return eds[0].EnumDisplayText;
        }

        /// <summary>
        /// 获得指定枚举类型中，指定值的描述文本。
        /// </summary>
        /// <param name="enumValue">枚举值，不要作任何类型转换</param>
        /// <returns>描述字符串</returns>
        public static string GetFieldText(object enumValue)
        {
            EnumDescription[] descriptions = GetFieldTexts(enumValue.GetType(), SortType.Default);
            foreach (EnumDescription ed in descriptions)
            {
                if (ed.fieldInfo.Name == enumValue.ToString()) return ed.EnumDisplayText;
            }
            return string.Empty;
        }

        /// <summary>
        /// 得到枚举类型定义的所有文本，按定义的顺序返回
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        /// <param name="enumType">枚举类型</param>
        /// <returns>所有定义的文本</returns>
        public static EnumDescription[] GetFieldTexts(Type enumType)
        {
            return GetFieldTexts(enumType, SortType.Default);
        }

        /// <summary>
        /// 得到枚举类型定义的所有文本
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        /// <param name="enumType">枚举类型</param>
        /// <param name="sortType">指定排序类型</param>
        /// <returns>所有定义的文本</returns>
        public static EnumDescription[] GetFieldTexts(Type enumType, SortType sortType)
        {
            EnumDescription[] descriptions = null;
            try
            {
                //缓存中没有找到，通过反射获得字段的描述信息

                string name = enumType.FullName;
                if (cachedEnum.Contains(enumType.FullName) == false)
                {
                    FieldInfo[] fields = enumType.GetFields();
                    ArrayList edAL = new ArrayList();
                    foreach (FieldInfo fi in fields)
                    {
                        object[] eds = fi.GetCustomAttributes(typeof(EnumDescription), false);
                        if (eds.Length != 1) continue;
                        ((EnumDescription)eds[0]).fieldInfo = fi;
                        edAL.Add(eds[0]);
                    }

                    cachedEnum.Add(enumType.FullName, (EnumDescription[])edAL.ToArray(typeof(EnumDescription)));
                }
                descriptions = (EnumDescription[])cachedEnum[enumType.FullName];
                if (descriptions.Length <= 0)
                    throw new NotSupportedException("枚举类型[" + enumType.Name + "]未定义属性EnumValueDescription");

                //按指定的属性进行冒泡排序
                for (int m = 0; m < descriptions.Length; m++)
                {
                    //默认不排序
                    if (sortType == SortType.Default) break;

                    for (int n = m; n < descriptions.Length; n++)
                    {
                        EnumDescription temp;
                        bool swap = false;

                        switch (sortType)
                        {
                            case SortType.Default:
                                break;
                            case SortType.DisplayText:
                                if (string.Compare(descriptions[m].EnumDisplayText, descriptions[n].EnumDisplayText) > 0) swap = true;
                                break;
                            case SortType.Rank:
                                if (descriptions[m].EnumRank > descriptions[n].EnumRank) swap = true;
                                break;
                        }

                        if (swap)
                        {
                            temp = descriptions[m];
                            descriptions[m] = descriptions[n];
                            descriptions[n] = temp;
                        }
                    }
                }
            }
            catch { }
            return descriptions;
        }
        #endregion
    }
}
