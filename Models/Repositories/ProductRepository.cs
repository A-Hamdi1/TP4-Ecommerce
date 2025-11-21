using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using WebApplication2.Models;
using WebApplication2.Models.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext context;

    public ProductRepository(AppDbContext context)
    {
        this.context = context;
    }

    // Récupérer tous les produits avec leur catégorie
    public IList<Product> GetAll()
    {
        return context.Products
                      .Include(p => p.Category)
                      .OrderBy(p => p.Name)
                      .ToList();
    }

    // Récupérer un produit par son ID
    public Product GetById(int id)
    {
        return context.Products
                      .Include(p => p.Category)
                      .SingleOrDefault(p => p.ProductId == id);
    }

    // Ajouter un nouveau produit
    public void Add(Product p)
    {
        context.Products.Add(p);
        context.SaveChanges();
    }

    // Rechercher des produits par nom ou catégorie
    public IList<Product> FindByName(string name)
    {
        return context.Products
                      .Include(p => p.Category)
                      .Where(p => p.Name.Contains(name) ||
                                  p.Category.CategoryName.Contains(name))
                      .ToList();
    }

    // Mettre à jour un produit
    public Product Update(Product p)
    {
        Product p1 = context.Products.Find(p.ProductId);
        if (p1 != null)
        {
            p1.Name = p.Name;
            p1.Price = p.Price;
            p1.QteStock = p.QteStock;
            p1.CategoryId = p.CategoryId;
            context.SaveChanges();
        }
        return p1;
    }

    // Supprimer un produit
    public void Delete(int ProductId)
    {
        Product p1 = context.Products.Find(ProductId);
        if (p1 != null)
        {
            context.Products.Remove(p1);
            context.SaveChanges();
        }
    }

    // Récupérer les produits par ID de catégorie
    public IList<Product> GetProductsByCategID(int? CategId)
    {
        return context.Products
                      .Include(p => p.Category)
                      .Where(p => p.CategoryId == CategId)
                      .OrderBy(p => p.ProductId)
                      .ToList();
    }
    public IQueryable<Product> GetAllProducts()
    {
        return context.Products.Include(p => p.Category);
    }

    // Mettre à jour le stock d'un produit
    public bool UpdateStock(int productId, int quantity)
    {
        var product = context.Products.Find(productId);
        if (product != null && product.QteStock >= quantity)
        {
            product.QteStock -= quantity;
            context.SaveChanges();
            return true;
        }
        return false;
    }
}
