using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OkReads.Models;

namespace OkReads.Controllers
{
    public class GenresController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Genres
        public ActionResult Index()
        {
            return View(db.Genres.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
