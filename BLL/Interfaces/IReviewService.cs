using Core.DTOs.Requests;
using Core.DTOs.Responses;

namespace BLL.Interfaces
{
    public interface IReviewService
    {
        /// <summary>
/// Assigns a review according to the provided request to the reviewer identified by <paramref name="reviewerId"/>.
/// </summary>
/// <param name="reviewerId">Identifier of the reviewer who will be assigned the review.</param>
/// <param name="request">Details of the review assignment (e.g., task identifiers, deadlines, and instructions).</param>
Task ReviewAssignmentAsync(string reviewerId, ReviewRequest request);
        /// <summary>
/// Retrieves tasks that require review for the specified project.
/// </summary>
/// <param name="projectId">Identifier of the project whose review tasks to retrieve.</param>
/// <returns>A list of TaskResponse objects representing tasks that require review for the specified project.</returns>
Task<List<TaskResponse>> GetTasksForReviewAsync(int projectId);
        /// <summary>
/// Performs an audit of a review using the specified manager identity and audit details.
/// </summary>
/// <param name="managerId">Identifier of the manager performing the audit.</param>
/// <param name="request">Audit parameters and metadata identifying the review and required audit actions.</param>
Task AuditReviewAsync(string managerId, AuditReviewRequest request);
    }
}