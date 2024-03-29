﻿//-----------------------------------------------------------------------
// <copyright file="IntegerConverter.cs" company="Visual JSON Editor">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>http://visualjsoneditor.codeplex.com/license</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;

namespace ExcelPatternTool.Converter.JsonEditorConverter
{
    public class IntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.ToString() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return long.Parse(value.ToString());
            }
            catch
            {
                return default(long);
            }
        }
    }
}