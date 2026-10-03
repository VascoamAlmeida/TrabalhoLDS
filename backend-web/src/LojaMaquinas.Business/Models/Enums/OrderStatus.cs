namespace LojaMaquinas.Business.Models.Enums
{
    /// <summary>
    /// Define os estados possíveis de um pedido (Order).
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Pedido pendente de confirmação ou pagamento.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Pedido ativo (confirmado e em processamento).
        /// </summary>
        Active = 1,

        /// <summary>
        /// Pedido concluído com sucesso.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Pedido cancelado pelo cliente ou sistema.
        /// </summary>
        Cancelled = 3
    }
}
