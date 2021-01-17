/*
 * OLProvinceController.cs
 * Description: Province Contoller
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 10/04/2020
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OLClubs.Models;

namespace OLClubs.Controllers
{
    /// <summary>
    /// controller to access and maintain the province table
    /// </summary>
    public class OLProvinceController : Controller
    {
        private readonly ClubsContext _context;

        /// <summary>
        /// constructor for OLProvinceController to initialize the controller
        /// </summary>
        /// <param name="context">provided to the controller by Dependency Injection in Startup.cs</param>
        public OLProvinceController(ClubsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// if the country code is in the URL/QueryString, it is saved to a session variable
        /// else if a session variable is found, use it
        /// else user is returned to the country contoller's index action with a message
        /// </summary>
        /// <param name="countryCode">Country Code passed from the view</param>
        /// <param name="countryName">Country Name passed from the view</param>
        /// <returns>the view to display</returns>
        public async Task<IActionResult> Index(string countryCode, string countryName)
        {
            Country country = _context.Country.Where(c => c.CountryCode == countryCode).FirstOrDefault();
            string provTerm = "";
            // if country code is not null & country is on file
            if (
                countryCode != null &&
                country != null // persist country data only if the country is on file (not random garbage like 'u')
                )
            {
                // save countryCode to a session variable
                HttpContext.Session.SetString(nameof(countryCode), countryCode);

                if (countryName == null)
                {
                    // fetch the record from the country table by countryCode to extract countryName
                    countryName = country.Name;
                }

                // save countryName to a session variable
                HttpContext.Session.SetString(nameof(countryName), countryName);
                // save province terminology to a session variable
                HttpContext.Session.SetString(nameof(provTerm), country.ProvinceTerminology);
            }
            else if (HttpContext.Session.GetString(nameof(countryCode)) != null)
            {
                countryCode = HttpContext.Session.GetString(nameof(countryCode));
                countryName = HttpContext.Session.GetString(nameof(countryName));
                provTerm = HttpContext.Session.GetString(nameof(provTerm));
            }
            else
            {
                TempData["message"] = "Please select a country to retrieve its provinces";
                return RedirectToAction("Index", "OLCountry");
            }

            // get provinces for a specific countryCode and order listing by name
            var provinces = _context.Province
                .Where(p => p.CountryCode == countryCode)
                .OrderBy(p => p.Name)
                .Include(p => p.CountryCodeNavigation);

            return View(await provinces.ToListAsync());
        }

        /// <summary>
        /// action to display the details of the selected record
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display data</returns>
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .Include(p => p.CountryCodeNavigation)
                .FirstOrDefaultAsync(m => m.ProvinceCode == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        /// <summary>
        /// action to display an empty page for a new record creation (set-up action)
        /// </summary>
        /// <returns>view to display</returns>
        public IActionResult Create()
        {
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode");
            return View();
        }

        // POST: OLProvince/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// insert a new record if it passes the validation (post-back action)
        /// ensure there is no province with the same Code or Name
        /// </summary>
        /// <param name="province">binded province object to be added</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProvinceCode,Name,CountryCode,SalesTaxCode,SalesTax,IncludesFederalTax,FirstPostalLetter")] Province province)
        {
            string errorsMsg = "";
            if (ModelState.IsValid)
            {
                // ensure there is no province with the same Code
                Province prov = await _context.Province.FindAsync(province.ProvinceCode);
                if (prov != null)
                {
                    errorsMsg += $"'{prov.ProvinceCode}' is already taken - Province Code must be unique. ";
                }
                // ensure there is no province with the same Name
                prov = _context.Province.FirstOrDefault(p => p.Name == province.Name);
                if (prov != null)
                {
                    errorsMsg += $"'{prov.Name}' is already taken - Province Name must be unique.";
                }

                if(errorsMsg == "")
                {
                    _context.Add(province);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            if(errorsMsg != "")
            {
                TempData["message"] = errorsMsg;
            }
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);
        }

        /// <summary>
        /// action to retrieve the selected record and display it for update (set-up action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display data</returns>
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province.FindAsync(id);
            if (province == null)
            {
                return NotFound();
            }
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);
        }

        // POST: OLProvince/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// write the updated record to the file if it passes the edits (post-back action)
        /// ensure Name entered does not exist under a different key
        /// user is still able to update the current record
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="province">binded and updated country object</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProvinceCode,Name,CountryCode,SalesTaxCode,SalesTax,IncludesFederalTax,FirstPostalLetter")] Province province)
        {
            string errorsMsg = "";
            if (id != province.ProvinceCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ensure province name does not exist under different key
                    Province prov = _context.Province.FirstOrDefault(
                        p => p.Name == province.Name && // find province records with the same name
                        p.ProvinceCode != province.ProvinceCode // skip the current province to be updated, check other records
                        );

                    if (prov != null)
                    {
                        errorsMsg += $"'{prov.Name}' is already taken - use a different Name.";
                    }

                    if (errorsMsg == "")
                    {
                        _context.Update(province);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProvinceExists(province.ProvinceCode))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (errorsMsg == "")
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            if (errorsMsg != "")
            {
                TempData["message"] = errorsMsg;
            }
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);
        }

        /// <summary>
        /// display the selected record for deletion confirmation (set-up action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display record data</returns>
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .Include(p => p.CountryCodeNavigation)
                .FirstOrDefaultAsync(m => m.ProvinceCode == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        /// <summary>
        /// delete the selected record on the file (post-back action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>redirect to the index action</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var province = await _context.Province.FindAsync(id);
            _context.Province.Remove(province);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// confirm the record with key passed exists
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>true/false</returns>
        private bool ProvinceExists(string id)
        {
            return _context.Province.Any(e => e.ProvinceCode == id);
        }
    }
}
