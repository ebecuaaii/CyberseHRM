namespace HRMCyberse.Constants
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Manager = "manager";
        public const string Employee = "employee";
        
        public static readonly string[] All = { Admin, Manager, Employee };
        
        public static readonly string[] AdminAndManager = { Admin, Manager };
        
        public static readonly string[] ManagerAndEmployee = { Manager, Employee };
    }
}