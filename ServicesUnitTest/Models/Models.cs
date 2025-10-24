namespace ServicesUnitTest.Models
{
    //public enum FileProcessStatus
    //{
    //    Success,
    //    Failed,
    //    InProgress,
    //    InReview,
    //    NotStarted
    //}

    public enum UserProcessStatus
    {
        Active,
        Inactive,
        Suspended,
        Deleted,
        Pending
    }

    public enum UserProcessTheme
    {
        Default,
        Dark,
        Light
    }

    public enum UserProcessRole
    {
        Admin,
        User,
        Guest
    }

    public enum UserProcessPermission
    {
        Read,
        Write,
        Execute,
        Delete
    }
    public class ActionRecordSettings
    {
        public bool LogRoles { get; set; }
        public bool LogPermission { get; set; }
        public bool LogStatuses { get; set; }
        public bool IncludeActorInfo { get; set; }
        public bool IncludeTimeStamp { get; set; }
        public bool IncludePayloadDetails { get; set; }
    }

}