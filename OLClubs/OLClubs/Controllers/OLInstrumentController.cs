/*
 * OLInstrumentController.cs
 * Description: Instrument Contoller
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
    /// controller to access and maintain the instrument table
    /// </summary>
    public class OLInstrumentController : Controller
    {
        private readonly ClubsContext _context;

        /// <summary>
        /// constructor for OLInstrumentController to initialize the controller
        /// </summary>
        /// <param name="context">provided to the controller by Dependency Injection in Startup.cs</param>
        public OLInstrumentController(ClubsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// retrieve all records from the target table and pass data to the view
        /// </summary>
        /// <returns>view to display</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Instrument.ToListAsync());
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

            var instrument = await _context.Instrument
                .FirstOrDefaultAsync(m => m.InstrumentId == id);
            if (instrument == null)
            {
                return NotFound();
            }

            return View(instrument);
        }

        /// <summary>
        /// action to display an empty page for a new record creation (set-up action)
        /// </summary>
        /// <returns>view to display</returns>
        public IActionResult Create()
        {
            return View();
        }

        // POST: OLInstrument/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// insert a new record if it passes the validation (post-back action)
        /// </summary>
        /// <param name="instrument">binded instrument object to be added</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InstrumentId,Name")] Instrument instrument)
        {
            if (ModelState.IsValid)
            {
                _context.Add(instrument);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(instrument);
        }

        /// <summary>
        /// action to retrieve the selected record and display it for update (set-up action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display data</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instrument = await _context.Instrument.FindAsync(id);
            if (instrument == null)
            {
                return NotFound();
            }
            return View(instrument);
        }

        // POST: OLInstrument/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// write the updated record to the file if it passes the edits (post-back action)
        /// otherwise, display error messages
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="instrument">binded and updated instrument object</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InstrumentId,Name")] Instrument instrument)
        {
            if (id != instrument.InstrumentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instrument);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstrumentExists(instrument.InstrumentId))
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
            return View(instrument);
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

            var instrument = await _context.Instrument
                .FirstOrDefaultAsync(m => m.InstrumentId == id);
            if (instrument == null)
            {
                return NotFound();
            }

            return View(instrument);
        }

        /// <summary>
        /// delete the selected record on the file (post-back action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>redirect to the index action</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instrument = await _context.Instrument.FindAsync(id);
            _context.Instrument.Remove(instrument);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// confirm the record with key passed exists
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>true/false</returns>
        private bool InstrumentExists(int id)
        {
            return _context.Instrument.Any(e => e.InstrumentId == id);
        }
    }
}
