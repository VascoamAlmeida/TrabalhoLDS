namespace LojaMaquinas.Business.Models
{
    /// <summary>
    /// Classe base abstrata para todas as entidades do sistema.
    /// Fornece identificador único (GUID) gerado automaticamente.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Construtor protegido que gera automaticamente um novo GUID para a entidade.
        /// </summary>
        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Identificador único da entidade (chave primária).
        /// Gerado automaticamente no construtor usando Guid.NewGuid().
        /// </summary>
        public Guid Id { get; set; }
    }
}
