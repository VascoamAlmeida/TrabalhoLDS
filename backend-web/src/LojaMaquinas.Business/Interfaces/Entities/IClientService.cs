using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities;

/// <summary>
/// Interface de serviço para gestão de clientes.
/// Contém a lógica de negócio e validações relacionadas com clientes.
/// </summary>
public interface IClientService : IDisposable
{
    /// <summary>
    /// Adiciona um novo cliente ao sistema.
    /// Valida os dados e verifica duplicações (NIF, email) antes de persistir.
    /// </summary>
    /// <param name="client">Cliente a ser adicionado</param>
    Task Add(Client client);

    /// <summary>
    /// Atualiza um cliente existente.
    /// Valida os dados e verifica se o cliente existe antes de atualizar.
    /// </summary>
    /// <param name="client">Cliente com os dados atualizados</param>
    Task Update(Client client);

    /// <summary>
    /// Remove um cliente do sistema.
    /// </summary>
    /// <param name="id">Identificador único do cliente a remover</param>
    Task Remove(Guid id);

    /// <summary>
    /// Obtém os aluguéis (OrderRent) que estão próximos de expirar para um cliente.
    /// Útil para notificações e gestão de renovações.
    /// </summary>
    /// <param name="id">Identificador único do cliente</param>
    /// <returns>Lista de aluguéis a expirar ou null</returns>
    Task<List<OrderRent?>?> GetExpiringOrderRents(Guid id);
}
    
