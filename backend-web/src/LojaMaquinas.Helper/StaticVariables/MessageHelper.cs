namespace LojaMaquinas.Helper.StaticVariables
{
    /// <summary>
    /// Classe auxiliar para geração de mensagens estáticas da aplicação.
    /// Suporta múltiplas línguas (Português e Inglês).
    /// </summary>
    public class MessageHelper
    {
        /// <summary>
        /// Enumeração dos idiomas suportados para respostas do sistema.
        /// </summary>
        public enum LanguageResponse
        {
            /// <summary>
            /// Português (idioma padrão).
            /// </summary>
            Portuguese = 1,

            /// <summary>
            /// Inglês.
            /// </summary>
            English
        }

        /// <summary>
        /// Classe estática com mensagens genéricas reutilizáveis em toda a aplicação.
        /// Todas as mensagens suportam Português (padrão) e Inglês.
        /// </summary>
        public static class Generics
        {
            /// <summary>
            /// Mensagem de erro indicando que um campo/valor já existe na base de dados.
            /// </summary>
            /// <param name="field">Nome do campo (ex: "cliente", "produto")</param>
            /// <param name="content">Valor do campo que já existe</param>
            /// <param name="lang">Idioma da mensagem (Português por defeito)</param>
            /// <returns>Mensagem formatada no idioma selecionado</returns>
            public static string HasField(string field, string content, LanguageResponse lang = LanguageResponse.Portuguese)
                => lang switch
                {
                    LanguageResponse.English => $"This {field} '{content}' already exists!",
                    _ => $"Este {field} '{content}' já existe!"
                };

            /// <summary>
            /// Mensagem de sucesso para criação de novo registo.
            /// </summary>
            /// <param name="module">Nome do módulo/entidade (ex: "cliente", "produto")</param>
            /// <param name="content">Identificador ou nome do registo criado</param>
            /// <param name="lang">Idioma da mensagem (Português por defeito)</param>
            /// <returns>Mensagem formatada no idioma selecionado</returns>
            public static string SuccessCreationRegistration(string module, string content, LanguageResponse lang = LanguageResponse.Portuguese)
                => lang switch
                {
                    LanguageResponse.English => $"The {module} {content} was created successfully!",
                    _ => $"O {module} {content} foi criado com sucesso!"
                };

            /// <summary>
            /// Mensagem de sucesso para alteração de registo existente.
            /// </summary>
            /// <param name="module">Nome do módulo/entidade (ex: "cliente", "produto")</param>
            /// <param name="content">Identificador ou nome do registo alterado</param>
            /// <param name="lang">Idioma da mensagem (Português por defeito)</param>
            /// <returns>Mensagem formatada no idioma selecionado</returns>
            public static string SuccessChangeRegistration(string module, string content, LanguageResponse lang = LanguageResponse.Portuguese)
                => lang switch
                {
                    LanguageResponse.English => $"The {module} {content} was changed successfully!",
                    _ => $"O {module} {content} foi alterado com sucesso!"
                };

            /// <summary>
            /// Mensagem de sucesso para remoção de registo.
            /// </summary>
            /// <param name="module">Nome do módulo/entidade (ex: "cliente", "produto")</param>
            /// <param name="content">Identificador ou nome do registo removido</param>
            /// <param name="lang">Idioma da mensagem (Português por defeito)</param>
            /// <returns>Mensagem formatada no idioma selecionado</returns>
            public static string SuccessRemovalRegistration(string module, string content, LanguageResponse lang = LanguageResponse.Portuguese)
                => lang switch
                {
                    LanguageResponse.English => $"The {module} {content} was removed successfully!",
                    _ => $"O {module} {content} foi removido com sucesso!"
                };

            /// <summary>
            /// Mensagem de erro quando um parâmetro/entidade não é encontrado.
            /// </summary>
            /// <param name="field">Nome do campo/entidade não encontrado</param>
            /// <param name="lang">Idioma da mensagem (Português por defeito)</param>
            /// <returns>Mensagem formatada no idioma selecionado</returns>
            public static string ErrorParameterNotFound(string field, LanguageResponse lang = LanguageResponse.Portuguese)
                => lang switch
                {
                    LanguageResponse.English => $"The {field}  was not found!",
                    _ => $"O {field}  não foi encontrado!"
                };

            /// <summary>
            /// Mensagem de erro genérica quando critérios de validação não são atendidos.
            /// </summary>
            /// <param name="lang">Idioma da mensagem (Português por defeito)</param>
            /// <returns>Mensagem formatada no idioma selecionado</returns>
            public static string ErrorInvalidValidationCriteria(LanguageResponse lang = LanguageResponse.Portuguese)
                => lang switch
                {
                    LanguageResponse.English => "Validation criteria not met! Please check!",
                    _ => "Os critérios de validação não foram atendidos! Favor verificar!"
                };
        }

        /// <summary>
        /// Classe estática com mensagens específicas para o módulo de Manutenção.
        /// </summary>
        public static class Maintenance
        {
            /// <summary>
            /// Mensagem de sucesso quando uma manutenção é criada.
            /// </summary>
            public static string SuccessfullyCreated => "Maintenance created successfully!";

            /// <summary>
            /// Mensagem de sucesso quando uma manutenção é atualizada.
            /// </summary>
            public static string SuccessfullyUpdated => "Maintenance updated successfully!";

            /// <summary>
            /// Mensagem de sucesso quando uma manutenção é removida.
            /// </summary>
            public static string SuccessfullyDeleted => "Maintenance deleted successfully!";
        }
    }
}
