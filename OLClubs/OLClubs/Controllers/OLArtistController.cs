/*
 * OLArtistController.cs
 * Description: Artist Contoller
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 11/03/2020
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
    /// controller to access and maintain the artist table
    /// </summary>
    public class OLArtistController : Controller
    {
        private readonly ClubsContext _context;

        /// <summary>
        /// constructor for OLArtistController to initialize the controller
        /// </summary>
        /// <param name="context">provided to the controller by Dependency Injection in Startup.cs</param>
        public OLArtistController(ClubsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// gets a list of the artists with their full name
        /// sorts the listing by full name
        /// </summary>
        /// <returns>view with a list of artists</returns>
        public async Task<IActionResult> Index()
        {
            var artists = await _context.Artist
                .Include(a => a.NameAddress)
                .ToListAsync();

            return View(artists.OrderBy(a => a.NameAddress.FullName));
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

            var artist = await _context.Artist
                .Include(a => a.NameAddress)
                .FirstOrDefaultAsync(m => m.ArtistId == id);
            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        /// <summary>
        /// action to display an empty page for a new record creation (set-up action)
        /// </summary>
        /// <returns>view to display</returns>
        public IActionResult Create()
        {
            ViewData["NameAddressid"] = new SelectList(_context.NameAddress, "NameAddressId", "NameAddressId");
            return View();
        }

        // POST: OLArtist/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// insert a new record (post-back action)
        /// </summary>
        /// <param name="artist">binded artist object to be added</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArtistId,MinimumHourlyRate,NameAddressid")] Artist artist)
        {
            if (ModelState.IsValid)
            {
                _context.Add(artist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NameAddressid"] = new SelectList(_context.NameAddress, "NameAddressId", "NameAddressId", artist.NameAddressid);
            return View(artist);
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

            var artist = await _context.Artist.FindAsync(id);
            if (artist == null)
            {
                return NotFound();
            }
            ViewData["NameAddressid"] = new SelectList(_context.NameAddress, "NameAddressId", "NameAddressId", artist.NameAddressid);
            return View(artist);
        }

        // POST: OLArtist/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// write the updated record to the file (post-back action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="artist">binded and updated artist object</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArtistId,MinimumHourlyRate,NameAddressid")] Artist artist)
        {
            if (id != artist.ArtistId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(artist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistExists(artist.ArtistId))
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
            ViewData["NameAddressid"] = new SelectList(_context.NameAddress, "NameAddressId", "NameAddressId", artist.NameAddressid);
            return View(artist);
        }

        /// <summary>
        /// display the selected record for deletion confirmation (set-up action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>view to display record data</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artist
                .Include(a => a.NameAddress)
                .FirstOrDefaultAsync(m => m.ArtistId == id);
            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
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
            var artist = await _context.Artist.FindAsync(id);
            _context.Artist.Remove(artist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// confirm the record with key passed exists
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>true/false</returns>
        private bool ArtistExists(int id)
        {
            return _context.Artist.Any(e => e.ArtistId == id);
        }
    }
}
