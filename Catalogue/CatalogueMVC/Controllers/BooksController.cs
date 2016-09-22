﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BooksEntitiesDAL;
using System.IO;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace CatalogueMVC.Controllers
{
    public class BooksController : Controller
    {
        private BooksCatalogueDBEntities db = new BooksCatalogueDBEntities();

        // GET: Books
        public async Task<ActionResult> Index( /*int? page*/)
        {
            //var books = db.Books.Include(b => b.Author).Include(b => b.Country);

            //int pageSize = 3;
            //int pageNumber = (page ?? 1);
            //return View(books.ToPagedList(pageNumber, pageSize));

            return View(/*await books.ToListAsync()*/);
        }

        public ActionResult SearchBook([StringLength(10, MinimumLength = 3, ErrorMessage = "3-10 characters required")]string keyword)
        {
            var books = db.Books.Include(b => b.Author).Include(b => b.Country);
            if (!string.IsNullOrEmpty(keyword))
            {
                books = books.Where(n => n.Title.Contains(keyword) || n.Author.FullName.Contains(keyword));

                if(!books.Any())
                {
                    return RedirectToAction("NotFound","Books");
                }
            }

            return View(books.ToList());
        }

        public ActionResult NotFound()
        {
            return View();
        }

        // GET: Books/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = await db.Books.FindAsync(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        const string _noImage = "no-image.png";

        // GET: Books/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "FullName");
            ViewBag.CountryID = new SelectList(db.Countries, "CountryID", "Country1");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "BookID,Title,AuthorID,CountryID,Price,Description,PagesCount,Picture")] Book book, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), fileName);
                    file.SaveAs(path);
                    book.Picture = fileName;
                }
                else
                {
                    book.Picture = _noImage;
                }
                db.Books.Add(book);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }


            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "FullName", book.AuthorID);
            ViewBag.CountryID = new SelectList(db.Countries, "CountryID", "Country1", book.CountryID);
            return View(book);
        }

        // GET: Books/Edit/5

        [Authorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = await db.Books.FindAsync(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "FullName", book.AuthorID);
            ViewBag.CountryID = new SelectList(db.Countries, "CountryID", "Country1", book.CountryID);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "BookID,Title,AuthorID,CountryID,Price,Description,PagesCount,Picture")] Book book, HttpPostedFileBase file)
        {
            book = await db.Books.FindAsync(book.BookID);
            if (ModelState.IsValid)
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (book.Picture != _noImage)
                    {
                        System.IO.File.Delete(Path.Combine(Server.MapPath("~/Images"), book.Picture));
                    }
                    
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), fileName);
                    file.SaveAs(path);
                    book.Picture = fileName;
                }

                db.Entry(book).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "FullName", book.AuthorID);
            ViewBag.CountryID = new SelectList(db.Countries, "CountryID", "Country1", book.CountryID);
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = await db.Books.FindAsync(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Book book = await db.Books.FindAsync(id);

            if (book.Picture != null && book.Picture != _noImage)
            {
                var fileName = Path.GetFileName(book.Picture);
                var path = Path.Combine(Server.MapPath("~/Images"), fileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
