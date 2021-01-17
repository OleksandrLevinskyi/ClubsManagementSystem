/*
 * OLNameAddressController.cs
 * Description: Name & Address Contoller
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 11/14/2020
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OLClubs.Models;

namespace OLClubs.Controllers
{
    /// <summary>
    /// controller with full CRUD support for the NameAddress table
    /// </summary>
    public class OLNameAddressController : Controller
    {
        private readonly ClubsContext _context;

        /// <summary>
        /// constructor for OLNameAddressController to initialize the controller
        /// </summary>
        /// <param name="context">provided to the controller by Dependency Injection in Startup.cs</param>
        public OLNameAddressController(ClubsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// retrieve all records from the target table and pass data to the view;
        /// sort by full name
        /// </summary>
        /// <returns>view to display</returns>
        public async Task<IActionResult> Index()
        {
            var clubsContext = await _context.NameAddress
                .Include(n => n.ProvinceCodeNavigation)
                .ToListAsync();

            return View(clubsContext.OrderBy(n => n.FullName));
        }

        /// <summary>
        /// action to display the details of the selected record
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display data</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress
                .Include(n => n.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.NameAddressId == id);

            if (nameAddress == null)
            {
                return NotFound();
            }

            return View(nameAddress);
        }

        /// <summary>
        /// action to display an empty page for a new record creation (set-up action);
        /// no drop-down for the view as per instructions
        /// </summary>
        /// <returns>view to display</returns>
        public IActionResult Create()
        {
            // ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode");
            return View();
        }

        // POST: OLNameAddress/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// insert a new record if it passes the validation (post-back action);
        /// if successfull, display a confirmation message on the Index view with TempData;
        /// no drop-down for the view as per instructions if error occurred;
        /// catch any exception thrown, place its innermost message into ModelState, allow processing to continue to the sad path (redisplay the user’s data with the error)
        /// </summary>
        /// <param name="nameAddress">binded nameAddress object to be added</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NameAddressId,FirstName,LastName,CompanyName,StreetAddress,City,PostalCode,ProvinceCode,Email,Phone")] NameAddress nameAddress)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(nameAddress);
                    await _context.SaveChangesAsync();

                    TempData["message"] = "Name & Address record added successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.GetBaseException().Message);
            }
            // ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode", nameAddress.ProvinceCode);
            return View(nameAddress);
        }

        /// <summary>
        /// action to retrieve the selected record and display it for update (set-up action);
        /// set a drop-down for province selection, ordered by name
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display data</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress.FindAsync(id);
            if (nameAddress == null)
            {
                return NotFound();
            }
            // display the province name in the drop-down, ordered by name
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(p => p.Name), "ProvinceCode", "Name", nameAddress.ProvinceCode);
            return View(nameAddress);
        }

        // POST: OLNameAddress/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// write the updated record to the file if it passes the edits (post-back action);
        /// if successfull, display a confirmation message on the Index view with TempData;
        /// set a drop-down for province selection, ordered by name if errors;
        /// catch any exception thrown, place its innermost message into ModelState, allow processing to continue to the sad path (redisplay the user’s data with the error)
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="nameAddress">binded and updated nameAddress object</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NameAddressId,FirstName,LastName,CompanyName,StreetAddress,City,PostalCode,ProvinceCode,Email,Phone")] NameAddress nameAddress)
        {
            if (id != nameAddress.NameAddressId)
            {
                ModelState.AddModelError("", "A record being updated is not the one requested.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nameAddress);
                    await _context.SaveChangesAsync();

                    TempData["message"] = $"Name & Address record updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!NameAddressExists(nameAddress.NameAddressId))
                    {
                        ModelState.AddModelError("", $"NameAddressId is not on file: {nameAddress.NameAddressId}");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Concurrency exception: {ex.GetBaseException().Message}");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error while updating a record: {ex.GetBaseException().Message}");
                }
            }
            // display the province name in the drop-down, ordered by name
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(p => p.Name), "ProvinceCode", "Name", nameAddress.ProvinceCode);
            return View(nameAddress);
        }

        /// <summary>
        /// display the selected record for delition confirmation (set-up action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display record data</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress
                .Include(n => n.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.NameAddressId == id);

            if (nameAddress == null)
            {
                return NotFound();
            }

            return View(nameAddress);
        }

        /// <summary>
        /// delete the selected record on the file (post-back action);
        /// if successfull, display a confirmation message on the Index view with TempData;
        /// catch any exception thrown, put the innermost message into TempData and return to the Delete view
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>redirect to the index action</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var nameAddress = await _context.NameAddress.FindAsync(id);
                _context.NameAddress.Remove(nameAddress);
                await _context.SaveChangesAsync();

                TempData["message"] = "Name & Address record deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["message"] = $"Error while deleting a record: {ex.GetBaseException().Message}";
            }
            return RedirectToAction("Delete", new { ID = id });
        }

        /// <summary>
        /// confirm the record with key passed exists
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>true/false</returns>
        private bool NameAddressExists(int id)
        {
            return _context.NameAddress.Any(e => e.NameAddressId == id);
        }
    }
}
