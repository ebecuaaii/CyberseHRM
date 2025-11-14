using HRMCyberse.DTOs;

namespace HRMCyberse.Services
{
    /// <summary>
    /// Service interface for managing work shifts and shift assignments.
    /// Provides comprehensive shift management functionality with performance optimizations.
    /// </summary>
    public interface IShiftService
    {
        // Shift CRUD operations
        
        /// <summary>
        /// Retrieves all work shifts in the system with caching for improved performance.
        /// </summary>
        /// <returns>A collection of all shifts with their details</returns>
        Task<IEnumerable<ShiftDto>> GetAllShiftsAsync();
        
        /// <summary>
        /// Retrieves a specific work shift by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the shift</param>
        /// <returns>The shift details if found, null otherwise</returns>
        Task<ShiftDto?> GetShiftByIdAsync(int id);
        
        /// <summary>
        /// Creates a new work shift with validation and audit logging.
        /// </summary>
        /// <param name="createShiftDto">The shift data to create</param>
        /// <param name="createdBy">The ID of the user creating the shift</param>
        /// <returns>The created shift with generated ID and metadata</returns>
        /// <exception cref="ArgumentException">Thrown when shift data is invalid or name already exists</exception>
        Task<ShiftDto> CreateShiftAsync(CreateShiftDto createShiftDto, int createdBy);
        
        /// <summary>
        /// Updates an existing work shift with validation and audit logging.
        /// </summary>
        /// <param name="id">The ID of the shift to update</param>
        /// <param name="updateShiftDto">The updated shift data</param>
        /// <param name="updatedBy">The ID of the user updating the shift</param>
        /// <returns>The updated shift details</returns>
        /// <exception cref="ArgumentException">Thrown when shift is not found or data is invalid</exception>
        Task<ShiftDto> UpdateShiftAsync(int id, UpdateShiftDto updateShiftDto, int updatedBy);
        
        /// <summary>
        /// Deletes a work shift if it has no existing assignments.
        /// </summary>
        /// <param name="id">The ID of the shift to delete</param>
        /// <param name="deletedBy">The ID of the user deleting the shift</param>
        /// <returns>True if deletion was successful, false if shift not found</returns>
        /// <exception cref="InvalidOperationException">Thrown when shift has existing assignments</exception>
        Task<bool> DeleteShiftAsync(int id, int deletedBy);

        // UserShift assignment operations
        
        /// <summary>
        /// Retrieves all shift assignments across all employees with optimized queries.
        /// </summary>
        /// <returns>A collection of all shift assignments with employee and shift details</returns>
        Task<IEnumerable<UserShiftDto>> GetAllAssignmentsAsync();
        
        /// <summary>
        /// Retrieves shift assignments for a specific employee.
        /// </summary>
        /// <param name="userId">The ID of the employee</param>
        /// <returns>A collection of shift assignments for the specified employee</returns>
        Task<IEnumerable<UserShiftDto>> GetUserAssignmentsAsync(int userId);
        
        /// <summary>
        /// Assigns a work shift to an employee for a specific date with conflict validation.
        /// </summary>
        /// <param name="assignShiftDto">The assignment data including user, shift, and date</param>
        /// <param name="assignedBy">The ID of the user making the assignment</param>
        /// <returns>The created assignment details</returns>
        /// <exception cref="ArgumentException">Thrown when user or shift doesn't exist</exception>
        /// <exception cref="InvalidOperationException">Thrown when assignment conflicts with existing shifts</exception>
        Task<UserShiftDto> AssignShiftAsync(AssignShiftDto assignShiftDto, int assignedBy);
        
        /// <summary>
        /// Removes a shift assignment from an employee.
        /// </summary>
        /// <param name="assignmentId">The ID of the assignment to remove</param>
        /// <param name="removedBy">The ID of the user removing the assignment</param>
        /// <returns>True if removal was successful, false if assignment not found</returns>
        Task<bool> RemoveAssignmentAsync(int assignmentId, int removedBy);

        // Validation methods
        
        /// <summary>
        /// Validates shift time configuration according to business rules.
        /// </summary>
        /// <param name="startTime">The shift start time</param>
        /// <param name="endTime">The shift end time</param>
        /// <returns>True if times are valid, false otherwise</returns>
        Task<bool> ValidateShiftTimesAsync(TimeOnly startTime, TimeOnly endTime);
        
        /// <summary>
        /// Checks if a shift name is unique in the system.
        /// </summary>
        /// <param name="name">The shift name to check</param>
        /// <param name="excludeId">Optional shift ID to exclude from the check (for updates)</param>
        /// <returns>True if name is unique, false if already exists</returns>
        Task<bool> IsShiftNameUniqueAsync(string name, int? excludeId = null);
        
        /// <summary>
        /// Checks if a user exists and is active in the system with caching.
        /// </summary>
        /// <param name="userId">The ID of the user to check</param>
        /// <returns>True if user exists and is active, false otherwise</returns>
        Task<bool> UserExistsAsync(int userId);
        
        /// <summary>
        /// Checks if a shift exists in the system with caching.
        /// </summary>
        /// <param name="shiftId">The ID of the shift to check</param>
        /// <returns>True if shift exists, false otherwise</returns>
        Task<bool> ShiftExistsAsync(int shiftId);
        
        /// <summary>
        /// Checks for time conflicts between a new shift assignment and existing assignments.
        /// </summary>
        /// <param name="userId">The ID of the employee</param>
        /// <param name="shiftDate">The date of the shift assignment</param>
        /// <param name="startTime">The start time of the shift</param>
        /// <param name="endTime">The end time of the shift</param>
        /// <param name="excludeShiftId">Optional shift ID to exclude from conflict checking</param>
        /// <returns>True if there are conflicts, false if no conflicts</returns>
        Task<bool> CheckShiftConflictAsync(int userId, DateOnly shiftDate, TimeOnly startTime, TimeOnly endTime, int? excludeShiftId = null);
    }
}