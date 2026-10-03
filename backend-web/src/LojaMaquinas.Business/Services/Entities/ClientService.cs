using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Validations.Entities;

namespace LojaMaquinas.Business.Services.Entities
{
    /// <summary>
    /// Implementação do serviço de gestão de clientes.
    /// Contém lógica de negócio para validação e manipulação de dados de clientes.
    /// </summary>
    public class ClientService : BaseService, IClientService
    {
        private readonly IClientRepository _clientRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de clientes.
        /// </summary>
        /// <param name="clientRepository">Repositório para acesso aos dados de clientes</param>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        public ClientService(IClientRepository clientRepository, INotifierService notifier) : base(notifier)
        {
            _clientRepository = clientRepository;
        }

        /// <summary>
        /// Adiciona um novo cliente ao sistema.
        /// Valida os dados e verifica se o NIF já existe antes de persistir.
        /// </summary>
        /// <param name="client">Cliente a ser adicionado</param>
        public async Task Add(Client client)
        {
            // 1. Run Validation
            if (!RunValidation(new ClientValidation(), client)) return;

            // 2. Check if NIF already exists
            var existingClient = await _clientRepository.GetByNif(client.NIF);
            if (existingClient != null && existingClient.Id != client.Id)
            {
                NotifyWithMessage($"A client with NIF {client.NIF} already exists!");
                return;
            }

            // 3. Run persistence
            await _clientRepository.Add(client);
        }

        /// <summary>
        /// Atualiza os dados de um cliente existente.
        /// Valida os dados e verifica se não existe outro cliente com o mesmo NIF.
        /// </summary>
        /// <param name="client">Cliente com os dados atualizados</param>
        public async Task Update(Client client)
        {
            // 1. Run Validation
            if (!RunValidation(new ClientValidation(), client)) return;

            // 2. Check if another client with same NIF exists
            var existingClient = await _clientRepository.GetByNif(client.NIF);
            if (existingClient != null && existingClient.Id != client.Id)
            {
                NotifyWithMessage($"Another client with NIF {client.NIF} already exists!");
                return;
            }

            // 3. Run persistence
            await _clientRepository.Update(client);
        }

        /// <summary>
        /// Remove um cliente do sistema.
        /// Verifica se o cliente tem pedidos associados antes de permitir a remoção.
        /// </summary>
        /// <param name="id">Identificador único do cliente a remover</param>
        public async Task Remove(Guid id)
        {
            var client = await _clientRepository.GetClientWithOrders(id);

            if (client == null)
            {
                NotifyWithMessage("Client not found!");
                return;
            }

            if (client.Orders != null && client.Orders.Any())
            {
                NotifyWithMessage($"The client {client.FirstName} {client.LastName} cannot be deleted as there are related orders!");
                return;
            }

            await _clientRepository.Remove(id);
        }

        /// <summary>
        /// Obtém os alugueres de um cliente que estão próximos de expirar (nos próximos 7 dias).
        /// Útil para notificações e gestão de renovações.
        /// </summary>
        /// <param name="id">Identificador único do cliente</param>
        /// <returns>Lista de alugueres a expirar ou null se não houver</returns>
        public async Task<List<OrderRent?>?> GetExpiringOrderRents(Guid id)
        {
            // 1. Fetch the client with necessary related data (Orders and OrderRents)
            var client = await _clientRepository.GetClientWithOrders(id);

            if (client == null)
            {
                NotifyWithMessage("Client not found!");
                return null;
            }

            var today = DateTime.UtcNow.Date;
            var expiryDateLimit = today.AddDays(7);

            var expiringRents = client.Orders
                .Where(o => o.OrderRent != null)
                .Select(o => o.OrderRent)
                .Where(rent =>
                    rent.EndDate.Date >= today &&
                    rent.EndDate.Date <= expiryDateLimit)
                .ToList();

            if (expiringRents.Any())
            {
                NotifyWithMessage($"Client {client.FirstName} has {expiringRents.Count} order rent(s) expiring within the next 7 days!");
                return expiringRents;
            }
            else
            {
                NotifyWithMessage($"Client {client.FirstName} has no order rents expiring soon.");
                return null;
            }
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo serviço.
        /// </summary>
        public void Dispose()
        {
            _clientRepository.Dispose();
        }
    }
}
