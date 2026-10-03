using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Validations.Entities;
using LojaMaquinas.Helper.Security;

namespace LojaMaquinas.Business.Services.Entities
{
    /// <summary>
    /// Implementação do serviço de gestão de utilizadores.
    /// Contém lógica de negócio para autenticação, validação e encriptação de passwords.
    /// </summary>
    public class UserService : BaseService, IUserService
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de utilizadores.
        /// </summary>
        /// <param name="userRepository">Repositório para acesso aos dados de utilizadores</param>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        public UserService(IUserRepository userRepository, INotifierService notifier) : base(notifier)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Adiciona um novo utilizador ao sistema.
        /// Valida os dados, verifica duplicações de email, encripta a password e ativa a conta.
        /// </summary>
        /// <param name="user">Utilizador a ser adicionado</param>
        public async Task Add(User user)
        {
            // 1. Run Validation
            if (!RunValidation(new UserValidation(), user)) return;

            // 2. Check if email already exists
            if ((await _userRepository.GetByEmail(user.Email)) != null)
            {
                NotifyWithMessage($"A user with email {user.Email} already exists!");
                return;
            }

            // 3. Set user as active by default
            user.IsActive = true;

            // 4. Hash password before saving
            user.Password = PasswordHasher.HashPassword(user.Password);

            // 5. Run persistence
            await _userRepository.Add(user);
        }

        /// <summary>
        /// Atualiza os dados de um utilizador existente.
        /// Valida os dados, verifica email único e encripta a password se foi alterada.
        /// </summary>
        /// <param name="user">Utilizador com os dados atualizados</param>
        public async Task Update(User user)
        {
            // 1. Run Validation
            if (!RunValidation(new UserValidation(), user)) return;

            // 2. Check if another user with same email exists
            var existingUser = await _userRepository.GetByEmail(user.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                NotifyWithMessage($"Another user with email {user.Email} already exists!");
                return;
            }

            // 3. Get current user from database to check if password changed
            var currentUser = await _userRepository.GetById(user.Id);
            if (currentUser != null && currentUser.Password != user.Password)
            {
                // Password was changed, hash the new password
                user.Password = PasswordHasher.HashPassword(user.Password);
            }

            // 4. Run persistence
            await _userRepository.Update(user);
        }

        /// <summary>
        /// Remove um utilizador do sistema.
        /// Verifica se o utilizador tem um cliente associado antes de permitir a remoção.
        /// </summary>
        /// <param name="id">Identificador único do utilizador a remover</param>
        public async Task Remove(Guid id)
        {
            var user = await _userRepository.GetUserWithClient(id);

            if (user == null)
            {
                NotifyWithMessage("User not found!");
                return;
            }

            if (user.Client != null)
            {
                NotifyWithMessage($"The user {user.Email} cannot be deleted as it has an associated client!");
                return;
            }

            await _userRepository.Remove(id);
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo serviço.
        /// </summary>
        public void Dispose()
        {
            _userRepository.Dispose();
        }
    }
}
