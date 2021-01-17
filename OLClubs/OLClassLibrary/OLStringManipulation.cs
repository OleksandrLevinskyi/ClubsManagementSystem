/*
 * OLStringManipulation.cs
 * Description: string manipulation methods in OLClassLibrary
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 11/14/2020
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OLClassLibrary
{
    /// <summary>
    /// static class containing methods for string manipulation
    /// </summary>
    public static class OLStringManipulation
    {
        /// <summary>
        /// extracts all digits from the given string;
        /// null/empty is possible
        /// </summary>
        /// <param name="value">string to observe</param>
        /// <returns>all digits from the given string</returns>
        public static string OLExtractDigits(string value)
        {
            string result = "";
            if (String.IsNullOrEmpty(value) || value.Trim() == "") return "";

            foreach (char c in value)
            {
                if (Char.IsDigit(c)) result += c;
            }
            return result;
        }

        /// <summary>
        /// validates a postal code against regex in the Country table;
        /// null/empty is possible
        /// </summary>
        /// <param name="postalCode">the given postal code</param>
        /// <param name="regexInCountry">postal code regex pattern from the Country table</param>
        /// <returns>if the given postal code matches regex</returns>
        public static bool OLPostalCodeIsValid(string postalCode, string regexInCountry)
        {
            Regex regex = new Regex(regexInCountry);
            if (String.IsNullOrEmpty(postalCode) || postalCode.Trim() == "" || regex.IsMatch(postalCode)) return true;
            return false;
        }

        /// <summary>
        /// changes input string to lower case and trims it;
        /// shifts the first letter of every word to upper case;
        /// removes all redundant spaces;
        /// if null, returns an empty string
        /// </summary>
        /// <param name="value">value to manipulate</param>
        /// <returns>new adjusted string</returns>
        public static string OLCapitilize(string value)
        {
            string result = "";
            if (String.IsNullOrEmpty(value)) return "";

            value = value.ToLower().Trim();
            result += value[0].ToString().ToUpper(); // capitilize the first letter

            for (int i = 1; i < value.Length; i++)
            {
                if (value[i - 1] == ' ')
                {
                    if (value[i] == ' ') continue;
                    result += value[i].ToString().ToUpper();
                }
                else result += value[i];
            }
            return result;
        }

        /// <summary>
        /// inserts space in the postal code at the specified position;
        /// removes all redundant spaces
        /// </summary>
        /// <param name="postalCode">postal code to manipulate on</param>
        /// <param name="position">index to insert the space into; if outside the bouds, ignored</param>
        /// <returns>modified postal code</returns>
        public static string OLInsertSpaceInPostal(string postalCode, int position)
        {
            string result = "";
            if (postalCode == null) return result;

            postalCode = postalCode.Trim();

            for (int i = 0; i < postalCode.Length; i++)
            {
                if (i == position) result += (" " + postalCode[i]);
                else if (postalCode[i] != ' ') result += postalCode[i];
            }
            return result;
        }
    }
}
