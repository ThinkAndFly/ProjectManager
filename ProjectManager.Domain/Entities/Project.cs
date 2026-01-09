namespace ProjectManager.Domain.Entities
{
    public class Project
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string OwnerId { get; set; } = string.Empty;

        public string Status { get; set; } = "active";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAtUtc { get; set; }
    }
}
