namespace TestPlatform.Contracts.Dashboard.DTOs;

public sealed record AdminDashboardResponse(
    int ActiveExams,
    int PublishedTests,
    int TotalAttempts,
    int UnfinishedAttempts,
    int FinishedAttempts,
    double PassRate);
