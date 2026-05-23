namespace ProjectManagementSystem.BL.Interfaces
{
    using Models;

    public interface ICommentRepository : IRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetCommentsByTaskAsync(int taskId);

        Task<bool> UpdateCommentAsync(int id, string content);
    }
}