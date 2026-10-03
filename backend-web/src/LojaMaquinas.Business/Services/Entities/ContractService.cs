using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Validations.Entities;


namespace LojaMaquinas.Business.Services.Entities
{
    /// <summary>
    /// Implementação do serviço de gestão de contratos de aluguer.
    /// Contém lógica de negócio para criação e gestão de contratos legais.
    /// </summary>
    public class ContractService : BaseService, IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ITermsService _termsService;

        /// <summary>
        /// Inicializa uma nova instância do serviço de contratos.
        /// </summary>
        /// <param name="contractRepository">Repositório para acesso aos dados de contratos</param>
        /// <param name="termsService">Serviço para obter termos e condições</param>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        public ContractService(IContractRepository contractRepository, ITermsService termsService, INotifierService notifier) : base(notifier)
        {
            _contractRepository = contractRepository;
            _termsService = termsService;
        }

        /// <summary>
        /// Gera um novo contrato a partir de um aluguel (OrderRent).
        /// Obtém os termos e condições do sistema e cria o contrato automaticamente.
        /// </summary>
        /// <param name="orderRentId">Identificador único do OrderRent</param>
        /// <returns>Identificador do contrato criado ou Guid.Empty em caso de erro</returns>
        public async Task<Guid> Add(Guid orderRentId)
        {
            // Verificar contrato
            var contractExist = await _contractRepository.GetOrderRentIDbyContract(orderRentId);
            if (contractExist != null)
            {
                NotifyWithMessage("Contract já existe para esta Order Rent");
            }

            // Obter termos
            var terms = await _termsService.GetTermsAndConditions();
            if (terms == null)
            {
                NotifyWithMessage("Dont exist terms and conditions");
                return Guid.Empty;
            }

            // Criar uma entidade Contract
            var contract = new Contract
            {
                OrderRentId = orderRentId,
                TermsConditions = terms
            };

            if (!RunValidation(new ContractValidation(), contract)) return Guid.Empty;

            // 2. Run persistence
            await _contractRepository.Add(contract);

            return contract.Id;

        }

        /// <summary>
        /// Atualiza os termos e condições de um contrato existente.
        /// </summary>
        /// <param name="contractId">Identificador único do contrato</param>
        /// <param name="newTerms">Novos termos e condições do contrato</param>
        public async Task Update(Guid contractId, string newTerms)
        {
            {
                var contract = await _contractRepository.GetById(contractId);
                if (contract == null)
                {
                    NotifyWithMessage("Contract not found.");
                }

                if (newTerms == null)
                {
                    NotifyWithMessage("Dont exist terms and conditions");
                    return;
                }

                // Atualizar os termos e condiçoes
                contract.TermsConditions = newTerms;

                if (!RunValidation(new ContractValidation(), contract)) return;


                // 2. Run persistence
                await _contractRepository.Update(contract);
            }
        }

        /// <summary>
        /// Remove um contrato do sistema.
        /// </summary>
        /// <param name="contractId">Identificador único do contrato a remover</param>
        public async Task Remove(Guid contractId)
        {

            await _contractRepository.Remove(contractId);
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo serviço.
        /// </summary>
        public void Dispose()
        {
            _contractRepository.Dispose();
        }
    }
}
