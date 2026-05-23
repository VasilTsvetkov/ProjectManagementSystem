namespace ProjectManagementSystem.BL.DTOs.TimeLogs
{
    using System;

    public class TimeLogDto
    {
        public int Id { get; init; }
        public int Days { get; init; }
        public double Hours { get; init; }
        public int Minutes { get; init; }
        public DateTime Date { get; init; }
        public string? Description { get; init; }
        public int TaskId { get; init; }
        public int ProjectId { get; init; }
        public string? UserName { get; init; }
        public bool CanEdit { get; init; }
    }
}