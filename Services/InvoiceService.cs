using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoicePdfAsync(Order order);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly IConfiguration _configuration;

        public InvoiceService(IConfiguration configuration)
        {
            _configuration = configuration;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(Order order)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);

                        // En-tête
                        page.Header().Element(header =>
                        {
                            header.Row(row =>
                            {
                                // Logo et informations de la société
                                row.ConstantItem(150).Height(50).Image("wwwroot/images/logo.png");
                                
                                row.AutoItem().PaddingLeft(10).Column(column =>
                                {
                                    column.Item().Text("PhantomShop").Bold().FontSize(14);
                                    column.Item().Text("123 rue du Commerce");
                                    column.Item().Text("75001 Paris, France");
                                    column.Item().Text("contact@phantomshop.com");
                                });

                                // Informations de facture
                                row.AutoItem().AlignRight().Column(column =>
                                {
                                    column.Item().Text($"Facture #{order.Id}").Bold().FontSize(14);
                                    column.Item().Text($"Date: {order.OrderDate:dd/MM/yyyy}");
                                    column.Item().Text($"Statut: {order.Status}");
                                });
                            });
                        });

                        // Informations client
                        page.Content().PaddingVertical(20).Column(column =>
                        {
                            // Section client
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(200);
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Informations client").Bold();
                                    header.Cell().Text("");
                                });

                                table.Cell().Text("Nom:");
                                table.Cell().Text(order.CustomerName);

                                table.Cell().Text("Email:");
                                table.Cell().Text(order.Email);

                                table.Cell().Text("Adresse de livraison:");
                                table.Cell().Text(order.Address);
                            });

                            // Liste des articles
                            column.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // En-tête du tableau
                                table.Header(header =>
                                {
                                    header.Cell().Text("Produit").Bold();
                                    header.Cell().AlignRight().Text("Quantité").Bold();
                                    header.Cell().AlignRight().Text("Prix unitaire").Bold();
                                    header.Cell().AlignRight().Text("Total").Bold();
                                });

                                // Lignes du tableau
                                foreach (var item in order.Items)
                                {
                                    table.Cell().Text(item.ProductName);
                                    table.Cell().AlignRight().Text(item.Quantity.ToString());
                                    table.Cell().AlignRight().Text($"{item.Price:C2}");
                                    table.Cell().AlignRight().Text($"{(item.Price * item.Quantity):C2}");
                                }
                            });

                            // Total
                            column.Item().AlignRight().PaddingTop(10).Text($"Total: {order.TotalAmount:C2}").Bold().FontSize(12);

                            // Mode de paiement
                            column.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(200);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Mode de paiement:").Bold();
                                table.Cell().Text(order.PaymentMethod);
                            });
                        });

                        // Pied de page
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ").FontSize(10);
                                x.CurrentPageNumber().FontSize(10);
                                x.Span(" sur ").FontSize(10);
                                x.TotalPages().FontSize(10);
                            });
                    });
                });

                return document.GeneratePdf();
            });
        }
    }
}