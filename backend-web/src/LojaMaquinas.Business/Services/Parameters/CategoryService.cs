using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.Business.Validations.Parameters;

namespace LojaMaquinas.Business.Services.Parameters
{
    /// <summary>
    /// Implementação do serviço de gestão de categorias de produtos.
    /// Contém lógica de negócio para validação e manipulação de categorias.
    /// </summary>
    public class CategoryService : BaseService, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de categorias.
        /// </summary>
        /// <param name="categoryRepository">Repositório para acesso aos dados de categorias</param>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        public CategoryService(ICategoryRepository categoryRepository, INotifierService notifier) : base(notifier)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Adiciona uma nova categoria ao sistema.
        /// Valida os dados e verifica se já existe uma categoria com o mesmo nome.
        /// </summary>
        /// <param name="category">Categoria a ser adicionada</param>
        public async Task Add(Category category)
        {
            // 1. Run Validation of data entry and data in the database
            if (!RunValidation(new CategoryValidation(), category)) return;

            if (_categoryRepository.Find(c => c.Name == category.Name).Result.Any())
            {
                NotifyWithMessage($"The category {category.Name} already exists!");
                return;
            }

            // 2. Run persistence
            await _categoryRepository.Add(category);
        }

        /// <summary>
        /// Atualiza os dados de uma categoria existente.
        /// Valida os dados e verifica duplicações de nome.
        /// </summary>
        /// <param name="category">Categoria com os dados atualizados</param>
        public async Task Update(Category category)
        {
            // 1. Run Validation of data entry and data in the database
            if (!RunValidation(new CategoryValidation(), category)) return;

            if (_categoryRepository.Find(c => c.Name == category.Name && c.Id != category.Id).Result.Any())
            {
                NotifyWithMessage($"The category {category.Name} already exists!");
                return;
            }

            // 2. Run persistence
            await _categoryRepository.Update(category);
        }

        /// <summary>
        /// Remove uma categoria do sistema.
        /// Verifica se existem produtos associados antes de permitir a remoção.
        /// </summary>
        /// <param name="id">Identificador único da categoria a remover</param>
        public async Task Remove(Guid id)
        {
            var category = await _categoryRepository.GetCategoryAndProductByCategoryId(id);

            if (category.Products.Any())
            {
                NotifyWithMessage($"The category {category.Name} cannot be deleted as there are related products!");
                return;
            }

            await _categoryRepository.Remove(id);
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo serviço.
        /// </summary>
        public void Dispose()
        {
            _categoryRepository.Dispose();
        }
    }
}
