using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.WebApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    /// <summary>
    /// Controller responsável pela autenticação e gestão de perfil de utilizadores.
    /// Inclui Login, Registo, Logout e Perfil.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IClientService _clientService;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public AccountController(
            IUserService userService,
            IUserRepository userRepository,
            IClientService clientService,
            IClientRepository clientRepository,
            IMapper mapper,
            INotifierService notifier) : base(notifier)
        {
            _userService = userService;
            _userRepository = userRepository;
            _clientService = clientService;
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        #region Login

        /// <summary>
        /// GET: Exibe a página de login.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            // Se já estiver autenticado, redireciona para o perfil
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(Profile));

            return View();
        }

        /// <summary>
        /// POST: Processa o login do utilizador.
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return View();
            }

            var user = await _userRepository.GetByEmail(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "User is inactive.");
                return View();
            }

            // Verifica password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!isPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View();
            }

            // Cria claims para autenticação
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redireciona para returnUrl ou perfil
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Profile));
        }

        #endregion

        #region Register

        /// <summary>
        /// GET: Exibe a página de registo.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            // Se já estiver autenticado, redireciona para o perfil
            if (User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(Profile));

            return View();
        }

        /// <summary>
        /// POST: Processa o registo de um novo utilizador.
        /// Cria apenas o User (email e password). O Client é criado depois no perfil.
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserViewModel viewModel)
        {
            // Remove UserType do ModelState pois é definido automaticamente
            ModelState.Remove(nameof(viewModel.UserType));

            if (!ModelState.IsValid)
                return View(viewModel);

            // Verifica se email já existe
            var existingUser = await _userRepository.GetByEmail(viewModel.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(viewModel.Email), "Email already registered.");
                return View(viewModel);
            }

            var user = new User
            {
                Email = viewModel.Email,
                Password = viewModel.Password,
                UserType = "CLIENT",
                IsActive = true
            };

			await _userService.Add(user);

            // Verifica se houve erros
            if (!PersistenceValidation())
            {
                // Se houve erro, mostra as notificações
                var errors = PersistenceResponse();
                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                return View(viewModel);
            }

            // Faz login automático após registo
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["SuccessMessage"] = "Registo efetuado com sucesso! Por favor completa o teu perfil.";
            return RedirectToAction(nameof(Profile));
        }

        #endregion

        #region Profile

        /// <summary>
        /// GET: Exibe a página de perfil do utilizador autenticado.
        /// Mostra dados do User e permite criar/editar dados do Client.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userRepository.GetUserWithClient(userId);

            if (user == null)
                return NotFound();

            var viewModel = _mapper.Map<UserViewModel>(user);

            // Se não tem Client, cria um ViewModel vazio para o formulário
            if (viewModel.Client == null)
            {
                viewModel.Client = new ClientViewModel
                {
                    UserId = user.Id
                };
            }

            return View(viewModel);
        }

        /// <summary>
        /// POST: Cria ou atualiza os dados do Client do utilizador autenticado.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserViewModel viewModel)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            viewModel.Client.UserId = userId;

            // Valida apenas o Client (ignora User)
            ModelState.Remove(nameof(UserViewModel.Email));
            ModelState.Remove(nameof(UserViewModel.Password));
            ModelState.Remove(nameof(UserViewModel.UserType));
            ModelState.Remove(nameof(UserViewModel.Client));

            if (!ModelState.IsValid)
            {
                // Recarrega user para mostrar na view
                var user = await _userRepository.GetUserWithClient(userId);
                var loadedViewModel = _mapper.Map<UserViewModel>(user);
                loadedViewModel.Client = viewModel.Client;
                return View(loadedViewModel);
            }

            var client = _mapper.Map<Client>(viewModel.Client);

            // Verifica se já existe Client para este User
            var existingClient = await _clientRepository.GetByUserId(userId);

            if (existingClient == null)
            {
                // Cria novo Client
                await _clientService.Add(client);
            }
            else
            {
                // Atualiza Client existente
                client.Id = existingClient.Id;
                await _clientService.Update(client);
            }

            // Verifica se houve erros
            if (!PersistenceValidation())
            {
                var user = await _userRepository.GetUserWithClient(userId);
                var loadedViewModel = _mapper.Map<UserViewModel>(user);
                loadedViewModel.Client = viewModel.Client;

                var errors = PersistenceResponse();
                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                return View(loadedViewModel);
            }

            TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
            return RedirectToAction(nameof(Profile));
        }

        #endregion

        #region Logout
        /// <summary>
        /// POST: Faz logout do utilizador.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Route("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
