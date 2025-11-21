namespace WebApplication2.Models
{
    public class Commande
    {
        public int CommandeId { get; set; }
        public int PanierId { get; set; }
        public virtual Panier Panier { get; set; }
    }
}
