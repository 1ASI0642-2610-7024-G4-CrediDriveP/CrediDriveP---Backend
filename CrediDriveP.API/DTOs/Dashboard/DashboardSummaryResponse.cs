namespace CrediDriveP.API.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalClients     { get; set; }
    public int TotalVehicles    { get; set; }
    public int TotalSimulations { get; set; }
    public int PendingLoans     { get; set; }
    public int ApprovedLoans    { get; set; }
    public List<RecentActivityDto> RecentActivity { get; set; } = [];
}

public class RecentActivityDto
{
    public string Type        { get; set; } = null!; // CLIENT / SIMULATION / LOAN
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}