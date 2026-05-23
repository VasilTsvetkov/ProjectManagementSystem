namespace ProjectManagementSystem.Common.Constants
{
    public static class MessageConstants
    {
        public const string UntitledProject = "Untitled Project";
        public const string UnknownProject = "Unknown Project";
        public const string ActivityCreatedProject = "Created project: {0}";
        public const string ActivityUpdatedProject = "Updated details for project: {0}";
        public const string ActivityDeletedProject = "Deleted project: {0}";

        public const string UntitledTask = "Untitled Task";
        public const string MissingTaskIdentifier = "a deleted or missing task";
        public const string Unassigned = "Unassigned";
        public const string ActivityCreatedTask = "Created task: {0}";
        public const string ActivityUpdatedTask = "Updated task details: {0}";
        public const string ActivityDeletedTask = "Deleted task: {0}";
        public const string ActivityMovedTask = "Moved {0} to {1}";

        public const string ActivityAddedComment = "Added a comment to {0}";
        public const string ActivityUpdatedComment = "Updated a comment on {0}";
        public const string ActivityDeletedComment = "Deleted a comment from {0}";

        public const string ActivityLoggedTime = "Logged {0} on {1}";
        public const string ActivityDeletedTimeLog = "Deleted {0} log from {1}";

        public const string SystemUser = "System User";
        public const string UserNotFound = "User not found";
        public const string NoEmailProvided = "No Email Provided";
        public const string CannotChangeAdminRole = "Cannot change Admin role.";
        public const string RoleUpdateFailed = "Failed to update role";
        public const string RoleChangedSuccessfully = "Role changed to {0} for {1}";
    }
}