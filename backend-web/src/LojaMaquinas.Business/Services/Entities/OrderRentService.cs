using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Validations.Entities;
using System.Text;

namespace LojaMaquinas.Business.Services.Entities
{
    /// <summary>
    /// Implementação do serviço de gestão de alugueres (OrderRent).
    /// Contém lógica de negócio complexa para gestão de estados de aluguer e assinaturas digitais.
    /// </summary>
    public class OrderRentService : BaseService, IOrderRentService
    {
        private readonly IOrderRentRepository _orderRentRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de alugueres.
        /// </summary>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        /// <param name="orderRentRepository">Repositório para acesso aos dados de alugueres</param>
        public OrderRentService(INotifierService notifier, IOrderRentRepository orderRentRepository) : base(notifier)
        {
            _orderRentRepository = orderRentRepository;
        }

        /// <summary>
        /// Adiciona um novo aluguer ao sistema.
        /// Valida os dados e define o estado inicial como Approved.
        /// </summary>
        /// <param name="orderRent">Aluguer a ser adicionado</param>
        public async Task Add(OrderRent orderRent)
        {
            // 1. Run Validation of data entry and data in the database
            if (!RunValidation(new OrderRentValidation(), orderRent)) return;

            // Verifica se existe Order Rent no repository
            if (_orderRentRepository.Find(c => c.Id == orderRent.Id).Result.Any())
            {
                NotifyWithMessage("Order not found");
                return;
            }

            // Muda o Status de order Rent para approved
            orderRent.Status = OrderRentStatus.Approved;


            // 2. Run persistence
            await _orderRentRepository.Add(orderRent);
        }

        /// <summary>
        /// Remove um aluguer do sistema.
        /// Apenas permite remoção se o aluguer estiver com estado Cancelled.
        /// </summary>
        /// <param name="orderRentId">Identificador único do aluguer a remover</param>
        public async Task Remove(Guid orderRentId)
        {

            // Buscar o aluguer já com o Contract ligado
            var orderRent = await _orderRentRepository.GetOrderRentAndOrderAndContractByOrderRentId(orderRentId);

            // AQUI VOU VERIFICAR SE EXISTEM OS DADOS NA ORDER RENT
            if (orderRent == null)
            {
                NotifyWithMessage("OrderRent not found");
                return;
            }

            // Verifica se o orderRent tem ou não um contract daquele id
            if (orderRent.Status != OrderRentStatus.Cancelled)
            {
                NotifyWithMessage($"Can only remove cancelled order");
                return;
            }

            // Remove primeiro o Contract
            if (orderRent.Contract != null)
            {
                await _orderRentRepository.DeleteContractByOrderRent(orderRent.Contract);
            }

            await _orderRentRepository.Remove(orderRentId);
        }


        /// <summary>
        /// Atualiza os dados de um aluguer existente.
        /// Gere transições de estado (Approved→Active→Completed), valida assinaturas digitais
        /// e permite editar detalhes apenas no estado Appdroved.
        /// </summary>
        /// <param name="orderRent">Aluguer com os dados atualizados</param>
        public async Task Update(OrderRent orderRent)
        {
            {
                // Verifica se existe Order Rent no repository
                var current = await _orderRentRepository.GetById(orderRent.Id);
                if (current == null)
                {
                    NotifyWithMessage("Order not found");
                    return;
                }

                // 1. Run Validation of data entry and data in the database
                if (!RunValidation(new OrderRentValidation(), orderRent)) return;

                if (current.Status == OrderRentStatus.Cancelled ||
                    current.Status == OrderRentStatus.Completed)
                {
                    NotifyWithMessage("Can´t change status");
                    return;
                }

                // Verificar se existe mudança estado
                var statusChanged = orderRent.Status != current.Status;

                if (statusChanged)
                {
                    // Vai verificar se existe a foto da company
                    if (current.Status == OrderRentStatus.Approved &&
                        orderRent.Status == OrderRentStatus.Active)
                    {
                        current.Status = OrderRentStatus.Active;
                    }

                    else if (current.Status == OrderRentStatus.Active &&
                                orderRent.Status == OrderRentStatus.Completed)

                    // Para mudar foto
                    {
                        if (string.IsNullOrEmpty(orderRent.UserImage))
                        {
                            NotifyWithMessage("User photo is needed!");
                            return;
                        }
                        // Para mudar foto
                        current.UserImage = orderRent.UserImage;
                        current.Status = OrderRentStatus.Completed;
                    }

                    else if (current.Status == OrderRentStatus.Approved &&
                                orderRent.Status == OrderRentStatus.Cancelled)
                    {
                        current.Status = OrderRentStatus.Cancelled;
                    }
                    else
                    {
                        NotifyWithMessage("Erro na mudança de estado.");
                        return;
                    }
                }
                else
                {
                    // Se nao houver mudança de estado só possivel editar detalhes só em Approved
                    if (current.Status != OrderRentStatus.Approved)
                    {
                        NotifyWithMessage("Só é possível editar detalhes em Approved.");
                        return;
                    }

                    // Atributos que podem ser alterados sem alteração do estado
                    current.InitialDate = orderRent.InitialDate;
                    current.EndDate = orderRent.EndDate;
                }

                // 2. Run persistence
                await _orderRentRepository.Update(current);
            }

        }

        /// <summary>
        /// Obtém os alugueres cujo contrato termina nos próximos N dias.
        /// Usado para o requisito "Alerta fim de contrato".
        /// </summary>
        /// <param name="daysAhead">Número de dias a partir de hoje (deve ser &gt; 0).</param>
        /// <returns>Lista de alugueres a terminar em breve.</returns>
        public async Task<IEnumerable<OrderRent>> GetRentsEndingSoon(int daysAhead)
        {
            if (daysAhead <= 0)
            {
                NotifyWithMessage("O número de dias deve ser maior que zero.");
                return Enumerable.Empty<OrderRent>();
            }

            // Trabalhar sempre com a parte da data (sem horas)
            var today = DateTime.UtcNow.Date;
            var end = today.AddDays(daysAhead);

            return await _orderRentRepository.GetRentsEndingSoon(today, end);
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo serviço.
        /// </summary>
        public void Dispose()
        {
            _orderRentRepository.Dispose();
        }
    }
}

