using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.API
{
    /// <summary>
    /// API Controller para gestão de utilizadores.
    /// Fornece endpoints CRUD completos + consulta por email + desativação (soft delete).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    public class UserApiController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        /// <summary>
        /// Inicializa o controller de utilizadores com dependências necessárias.
        /// </summary>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        /// <param name="userRepository">Repositório de acesso a dados de utilizadores</param>
        /// <param name="userService">Serviço de lógica de negócio de utilizadores</param>
        public UserApiController(
            IMapper mapper,
            INotifierService notifier,
            IUserRepository userRepository,
            IUserService userService
        ) : base(notifier)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _userService = userService;
        }

        /// <summary>
        /// Obtém todos os utilizadores.
        /// GET: api/userapi
        /// </summary>
        /// <returns>Lista de todos os utilizadores</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAll();
            var viewModels = _mapper.Map<IEnumerable<UserApiViewModel>>(users);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém um utilizador específico por ID.
        /// GET: api/userapi/{id}
        /// </summary>
        /// <param name="id">ID único do utilizador</param>
        /// <returns>Utilizador encontrado ou NotFound</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            var viewModel = _mapper.Map<UserApiViewModel>(user);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obtém um utilizador por email.
        /// GET: api/userapi/email/{email}
        /// </summary>
        /// <param name="email">Email do utilizador</param>
        /// <returns>Utilizador encontrado ou NotFound</returns>
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userRepository.GetByEmail(email);
            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            var viewModel = _mapper.Map<UserApiViewModel>(user);
            return Ok(viewModel);
        }

        /// <summary>
        /// Cria um novo utilizador.
        /// POST: api/userapi
        /// </summary>
        /// <param name="userViewModel">Dados do utilizador a criar</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserApiViewModel userViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = _mapper.Map<User>(userViewModel);
                user.UserType = "CLIENT";
                user.IsActive = true;
                await _userService.Add(user);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessCreationRegistration("User", userViewModel.Email)
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Atualiza um utilizador existente.
        /// PUT: api/userapi/{id}
        /// </summary>
        /// <param name="id">ID do utilizador a atualizar</param>
        /// <param name="userViewModel">Dados atualizados do utilizador</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserApiViewModel userViewModel)
        {
            if (id != userViewModel.Id)
                return BadRequest(new { success = false, message = "User Id invalid." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userRepository.GetById(id);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found." });

                user.Email = userViewModel.Email;
                user.UserType = userViewModel.UserType;
                user.IsActive = userViewModel.IsActive;

                await _userService.Update(user);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("User", userViewModel.Email)
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Desativa um utilizador (soft delete - marca IsActive = false).
        /// PATCH: api/userapi/{id}/deactivate
        /// </summary>
        /// <param name="id">ID do utilizador a desativar</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                var user = await _userRepository.GetById(id);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found." });

                user.IsActive = false;
                await _userService.Update(user);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = $"User {user.Email} deactivated successfully!"
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Remove um utilizador permanentemente.
        /// DELETE: api/userapi/{id}
        /// </summary>
        /// <param name="id">ID do utilizador a remover</param>
        /// <returns>Mensagem de sucesso ou erro se utilizador não encontrado</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var user = await _userRepository.GetById(id);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found." });

                await _userService.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("User", user.Email)
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
    }
}
