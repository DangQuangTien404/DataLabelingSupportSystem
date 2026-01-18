using DTOs.Requests;
using DTOs.Responses;
using DTOs.Entities;

namespace BLL.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDetailResponse> CreateProjectAsync(string managerId, CreateProjectRequest request);

        Task ImportDataItemsAsync(int projectId, List<string> storageUrls);
        Task<ProjectDetailResponse?> GetProjectDetailsAsync(int projectId);

        Task<List<ProjectSummaryResponse>> GetProjectsByManagerAsync(string managerId);

        Task<List<ProjectSummaryResponse>> GetAssignedProjectsAsync(string annotatorId);

        Task UpdateProjectAsync(int projectId, UpdateProjectRequest request);
        Task DeleteProjectAsync(int projectId);

        Task<List<DataItemResponse>> GetDataItemsAsync(int projectId, int page, int pageSize);
        Task<List<ExportDataItemDto>> ExportProjectDataAsync(int projectId);
        Task DeleteDataItemAsync(int dataItemId, string managerId);
    }
}