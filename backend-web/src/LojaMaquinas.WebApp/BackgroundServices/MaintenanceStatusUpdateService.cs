using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.WebApp.BackgroundServices
{
    /// <summary>
    /// Background service que atualiza automaticamente o status das manutenções.
    /// Verifica periodicamente se existem manutenções com status Pending cuja data agendada já chegou,
    /// e atualiza o status para InProgress automaticamente.
    /// </summary>
    public class MaintenanceStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MaintenanceStatusUpdateService> _logger;
        private readonly TimeSpan _checkInterval;

        public MaintenanceStatusUpdateService(
            IServiceProvider serviceProvider,
            ILogger<MaintenanceStatusUpdateService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Lê o intervalo de verificação do appsettings (padrão: 1 hora)
            var intervalMinutes = configuration.GetValue<int>("MaintenanceStatusCheck:IntervalMinutes", 60);
            _checkInterval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Maintenance Status Update Service started. Check interval: {Interval} minutes",
                _checkInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateMaintenanceStatuses(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating maintenance statuses");
                }

                // Aguarda o intervalo configurado antes da próxima verificação
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Maintenance Status Update Service stopped");
        }

        private async Task UpdateMaintenanceStatuses(CancellationToken stoppingToken)
        {
            // Cria um scope para resolver dependências scoped (repositories, services)
            using var scope = _serviceProvider.CreateScope();
            var maintenanceRepository = scope.ServiceProvider.GetRequiredService<IMaintenanceRepository>();

            try
            {
                var now = DateTime.Now;
                var today = now.Date;
                var pendingToInProgressCount = 0;
                var inProgressToCompletedCount = 0;

                // 1. Atualiza Pending → InProgress (quando chega o dia agendado)
                var pendingMaintenances = await maintenanceRepository.GetMaintenancesByStatus(MaintenanceStatus.Pending);

                if (pendingMaintenances != null && pendingMaintenances.Any())
                {
                    foreach (var maintenance in pendingMaintenances)
                    {
                        // Se a data agendada já chegou (hoje ou antes), atualiza para InProgress
                        if (maintenance.ScheduledDate.Date <= today)
                        {
                            maintenance.Status = MaintenanceStatus.InProgress;
                            await maintenanceRepository.Update(maintenance);
                            pendingToInProgressCount++;

                            _logger.LogInformation(
                                "Maintenance {MaintenanceId} for Order {OrderId} automatically updated to InProgress. Scheduled: {ScheduledDate}",
                                maintenance.Id,
                                maintenance.OrderId,
                                maintenance.ScheduledDate);
                        }
                    }
                }

                // 2. Atualiza InProgress → Completed (no dia seguinte à data agendada)
                var inProgressMaintenances = await maintenanceRepository.GetMaintenancesByStatus(MaintenanceStatus.InProgress);

                if (inProgressMaintenances != null && inProgressMaintenances.Any())
                {
                    foreach (var maintenance in inProgressMaintenances)
                    {
                        // Se já passou 1 dia completo após a data agendada, marca como Completed
                        if (maintenance.ScheduledDate.Date < today)
                        {
                            maintenance.Status = MaintenanceStatus.Completed;
                            await maintenanceRepository.Update(maintenance);
                            inProgressToCompletedCount++;

                            _logger.LogInformation(
                                "Maintenance {MaintenanceId} for Order {OrderId} automatically updated to Completed. Scheduled: {ScheduledDate}, Completed: {CompletedDate}",
                                maintenance.Id,
                                maintenance.OrderId,
                                maintenance.ScheduledDate,
                                today);
                        }
                    }
                }

                if (pendingToInProgressCount > 0 || inProgressToCompletedCount > 0)
                {
                    _logger.LogInformation(
                        "Status update completed: {PendingToInProgress} Pending→InProgress, {InProgressToCompleted} InProgress→Completed",
                        pendingToInProgressCount,
                        inProgressToCompletedCount);
                }
                else
                {
                    _logger.LogDebug("No maintenance statuses needed updating");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance statuses in repository");
                throw;
            }
        }
    }
}
