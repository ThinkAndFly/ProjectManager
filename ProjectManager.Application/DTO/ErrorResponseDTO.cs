namespace ProjectManager.Application.DTO
{
    public sealed class ErrorResponseDTO
    {
        public int StatusCode { get; init; }

        public string Error { get; init; } = string.Empty;

        public string? Details { get; init; }

        public string TraceId { get; init; } = string.Empty;
    }
}