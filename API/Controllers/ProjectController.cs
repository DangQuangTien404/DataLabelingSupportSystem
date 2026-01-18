using BLL.Interfaces;
using DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(managerId))
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }
                var project = await _projectService.CreateProjectAsync(managerId, request);

                return Ok(new { Message = "Project created successfully", ProjectId = project.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/upload-direct")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDataDirect(int id, [FromForm] UploadDataRequest request)
        {
            var urls = new List<string>();

            try
            {
                var folderName = Path.Combine("wwwroot", "uploads", $"project-{id}");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }

                foreach (var file in request.Files)
                {
                    if (file.Length > 0)
                    {

                        var fileName = $"{DateTime.Now.Ticks}_{file.FileName}";
                        var fullPath = Path.Combine(pathToSave, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var dbUrl = $"/uploads/project-{id}/{fileName}";
                        urls.Add(dbUrl);
                    }
                }

                if (urls.Any())
                {
                    await _projectService.ImportDataItemsAsync(id, urls);
                }

                return Ok(new { Message = $"{urls.Count} files uploaded successfully", Urls = urls });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var projectDto = await _projectService.GetProjectDetailsAsync(id);
            if (projectDto == null) return NotFound(new { Message = "Project not found" });

            return Ok(projectDto);
        }

        [HttpPost("{id}/import-data")]
        public async Task<IActionResult> ImportData(int id, [FromBody] ImportDataRequest request)
        {
            try
            {
                await _projectService.ImportDataItemsAsync(id, request.StorageUrls);
                return Ok(new { Message = $"{request.StorageUrls.Count} items imported successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("annotator/assigned")]
        public async Task<IActionResult> GetAssignedProjects()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var projects = await _projectService.GetAssignedProjectsAsync(userId);
            return Ok(projects);
        }

        [HttpGet("manager/me")]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var projects = await _projectService.GetProjectsByManagerAsync(userId);
            return Ok(projects);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
        {
            try
            {
                await _projectService.UpdateProjectAsync(id, request);
                return Ok(new { Message = "Project updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                await _projectService.DeleteProjectAsync(id);
                return Ok(new { Message = "Project deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}/data-items")]
        public async Task<IActionResult> GetDataItems(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var items = await _projectService.GetDataItemsAsync(id, page, pageSize);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}/export")]
        public async Task<IActionResult> ExportData(int id)
        {
            try
            {
                var data = await _projectService.ExportProjectDataAsync(id);
                var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);
                return File(jsonBytes, "application/json", $"project-{id}-export.json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("data-items/{dataItemId}")]
        public async Task<IActionResult> DeleteDataItem(int dataItemId)
        {
            try
            {
                var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(managerId)) return Unauthorized();

                await _projectService.DeleteDataItemAsync(dataItemId, managerId);
                return Ok(new { Message = "Data item deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}