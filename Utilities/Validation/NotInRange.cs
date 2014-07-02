﻿/*
Copyright (c) 2014 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Utilities.DataTypes;
using Utilities.DataTypes.Comparison;

namespace Utilities.Validation
{
    /// <summary>
    /// Not in range attribute
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class NotInRangeAttribute : ValidationAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Max">Max value</param>
        /// <param name="Min">Min value</param>
        /// <param name="ErrorMessage">Error message</param>
        public NotInRangeAttribute(object Min, object Max, string ErrorMessage = "")
            : base(string.IsNullOrEmpty(ErrorMessage) ? "{0} is between {1} and {2}" : ErrorMessage)
        {
            this.Min = (IComparable)Min;
            this.Max = (IComparable)Max;
        }

        /// <summary>
        /// Max value to compare to
        /// </summary>
        public IComparable Max { get; private set; }

        /// <summary>
        /// Min value to compare to
        /// </summary>
        public IComparable Min { get; private set; }

        /// <summary>
        /// Formats the error message
        /// </summary>
        /// <param name="name">Property name</param>
        /// <returns>The formatted string</returns>
        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, ErrorMessageString, name, Min.ToString(), Max.ToString());
        }

        /// <summary>
        /// Determines if the property is valid
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="validationContext">Validation context</param>
        /// <returns>The validation result</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            GenericComparer<IComparable> Comparer = new GenericComparer<IComparable>();
            IComparable MaxValue = (IComparable)Max.To<object>(value.GetType());
            IComparable MinValue = (IComparable)Min.To<object>(value.GetType());
            IComparable TempValue = value as IComparable;
            return (Comparer.Compare(MaxValue, TempValue) >= 0
                    && Comparer.Compare(TempValue, MinValue) >= 0) ?
                new ValidationResult(FormatErrorMessage(validationContext.DisplayName)) :
                ValidationResult.Success;
        }
    }
}