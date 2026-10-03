using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de serviço para gestão de aluguéis (OrderRent).
    /// Contém a lógica de negócio para criação e gestão de aluguéis de equipamentos.
    /// </summary>
    public interface IOrderRentService : IDisposable
    {
        /// <summary>
        /// Adiciona um novo aluguel ao sistema a partir de um pedido.
        /// Valida os dados e cria o registo de aluguel com datas e valores.
        /// </summary>
        /// <param name="orderRent">Aluguel a ser adicionado</param>
        Task Add(OrderRent orderRent);

        /// <summary>
        /// Atualiza um aluguel existente.
        /// Permite alterar datas, valores e estado do aluguel.
        /// </summary>
        /// <param name="orderRent">Aluguel com os dados atualizados</param>
        Task Update(OrderRent orderRent);

        /// <summary>
        /// Remove um aluguel do sistema.
        /// </summary>
        /// <param name="orderRent">Identificador único do aluguel a remover</param>
        Task Remove(Guid orderRent);

        /// <summary>
        /// Devolve os alugueres cujo contrato termina
        /// dentro dos próximos N dias.
        /// </summary>
        /// <param name="daysAhead">Número de dias a partir de hoje.</param>
        Task<IEnumerable<OrderRent>> GetRentsEndingSoon(int daysAhead);
    }
}
