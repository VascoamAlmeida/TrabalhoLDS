using FluentValidation;
using FluentValidation.Results;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Models;

namespace LojaMaquinas.Business.Services
{
    /// <summary>
    /// Classe base abstrata para todos os serviços do sistema.
    /// Fornece funcionalidades comuns de validação e notificação de erros.
    /// </summary>
    public abstract class BaseService
    {
        private readonly INotifierService _notifier;

        /// <summary>
        /// Inicializa uma nova instância do serviço base.
        /// </summary>
        /// <param name="notifier">Serviço de notificações para comunicar erros de validação</param>
        protected BaseService(INotifierService notifier)
        {
            _notifier = notifier;
        }

        /// <summary>
        /// Executa validação de uma entidade usando FluentValidation.
        /// Se a validação falhar, regista as mensagens de erro no notifier.
        /// </summary>
        /// <typeparam name="VT">Tipo do validador (deve herdar de AbstractValidator)</typeparam>
        /// <typeparam name="VE">Tipo da entidade a validar (deve herdar de Entity)</typeparam>
        /// <param name="validationType">Instância do validador a utilizar</param>
        /// <param name="validationEntity">Entidade a ser validada</param>
        /// <returns>True se a validação passar, False se falhar</returns>
        protected bool RunValidation<VT, VE>(VT validationType, VE validationEntity)
                                        where VT : AbstractValidator<VE>
                                        where VE : Entity
        {
            var validateResult = validationType.Validate(validationEntity);

            if (validateResult.IsValid) return true;

            NotifyWithMessage(validateResult); //Launch Notifications

            return false;
        }

        /// <summary>
        /// Regista todas as mensagens de erro de um resultado de validação.
        /// Itera sobre todos os erros e adiciona-os ao notifier.
        /// </summary>
        /// <param name="validationResult">Resultado da validação do FluentValidation</param>
        protected void NotifyWithMessage(ValidationResult validationResult)
        {
            foreach (var item in validationResult.Errors)
            {
                NotifyWithMessage(item.ErrorMessage);
            }
        }

        /// <summary>
        /// Regista uma mensagem de erro no sistema de notificações.
        /// </summary>
        /// <param name="message">Mensagem de erro a registar</param>
        protected void NotifyWithMessage(string message)
        {
            _notifier.Handle(new Notification(message));
        }
    }
}
