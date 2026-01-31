using BLL.Interfaces;
using Core.DTOs.Requests;
using Core.DTOs.Responses;
using DTOs.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Submits a review for an assignment (approve/reject).
        /// </summary>
        /// <param name="request">The review details containing AssignmentId, Approval status, and comments.</param>
        /// <returns>A message indicating whether the task was Approved or Rejected.</returns>
        /// <response code="200">Review submitted successfully.</response>
        /// <response code="400">If the review submission fails (e.g., task not found).</response>
        /// <summary>
        /// Submits a review decision for an assignment on behalf of the current user.
        /// </summary>
        /// <param name="request">The review details, including the target assignment and the approval decision.</param>
        /// <returns>
        /// An IActionResult indicating the outcome:
        /// 200 with a message "Approved" or "Rejected" on success;
        /// 400 with an error message if processing fails;
        /// 401 if the caller is not authenticated.
        /// </returns>
        /// <response code="200">Review processed successfully; response contains a message "Approved" or "Rejected".</response>
        /// <response code="400">Request failed to process; response contains the error message.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost("submit")]
        [Authorize(Roles = "Reviewer,Manager,Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> ReviewTask([FromBody] ReviewRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _reviewService.ReviewAssignmentAsync(userId, request);
                return Ok(new { Message = request.IsApproved ? "Approved" : "Rejected" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// (For Manager) Audits a past review to evaluate the Reviewer's quality (RQS).
        /// </summary>
        /// <param name="request">The audit details including ReviewLogId and whether the Manager agrees with the Reviewer's decision.</param>
        /// <returns>A success message confirming the audit was recorded.</returns>
        /// <response code="200">Audit submitted successfully.</response>
        /// <response code="400">If the audit fails (e.g., review log not found or already audited).</response>
        /// <summary>
        /// Submits an audit for a review on behalf of the current manager.
        /// </summary>
        /// <param name="request">The audit details to submit.</param>
        /// <returns>An IActionResult containing a success message on 200 OK, an error message on 400 Bad Request, or 401 Unauthorized if the caller identity is missing.</returns>
        /// <response code="200">Audit submitted successfully.</response>
        /// <response code="400">If the audit could not be processed; returns an error message.</response>
        /// <response code="401">If the user is not authorized (must be Manager or Admin).</response>
        [HttpPost("audit")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> AuditReview([FromBody] AuditReviewRequest request)
        {
            var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerId)) return Unauthorized();

            try
            {
                await _reviewService.AuditReviewAsync(managerId, request);
                return Ok(new { Message = "Audit submitted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets a list of tasks (assignments) that need to be reviewed for a specific project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of tasks awaiting review.</returns>
        /// <response code="200">Returns list of tasks.</response>
        /// <summary>
        /// Retrieves tasks pending review for the specified project.
        /// </summary>
        /// <param name="projectId">The project identifier to fetch review tasks for.</param>
        /// <returns>An enumerable of TaskResponse representing tasks available for review.</returns>
        /// <response code="200">A collection of tasks for review.</response>
        /// <response code="400">If retrieval fails.</response>
        [HttpGet("project/{projectId}")]
        [Authorize(Roles = "Reviewer,Manager,Admin")]
        [ProducesResponseType(typeof(IEnumerable<TaskResponse>), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> GetTasksForReview(int projectId)
        {
            try
            {
                var tasks = await _reviewService.GetTasksForReviewAsync(projectId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets the list of available error categories (e.g., TE-01, LU-01).
        /// </summary>
        /// <returns>A list of error codes and descriptions.</returns>
        /// <summary>
        /// Retrieves all configured error category names.
        /// </summary>
        /// <returns>An enumerable of all error category names.</returns>
        /// <response code="200">Returns list of error categories.</response>
        [HttpGet("error-categories")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public IActionResult GetErrorCategories()
        {
            return Ok(ErrorCategories.All);
        }
    }
}