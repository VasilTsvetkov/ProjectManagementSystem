namespace ProjectManagementSystem.ViewModels.Home
{
    public class ErrorViewModel
    {
        public string? RequestId { get; init; }

        public int? StatusCode { get; init; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}