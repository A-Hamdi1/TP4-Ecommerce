namespace WebApplication2.Models.Repositories
{
    public interface IOrderRepository
    {
        Order GetById(int Id);
        Task Add(Order o);
        Task UpdateStatus(int orderId, OrderStatus newStatus, string changedByUserId = null, string note = null);
        IEnumerable<OrderHistory> GetHistory(int orderId);
        IEnumerable<Order> GetAllOrders();
        IEnumerable<Order> GetOrdersByUser(string userId);
    }

}
