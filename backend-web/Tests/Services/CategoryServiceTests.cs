using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.Business.Services.Parameters;
using Moq;
using System.Linq.Expressions;

namespace Tests.Services
{
    /// <summary>
    /// Testes unitários para a Category
    /// </summary>
    public class CategoryServiceTests
    {

        /// <summary>
        /// Instância para ser usada em testes
        /// </summary>
        private readonly CategoryService _sut;

        /// <summary>
        /// Para criar os mocks para ser possivel testar
        /// </summary>
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new Mock<ICategoryRepository>();
        private readonly Mock<INotifierService> _notifierMock = new Mock<INotifierService>();

        /// <summary>
        /// Construtor para criar os mocks
        /// </summary>
        public CategoryServiceTests()
        {
            _sut = new CategoryService(_categoryRepositoryMock.Object, _notifierMock.Object);
        }

        /// <summary>
        /// Exemplo de uma Category valida, se for necessario utilizar
        /// </summary>
        /// <returns>Exemplo de category valida</returns>
        private static Category NewValidCategory() => new Category
        {
            Id = Guid.NewGuid(),
            Name = "Teste",
            Description = "Teste2"
        };

        /// <summary>
        /// Adicionar uma Category válida nome diferente
        /// </summary>
        /// <returns>Devolve OK, adiciona category válida</returns>
        [Fact]
        public async Task AddCategoryServiceTest_ReturnOK_DifferenteName()

        {
            // Arrange
            // Prepara para realizar o teste
            var validCategory = NewValidCategory();

            // Não existem categorias com o mesmo nome
            _categoryRepositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(Enumerable.Empty<Category>());

            // Chamar o add no repositoria e returnar tarefa completa
            _categoryRepositoryMock
                .Setup(r => r.Add(validCategory))
                .Returns(Task.CompletedTask);

            // Act
            // O que o teste vai realizar para ser testado, tem a resposta atual
            await _sut.Add(validCategory);

            // Assert
            // Verifica o que é expectavel
            // Repositorio é chamado um unica vez (Times.Once)
            _categoryRepositoryMock.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);

            // Suposto nao emitir notificação de erro
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.Never);
        }


        /// <summary>
        /// Não adicionar uma Category válida nome igual
        /// </summary>
        /// <returns>Devolve NotOK, não adiciona category válida, nome já existe</returns>
        [Fact]
        public async Task AddCategoryServiceTest_ReturnNotOK_SameName()

        {
            // Arrange
            // Prepara para realizar o teste
            var validCategory = NewValidCategory();

            // Já existe categoria com o mesmo nome
            _categoryRepositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category> { validCategory });

            // Chamar o add no repositoria e returnar tarefa completa
            _categoryRepositoryMock
                .Setup(r => r.Add(validCategory))
                .Returns(Task.CompletedTask);

            // Act
            // O que o teste vai realizar para ser testado, tem a resposta atual
            await _sut.Add(validCategory);

            // Assert
            // Verifica o que é expectavel
            // Repositorio é chamado um unica vez (Times.Once)
            _categoryRepositoryMock.Verify(r => r.Add(It.IsAny<Category>()), Times.Never);

            // Suposto nao emitir notificação de erro
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }


        /// <summary>
        /// Rrealiza o update a category
        /// </summary>
        /// <returns>Devolve OK, category não tem esse nome</returns>
        [Fact]
        public async Task UpdateCategoryServiceTest_ReturnOK_DifferenteName()
        {
            // Arrange
            var validCategory = NewValidCategory();

            _categoryRepositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(Enumerable.Empty<Category>());

            _categoryRepositoryMock
                .Setup(r => r.Update(validCategory))
                .Returns(Task.CompletedTask);


            // Act
            await _sut.Update(validCategory);

            // Assert
            // Repositorio é suposto não ir ao repositorio (Times.Nver), pois tem erro
            _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);

            // Suposto emitir notificação de erro, pois StockId esta errado
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Não realiza o update a category
        /// </summary>
        /// <returns>Devolve NotOK, category name já existe</returns>
        [Fact]
        public async Task UpdateCategoryServiceTest_ReturnNotOK_SameName()
        {
            // Arrange
            var validCategory = NewValidCategory();

            _categoryRepositoryMock
    .Setup(r => r.Find(It.IsAny<Expression<Func<Category, bool>>>()))
    .ReturnsAsync(new List<Category> { validCategory });

            // Chamar o notifier pois tem de apresentar uma
            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));


            // Act
            await _sut.Update(validCategory);

            // Assert
            // Repositorio é suposto não ir ao repositorio (Times.Nver), pois tem erro
            _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Never);

            // Suposto emitir notificação de erro, pois StockId esta errado
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Permite remover category qeu nao tem produtos associados
        /// </summary>
        /// <returns>Devolve OK, permite remover category</returns>
        [Fact]
        public async Task RemoveCategoryServiceTest_ReturnOK_WhenCategoryHasNoProducts()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = NewValidCategory();
            category.Id = categoryId;

            // Nenhum produto associado a category
            category.Products = new List<Product>();


            _categoryRepositoryMock
                .Setup(r => r.GetCategoryAndProductByCategoryId(categoryId))
                .ReturnsAsync(category);

            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));

            // Act
            await _sut.Remove(categoryId);

            // Assert
            // Repositorio é suposto não ir ao repositorio (Times.Nver), pois tem erro
            _categoryRepositoryMock.Verify(r => r.Remove(It.IsAny<Guid>()), Times.Once);

            // Suposto emitir notificação de erro, pois ClientId esta errado
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Não permite remover a categoria quando existe produtos
        /// </summary>
        /// <returns>Devolve NotOK, não permite remover category</returns>
        [Fact]
        public async Task RemoveCategoryServiceTest_ReturnNotOK_WhenCategoryHasProducts()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = NewValidCategory();
            category.Id = categoryId;

            // Associar produtos a categoria
            category.Products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Produto associado" }
            };


            _categoryRepositoryMock
                .Setup(r => r.GetCategoryAndProductByCategoryId(categoryId))
                .ReturnsAsync(category);

            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));

            // Act
            await _sut.Remove(categoryId);

            // Assert
            // Repositorio é suposto não ir ao repositorio (Times.Nver), pois tem erro
            _categoryRepositoryMock.Verify(r => r.Remove(It.IsAny<Guid>()), Times.Never);

            // Suposto emitir notificação de erro, pois ClientId esta errado
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }
    }
}
