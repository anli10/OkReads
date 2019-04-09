using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OkReads.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

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

            // Read JSON datafile
            string inputFilename = HttpContext.Current.Server.MapPath("okreads-dataset.json");
            string json = System.IO.File.ReadAllText(inputFilename);
            IEnumerable<JSONEntry> jsonEntries = new JavaScriptSerializer().Deserialize<IEnumerable<JSONEntry>>(json);

            IEnumerable<Genre> genres = jsonEntries.Where(x => x.genre != null).Select(x => x.genre).Distinct().Select(x => new Genre(x)).ToList(); // TODO: Don't call select twice
            context.Genres.AddRange(genres);

            IEnumerable<Author> authors = jsonEntries.Select(x => x.authors).Aggregate((a, b) => a.Concat(b)).Distinct().Select(x => new Author(x)).ToList();
            context.Authors.AddRange(authors);

            context.SaveChanges();

            foreach (JSONEntry e in jsonEntries) {
                ICollection<Author> bookAuthors = context.Authors.Where(x => e.authors.Contains(x.Name)).ToList();
                Genre bookGenre = context.Genres.Where(x => x.Name == e.genre).FirstOrDefault();
                context.Books.Add(new Book(e.title, e.isbn, e.cover_src, bookGenre, bookAuthors));
            }
            //context.Books.AddRange(books);
            base.Seed(context);
        }

        private class JSONEntry
        {
            public string title { get; set; }
            public IEnumerable<string> authors { get; set; }
            public string description { get; set; }
            public string isbn { get; set; }
            public string cover_src { get; set; }
            public string genre { get; set; }
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