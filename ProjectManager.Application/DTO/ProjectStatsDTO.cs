using System;

namespace ProjectManager.Application.DTO
{
    public class ProjectStatsDTO
    {
        public string ProjectId { get; set; } = string.Empty;

        public int DaysActive { get; set; }

        public DateTime LastUpdateUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}