/*
 * OLRoleController.cs
 * Description: Role Contoller
 * 
 * Author: Oleksandr Levinskyi (section 4)
 * Student Number: 865 88 51
 * Date Created: 11/30/2020
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OLClubs.Controllers
{
    /// <summary>
    /// controller to access and maintain roles and users
    /// accessible to users in 'administrators' role only
    /// </summary>
    [Authorize(Roles = "administrators")]
    public class OLRoleController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// constructor for OLRoleController to initialize the controller
        /// </summary>
        /// <param name="userManager">user manager provided by Dependency Injection in Startup.cs</param>
        /// <param name="roleManager">role manager provided by Dependency Injection in Startup.cs</param>
        public OLRoleController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// lists all current roles ordered by role name
        /// </summary>
        /// <returns>view to display data</returns>
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name);
            return View(roles.ToList());
        }

        /// <summary>
        /// stop the insert and return the error message if:
        /// 1) the proposed role name is null, empty, or just blanks;
        /// 2) the proposed role name is already on file;
        /// 
        /// if the insert succeeds, return to the role listing and display a success message with the role name;
        /// if the insert fails, return to the role listing and display the exception's innermost message, or the first message from IdentityResult
        /// </summary>
        /// <param name="roleName">the name of the role to create</param>
        /// <returns>view to display data with confirmation/error messages</returns>
        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (String.IsNullOrWhiteSpace(roleName))
            {
                TempData["message"] = "Role name cannot be null, empty or just blanks";
                return RedirectToAction(nameof(Index));
            }

            roleName = roleName.Trim();
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                TempData["message"] = $"Role name '{roleName}' is already on file";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.FirstOrDefault().Description);
                }

                TempData["message"] = $"New Role '{roleName}' created successfully";
            }
            catch (Exception ex)
            {
                TempData["message"] = $"Exception creating '{roleName}' role: {ex.GetBaseException().Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// lists all users in the given role, sorted by user name;
        /// has a drop-down showing all users not in the role, sorted;
        /// handles thown exceptions
        /// </summary>
        /// <param name="roleName">the name of the role to get users from</param>
        /// <returns>view with data to display</returns>
        public async Task<IActionResult> ListRoleMembers(string roleName)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            var allUsers = _userManager.Users;
            List<IdentityUser> usersNotInRole = new List<IdentityUser>();

            // Time: O(m + n)
            // Space: O(m)
            // where m - number of users in a role, n - number of all users
            Dictionary<IdentityUser, bool> userInRoleDictionary = new Dictionary<IdentityUser, bool>();
            foreach (var item in usersInRole)
            {
                userInRoleDictionary[item] = true;
            }

            foreach (var user in allUsers)
            {
                if (!userInRoleDictionary.ContainsKey(user))
                {
                    usersNotInRole.Add(user);
                }
            }

            ViewData["RoleName"] = roleName;
            ViewData["UsersNotInRole"] = new SelectList(usersNotInRole.OrderBy(u => u.UserName), "UserName", "UserName");
            return View(usersInRole.OrderBy(u => u.UserName).ToList());
        }

        /// <summary>
        /// adds a user to the role;
        /// handles thown exceptions
        /// </summary>
        /// <param name="userName">the user to add</param>
        /// <param name="roleName">the role to add to</param>
        /// <returns>view listing all users of the role with confirmation/error messages</returns>
        [HttpPost]
        public async Task<IActionResult> AddToRole(string userName, string roleName)
        {
            IdentityUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                TempData["message"] = $"User '{userName}' is not found";
                return RedirectToAction(nameof(ListRoleMembers), new { roleName });
            }

            try
            {
                IdentityResult result = await _userManager.AddToRoleAsync(user, roleName);

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.FirstOrDefault().Description);
                }

                TempData["message"] = $"User '{userName}' added to '{roleName}' role successfully";
            }
            catch (Exception ex)
            {
                TempData["message"] = $"Exception adding user to '{roleName}' role: {ex.GetBaseException().Message}";
            }
            return RedirectToAction(nameof(ListRoleMembers), new { roleName });
        }

        /// <summary>
        /// removes a user from the role;
        /// cannot remove a current user who is in 'administrators' role;
        /// handles thown exceptions
        /// </summary>
        /// <param name="userName">the user to remove</param>
        /// <param name="roleName">the role to remove from</param>
        /// <returns>view listing all users of the role with confirmation/error messages</returns>
        public async Task<IActionResult> RemoveFromRole(string userName, string roleName)
        {
            IdentityUser user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                TempData["message"] = $"User '{userName}' is not found";
                return RedirectToAction(nameof(ListRoleMembers), new { roleName });
            }

            if (roleName == "administrators" && user.UserName == User.Identity.Name && await _userManager.IsInRoleAsync(user, "administrators"))
            {
                TempData["message"] = "You cannot remove yourself from the 'administrators' role";
                return RedirectToAction(nameof(ListRoleMembers), new { roleName });
            }

            try
            {
                IdentityResult result = await _userManager.RemoveFromRoleAsync(user, roleName);

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.FirstOrDefault().Description);
                }

                TempData["message"] = $"User '{userName}' removed from '{roleName}' role successfully";
            }
            catch (Exception ex)
            {
                TempData["message"] = $"Exception removing user from '{roleName}' role: {ex.GetBaseException().Message}";
            }
            return RedirectToAction(nameof(ListRoleMembers), new { roleName });
        }

        /// <summary>
        /// dipslays role member to confirm the delete procedure;
        /// handles thown exceptions
        /// </summary>
        /// <param name="roleName">the role to be removed</param>
        /// <returns>view with role members</returns>
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            try
            {
                var roleMembers = await _userManager.GetUsersInRoleAsync(roleName);

                ViewData["RoleName"] = roleName;
                return View(roleMembers.OrderBy(r => r.UserName).ToList());
            }
            catch (Exception ex)
            {
                TempData["message"] = $"Exception on set-up delete: {ex.GetBaseException().Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// deletes a selected role except 'administrators' one;
        /// handles thown exceptions
        /// </summary>
        /// <param name="roleName">the role to be removed</param>
        /// <returns>view listing roles (without the deleted one)</returns>
        [HttpPost, ActionName("DeleteRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoleConfirmed(string roleName)
        {
            if (roleName == "administrators")
            {
                TempData["message"] = $"'{roleName}' cannot be deleted";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    IdentityRole role = await _roleManager.FindByNameAsync(roleName);
                    IdentityResult result = await _roleManager.DeleteAsync(role);

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.FirstOrDefault().Description);
                    }

                    TempData["message"] = $"Role '{roleName}' deleted successfully";
                }
                else
                {
                    TempData["message"] = $"Role '{roleName}' does not exist";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = $"Exception deleting a role: {ex.GetBaseException().Message}";
                return RedirectToAction(nameof(DeleteRole), new { roleName });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
