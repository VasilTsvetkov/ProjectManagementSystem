namespace ProjectManagementSystem.Interfaces
{
    using ViewModels.Home;

    public interface IHomeService
    {
        Task<IndexViewModel> GetHomeIndexDataAsync(string userId);
    }
}