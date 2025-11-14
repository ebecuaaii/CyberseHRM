namespace HRMCyberse.Constants
{
    public static class PayrollConstants
    {
        public const decimal NightShiftBonus = 50000m; // 50,000 VND per night shift

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
