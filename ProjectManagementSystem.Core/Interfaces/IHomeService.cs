namespace ProjectManagementSystem.Core.Interfaces
{
    using Common.DTOs;

    public interface IHomeService
    {
        Task<HomeIndexDto> GetHomeIndexDataAsync(string userId);
    }
}