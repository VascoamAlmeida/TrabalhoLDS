using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LojaMaquinas.WebApp.ViewModels.API
{
    /// <summary>
    /// ViewModel de categoria para API REST.
    /// Representa dados de categoria transferidos entre API e cliente.
    /// </summary>
    public class CategoryApiViewModel
    {
        /// <summary>
        /// Identificador único da categoria.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nome da categoria.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrição detalhada da categoria.
        /// </summary>
        public string Description { get; set; }
    }
}
