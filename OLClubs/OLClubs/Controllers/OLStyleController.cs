/*
 * OLStyleController.cs
 * Description: Style Contoller
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 09/15/2020
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
    /// controller to access and maintain the style table
    /// </summary>
    public class OLStyleController : Controller
    {
        private readonly ClubsContext _context;

        /// <summary>
        /// constructor for OLStyleController to initialize the controller
        /// </summary>
        /// <param name="context">provided to the controller by Dependency Injection in Startup.cs</param>
        public OLStyleController(ClubsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// retrieve all records from the target table and pass data to the view
        /// </summary>
        /// <returns>view to display</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Style.ToListAsync());
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

            var style = await _context.Style
                .FirstOrDefaultAsync(m => m.StyleName == id);
            if (style == null)
            {
                return NotFound();
            }

            return View(style);
        }

        /// <summary>
        /// action to display an empty page for a new record creation (set-up action)
        /// </summary>
        /// <returns>view to display</returns>
        public IActionResult Create()
        {
            return View();
        }

        // POST: OLStyle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// insert a new record if it passes the validation (post-back action)
        /// </summary>
        /// <param name="style">binded style object to be added</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StyleName,Description")] Style style)
        {
            if (ModelState.IsValid)
            {
                _context.Add(style);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(style);
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

            var style = await _context.Style.FindAsync(id);
            if (style == null)
            {
                return NotFound();
            }
            return View(style);
        }

        // POST: OLStyle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// write the updated record to the file if it passes the edits (post-back action)
        /// otherwise, display error messages
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="style">binded and updated style object</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("StyleName,Description")] Style style)
        {
            if (id != style.StyleName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(style);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StyleExists(style.StyleName))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(style);
        }

        /// <summary>
        /// display the selected record for delition confirmation (set-up action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display record data</returns>
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var style = await _context.Style
                .FirstOrDefaultAsync(m => m.StyleName == id);
            if (style == null)
            {
                return NotFound();
            }

            return View(style);
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
            var style = await _context.Style.FindAsync(id);
            _context.Style.Remove(style);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// confirm the record with key passed exists
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>true/false</returns>
        private bool StyleExists(string id)
        {
            return _context.Style.Any(e => e.StyleName == id);
        }
    }
}
