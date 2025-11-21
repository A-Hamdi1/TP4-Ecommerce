using System.ComponentModel.DataAnnotations;

namespace WebApplication2.ViewModels
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "L'adresse de livraison est requise")]
        [Display(Name = "Adresse de livraison")]
        public string? Address { get; set; }
        
        [Required(ErrorMessage = "Veuillez sélectionner une méthode de paiement")]
        [Display(Name = "Méthode de paiement")]
        public string? PaymentMethod { get; set; }
        
        public float TotalAmount { get; set; }
        public List<CartItemViewModel>? CartItems { get; set; }
    }

}
