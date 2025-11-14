namespace HRMCyberse.Constants
{
    public static class RequestConstants
    {
        public static class Status
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
        }

        public static class RequestType
        {
            public const string Leave = "Leave";
            public const string ShiftChange = "ShiftChange";
            public const string LateArrival = "LateArrival";
        }
    }
}
