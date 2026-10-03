using FluentAssertions;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Services.Entities;
using Moq;

namespace Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para o serviço de gestão de contratos.
    /// Testa as operações de criação, atualização e remoção de contratos.
    /// </summary>
    public class ContractServiceTests
    {
        /// <summary>
        /// Mock do repositório de contratos para simular operações na base de dados.
        /// </summary>
        private readonly Mock<IContractRepository> _mockContractRepository;

        /// <summary>
        /// Mock do serviço de termos e condições.
        /// </summary>
        private readonly Mock<ITermsService> _mockTermsService;

        /// <summary>
        /// Mock do serviço de notificações para verificar erros e validações.
        /// </summary>
        private readonly Mock<INotifierService> _mockNotifier;

        /// <summary>
        /// Instância do serviço de contratos a ser testado.
        /// </summary>
        private readonly ContractService _service;

        /// <summary>
        /// Construtor que inicializa os mocks e a instância do serviço para cada teste.
        /// </summary>
        public ContractServiceTests()
        {
            _mockContractRepository = new Mock<IContractRepository>();
            _mockTermsService = new Mock<ITermsService>();
            _mockNotifier = new Mock<INotifierService>();
            _service = new ContractService(
                _mockContractRepository.Object,
                _mockTermsService.Object,
                _mockNotifier.Object
            );
        }

        #region Add Tests
        /// <summary>
        /// Testa a criação de um contrato com OrderRentId válido e termos disponíveis.
        /// Verifica se o contrato é criado com sucesso e retorna um ID válido.
        /// </summary>
        [Fact]
        public async Task Add_ShouldCreateContract_WhenValidOrderRentIdAndTermsExist()
        {
            // Arrange
            var orderRentId = Guid.NewGuid();
            var termsAndConditions = "Terms and conditions text";
            var expectedContractId = Guid.NewGuid();

            _mockContractRepository
                .Setup(r => r.GetOrderRentIDbyContract(orderRentId))
                .ReturnsAsync((Contract)null);

            _mockTermsService
                .Setup(s => s.GetTermsAndConditions())
                .ReturnsAsync(termsAndConditions);

            _mockContractRepository
                .Setup(r => r.Add(It.IsAny<Contract>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.Add(orderRentId);

            // Assert
            result.Should().NotBe(Guid.Empty);
            _mockContractRepository.Verify(r => r.Add(It.Is<Contract>(c =>
                c.OrderRentId == orderRentId &&
                c.TermsConditions == termsAndConditions
            )), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de criar um contrato quando já existe um para o OrderRentId.
        /// Verifica se retorna um Guid vazio e não cria um contrato duplicado.
        /// </summary>
        [Fact]
        public async Task Add_ShouldReturnEmptyGuid_WhenContractAlreadyExists()
        {
            // Arrange
            var orderRentId = Guid.NewGuid();
            var existingContract = new Contract
            {
                Id = Guid.NewGuid(),
                OrderRentId = orderRentId,
                TermsConditions = "Existing terms"
            };

            _mockContractRepository
                .Setup(r => r.GetOrderRentIDbyContract(orderRentId))
                .ReturnsAsync(existingContract);

            // Act
            var result = await _service.Add(orderRentId);

            // Assert
            result.Should().Be(Guid.Empty);
            _mockContractRepository.Verify(r => r.Add(It.IsAny<Contract>()), Times.Never);
        }
        #endregion

        #region Update Tests
        /// <summary>
        /// Testa a atualização dos termos e condições de um contrato existente.
        /// Verifica se os termos são atualizados corretamente no contrato.
        /// </summary>
        [Fact]
        public async Task Update_ShouldUpdateContract_WhenValidContractAndTermsProvided()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            var newTerms = "Updated terms and conditions";
            var existingContract = new Contract
            {
                Id = contractId,
                OrderRentId = Guid.NewGuid(),
                TermsConditions = "Old terms"
            };

            _mockContractRepository
                .Setup(r => r.GetById(contractId))
                .ReturnsAsync(existingContract);

            _mockContractRepository
                .Setup(r => r.Update(It.IsAny<Contract>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Update(contractId, newTerms);

            // Assert
            existingContract.TermsConditions.Should().Be(newTerms);
            _mockContractRepository.Verify(r => r.Update(existingContract), Times.Once);
        }
        #endregion

        #region Delete Tests
        /// <summary>
        /// Testa a remoção de um contrato com ID válido.
        /// Verifica se o contrato é removido com sucesso.
        /// </summary>
        [Fact]
        public async Task Remove_ShouldRemoveContract_WhenValidContractId()
        {
            // Arrange
            var contractId = Guid.NewGuid();

            _mockContractRepository
                .Setup(r => r.Remove(contractId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Remove(contractId);

            // Assert
            _mockContractRepository.Verify(r => r.Remove(contractId), Times.Once);
        }
        #endregion
    }
}