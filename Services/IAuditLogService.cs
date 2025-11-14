using HRMCyberse.Models;

namespace HRMCyberse.Services
{
    /// <summary>
    /// Interface for audit logging service
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Log shift management activities
        /// </summary>
        /// <param name="userId">ID of user performing the action</param>
        /// <param name="action">Action being performed</param>
        /// <param name="entityType">Type of entity (Shift, UserShift)</param>
        /// <param name="entityId">ID of the entity</param>
        /// <param name="details">Additional details about the action</param>
        /// <param name="oldValues">Previous values (for updates)</param>
        /// <param name="newValues">New values (for creates/updates)</param>
        Task LogShiftActivityAsync(int userId, string action, string entityType, int? entityId = null, 
            string? details = null, object? oldValues = null, object? newValues = null);

        /// <summary>
        /// Log shift assignment activities
        /// </summary>
        /// <param name="userId">ID of user performing the action</param>
        /// <param name="action">Action being performed</param>
        /// <param name="assignmentId">ID of the assignment</param>
        /// <param name="targetUserId">ID of user being assigned/unassigned</param>
        /// <param name="shiftId">ID of the shift</param>
        /// <param name="details">Additional details</param>
        Task LogShiftAssignmentActivityAsync(int userId, string action, int? assignmentId = null, 
            int? targetUserId = null, int? shiftId = null, string? details = null);

        /// <summary>
        /// Log general shift management activities
        /// </summary>
        /// <param name="userId">ID of user performing the action</param>
        /// <param name="action">Action description</param>
        /// <param name="details">Detailed information</param>
        Task LogGeneralShiftActivityAsync(int userId, string action, string? details = null);
    }
}