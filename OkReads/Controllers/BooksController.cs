using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OkReads.Models;

namespace OkReads.Controllers
{
    public class BooksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManager<ApplicationUser> userManager;

        public BooksController()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            Classifier.Initialize("http://localhost:8000/");
        }

        // GET: Books
        public ActionResult Index()
        {
            return View(db.Books.ToList());
        }

        // GET: Books/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            if (Request.IsAuthenticated)
            {
                var currentUser = userManager.FindByName(User.Identity.Name);
                ViewBag.LibraryEntry = db.LibraryEntries.Where(x => x.Book.BookId == book.BookId && x.User.Id == currentUser.Id).FirstOrDefault();
                ViewBag.User= currentUser;
                if (ViewBag.LibraryEntry != null)
                {
                    ViewBag.BookStatus = new Dictionary<string, bool>()
                    {
                        { "toRead", ViewBag.LibraryEntry.BookStatus == BookStatus.ToRead},
                        { "reading", ViewBag.LibraryEntry.BookStatus == BookStatus.Reading},
                        { "read", ViewBag.LibraryEntry.BookStatus == BookStatus.Read}
                    };
                } else
                {
                    ViewBag.BookStatus = new Dictionary<string, bool>()
                    {
                        { "toRead", false},
                        { "reading", false},
                        { "read", false}
                    };
                }
            }
            return View(book);
        }

        // GET: Books/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookId,Title,Isbn,Description")] Book book, string AuthorNames)
        {
            // TODO: Upload book cover
            if (ModelState.IsValid)
            {
                /* Get all authors from database */
                IEnumerable<string> authorNamesList = AuthorNames.Split(';');
                book.Authors = db.Authors.Where(x => authorNamesList.Contains(x.Name)).ToArray();
                /* Get book genre from classifier */
                try
                {
                    string genreName = Classifier.Predict(book.Description);
                    book.Genre = db.Genres.Where(x => x.Name == genreName).FirstOrDefault();
                    if (book.Genre == null)
                    {
                        book.Genre = new Genre(genreName);
                    }
                }
                catch
                {

                }
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(book);
        }

        // GET: Books/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookId,Title,Isbn,Description")] Book book, string AuthorNames)
        {
            // TODO: Edit Author value
            // TODO: Upload new book cover
            if (ModelState.IsValid)
            {
                IEnumerable<string> namesList = AuthorNames.Split(';');
                IEnumerable<Author> authors = db.Authors.Where(x => namesList.Contains(x.Name));
                // TODO: Upload new book cover. Delete old one ?
                /* If book description changed, get new genre value from classifier*/
                Book oldBook = db.Books.AsNoTracking().Where(x => x.BookId == book.BookId).FirstOrDefault();
                if (oldBook.Description != book.Description)
                {
                    try
                    {
                        string genreName = Classifier.Predict(book.Description);
                        book.Genre = db.Genres.Where(x => x.Name == genreName).FirstOrDefault();
                        if (book.Genre == null)
                        {
                            book.Genre = new Genre(genreName);
                        }
                    }
                    catch
                    {

                    }
                    
                }
                book.Authors = authors.ToArray();
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            // TODO: Remove book cover image file
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                Classifier.Deinitialize();
            }
            base.Dispose(disposing);
        }

        public ActionResult Search(string s)
        {
            IEnumerable<Book> books = db.Books.Where(x => x.Title.Contains(s) 
                                                       || x.Authors.Select(a => a.Name).Any(b => b.Contains(s))
                                                       || x.Isbn.Contains(s)).ToList();
            return View(books);
        }

        [HttpPost]
        public ActionResult AddToLibrary(string userId, int bookId, string value) {
            BookStatus status = BookStatus.ToRead;
            switch (value)
            {
                case "To read":
                    status = BookStatus.ToRead;
                    break;
                case "Reading":
                    status = BookStatus.Reading;
                    break;
                case "Read":
                    status = BookStatus.Read;
                    break;
            }
            ApplicationUser user = userManager.FindById(userId);
            Book book = db.Books.Where(x => x.BookId == bookId).FirstOrDefault(); 
            LibraryEntry entry = db.LibraryEntries.Where(x => x.Book.BookId == bookId && x.User.Id == userId).FirstOrDefault();
            if (entry == null)
            {
                db.LibraryEntries.Add(new LibraryEntry(user, book, status));
            }
            else
            {
                entry.BookStatus = status;
                db.Entry(entry).State = EntityState.Modified;
            }
            db.SaveChanges();
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        // TODO: Add action to remove book from library
    }
}
