// Controllers/AuthController.cs
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.WebApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller responsável pela autenticação de utilizadores.
/// Gera tokens JWT válidos para acesso à API.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
    {
    private readonly IUserRepository userRepository;
    private readonly TokenGenerator tokenGenerator;

    /// <summary>
    /// Construtor do controlador de autenticação.
    /// </summary>
    /// <param name="userRepository">Repositório de utilizadores para validação de credenciais.</param>
    /// <param name="tokenGenerator">Serviço responsável por gerar tokens JWT.</param>
    public AuthController(IUserRepository userRepository, TokenGenerator tokenGenerator)
        {
        this.userRepository = userRepository;
        this.tokenGenerator = tokenGenerator;
        }

    /// <summary>
    /// Modelo do pedido de login, contendo email e password.
    /// </summary>
    /// <param name="Email">Email do utilizador.</param>
    /// <param name="Password">Password em texto simples (não encriptada).</param>
    public record LoginRequest(string Email, string Password);

    /// <summary>
    /// Endpoint de autenticação.
    /// Valida as credenciais do utilizador e, se válidas, devolve um token JWT.
    /// </summary>
    /// <param name="loginRequest">Objeto com email e password do utilizador.</param>
    /// <returns>
    /// Retorna:
    /// <list type="bullet">
    /// <item><description><see cref="OkObjectResult"/> com o token JWT se as credenciais forem válidas.</description></item>
    /// <item><description><see cref="BadRequestResult"/> se os campos obrigatórios estiverem em falta.</description></item>
    /// <item><description><see cref="UnauthorizedResult"/> se o utilizador não existir, estiver inativo ou tiver password incorreta.</description></item>
    /// </list>
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {

        if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            return BadRequest("email e password são obrigatório");


        var user = await userRepository.GetByEmail(loginRequest.Email);
        if (user == null)
            return Unauthorized("Credenciais inválidas.");


        if (!user.IsActive)
            return Unauthorized("Utilizador inativo.");


        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);
        if (!isPasswordValid)
            return Unauthorized("Credenciais inválidas.");


        var token = tokenGenerator.GenerateToken(user.Id, user.Email, user.UserType);

        return Ok(new { Token = token });
        }
    }
