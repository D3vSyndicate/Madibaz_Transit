using System;

namespace Madibaz_Transit_BackEnd.Models.Entities
{
    public class DailyDeploymentLogs
    {
        public int DailyDeploymentLogId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public DateTime LogDate { get; set; }
        public int DeployedVehicleCount { get; set; }
        public string? Notes { get; set; }
    }
}