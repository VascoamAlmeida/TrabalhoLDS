namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de serviço para gestão de termos e condições.
    /// Fornece acesso ao template de termos e condições do sistema.
    /// </summary>
    public interface ITermsService
    {
        /// <summary>
        /// Obtém o template atual de termos e condições do sistema.
        /// Utilizado na criação de contratos de aluguel.
        /// </summary>
        /// <returns>String com os termos e condições ou null se não existir template</returns>
        Task<string> GetTermsAndConditions();
    }
}