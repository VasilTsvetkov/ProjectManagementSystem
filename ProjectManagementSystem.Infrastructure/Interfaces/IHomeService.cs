namespace ProjectManagementSystem.BL.Interfaces
{
    using DTOs;

    public interface IHomeService
    {
        Task<HomeIndexDto> GetHomeIndexDataAsync(string userId);
    }
}