namespace HRMCyberse.Constants
{
    public static class PayrollConstants
    {
        // Note: NightShiftBonus is now configurable in appsettings.json under PayrollSettings
        // Use IConfiguration or PayrollSettings to access the value
        
        public static class RewardPenaltyType
        {
            public const string Reward = "Reward";
            public const string Penalty = "Penalty";
        }

        public static class SalaryDetailType
        {
            public const string BaseSalary = "Base Salary";
            public const string NightShiftBonus = "Night Shift Bonus";
            public const string Overtime = "Overtime";
            public const string Reward = "Reward";
            public const string Penalty = "Penalty";
            public const string Deduction = "Deduction";
        }
    }
}
