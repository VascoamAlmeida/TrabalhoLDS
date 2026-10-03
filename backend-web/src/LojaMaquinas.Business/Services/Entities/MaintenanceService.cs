using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Validations.Entities;

namespace LojaMaquinas.Business.Services.Entities
{
    /// <summary>
    /// Implementação do serviço de gestão de manutenções.
    /// Contém lógica de negócio para registo e acompanhamento de manutenções de equipamentos.
    /// </summary>
    public class MaintenanceService : BaseService, IMaintenanceService
    {
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de manutenções.
        /// </summary>
        /// <param name="maintenanceRepository">Repositório para acesso aos dados de manutenções</param>
        /// <param name="orderRepository">Repositório para verificação de pedidos</param>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        public MaintenanceService(IMaintenanceRepository maintenanceRepository,
                                  IOrderRepository orderRepository,
                                  INotifierService notifier) : base(notifier)
        {
            _maintenanceRepository = maintenanceRepository;
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Adiciona uma nova manutenção ao sistema.
        /// Valida os dados e verifica se o pedido associado existe.
        /// </summary>
        /// <param name="maintenance">Manutenção a ser adicionada</param>
        public async Task Add(Maintenance maintenance)
        {
            // 1. Run Validation
            if (!RunValidation(new MaintenanceValidation(), maintenance)) return;

            // 2. Verify if Order exists
            var order = await _orderRepository.GetById(maintenance.OrderId);
            if (order == null)
            {
                NotifyWithMessage($"Order with ID {maintenance.OrderId} not found!");
                return;
            }

            // 3. Run persistence
            await _maintenanceRepository.Add(maintenance);
        }

        /// <summary>
        /// Atualiza os dados de uma manutenção existente.
        /// Permite alterar estado, datas e observações da manutenção.
        /// </summary>
        /// <param name="maintenance">Manutenção com os dados atualizados</param>
        public async Task Update(Maintenance maintenance)
        {
            // 1. Run Validation
            if (!RunValidation(new MaintenanceValidation(), maintenance)) return;

            // 2. Run persistence
            await _maintenanceRepository.Update(maintenance);
        }

        /// <summary>
        /// Remove uma manutenção do sistema.
        /// </summary>
        /// <param name="id">Identificador único da manutenção a remover</param>
        public async Task Remove(Guid id)
        {
            var maintenance = await _maintenanceRepository.GetById(id);

            if (maintenance == null)
            {
                NotifyWithMessage("Maintenance not found!");
                return;
            }

            await _maintenanceRepository.Remove(id);
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo serviço.
        /// </summary>
        public void Dispose()
        {
            _maintenanceRepository.Dispose();
        }
    }
}
