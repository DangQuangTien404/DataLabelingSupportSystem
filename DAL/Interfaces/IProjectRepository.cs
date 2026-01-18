using DTOs.Entities;

namespace DAL.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<Project?> GetProjectWithDetailsAsync(int id);
        Task<List<Project>> GetProjectsByManagerIdAsync(string managerId);

        Task<List<Project>> GetProjectsByAnnotatorAsync(string annotatorId);

        Task<List<DataItem>> GetProjectDataItemsAsync(int projectId, int page, int pageSize);
        Task<List<DataItem>> GetProjectExportDataAsync(int projectId);
    }
}