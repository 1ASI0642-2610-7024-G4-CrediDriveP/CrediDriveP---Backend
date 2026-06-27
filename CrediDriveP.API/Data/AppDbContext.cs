using CrediDriveP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Insurance> Insurances => Set<Insurance>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<LoanPlan> LoanPlans => Set<LoanPlan>();
    public DbSet<LoanPlanInsurance> LoanPlanInsurances => Set<LoanPlanInsurance>();
    public DbSet<LoanPlanCommission> LoanPlanCommissions => Set<LoanPlanCommission>();
    public DbSet<Simulation> Simulations => Set<Simulation>();
    public DbSet<SimulationSchedule> SimulationSchedules => Set<SimulationSchedule>();
    public DbSet<SimulationIndicator> SimulationIndicators => Set<SimulationIndicator>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<PaymentSchedule> PaymentSchedules => Set<PaymentSchedule>();
    public DbSet<LoanIndicator> LoanIndicators => Set<LoanIndicator>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasDefaultValue("OFFICER");
            e.Property(u => u.IsActive).HasDefaultValue(true);
        });

        // ── Client ────────────────────────────────────────────────
        modelBuilder.Entity<Client>(e =>
        {
            e.HasIndex(c => c.Dni).IsUnique();
            e.HasOne(c => c.Creator)
             .WithMany(u => u.CreatedClients)
             .HasForeignKey(c => c.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Vehicle ───────────────────────────────────────────────
        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasIndex(v => v.Vin).IsUnique();
            e.HasOne(v => v.Creator)
             .WithMany(u => u.CreatedVehicles)
             .HasForeignKey(v => v.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── LoanPlan ──────────────────────────────────────────────
        modelBuilder.Entity<LoanPlan>(e =>
        {
            e.HasOne(lp => lp.Creator)
             .WithMany()
             .HasForeignKey(lp => lp.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── LoanPlanInsurance (clave compuesta) ───────────────────
        modelBuilder.Entity<LoanPlanInsurance>(e =>
        {
            e.HasKey(x => new { x.PlanId, x.InsuranceId });
            e.HasOne(x => x.LoanPlan)
             .WithMany(lp => lp.LoanPlanInsurances)
             .HasForeignKey(x => x.PlanId);
            e.HasOne(x => x.Insurance)
             .WithMany(i => i.LoanPlanInsurances)
             .HasForeignKey(x => x.InsuranceId);
        });

        // ── LoanPlanCommission (clave compuesta) ──────────────────
        modelBuilder.Entity<LoanPlanCommission>(e =>
        {
            e.HasKey(x => new { x.PlanId, x.CommissionId });
            e.HasOne(x => x.LoanPlan)
             .WithMany(lp => lp.LoanPlanCommissions)
             .HasForeignKey(x => x.PlanId);
            e.HasOne(x => x.Commission)
             .WithMany(c => c.LoanPlanCommissions)
             .HasForeignKey(x => x.CommissionId);
        });

        // ── Simulation ────────────────────────────────────────────
        modelBuilder.Entity<Simulation>(e =>
        {
            e.HasOne(s => s.Creator)
             .WithMany(u => u.Simulations)
             .HasForeignKey(s => s.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Client)
             .WithMany(c => c.Simulations)
             .HasForeignKey(s => s.ClientId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(s => s.Vehicle)
             .WithMany(v => v.Simulations)
             .HasForeignKey(s => s.VehicleId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Indicator)
             .WithOne(i => i.Simulation)
             .HasForeignKey<SimulationIndicator>(i => i.SimulationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── SimulationSchedule ────────────────────────────────────
        modelBuilder.Entity<SimulationSchedule>(e =>
        {
            e.HasIndex(x => new { x.SimulationId, x.PeriodNumber }).IsUnique();
            e.HasOne(x => x.Simulation)
             .WithMany(s => s.Schedules)
             .HasForeignKey(x => x.SimulationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Loan ──────────────────────────────────────────────────
        modelBuilder.Entity<Loan>(e =>
        {
            e.HasOne(l => l.Creator)
             .WithMany(u => u.CreatedLoans)
             .HasForeignKey(l => l.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Approver)
             .WithMany(u => u.ApprovedLoans)
             .HasForeignKey(l => l.ApprovedBy)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Client)
             .WithMany(c => c.Loans)
             .HasForeignKey(l => l.ClientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Vehicle)
             .WithMany(v => v.Loans)
             .HasForeignKey(l => l.VehicleId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Simulation)
             .WithOne(s => s.Loan)
             .HasForeignKey<Loan>(l => l.SimulationId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.Indicator)
             .WithOne(i => i.Loan)
             .HasForeignKey<LoanIndicator>(i => i.LoanId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── PaymentSchedule ───────────────────────────────────────
        modelBuilder.Entity<PaymentSchedule>(e =>
        {
            e.HasIndex(x => new { x.LoanId, x.PeriodNumber }).IsUnique();
            e.HasOne(x => x.Loan)
             .WithMany(l => l.Schedules)
             .HasForeignKey(x => x.LoanId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLog ──────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasOne(a => a.User)
             .WithMany()
             .HasForeignKey(a => a.UserId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Seed: Admin inicial ───────────────────────────────────
        modelBuilder.Entity<User>().HasData(new User
        {
            Id           = 1,
            Name         = "Administrador",
            Email        = "admin@credidrivep.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role         = "ADMIN",
            IsActive     = true,
            CreatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}