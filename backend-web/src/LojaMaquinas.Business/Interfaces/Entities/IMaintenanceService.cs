using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de serviço para gestão de manutenções.
    /// Contém a lógica de negócio para registo e acompanhamento de manutenções de equipamentos.
    /// </summary>
    public interface IMaintenanceService : IDisposable
    {
        /// <summary>
        /// Adiciona uma nova manutenção ao sistema.
        /// Valida os dados e regista a manutenção associada a um pedido.
        /// </summary>
        /// <param name="maintenance">Manutenção a ser adicionada</param>
        Task Add(Maintenance maintenance);

        /// <summary>
        /// Atualiza uma manutenção existente.
        /// Permite alterar estado, datas e observações da manutenção.
        /// </summary>
        /// <param name="maintenance">Manutenção com os dados atualizados</param>
        Task Update(Maintenance maintenance);

        /// <summary>
        /// Remove uma manutenção do sistema.
        /// </summary>
        /// <param name="id">Identificador único da manutenção a remover</param>
        Task Remove(Guid id);
    }
}
