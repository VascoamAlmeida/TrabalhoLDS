using LojaMaquinas.Business.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LojaMaquinas.Business.Services.Auxiliaries
{
    /// <summary>
    /// Serviço para geração de documentos PDF.
    /// Utiliza a biblioteca QuestPDF para criar relatórios e documentos de pedidos.
    /// </summary>
    public class PdfService
    {
        private readonly string _imagePath;

        /// <summary>
        /// Inicializa uma nova instância do serviço PDF.
        /// Configura a licença QuestPDF e define o caminho do logotipo.
        /// </summary>
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logo.png");
        }

        /// <summary>
        /// Gera um documento PDF com os detalhes de um pedido.
        /// Inclui informações do cliente, pedido, stock e produto.
        /// </summary>
        /// <param name="order">Pedido a incluir no PDF</param>
        /// <param name="client">Cliente associado ao pedido</param>
        /// <param name="stock">Item de stock do pedido</param>
        /// <param name="product">Produto do pedido</param>
        /// <returns>Array de bytes representando o PDF gerado</returns>
        public byte[] GenerateOrderPdf(Order order, Client client, Stock stock, Product product)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Component(new OrderHeaderComponent(_imagePath));

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            column.Item()
                                .AlignCenter()
                                .Text("ORDER DETAILS")
                                .FontSize(20)
                                .Bold();

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            column.Item().Component(new ClientInfoComponent(client));
                            column.Item().Component(new OrderInfoComponent(order));

                            if (stock != null)
                            {
                                column.Item().Component(new StockInfoComponent(stock));
                            }

                            if (product != null)
                            {
                                column.Item().Component(new ProductInfoComponent(product));
                            }
                            else if (stock != null)
                            {
                                column.Item()
                                    .Text("No product information available.")
                                    .Italic()
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                            x.Span($" | Generated on {DateTime.Now:dd/MM/yyyy HH:mm}");
                        });
                });
            });

            return document.GeneratePdf();
        }
        /// <summary>
        /// Gera um documento PDF com os detalhes de um pedido.
        /// Inclui informações do cliente, pedido, stock e produto.
        /// </summary>
        /// <param name="order">Pedido a incluir no PDF</param>
        /// <param name="client">Cliente associado ao pedido</param>
        /// <param name="stock">Item de stock do pedido</param>
        /// <param name="product">Produto do pedido</param>
        /// <param name="orderRent">Pedido de aluguer</param>
        /// <returns>Array de bytes representando o PDF gerado</returns>
        public byte[] GenerateOrderRentPdf(Order order, Client client, Stock stock, Product product, OrderRent orderRent)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Component(new OrderHeaderComponent(_imagePath));

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            column.Item()
                                .AlignCenter()
                                .Text("ORDER DETAILS")
                                .FontSize(20)
                                .Bold();

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            column.Item().Component(new ClientInfoComponent(client));
                            column.Item().Component(new OrderInfoComponent(order));
                            if (order.Type == Models.Enums.OrderType.Rent)
                            {
                                column.Item().Component(new OrderRentInfoComponent(orderRent));
                            }


                            if (stock != null)
                            {
                                column.Item().Component(new StockInfoComponent(stock));
                            }

                            if (product != null)
                            {
                                column.Item().Component(new ProductInfoComponent(product));
                            }
                            else if (stock != null)
                            {
                                column.Item()
                                    .Text("No product information available.")
                                    .Italic()
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                            x.Span($" | Generated on {DateTime.Now:dd/MM/yyyy HH:mm}");
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}

// Componente para o Header
public class OrderHeaderComponent : IComponent
{
    private readonly string _imagePath;

    public OrderHeaderComponent(string imagePath)
    {
        _imagePath = imagePath;
    }

    public void Compose(IContainer container)
    {
        container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingBottom(10)
            .Row(row =>
            {
                row.RelativeItem()
                    .AlignLeft()
                    .Column(column =>
                    {
                        column.Item()
                            .Width(150)
                            .Image(_imagePath);
                    });

                row.RelativeItem()
                    .AlignRight()
                    .Border(1)
                    .BorderColor(Colors.Black)
                    .Padding(10)
                    .Column(column =>
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Projeto: ").Bold();
                            text.Span("MachineryTrack – Sistema de Gestão para Loja de Máquinas e Equipamento do Setor do Calçado");
                        });

                        column.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Grupo: ").Bold();
                            text.Span("21");
                        });

                        column.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Unidade Curricular: ").Bold();
                            text.Span("Laboratório de Desenvolvimento de Software");
                        });
                    });
            });
    }
}

// Componente para Informações do Cliente
public class ClientInfoComponent : IComponent
{
    private readonly Client _client;

    public ClientInfoComponent(Client client)
    {
        _client = client;
    }

    public void Compose(IContainer container)
    {
        container
            .Background(Colors.Grey.Lighten3)
            .Padding(10)
            .Column(column =>
            {
                column.Item()
                    .Text("CLIENT INFORMATION")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("User ID: ").Bold();
                    text.Span(_client?.UserId.ToString() ?? "N/A");
                });

                column.Item().Text(text =>
                {
                    text.Span("Name: ").Bold();
                    text.Span($"{_client?.FirstName} {_client?.LastName}" ?? "N/A");
                });

                column.Item().Text(text =>
                {
                    text.Span("NIF: ").Bold();
                    text.Span(_client?.NIF.ToString() ?? "N/A");
                });
            });
    }
}

// Componente para Informações da Order
public class OrderInfoComponent : IComponent
{
    private readonly Order _order;

    public OrderInfoComponent(Order order)
    {
        _order = order;
    }

    public void Compose(IContainer container)
    {
        container
            .Background(Colors.Grey.Lighten3)
            .Padding(10)
            .Column(column =>
            {
                column.Item()
                    .Text("ORDER INFORMATION")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Order ID: ").Bold();
                    text.Span(_order.Id.ToString()[..8]);
                });

                column.Item().Text(text =>
                {
                    text.Span("Order Type: ").Bold();
                    text.Span(_order.Type.ToString());
                });

                column.Item().Text(text =>
                {
                    text.Span("Status: ").Bold();
                    text.Span(_order.Status.ToString());
                });

                column.Item().Text(text =>
                {
                    text.Span("Order Date: ").Bold();
                    text.Span(_order.OrderDate.ToString("dd/MM/yyyy HH:mm"));
                });

                column.Item().Text(text =>
                {
                    text.Span("Stock ID: ").Bold();
                    text.Span(_order.StockId.ToString() ?? "N/A");
                });
                column.Item().Text(text =>
                {
                    text.Span("Total: ").Bold();
                    text.Span(_order.Total.ToString("C"));
                });
            });
    }
}

public class OrderRentInfoComponent : IComponent
{
    private readonly OrderRent _orderRent;

    public OrderRentInfoComponent(OrderRent orderRent)
    {
        _orderRent = orderRent;
    }

    public void Compose(IContainer container)
    {
        container
            .Background(Colors.Grey.Lighten3)
            .Padding(10)
            .Column(column =>
            {
                column.Item()
                    .Text("ORDER RENT INFORMATION")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Order ID: ").Bold();
                    text.Span(_orderRent.Id.ToString()[..8]);
                });

                column.Item().Text(text =>
                {
                    text.Span("Order Type: ").Bold();
                    text.Span(_orderRent.InitialDate.ToString());
                });

                column.Item().Text(text =>
                {
                    text.Span("Order Type: ").Bold();
                    text.Span(_orderRent.EndDate.ToString());
                });

                column.Item().Text(text =>
                {
                    text.Span("Status: ").Bold();
                    text.Span(_orderRent.Status.ToString());
                });
            });
    }
}

// Componente para Informações do Stock
public class StockInfoComponent : IComponent
{
    private readonly Stock _stock;

    public StockInfoComponent(Stock stock)
    {
        _stock = stock;
    }

    public void Compose(IContainer container)
    {
        container
            .Background(Colors.Grey.Lighten3)
            .Padding(10)
            .Column(column =>
            {
                column.Item()
                    .Text("STOCK DETAILS")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Reference: ").Bold();
                    text.Span(_stock.Ref.ToString() ?? "N/A");
                });

                column.Item().Text(text =>
                {
                    text.Span("Product ID: ").Bold();
                    text.Span(_stock.ProductId.ToString());
                });

                column.Item().Text(text =>
                {
                    text.Span("Status: ").Bold();
                    text.Span(_stock.StockStatus.ToString());
                });
            });
    }
}

// Componente para Informações do Product
public class ProductInfoComponent : IComponent
{
    private readonly Product _product;

    public ProductInfoComponent(Product product)
    {
        _product = product;
    }

    public void Compose(IContainer container)
    {
        container
            .Background(Colors.Grey.Lighten3)
            .Padding(10)
            .Column(column =>
            {
                column.Item()
                    .Text("PRODUCT DETAILS")
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Product ID: ").Bold();
                    text.Span(_product.Id.ToString()[..8]);
                });

                column.Item().Text(text =>
                {
                    text.Span("Name: ").Bold();
                    text.Span(_product.Name ?? "N/A");
                });

                column.Item().Text(text =>
                {
                    text.Span("Description: ").Bold();
                    text.Span(_product.Description ?? "N/A");
                });

                column.Item().Text(text =>
                {
                    text.Span("Price: ").Bold();
                    text.Span(_product.Price.ToString("C"));
                });

                // Adicione mais campos do Product conforme necessário
            });
    }
}
