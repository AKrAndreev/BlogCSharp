using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;

namespace Blog.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            var articles = _context.Articles
                .Include(a => a.Author)
                .ToList();
            return View(articles);
        }

        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

          
        

        // GET: Articles/Details/5
        public   ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article =   _context.Articles
                .Include(a => a.Author)
                .First(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: Articles/Create
        [Authorize]
        public ActionResult Create()
        {
             
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        public   ActionResult Create(Article article)
        {
            if (ModelState.IsValid)
            {
                var authorId = _context.Users
                     .Where(u => u.UserName == this.User.Identity.Name)
                     .First()
                     .Id;
                article.AuthorId = authorId;
                _context.Articles.Add(article);
                _context.SaveChanges();

                return RedirectToAction("Index");

            }
            
            return View(article);
        }

        // GET: Articles/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article =  _context.Articles
                .Where(a=>a.Id == id)
                .First();
            if (IsAuthorizedToEdit(article) == false)
            {
                return Forbid();
            }
            if (article == null)
            {
                return NotFound();
            }

            var model = new ArticleViewModel();
            model.Id = article.Id;
            model.Title = article.Title;
            model.Content = model.Content;

            return View(model);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var article = _context.Articles
                   .FirstOrDefault(a => a.Id == model.Id);
                article.Title = model.Title;
                article.Content = model.Content;
                _context.Update(article);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Articles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article =   _context.Articles
                .Include(a => a.Author)
                .First(m => m.Id == id);

            if (IsAuthorizedToEdit(article) == false)
            {
                return Forbid();
            }
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var article =   _context.Articles
                .Include(a=>a.Author)
                .First(m=>m.Id==id);

            if (article==null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }

         private bool IsAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }
}
