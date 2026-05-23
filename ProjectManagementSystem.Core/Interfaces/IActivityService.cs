namespace ProjectManagementSystem.Core.Interfaces
{
    using Common.Enums;

    public interface IActivityService
    {
        Task LogAsync(string userId, string message, ActivityType type);
    }
}