using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OkReads.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OkReads.App_Start
{
    public class DatabaseInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        public DatabaseInitializer() : base() { }

        protected override void Seed(ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            CreateRoles(roleManager);

            var admin = new ApplicationUser { UserName = "admin@okreads.org", Email = "admin@okreads.org" };
            var user = new ApplicationUser { UserName = "user@okreads.org", Email = "user@okreads.org" };
            SeedUser(context, userManager, roleManager, admin, "asAS12!@", "Admin");
            SeedUser(context, userManager, roleManager, user, "asAS12!@", "User");

            base.Seed(context);
        }

        private void CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = new string[] { "Admin", "Editor", "User" };
            foreach (var name in roleNames)
            {
                if (!roleManager.RoleExists(name))
                {
                    roleManager.Create(new IdentityRole { Name = name });
                }
            }
        }

        private void SeedUser(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
                              RoleManager<IdentityRole> roleManager, ApplicationUser user, 
                              string password, string roleName)
        {
            userManager.Create(user, password);
            userManager.AddToRole(user.Id, roleName);
        }
    }
}