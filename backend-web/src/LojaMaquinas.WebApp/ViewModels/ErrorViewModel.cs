namespace LojaMaquinas.WebApp.Models
{
    /// <summary>
    /// ViewModel de erro para apresentação de erros HTTP.
    /// Contém código de erro, título e mensagem descritiva.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Código HTTP do erro (ex: 404, 500).
        /// </summary>
        public int ErroCode { get; set; }

        /// <summary>
        /// Título do erro a apresentar ao utilizador.
        /// </summary>
        public string Titulo { get; set; }

        /// <summary>
        /// Mensagem descritiva do erro.
        /// </summary>
        public string Mensagem { get; set; }
    }
}
