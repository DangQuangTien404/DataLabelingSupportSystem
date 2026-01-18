using BLL.Interfaces;
using DAL.Interfaces;
using DTOs.Constants;
using DTOs.Entities;
using DTOs.Requests;
using DTOs.Responses;

namespace BLL.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<DataItem> _dataItemRepository;

        public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository, IRepository<DataItem> dataItemRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _dataItemRepository = dataItemRepository;
        }
        public async Task<ProjectDetailResponse> CreateProjectAsync(string managerId, CreateProjectRequest request)
        {
            var manager = await _userRepository.GetByIdAsync(managerId);
            if (manager == null) throw new Exception("User not found");

            if (manager.Role != UserRoles.Manager && manager.Role != UserRoles.Admin)
                throw new Exception("Only Manager or Admin can create projects.");

            var project = new Project
            {
                ManagerId = managerId,
                Name = request.Name,
                Description = request.Description,
                PricePerLabel = request.PricePerLabel,
                TotalBudget = request.TotalBudget,
                Deadline = request.Deadline,

            };
            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();
            return new ProjectDetailResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                PricePerLabel = project.PricePerLabel,
                TotalBudget = project.TotalBudget,
                Deadline = project.Deadline,
                ManagerId = project.ManagerId,
                ManagerName = manager.FullName, 
                ManagerEmail = manager.Email,
                Labels = new List<LabelResponse>(),
                TotalDataItems = 0,            
                ProcessedItems = 0
            };
        }

        public async Task<List<ProjectSummaryResponse>> GetAssignedProjectsAsync(string annotatorId)
        {
            var projects = await _projectRepository.GetProjectsByAnnotatorAsync(annotatorId);

            return projects.Select(p => new ProjectSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Deadline = p.Deadline,
                Status = p.Deadline < DateTime.UtcNow ? "Expired" : "Active",
                TotalDataItems = 0,
                Progress = 0
            }).ToList();
        }

        public async Task ImportDataItemsAsync(int projectId, List<string> storageUrls)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            foreach (var url in storageUrls)
            {
                project.DataItems.Add(new DataItem
                {
                    StorageUrl = url,
                    Status = "New",
                    MetaData = "{}"
                });
            }

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();
        }

        public async Task<ProjectDetailResponse?> GetProjectDetailsAsync(int projectId)
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null) return null;

            return new ProjectDetailResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                PricePerLabel = project.PricePerLabel,
                TotalBudget = project.TotalBudget,
                Deadline = project.Deadline,
                ManagerId = project.ManagerId,
                ManagerName = project.Manager?.FullName ?? "Unknown",
                ManagerEmail = project.Manager?.Email ?? "",
                Labels = project.LabelClasses.Select(l => new LabelResponse
                {
                    Id = l.Id,
                    Name = l.Name,
                    Color = l.Color,
                    GuideLine = l.GuideLine
                }).ToList(),
                TotalDataItems = project.DataItems.Count,
                ProcessedItems = project.DataItems.Count(d => d.Status == "Done")
            };
        }

        public async Task<List<ProjectSummaryResponse>> GetProjectsByManagerAsync(string managerId)
        {
            var projects = await _projectRepository.GetProjectsByManagerIdAsync(managerId);

            return projects.Select(p => new ProjectSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Deadline = p.Deadline,
                TotalDataItems = p.DataItems.Count,
                Status = DateTime.UtcNow > p.Deadline ? "Expired" : "Active",
                Progress = p.DataItems.Count > 0
                           ? (decimal)p.DataItems.Count(d => d.Status == "Done") / p.DataItems.Count * 100
                           : 0
            }).ToList();
        }

        public async Task UpdateProjectAsync(int projectId, UpdateProjectRequest request)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            project.Name = request.Name;
            project.Description = request.Description;
            project.PricePerLabel = request.PricePerLabel;
            project.TotalBudget = request.TotalBudget;
            project.Deadline = request.Deadline;

            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Project not found");

            _projectRepository.Delete(project);
            await _projectRepository.SaveChangesAsync();
        }

        public async Task<List<DataItemResponse>> GetDataItemsAsync(int projectId, int page, int pageSize)
        {
            var items = await _projectRepository.GetProjectDataItemsAsync(projectId, page, pageSize);
            return items.Select(i => new DataItemResponse
            {
                Id = i.Id,
                StorageUrl = i.StorageUrl,
                Status = i.Status,
                MetaData = i.MetaData
            }).ToList();
        }

        public async Task<List<ExportDataItemDto>> ExportProjectDataAsync(int projectId)
        {
            var dataItems = await _projectRepository.GetProjectExportDataAsync(projectId);

            return dataItems.Select(d => new ExportDataItemDto
            {
                Id = d.Id,
                StorageUrl = d.StorageUrl,
                MetaData = d.MetaData,
                Annotations = d.Assignments
                    .Where(a => a.Status == "Completed")
                    .SelectMany(a => a.Annotations)
                    .Select(an => new ExportAnnotationDto
                    {
                        Label = an.LabelClass.Name,
                        Value = an.Value
                    }).ToList()
            }).ToList();
        }

        public async Task DeleteDataItemAsync(int dataItemId, string managerId)
        {
            var item = await _dataItemRepository.GetByIdAsync(dataItemId);
            if (item == null) throw new Exception("Data item not found");

            var project = await _projectRepository.GetByIdAsync(item.ProjectId);
            if (project == null || project.ManagerId != managerId)
                throw new Exception("Unauthorized to delete this item.");

            _dataItemRepository.Delete(item);
            await _dataItemRepository.SaveChangesAsync();
        }
    }
}