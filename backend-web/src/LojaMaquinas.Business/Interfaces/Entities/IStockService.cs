using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Parameters
{
    /// <summary>
    /// Interface de serviço para gestão de inventário (Stock).
    /// Contém a lógica de negócio para controlo de stock e processamento de devoluções.
    /// </summary>
    public interface IStockService : IDisposable
    {
        /// <summary>
        /// Adiciona um novo item de stock ao sistema.
        /// Valida os dados e atualiza a quantidade do produto associado.
        /// </summary>
        /// <param name="stock">Item de stock a ser adicionado</param>
        Task Add(Stock stock);

        /// <summary>
        /// Atualiza um item de stock existente.
        /// Valida os dados e verifica se o stock existe antes de atualizar.
        /// </summary>
        /// <param name="stock">Stock com os dados atualizados</param>
        Task Update(Stock stock);

        /// <summary>
        /// Remove um item de stock do sistema.
        /// Atualiza a quantidade do produto associado.
        /// </summary>
        /// <param name="id">Identificador único do stock a remover</param>
        Task Remove(Guid id);

        /// <summary>
        /// Processa a devolução de um equipamento alugado.
        /// Altera o estado do stock para Available.
        /// </summary>
        /// <param name="stockId">Identificador único do stock devolvido</param>
        Task ProcessRentReturn(Guid stockId);

        /// <summary>
        /// Processa o cancelamento de um pedido e devolve o stock.
        /// Altera o estado do stock para Available.
        /// </summary>
        /// <param name="stockId">Identificador único do stock a devolver</param>
        Task ProcessCancelReturn(Guid stockId);
    }
}
