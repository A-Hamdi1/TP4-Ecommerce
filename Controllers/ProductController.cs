using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication2.Models;  
using WebApplication2.Models.Repositories;
using WebApplication2.Services;
using WebApplication2.ViewModels;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin,Manager, User")]
    public class ProductController : Controller
    {
        private readonly IProductRepository ProductRepository;
        private readonly ICategoryRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IAnalyticsService _analyticsService;

        public ProductController(
            IProductRepository prodRepository, 
            ICategoryRepository categRepository, 
            IWebHostEnvironment hostingEnvironment,
            IAnalyticsService analyticsService)
        {
            ProductRepository = prodRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
            _analyticsService = analyticsService;
        }



        // GET: ProductController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            var product = ProductRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            // Incrémenter le compteur de vues
            await _analyticsService.IncrementProductViewCount(id);
            
            return View(product);
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");

            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.ImagePath != null)
                {
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    model.ImagePath.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                Product newProduct = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = uniqueFileName
                };

                ProductRepository.Add(newProduct);
                return RedirectToAction("Details", new { id = newProduct.ProductId });
            }
            return View(model);
        }

        // GET: ProductController/Edit/5
        public ActionResult Edit(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            Product product = ProductRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            EditViewModel productEditViewModel = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };
            return View(productEditViewModel);
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            if (ModelState.IsValid)
            {
                Product product = ProductRepository.GetById(model.ProductId);
                if (product == null)
                {
                    return NotFound();
                }
                product.Name = model.Name;
                product.Price = model.Price;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;
                if (model.ImagePath != null)
                {
                    if (!string.IsNullOrEmpty(model.ExistingImagePath))
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        if (System.IO.File.Exists(filePath))
                        {
                            try
                            {
                                System.IO.File.Delete(filePath);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("", $"Erreur lors de la suppression de l'ancienne image: {ex.Message}");
                            }
                        }
                    }
                    product.Image = ProcessUploadedFile(model);
                }

                Product updatedProduct = ProductRepository.Update(product);

                if (updatedProduct != null)
                    return RedirectToAction("Index");
                else
                    return NotFound();
            }

            return View(model);
        }

        [NonAction]
        private string ProcessUploadedFile(EditViewModel model)
        {
            string uniqueFileName = null;

            if (model.ImagePath != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImagePath.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            var product = ProductRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: ProductController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Search(string val)
        {
            var result = ProductRepository.FindByName(val);

            return View("Index", result);
        }
        // GET: ProductController
        [AllowAnonymous]
        public IActionResult Index(int? categoryId, int page = 1)
        {
            int pageSize = 6; 
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            IQueryable<Product> productsQuery = ProductRepository.GetAllProducts();
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }
            // Pagination
            var totalProducts = productsQuery.Count();
            var products = productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CategoryId = categoryId;
            return View(products);
        }
    }
}
