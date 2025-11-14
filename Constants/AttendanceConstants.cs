namespace HRMCyberse.Constants
{
    public static class AttendanceConstants
    {
        public static class Status
        {
            public const string OnTime = "On Time";
            public const string Late = "Late";
            public const string Absent = "Absent";
            public const string ManualEntry = "Manual Entry";
            public const string EarlyCheckout = "Early Checkout";
        }

        public static class ImageType
        {
            public const string CheckIn = "CheckIn";
            public const string CheckOut = "CheckOut";
        }

        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Manager = "Manager";
            public const string Employee = "Employee";
        }
    }
}