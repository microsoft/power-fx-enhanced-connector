// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace CdpValidator
{
    internal static class Extensions
    {
        internal static string Display(string str)
        {
            if (str == null)
            {
                return "<null>";
            }

            if (string.IsNullOrEmpty(str))
            {
                return "<empty>";
            }

            return str;
        }

        public static void AddError(this List<ValidationError> errors, string uri, string msg, string err)
        {
            errors.Add(new ValidationError()
            {
                Uri = uri,
                Message = msg,
                Details = err,
                Exception = null,
                Category = ValidationCategory.Error
            });
        }

        public static void AddException(this List<ValidationError> errors, string uri, string msg, Exception ex)
        {
            errors.Add(new ValidationError()
            {
                Uri = uri,
                Message = msg,
                Details = ex.Message,
                Exception = ex,
                Category = ValidationCategory.Error
            });
        }

        public static void AddWarning(this List<ValidationError> errors, string uri, string msg, string wrn)
        {
            errors.Add(new ValidationError()
            {
                Uri = uri,
                Message = msg,
                Details = wrn,
                Exception = null,
                Category = ValidationCategory.Warning
            });
        }
    }
}
