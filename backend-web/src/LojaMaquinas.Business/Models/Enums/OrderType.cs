namespace LojaMaquinas.Business.Models.Enums
{
    /// <summary>
    /// Define os tipos de pedido possíveis no sistema.
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// Pedido de compra (venda definitiva).
        /// </summary>
        Sale = 0,

        /// <summary>
        /// Pedido de aluguer (rental temporário).
        /// </summary>
        Rent = 1
    }
}
