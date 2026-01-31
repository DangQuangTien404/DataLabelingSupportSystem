using BLL.Interfaces;
using Core.DTOs.Requests;
using Core.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    /// <summary>
    /// Controller quản lý các tác vụ (Task) của Annotator và việc giao việc của Manager.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// (Manager) Giao việc cho nhân viên Annotator.
        /// </summary>
        /// <param name="request">Thông tin giao việc (Project ID, Annotator ID, Số lượng ảnh).</param>
        /// <returns>Thông báo thành công.</returns>
        /// <response code="200">Giao việc thành công.</response>
        /// <response code="400">Lỗi giao việc (ví dụ: không đủ ảnh, user không tồn tại).</response>
        /// <response code="401">User không phải là Manager.</response>
        [HttpPost("assign")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> AssignTasks([FromBody] AssignTaskRequest request)
        {
            try
            {
                await _taskService.AssignTasksToAnnotatorAsync(request);
                return Ok(new { Message = "Tasks assigned successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// (Annotator) Lấy chi tiết 1 ảnh cụ thể bằng ID.
        /// </summary>
        /// <remarks>
        /// Dùng API này khi muốn nhảy trực tiếp đến một ảnh (ví dụ: bấm từ thông báo lỗi, hoặc F5 lại trang).
        /// </remarks>
        /// <param name="id">ID của Assignment (AssignmentId).</param>
        /// <returns>Chi tiết ảnh và dữ liệu vẽ.</returns>
        [HttpGet("assignment/{id}")]
        [ProducesResponseType(typeof(AssignmentResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetSingleAssignment(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var assignment = await _taskService.GetAssignmentByIdAsync(id, userId);
                return Ok(assignment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// (Annotator - Dashboard) Lấy danh sách Dự án được phân công.
        /// </summary>
        /// <remarks>
        /// API này dùng cho màn hình Dashboard chính. 
        /// Nó sẽ gom nhóm các ảnh lẻ tẻ thành các thẻ Dự án (Project Card).
        /// Trả về tiến độ (%), deadline, và trạng thái tổng quan.
        /// </remarks>
        /// <returns>Danh sách các dự án mà user đang tham gia.</returns>
        /// <response code="200">Trả về danh sách dự án thành công.</response>
        /// <response code="401">Chưa đăng nhập.</response>
        [HttpGet("my-projects")]
        [ProducesResponseType(typeof(List<AssignedProjectResponse>), 200)]
        [ProducesResponseType(typeof(void), 401)]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var projects = await _taskService.GetAssignedProjectsAsync(userId);
            return Ok(projects);
        }

        /// <summary>
        /// (Annotator - Work Area) Lấy danh sách Ảnh chi tiết trong một dự án để làm việc.
        /// </summary>
        /// <remarks>
        /// API này dùng khi user bấm vào một thẻ Dự án. 
        /// Nó trả về toàn bộ list ảnh (Assignments) để FE xử lý nút Next/Back.
        /// </remarks>
        /// <param name="projectId">ID của dự án muốn làm.</param>
        /// <returns>Danh sách ảnh kèm trạng thái và dữ liệu vẽ cũ (nếu có).</returns>
        /// <response code="200">Trả về danh sách ảnh thành công.</response>
        /// <response code="401">Chưa đăng nhập.</response>
        [HttpGet("project/{projectId}/images")]
        [ProducesResponseType(typeof(List<AssignmentResponse>), 200)]
        [ProducesResponseType(typeof(void), 401)]
        public async Task<IActionResult> GetProjectImages(int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var images = await _taskService.GetTaskImagesAsync(projectId, userId);
            return Ok(images);
        }

        /// <summary>
        /// (Annotator) Lưu nháp (Save Draft).
        /// </summary>
        /// <remarks>
        /// Gọi API này khi user bấm nút "Next" hoặc "Save".
        /// Hệ thống sẽ lưu đè dữ liệu vẽ (DataJSON) và cập nhật trạng thái thành 'InProgress'.
        /// </remarks>
        /// <param name="request">Chứa AssignmentId và cục DataJSON (Canvas).</param>
        /// <returns>Thông báo thành công.</returns>
        /// <response code="200">Lưu nháp thành công.</response>
        /// <response code="400">Lỗi dữ liệu đầu vào.</response>
        [HttpPost("save-draft")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> SaveDraft([FromBody] SubmitAnnotationRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _taskService.SaveDraftAsync(userId, request);
                return Ok(new { Message = "Draft saved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// (Annotator) Nộp bài (Submit).
        /// </summary>
        /// <remarks>
        /// Gọi API này khi user bấm nút "Submit".
        /// Hệ thống sẽ lưu dữ liệu vẽ và cập nhật trạng thái thành 'Submitted' (Chờ duyệt).
        /// </remarks>
        /// <param name="request">Chứa AssignmentId và cục DataJSON (Canvas).</param>
        /// <returns>Thông báo thành công.</returns>
        /// <response code="200">Nộp bài thành công.</response>
        /// <response code="400">Lỗi nộp bài.</response>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> SubmitTask([FromBody] SubmitAnnotationRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _taskService.SubmitTaskAsync(userId, request);
                return Ok(new { Message = "Task submitted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}