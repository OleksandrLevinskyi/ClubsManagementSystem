/*
 * OLEmailAnnotation.cs
 * Description: Custom Email annotation to validate email address
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 11/14/2020
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;

namespace OLClassLibrary
{
    /// <summary>
    /// custom annotation to validate an email address
    /// </summary>
    public class OLEmailAnnotation : ValidationAttribute
    {
        /// <summary>
        /// validates the given email;
        /// returns an error message with the field name if invalid;
        /// utilizes MailAddress class (from System.Net.Mail library);
        /// email is optional (null/empty is possible)
        /// </summary>
        /// <param name="value">property value</param>
        /// <param name="validationContext">context of the validation procedure</param>
        /// <returns>success/error message</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || value.ToString() == "")
            {
                return ValidationResult.Success;
            }

            try
            {
                MailAddress email = new MailAddress(value.ToString());
                return ValidationResult.Success;
            }
            catch (Exception ex)
            {
                return new ValidationResult(String.Format("{0} is invalid: {1}", validationContext.DisplayName, ex.GetBaseException().Message));
            }
        }
    }
}
