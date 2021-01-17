/*
 * OLGroupMemberController.cs
 * Description: GroupMember Contoller
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 11/03/2020
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
    /// controller to access and maintain the group member table
    /// </summary>
    public class OLGroupMemberController : Controller
    {
        private readonly ClubsContext _context;

        /// <summary>
        /// constructor for OLGroupMemberController to initialize the controller
        /// </summary>
        /// <param name="context">provided to the controller by Dependency Injection in Startup.cs</param>
        public OLGroupMemberController(ClubsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// saves the data passed in session variables;
        /// filters records to display on the view
        /// </summary>
        /// <param name="artistId">artist ID</param>
        /// <param name="artistName">artist's full name</param>
        /// <returns>view with the list of group members</returns>
        public async Task<IActionResult> Index(int? artistId, string artistName)
        {
            // if the artistId is in the URL or a QueryString variable, save it to a session variable;
            // if no artistId was passed in, look for a session variable with it, and use that;
            // if there is no artistId in a session variable, return to the OLArtistController with a message asking to select an artist
            if (artistId != null)
            {
                HttpContext.Session.SetString(nameof(artistId), artistId.ToString());
                if (artistName == null)
                {
                    Artist artist = _context.Artist
                        .Include(a => a.NameAddress)
                        .FirstOrDefault(a => a.ArtistId == artistId);
                    artistName = artist.NameAddress.FullName;
                }
                HttpContext.Session.SetString(nameof(artistName), artistName);
            }
            else if (HttpContext.Session.GetString(nameof(artistId)) != null)
            {
                artistId = Convert.ToInt32(HttpContext.Session.GetString(nameof(artistId)));
                artistName = HttpContext.Session.GetString(nameof(artistName));
            }
            else
            {
                TempData["message"] = "Please select an artist";
                return RedirectToAction("Index", "OLArtist");
            }

            // order the listing by dateLeft, then by dateJoined (oldest to newest)
            var groupMembersData = _context.GroupMember
                 .Include(g => g.ArtistIdGroupNavigation)
                 .ThenInclude(a => a.NameAddress)
                 .Include(g => g.ArtistIdMemberNavigation)
                 .ThenInclude(a => a.NameAddress)
                 .OrderBy(gm => gm.DateLeft)
                 .ThenBy(gm => gm.DateJoined);

            // if artist is a group, list all group members
            var groupMembers = groupMembersData
                .Where(gm => gm.ArtistIdGroup == artistId);

            if (groupMembers.Any())
            {
                return View(await groupMembers.ToListAsync());
            }

            // if artist is a member, list all groups they belonged to
            groupMembers = groupMembersData
                .Where(gm => gm.ArtistIdMember == artistId);

            if (groupMembers.Any())
            {
                TempData["message"] = "The artist is an individual, not a group. These are their historic group memberships.";
                return View("GroupsForArtist", await groupMembers.ToListAsync());
            }

            // if neither a group nor member
            TempData["message"] = "The artist is neither a group nor a group member. Please make them a new group.";
            return RedirectToAction("Create");
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

            var groupMember = await _context.GroupMember
                .Include(g => g.ArtistIdGroupNavigation)
                .Include(g => g.ArtistIdMemberNavigation)
                .ThenInclude(g => g.NameAddress)
                .FirstOrDefaultAsync(m => m.ArtistIdMember == id &&
                    m.ArtistIdGroup == Convert.ToInt32(HttpContext.Session.GetString("artistId")));

            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        /// <summary>
        /// action to display an empty page for a new record creation (set-up action);
        /// displays a drop-down of members available sorted by full name
        /// </summary>
        /// <returns>view to display</returns>
        public IActionResult Create()
        {
            var artists = GetValidArtists()
                .Select(a => new
                {
                    ArtistId = a.ArtistId,
                    FullName = a.NameAddress.FullName
                })
                .ToList()
                .OrderBy(a => a.FullName);

            //ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId");
            ViewData["ArtistIdMember"] = new SelectList(artists, "ArtistId", "FullName");
            return View();
        }

        // POST: OLGroupMember/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// insert a new record (post-back action)
        /// </summary>
        /// <param name="groupMember">binded groupMember object to be added</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArtistIdGroup,ArtistIdMember,DateJoined,DateLeft")] GroupMember groupMember)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(groupMember);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Record saved successfully";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.GetBaseException().Message);
            }

            // display a drop-down of members available sorted by full name
            var artists = GetValidArtists()
                .Select(a => new
                {
                    ArtistId = a.ArtistId,
                    FullName = a.NameAddress.FullName
                })
                .ToList()
                .OrderBy(a => a.FullName);

            //ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId");
            ViewData["ArtistIdMember"] = new SelectList(artists, "ArtistId", "FullName");
            return View(groupMember);
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

            var groupMember = await _context.GroupMember
                .Include(g => g.ArtistIdGroupNavigation)
                .Include(g => g.ArtistIdMemberNavigation)
                .ThenInclude(g => g.NameAddress)
                .FirstOrDefaultAsync(gm => gm.ArtistIdMember == id &&
                gm.ArtistIdGroup == Convert.ToInt32(HttpContext.Session.GetString("artistId")));

            if (groupMember == null)
            {
                return NotFound();
            }

            //var artists = GetValidArtists();

            //ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId");
            //ViewData["ArtistIdMember"] = new SelectList(artists, "ArtistId", "FullName");
            return View(groupMember);
        }

        // POST: OLGroupMember/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// write the updated record to the file (post-back action)
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="groupMember">binded and updated grouopMember object</param>
        /// <returns>view to display</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArtistIdGroup,ArtistIdMember,DateJoined,DateLeft")] GroupMember groupMember)
        {
            if (id != groupMember.ArtistIdMember ||
                groupMember.ArtistIdGroup != Convert.ToInt32(HttpContext.Session.GetString("artistId")))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMember);
                    await _context.SaveChangesAsync();
                    TempData["message"] = "Record updated successfully";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!GroupMemberExists(groupMember.ArtistIdMember))
                    {
                        ModelState.AddModelError("", $"group member is not on file {groupMember.ArtistIdMember}");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"concurrency exception: {ex.GetBaseException().Message}");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"update error: {ex.GetBaseException().Message}");
                }
                return RedirectToAction(nameof(Index));
            }

            //ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdGroup);
            //ViewData["ArtistIdMember"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdMember);
            return View(groupMember);
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

            var groupMember = await _context.GroupMember
                .Include(g => g.ArtistIdGroupNavigation)
                .Include(g => g.ArtistIdMemberNavigation)
                .ThenInclude(g => g.NameAddress)
                .FirstOrDefaultAsync(m => m.ArtistIdMember == id &&
                    m.ArtistIdGroup == Convert.ToInt32(HttpContext.Session.GetString("artistId")));

            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
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
            try
            {
                var groupMember = await _context.GroupMember
                        .FirstOrDefaultAsync(gm => gm.ArtistIdMember == id &&
                        gm.ArtistIdGroup == Convert.ToInt32(HttpContext.Session.GetString("artistId")));

                _context.GroupMember.Remove(groupMember);
                await _context.SaveChangesAsync();
                TempData["message"] = "Record deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.GetBaseException().Message);
            }
            return RedirectToAction("Delete", new { ID = id });
        }

        /// <summary>
        /// confirm the record with key passed exists
        /// </summary>
        /// <param name="id">record id</param>
        /// <returns>true/false</returns>
        private bool GroupMemberExists(int id)
        {
            return _context.GroupMember.Any(e => e.ArtistIdGroup == id);
        }

        /// <summary>
        /// filters artists based on:
        /// 1) artist is not themself a group;
        /// 2) artist is not a current member of any group
        /// </summary>
        /// <returns>filtered artists</returns>
        private IQueryable<Artist> GetValidArtists()
        {
            return _context.Artist
                .Include(a => a.GroupMemberArtistIdGroupNavigation)
                .Include(a => a.GroupMemberArtistIdMemberNavigation)
                .Include(a => a.NameAddress)
                .Where(a => !a.GroupMemberArtistIdGroupNavigation.Any(gm => gm.ArtistIdGroup == a.ArtistId))
                .Where(a => !a.GroupMemberArtistIdMemberNavigation.Any(gm => gm.DateLeft == null));
        }
    }
}
