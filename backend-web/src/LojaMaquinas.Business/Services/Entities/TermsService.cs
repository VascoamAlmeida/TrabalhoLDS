using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces.Parameters;

namespace LojaMaquinas.Business.Services.Entities
{
    /// <summary>
    /// Implementação do serviço de gestão de termos e condições.
    /// Fornece acesso ao template padrão de termos e condições do sistema.
    /// </summary>
    public class TermsService : ITermsService
    {
        /// <summary>
        /// Template padrão de termos e condições para contratos de aluguer.
        /// Contém as cláusulas base aplicadas a todos os contratos.
        /// </summary>
        private const string DefaultTerms =
            "Contrato de aluguer.\n" +
            "1️- O cliente é responsável pelo equipamento.\n" +
            "2️- O pagamento deve ser efetuado na entrega.\n" +
            "3️- Tem manutenção.";

        /// <summary>
        /// Obtém o template atual de termos e condições do sistema.
        /// Utilizado na criação de contratos de aluguer.
        /// </summary>
        /// <returns>String com os termos e condições padrão</returns>
        public async Task<string> GetTermsAndConditions()
        {

            return DefaultTerms;
        }
    }
}
