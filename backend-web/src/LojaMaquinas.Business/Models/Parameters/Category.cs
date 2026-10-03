using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Models.Parameters
{
    /// <summary>
    /// Representa uma categoria de produtos no sistema.
    /// Utilizada para organizar e classificar produtos em grupos.
    /// </summary>
    public class Category : Entity
    {
        /// <summary>
        /// Nome da categoria.
        /// Deve ser único e descritivo (ex: "Escavadoras", "Guindastes").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrição detalhada da categoria.
        /// Explica que tipo de produtos pertencem a esta categoria.
        /// </summary>
        public string Description { get; set; }

        /* EF Relations */

        /// <summary>
        /// Coleção de produtos pertencentes a esta categoria (relação 1:N).
        /// Navigation property do Entity Framework.
        /// </summary>
        public IEnumerable<Product> Products { get; set; }

    }
}
