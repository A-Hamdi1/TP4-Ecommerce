using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;   // Pour Category
using WebApplication2.Models.Repositories;


namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin,Manager, User")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository categoryRepository;

        // Injection du repository dans le constructeur
        public CategoryController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        // GET: Category
        [AllowAnonymous]
        public ActionResult Index()
        {

            var categories = categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            
            return View(categories);
        }

        // GET: Category/Details/5
        public ActionResult Details(int id)
        {
            var categories = categoryRepository.GetAll();
            ViewData["Categories"] = categories;

            var category = categoryRepository.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            var categories = categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepository.Add(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public ActionResult Edit(int id)
        {
            var categories = categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            var category = categoryRepository.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                categoryRepository.Update(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public ActionResult Delete(int id)
        {
            var categories = categoryRepository.GetAll();
            ViewData["Categories"] = categories;
            var category = categoryRepository.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            categoryRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
