// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace CdpValidator
{
    /// <summary>
    /// Provides extension methods for string display and error handling in validation.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Returns a display-friendly string for null or empty values.
        /// </summary>
        /// <param name="str">The string to display.</param>
        /// <returns>"&lt;null&gt;" if null, "&lt;empty&gt;" if empty, otherwise the original string.</returns>
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

        /// <summary>
        /// Adds a validation error to the list of errors.
        /// </summary>
        /// <param name="errors">The list of validation errors.</param>
        /// <param name="uri">The relevant URI.</param>
        /// <param name="msg">The error message.</param>
        /// <param name="err">The error details.</param>
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

        /// <summary>
        /// Adds a validation error with an exception to the list of errors.
        /// </summary>
        /// <param name="errors">The list of validation errors.</param>
        /// <param name="uri">The relevant URI.</param>
        /// <param name="msg">The error message.</param>
        /// <param name="ex">The exception to include.</param>
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

        /// <summary>
        /// Adds a validation warning to the list of errors.
        /// </summary>
        /// <param name="errors">The list of validation errors.</param>
        /// <param name="uri">The relevant URI.</param>
        /// <param name="msg">The warning message.</param>
        /// <param name="wrn">The warning details.</param>
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
