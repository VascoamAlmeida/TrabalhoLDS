using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt; //Handler para criar e escrever tokens
using System.Security.Claims;
using System.Text;  // Encoding.UTF8.GetBytes

namespace LojaMaquinas.WebApp
{
    public class TokenGenerator
    {
        // IConfiguration permite ler appsettings / variáveis de ambiente / user-secrets
        private readonly IConfiguration configuration;

        // Construtor: guarda a IConfiguration recebida via injeção de dependências
        public TokenGenerator(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string GenerateToken(Guid userId, string email, string role)
        {
            // Lê a secção JwtSettings do appsettings.json
            var jwt = configuration.GetSection("JwtSettings");
            var issuer = jwt["Issuer"];
            var audience = jwt["Audience"];
            var keyPlainText = jwt["Key"];
            var expireMinutes = int.TryParse(jwt["ExpireMinutes"], out var m) ? m : 60;

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience) ||
                string.IsNullOrWhiteSpace(keyPlainText))
            {
                throw new InvalidOperationException("JWT configuration is incomplete.");
            }

            // Cria o handler e escreve token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Converter chave segurança para bytes
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyPlainText!)); 

            // O que quero criar no token
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, email),
                new(JwtRegisteredClaimNames.Email, email),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new("userId", userId.ToString()),
                new(ClaimTypes.Role, role)
            };

            // Descrição do token (quem emite, para quem, quando expira, assinatura)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials =
                new SigningCredentials(key,
                SecurityAlgorithms.HmacSha256Signature)
            };
            
            // Cria token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Devolve o token
            return tokenHandler.WriteToken(token);
        }
    }
}