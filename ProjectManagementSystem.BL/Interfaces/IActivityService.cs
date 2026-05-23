namespace ProjectManagementSystem.BL.Interfaces
{
    using Enums;

    public interface IActivityService
    {
        Task LogAsync(string userId, string message, ActivityType type);
    }
}