/* 
 * OLNameAddressMetadata.cs
 * Description: NameAddress partial class and metadata
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 11/14/2020
 */

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using OLClassLibrary;
using System.Text.RegularExpressions;

namespace OLClubs.Models
{
    /// <summary>
    /// partial class of NameAddress model with Metadata annotation added for data formatting;
    /// includes accessor for FullName
    /// </summary>
    [ModelMetadataType(typeof(OLNameAddressMetadata))]
    public partial class NameAddress : IValidatableObject
    {
        private const int SPACE_POSITION_POSTAL = 3;
        private const int PHONE_LENGTH = 10;
        private const string CANADA_CODE = "CA";
        private ClubsContext _context;

        /// <summary>
        /// full name (accessor) property for an artist:
        /// if both a first and last name, return as “LastName, FirstName”;
        /// if just a first or last name, return that;
        /// returns "" if neither is found
        /// </summary>
        public string FullName
        {
            get
            {
                if (!String.IsNullOrEmpty(FirstName) && !String.IsNullOrEmpty(LastName))
                {
                    return $"{LastName}, {FirstName}";
                }
                else if (!String.IsNullOrEmpty(FirstName))
                {
                    return FirstName;
                }
                else if (!String.IsNullOrEmpty(LastName))
                {
                    return LastName;
                }
                return "";
            }
        }

        /// <summary>
        /// Formats data:
        /// trim all strings of leading & trailing spaces, and convert null to an empty string;
        /// capitilize first name, last name, company name, street-address, and city;
        /// shift postal and province codes to an upper case;
        /// reduce phone to just digits;
        /// 
        /// Verifies data according to the specifications
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            Province province = null;
            Country country = null;
            string errors = "";

            _context = new ClubsContext();

            FirstName = OLStringManipulation.OLCapitilize((FirstName + "").Trim());
            LastName = OLStringManipulation.OLCapitilize((LastName + "").Trim());
            CompanyName = OLStringManipulation.OLCapitilize((CompanyName + "").Trim());
            StreetAddress = OLStringManipulation.OLCapitilize((StreetAddress + "").Trim());
            City = OLStringManipulation.OLCapitilize((City + "").Trim());

            PostalCode = (PostalCode + "").Trim().ToUpper();
            ProvinceCode = (ProvinceCode + "").Trim().ToUpper();

            Email = (Email + "").Trim();
            Phone = OLStringManipulation.OLExtractDigits((Phone + "").Trim());

            // at least one of FirstName, LastName or CompanyName must be specified
            if (FirstName == "" && LastName == "" && CompanyName == "")
            {
                yield return new ValidationResult("At least one of First Name, Last Name, or Company Name must be specified.",
                    new string[] { nameof(FirstName), nameof(LastName), nameof(CompanyName) });
            }

            // ProvinceCode is conditionally optional, but if provided: 
            // * validates by fetching province record from the database (error if not found);
            // * if fetching the province code throws an exception, put its innermost message into an error for that field
            if (ProvinceCode != "")
            {
                try
                {
                    province = _context.Province.Find(ProvinceCode);
                    if (province == null)
                    {
                        errors = "The given Province Code is not found.";
                    }
                }
                catch (Exception ex)
                {
                    errors = ex.GetBaseException().Message;
                }
                if (errors != "") {
                    yield return new ValidationResult(errors, new string[] { nameof(ProvinceCode) });
                }
            }

            // PostalCode is conditionally optional, but if provided:
            // * produce an error if there is no province code (it is required to edit a postal code);
            // * otherwise, fetch the country record for the given province;
            // * validate the postal code to the country’s postal pattern;
            // * if the address is in Canada:
            //       - confirm the first letter is valid for the specified province;
            //       - add a space in the middle, if it’s not there already
            if (PostalCode != "")
            {
                if (province == null)
                {
                    yield return new ValidationResult("Province Code is required for Postal Code adjustment.", new string[] { nameof(PostalCode) });
                }
                else
                {
                    country = _context.Country.Find(province.CountryCode);
                    if (!OLStringManipulation.OLPostalCodeIsValid(PostalCode, country.PostalPattern))
                    {
                        yield return new ValidationResult("Province Code does not match the Country Postal Pattern.", new string[] { nameof(PostalCode) });
                    }
                    else
                    {
                        if (country.CountryCode == CANADA_CODE)
                        {
                            if (!province.FirstPostalLetter.ToCharArray().Contains(PostalCode[0]))
                                yield return new ValidationResult($"Province Code does not match the Province First Postal Letter. Possible values: {province.FirstPostalLetter}", new string[] { nameof(PostalCode) });
                            else
                                PostalCode = OLStringManipulation.OLInsertSpaceInPostal(PostalCode, SPACE_POSITION_POSTAL);
                        }
                    }
                }
            }


            // if email is not provided, all the postal addressing information is required
            if (Email == "")
            {
                if (StreetAddress == "" || City == "" || PostalCode == "" || ProvinceCode == "")
                {
                    yield return new ValidationResult("All the postal addressing information is required if Email is not provided.",
                        new string[] { nameof(Email) });
                }
            }

            // phone must contain exactly 10 digits;
            // reformatted into dash notation
            if (Phone.Length != PHONE_LENGTH)
            {
                yield return new ValidationResult($"Phone must be exactly {PHONE_LENGTH} digits.",
                    new string[] { nameof(Phone) });
            }
            else
            {
                Phone = Regex.Replace(Phone, @"^(\d{3})(\d{3})(\d{4})$", "$1-$2-$3");
            }

            yield return ValidationResult.Success;
        }
    }

    /// <summary>
    /// metadata class for the NameAddress model to format data appropriately
    /// </summary>
    public class OLNameAddressMetadata
    {
        [Display(Name = "ID")]
        public int NameAddressId { get; set; }

        [Display(Name = "Name")]
        public string FullName { get; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "Province Code")]
        public string ProvinceCode { get; set; }

        // email is optional, but if provided, it must be a valid pattern
        [OLEmailAnnotation]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // phone is required
        [Required]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
    }
}
