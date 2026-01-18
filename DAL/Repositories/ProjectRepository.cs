using DAL.Interfaces;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Project?> GetProjectWithDetailsAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.LabelClasses)
                .Include(p => p.DataItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Project>> GetProjectsByManagerIdAsync(string managerId)
        {
            return await _context.Projects
                .Include(p => p.DataItems)
                .Where(p => p.ManagerId == managerId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<List<Project>> GetProjectsByAnnotatorAsync(string annotatorId)
        {
            return await _context.Assignments
                .Where(a => a.AnnotatorId == annotatorId)
                .Include(a => a.Project)
                .Select(a => a.Project)
                .Distinct()
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<List<DataItem>> GetProjectDataItemsAsync(int projectId, int page, int pageSize)
        {
            return await _context.DataItems
                .Where(d => d.ProjectId == projectId)
                .OrderBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<DataItem>> GetProjectExportDataAsync(int projectId)
        {
            // Get DataItems that are Done, include Assignments and Annotations
            return await _context.DataItems
                .Where(d => d.ProjectId == projectId && d.Status == "Done")
                .Include(d => d.Assignments)
                .ThenInclude(a => a.Annotations)
                .ThenInclude(an => an.LabelClass)
                .ToListAsync();
        }
    }
}