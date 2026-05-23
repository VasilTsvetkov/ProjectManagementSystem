namespace ProjectManagementSystem.Common.DTOs
{
    using Enums;
    using System;

    public class ActivityDto
    {
        public required string Message { get; init; }

        public required DateTime Timestamp { get; init; }

        public required ActivityType Type { get; init; }
    }
}