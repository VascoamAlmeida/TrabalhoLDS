namespace LojaMaquinas.Business.Models.Enums
{
    /// <summary>
    /// Define os estados possíveis de um aluguer (OrderRent).
    /// </summary>
    public enum OrderRentStatus
    {
        /// <summary>
        /// Aluguer aprovado mas ainda não iniciado.
        /// </summary>
        Approved = 0,

        /// <summary>
        /// Aluguer ativo (equipamento em posse do cliente).
        /// </summary>
        Active = 1,

        /// <summary>
        /// Aluguer concluído (equipamento devolvido).
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Aluguer cancelado antes ou durante o período.
        /// </summary>
        Cancelled = 3
    }
}
