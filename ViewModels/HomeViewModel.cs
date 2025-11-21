using System.Collections.Generic;
using WebApplication2.Models;

namespace WebApplication2.ViewModels
{
    public class HomeViewModel
    {
        public List<Product> TopProducts { get; set; } = new List<Product>();
        public Promotion Promotion { get; set; } = new Promotion();
        public List<CustomerReview> Reviews { get; set; } = new List<CustomerReview>();
    }

    public class Promotion
    {
        public string Title { get; set; } = "Découvrez nos offres exceptionnelles";
        public string Description { get; set; } = "Profitez de réductions incroyables sur une sélection de produits premium";
        public string ButtonText { get; set; } = "Découvrir maintenant";
        public string ButtonLink { get; set; } = "/Product";
    }

    public class CustomerReview
    {
        public string CustomerName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; } = 4;
        public string Avatar { get; set; } = "👤";
    }
}
