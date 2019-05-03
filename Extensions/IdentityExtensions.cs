using Parkeasy.Models;
using Parkeasy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace Parkeasy.Extensions
{
    /// <summary>
    /// Contains methods that are used to get information for current User.
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Returns all users from the database in list of UserViewModel class instances.
        /// </summary>
        /// <param name="users">List of UserViewModel used to call method</param>
        /// <returns>List of UserViewModel class instances</returns>
        public static async Task GetUsers(this List<UserViewModel> users, ApplicationDbContext context)
        {
            users.AddRange( (from u in context.Users
                                  select new UserViewModel
                                  {
                                      Id = u.Id,
                                      Email = u.Email,
                                      FirstName = u.FirstName,
                                      LastName = u.LastName
                                  }).OrderBy(o => o.Email).ToList());
        }
    }
}