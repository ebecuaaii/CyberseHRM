namespace HRMCyberse.DTOs
{
    /// <summary>
    /// DTO để admin cập nhật thông tin user (role, position, department)
    /// </summary>
    public class UpdateUserDto
    {
        public string? RoleName { get; set; }
        public string? PositionName { get; set; }
        public string? DepartmentName { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
    }
}



