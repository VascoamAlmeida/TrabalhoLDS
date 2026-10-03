using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Services.Entities;
using LojaMaquinas.Helper.Security;
using Moq;
using LojaMaquinas.Business.Models; // Need this for the Notification object

namespace LojaMaquinas.Business.Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para o serviço de gestão de utilizadores.
    /// Testa as operações CRUD, hashing de passwords e validações de regras de negócio do UserService.
    /// </summary>
    public class UserServiceTests
    {
        /// <summary>
        /// Mock do repositório de utilizadores para simular operações na base de dados.
        /// </summary>
        private readonly Mock<IUserRepository> _userRepositoryMock;

        /// <summary>
        /// Mock do serviço de notificações para verificar erros e validações.
        /// </summary>
        private readonly Mock<INotifierService> _notifierMock;

        /// <summary>
        /// Instância do serviço de utilizadores a ser testado.
        /// </summary>
        private readonly UserService _userService;

        /// <summary>
        /// Construtor que inicializa os mocks e a instância do serviço para cada teste.
        /// </summary>
        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _notifierMock = new Mock<INotifierService>();
            _userService = new UserService(_userRepositoryMock.Object, _notifierMock.Object);
        }

        /// <summary>
        /// Testa a adição de um utilizador válido.
        /// Verifica se a password é encriptada e o utilizador é definido como ativo.
        /// </summary>
        [Fact]
        public async Task Add_ValidUser_ShouldAddWithHashedPassword()
        {
            // Arrange
            var plainPassword = "MySecurePassword123!";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "john.doe@example.com",
                Password = plainPassword,
                UserType = "CLIENT"
            };

            _userRepositoryMock.Setup(x => x.GetByEmail(user.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.Add(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.Add(user);

            // Assert
            _userRepositoryMock.Verify(x => x.Add(It.Is<User>(u =>
                u.Email == user.Email &&
                u.Password != plainPassword && // Password should be hashed
                u.IsActive == true &&
                PasswordHasher.VerifyPassword(plainPassword, u.Password) // Verify hash is correct
            )), Times.Once);

            // VERIFIED: No notification should be sent on success
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a adição de um utilizador com email inválido.
        /// Verifica se o serviço notifica os erros de validação e não adiciona o utilizador.
        /// </summary>
        [Fact]
        public async Task Add_InvalidUser_ShouldNotifyValidationErrors()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "invalid-email",
                Password = "MySecurePassword123!",
                UserType = "CLIENT"
            };

            // Act
            await _userService.Add(user);

            // Assert
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);

            // VERIFIED: Notification sent due to validation failure
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a tentativa de adicionar um utilizador com email duplicado.
        /// Verifica se o serviço notifica o erro e não adiciona o utilizador.
        /// </summary>
        [Fact]
        public async Task Add_DuplicateEmail_ShouldNotify()
        {
            // Arrange
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "john.doe@example.com",
                Password = PasswordHasher.HashPassword("ExistingPassword123!"),
                UserType = "CLIENT",
                IsActive = true
            };

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "john.doe@example.com", // Same email
                Password = "NewPassword123!",
                UserType = "ADMIN"
            };

            _userRepositoryMock.Setup(x => x.GetByEmail(newUser.Email))
                .ReturnsAsync(existingUser);

            // Act
            await _userService.Add(newUser);

            // Assert
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
            // VERIFIED: Notification sent due to duplicate email. We verify Handle was called once.
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);

            // Note: Since Notification is a model, verifying the message text requires complex setup 
            // inside the verification, which often adds fragility. Verifying the call count (Times.Once) 
            // is usually sufficient for service-level tests.
        }

        /// <summary>
        /// Testa se o campo IsActive é definido como true ao adicionar um utilizador.
        /// Verifica que o utilizador é sempre ativado automaticamente.
        /// </summary>
        [Fact]
        public async Task Add_User_ShouldSetIsActiveToTrue()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "new.user@example.com",
                Password = "SecurePassword123!",
                UserType = "CLIENT",
                IsActive = false // Explicitly set to false
            };

            _userRepositoryMock.Setup(x => x.GetByEmail(user.Email))
                .ReturnsAsync((User)null);

            _userRepositoryMock.Setup(x => x.Add(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.Add(user);

            // Assert
            _userRepositoryMock.Verify(x => x.Add(It.Is<User>(u =>
                u.IsActive == true // Should be set to true
            )), Times.Once);
        }

        /// <summary>
        /// Testa a atualização de um utilizador válido sem alteração de password.
        /// Verifica se o utilizador é atualizado mantendo a password existente.
        /// </summary>
        [Fact]
        public async Task Update_ValidUser_ShouldUpdateSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hashedPassword = PasswordHasher.HashPassword("ExistingPassword123!");

            var currentUser = new User
            {
                Id = userId,
                Email = "john.doe@example.com",
                Password = hashedPassword,
                UserType = "CLIENT",
                IsActive = true
            };

            var updatedUser = new User
            {
                Id = userId,
                Email = "john.doe@example.com",
                Password = hashedPassword, // Same password (not changed)
                UserType = "ADMIN", // Role changed
                IsActive = true
            };

            // FIX: Mock GetByEmail to return the current user (same ID), allowing the duplicate check to pass.
            _userRepositoryMock.Setup(x => x.GetByEmail(updatedUser.Email))
                .ReturnsAsync(currentUser);

            // REQUIRED: Mock GetById for password check
            _userRepositoryMock.Setup(x => x.GetById(userId))
                .ReturnsAsync(currentUser);

            _userRepositoryMock.Setup(x => x.Update(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.Update(updatedUser);

            // Assert
            _userRepositoryMock.Verify(x => x.Update(It.Is<User>(u =>
                u.Id == userId &&
                u.UserType == "ADMIN" &&
                u.Password == hashedPassword // Password should remain the same (not re-hashed)
            )), Times.Once);

            // VERIFIED: No notification should be sent on success
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a atualização de um utilizador com alteração de password.
        /// Verifica se a nova password é encriptada corretamente.
        /// </summary>
        [Fact]
        public async Task Update_PasswordChanged_ShouldHashNewPassword()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldHashedPassword = PasswordHasher.HashPassword("OldPassword123!");
            var newPlainPassword = "NewPassword456!";

            var currentUser = new User
            {
                Id = userId,
                Email = "john.doe@example.com",
                Password = oldHashedPassword,
                UserType = "CLIENT",
                IsActive = true
            };

            var updatedUser = new User
            {
                Id = userId,
                Email = "john.doe@example.com",
                Password = newPlainPassword, // New password
                UserType = "CLIENT",
                IsActive = true
            };

            // FIX: Mock GetByEmail to return the current user (same ID), allowing the duplicate check to pass.
            _userRepositoryMock.Setup(x => x.GetByEmail(updatedUser.Email))
                .ReturnsAsync(currentUser);

            // REQUIRED: Mock GetById for password check
            _userRepositoryMock.Setup(x => x.GetById(userId))
                .ReturnsAsync(currentUser);

            _userRepositoryMock.Setup(x => x.Update(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.Update(updatedUser);

            // Assert
            _userRepositoryMock.Verify(x => x.Update(It.Is<User>(u =>
                u.Id == userId &&
                u.Password != newPlainPassword && // Password should be hashed
                u.Password != oldHashedPassword && // Password should be different from old hash
                PasswordHasher.VerifyPassword(newPlainPassword, u.Password) // Verify new hash is correct
            )), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de atualização com email duplicado.
        /// Verifica se o serviço notifica o erro e não atualiza o utilizador.
        /// </summary>
        [Fact]
        public async Task Update_DuplicateEmail_ShouldNotify()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUserId = Guid.NewGuid();

            var existingUser = new User
            {
                Id = existingUserId,
                Email = "existing.user@example.com",
                Password = PasswordHasher.HashPassword("Password123!"),
                UserType = "CLIENT",
                IsActive = true
            };

            var userToUpdate = new User
            {
                Id = userId,
                Email = "existing.user@example.com", // Trying to use existing email
                Password = "MyPassword123!",
                UserType = "ADMIN",
                IsActive = true
            };

            // REQUIRED: Mock GetById for password check logic. 
            _userRepositoryMock.Setup(x => x.GetById(userToUpdate.Id))
                .ReturnsAsync(new User { Id = userId, Email = "original@email.com" }); // A necessary stub user

            // Setup for the failing duplicate email check
            _userRepositoryMock.Setup(x => x.GetByEmail(userToUpdate.Email))
                .ReturnsAsync(existingUser);

            // Act
            await _userService.Update(userToUpdate);

            // Assert
            _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
            // VERIFIED: Notification sent due to duplicate email
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de remoção de um utilizador inexistente.
        /// Verifica se o serviço notifica o erro e não remove.
        /// </summary>
        [Fact]
        public async Task Remove_UserNotFound_ShouldNotify()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(x => x.GetUserWithClient(userId))
                .ReturnsAsync((User)null);

            // Act
            await _userService.Remove(userId);

            // Assert
            _userRepositoryMock.Verify(x => x.Remove(It.IsAny<Guid>()), Times.Never);
            // VERIFIED: Notification sent because user was not found
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de remoção de um utilizador com cliente associado.
        /// Verifica se o serviço notifica o erro de integridade referencial e não remove.
        /// </summary>
        [Fact]
        public async Task Remove_UserWithClient_ShouldNotify()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "john.doe@example.com",
                Password = PasswordHasher.HashPassword("Password123!"),
                UserType = "CLIENT",
                IsActive = true,
                Client = new Client
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FirstName = "John",
                    LastName = "Doe",
                    NIF = "123456789"
                }
            };

            _userRepositoryMock.Setup(x => x.GetUserWithClient(userId))
                .ReturnsAsync(user);

            // Act
            await _userService.Remove(userId);

            // Assert
            _userRepositoryMock.Verify(x => x.Remove(It.IsAny<Guid>()), Times.Never);
            // VERIFIED: Notification sent because user has an associated client
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a remoção de um utilizador sem cliente associado.
        /// Verifica se o utilizador é removido com sucesso.
        /// </summary>
        [Fact]
        public async Task Remove_UserWithoutClient_ShouldRemoveSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "john.doe@example.com",
                Password = PasswordHasher.HashPassword("Password123!"),
                UserType = "CLIENT",
                IsActive = true,
                Client = null // No associated client
            };

            _userRepositoryMock.Setup(x => x.GetUserWithClient(userId))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(x => x.Remove(userId))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.Remove(userId);

            // Assert
            _userRepositoryMock.Verify(x => x.Remove(userId), Times.Once);
            // VERIFIED: No notification should be sent on success
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }
    }
}