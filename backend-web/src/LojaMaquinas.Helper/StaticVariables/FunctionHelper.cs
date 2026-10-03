namespace LojaMaquinas.Helper.StaticVariables
{
    /// <summary>
    /// Classe auxiliar contendo funções utilitárias e validações específicas.
    /// Inclui validação de NIF (Número de Identificação Fiscal) português.
    /// </summary>
    public class FunctionHelper
    {
        /// <summary>
        /// Classe para validação de NIF (Número de Identificação Fiscal) português.
        /// Valida formato, tamanho e deteta padrões inválidos (dígitos repetidos).
        /// </summary>
        public class NifValidation
        {
            /// <summary>
            /// Tamanho válido de um NIF português (9 dígitos).
            /// </summary>
            public const int SizeNIF = 9;

            /// <summary>
            /// Valida se um NIF é válido segundo as regras portuguesas.
            /// Verifica: apenas números, tamanho de 9 dígitos, sem dígitos repetidos.
            /// </summary>
            /// <param name="nif">String com o NIF a validar</param>
            /// <returns>True se o NIF é válido, False caso contrário</returns>
            public static bool Validate(string nif)
            {
                if (!Utils.ValidOnlyNumbers(nif)) return false;

                if (!Utils.ValidSize(nif, SizeNIF)) return false;

                if (Utils.HasRepeatedDigits(nif)) return false;

                return true;
            }
        }

        /// <summary>
        /// Classe utilitária com métodos auxiliares de validação genéricos.
        /// Contém validações reutilizáveis para strings numéricas.
        /// </summary>
        public class Utils
        {
            /// <summary>
            /// Verifica se uma string contém apenas dígitos numéricos.
            /// </summary>
            /// <param name="value">String a validar</param>
            /// <returns>True se contém apenas números, False caso contrário</returns>
            public static bool ValidOnlyNumbers(string value)
            {
                foreach (var s in value)
                {
                    if (!char.IsDigit(s))
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Verifica se uma string tem o tamanho esperado.
            /// </summary>
            /// <param name="value">String a validar</param>
            /// <param name="size">Tamanho esperado</param>
            /// <returns>True se o tamanho coincide, False caso contrário</returns>
            public static bool ValidSize(string value, int size)
            {
                return value.Length == size;
            }

            /// <summary>
            /// Verifica se uma string numérica tem todos os dígitos repetidos (ex: "111111111").
            /// Usado para detetar NIFs inválidos com padrões como "000000000" ou "999999999".
            /// </summary>
            /// <param name="value">String numérica a validar</param>
            /// <returns>True se todos os dígitos são iguais (inválido), False caso contrário</returns>
            public static bool HasRepeatedDigits(string value)
            {
                string[] invalidNumbers =
                {
                "00000000000",
                "11111111111",
                "22222222222",
                "33333333333",
                "44444444444",
                "55555555555",
                "66666666666",
                "77777777777",
                "88888888888",
                "99999999999"
                };
                return invalidNumbers.Contains(value);
            }
        }
    }
}
