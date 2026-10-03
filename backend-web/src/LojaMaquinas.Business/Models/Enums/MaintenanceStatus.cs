namespace LojaMaquinas.Business.Models.Enums
{
    /// <summary>
    /// Define os estados possíveis de uma manutenção.
    /// </summary>
    public enum MaintenanceStatus
    {
        /// <summary>
        /// Manutenção agendada mas ainda não iniciada.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Manutenção em curso (técnico a trabalhar).
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Manutenção concluída com sucesso.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Manutenção cancelada (não realizada).
        /// </summary>
        Cancelled = 3
    }
}
