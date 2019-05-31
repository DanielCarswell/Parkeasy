using Parkeasy.Models;
using Parkeasy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Parkeasy.Models.AccountViewModels;

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
            //Gets all users from the database table and returns them in a list of UserViewModel class instances.
            users.AddRange((from u in context.Users
                            join ur in context.UserRoles on u.Id equals ur.UserId
                            join r in context.Roles on ur.RoleId equals r.Id
                            select new UserViewModel
                            {
                                Id = u.Id,
                                Email = u.Email,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                Role = r.Name
                            }).OrderBy(o => o.Email).ToList());
        }

        /// <summary>
        /// Queries database and returns list of UserViewModels that are targetted to the "Customer" role
        /// filtering out all other roles from the data displayed.
        /// </summary>
        /// <param name="users">List of UserViewModel Class</param>
        /// <param name="context">Instance of ApplicationDbContext Class</param>
        /// <returns>List of UserViewModel Class Instances</returns>
        public static async Task GetCustomers(this List<UserViewModel> users, ApplicationDbContext context)
        {
            //Joins the users table to the userroles table joining user id to userrole userid, then joins
            //userrole roleid to roles table id column. This allows us to query for specific roles with associated users.
            //Then orders the outputs by email.
            users.AddRange((from u in context.Users
                            join ur in context.UserRoles on u.Id equals ur.UserId
                            join r in context.Roles on ur.RoleId equals r.Id
                            where r.Name.Equals("Customer")
                            select new UserViewModel
                            {
                                Id = u.Id,
                                Email = u.Email,
                                FirstName = u.FirstName,
                                LastName = u.LastName
                            }).OrderBy(o => o.Email).ToList());
        }

        public static ChangeRoleViewModel GetUserAndRole(ApplicationDbContext context, string id)
        {
            //Joins the users table to the userroles table joining user id to userrole userid, then joins
            //userrole roleid to roles table id column. This allows us to query for specific roles with associated users.
            //Then orders the outputs by email.
            var user = ((from u in context.Users
                         join ur in context.UserRoles on u.Id equals ur.UserId
                         join r in context.Roles on ur.RoleId equals r.Id
                         where u.Id.Equals(id)
                         select new ChangeRoleViewModel
                         {
                             Id = u.Id,
                             Role = r.Name,
                             FirstName = u.FirstName,
                             LastName = u.LastName
                         }).FirstOrDefault());

            return user;
        }

    }
}