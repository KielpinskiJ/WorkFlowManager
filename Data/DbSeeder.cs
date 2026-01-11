using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkFlowManager.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace WorkFlowManager.Data;

public static class DbSeeder
{
    private const string DefaultAdminPassword = "Admin123!";
    
    public const string AdminEmail = "admin@wsb.pl";
    private static readonly string AdminPassword = GetAdminPassword();

    private static string GetAdminPassword()
    {
        // Try to load from .env file first
        var envPassword = LoadPasswordFromEnvFile();
        if (!string.IsNullOrEmpty(envPassword))
        {
            return envPassword;
        }

        // Fallback to hardcoded default password
        Debug.WriteLine(
            "Info: No .env file found or ADMIN_PASSWORD not set. Using default admin password.");
        return DefaultAdminPassword;
    }

    private static string? LoadPasswordFromEnvFile()
    {
        var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        
        if (!File.Exists(envFilePath))
        {
            return null;
        }

        try
        {
            var lines = File.ReadAllLines(envFilePath);
            foreach (var line in lines)
            {
                // Skip empty lines and comments
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                {
                    continue;
                }

                var separatorIndex = trimmedLine.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = trimmedLine.Substring(0, separatorIndex).Trim();
                var value = trimmedLine.Substring(separatorIndex + 1).Trim();

                if (key == "ADMIN_PASSWORD" && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Warning: Failed to read .env file: {ex.Message}");
        }

        return null;
    }
    
    public const string RoleAdmin = "Admin";
    public const string RoleEmployee = "Employee";

    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await CreateRoleIfNotExists(roleManager, RoleAdmin);
        await CreateRoleIfNotExists(roleManager, RoleEmployee);

        await CreateAdminUserIfNotExists(userManager);

        await AssignEmployeeRoleToUsersWithoutRole(userManager);
    }

    // ============================================================================
    // DEVELOPMENT DATA SEEDING - REMOVE IN PRODUCTION!
    // This method generates fake employees, shifts, bonuses, and leave requests
    // for testing purposes only. Delete or comment out this entire section
    // and its helper methods before deploying to production.
    // ============================================================================
    public static async Task SeedDevelopmentDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Only seed if database is empty (no departments)
        if (await context.Departments.AnyAsync())
        {
            return;
        }

        var random = new Random(42);

        var departments = CreateDepartments();
        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();

        var employees = await CreateEmployeesAsync(userManager, departments, random);

        var shifts = CreateShiftsForEmployees(employees, departments, random);
        await context.WorkShifts.AddRangeAsync(shifts);

        var bonuses = CreateBonusesForEmployees(employees, random);
        await context.Bonuses.AddRangeAsync(bonuses);

        var requests = CreateRequestsForEmployees(employees, departments, random);
        await context.LeaveRequests.AddRangeAsync(requests);

        await context.SaveChangesAsync();
    }
    // ============================================================================
    // HELPER METHODS FOR DEVELOPMENT DATA - REMOVE IN PRODUCTION!
    // ============================================================================

    private static List<Department> CreateDepartments()
    {
        return new List<Department>
        {
            new Department { Name = "IT Department", Description = "Software development and technical support", HourlyRate = 85.00m },
            new Department { Name = "Human Resources", Description = "Employee relations and recruitment", HourlyRate = 55.00m },
            new Department { Name = "Finance", Description = "Accounting, budgeting and financial planning", HourlyRate = 70.00m },
            new Department { Name = "Marketing", Description = "Brand management and advertising", HourlyRate = 60.00m },
            new Department { Name = "Operations", Description = "Day-to-day business operations", HourlyRate = 50.00m },
            new Department { Name = "Customer Service", Description = "Client support and satisfaction", HourlyRate = 45.00m }
        };
    }

    private static readonly string[] FirstNames = {
        "Anna", "Piotr", "Katarzyna", "Michał", "Magdalena", "Tomasz", "Agnieszka", "Marcin",
        "Monika", "Krzysztof", "Joanna", "Paweł", "Aleksandra", "Jakub", "Natalia", "Adam",
        "Karolina", "Łukasz", "Dominika", "Mateusz", "Patrycja", "Kamil", "Justyna", "Bartosz",
        "Weronika", "Dawid", "Sylwia", "Artur", "Ewelina", "Sebastian", "Izabela", "Grzegorz",
        "Paulina", "Rafał", "Martyna", "Wojciech", "Kinga", "Robert", "Adrianna", "Damian",
        "Beata", "Przemysław", "Ola", "Norbert", "Renata", "Emil", "Wiktoria", "Marek",
        "Zuzanna", "Daniel"
    };

    private static readonly string[] LastNames = {
        "Nowak", "Kowalski", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamiński", "Lewandowski",
        "Zieliński", "Szymański", "Woźniak", "Dąbrowski", "Kozłowski", "Jankowski", "Mazur",
        "Wojciechowski", "Kwiatkowski", "Krawczyk", "Piotrowski", "Grabowski", "Zając",
        "Pawłowski", "Michalski", "Król", "Wieczorek", "Jabłoński", "Wróbel", "Nowakowski",
        "Majewski", "Olszewski", "Stępień", "Malinowski", "Jaworski", "Adamczyk", "Dudek",
        "Pawlak", "Górski", "Sikora", "Walczak", "Baran", "Rutkowski", "Michalak", "Szewczyk",
        "Ostrowski", "Tomczyk", "Pietrzak", "Wróblewski", "Zalewski", "Marciniak", "Jasiński", "Bąk"
    };

    private static async Task<List<(ApplicationUser User, DateTime HireDate)>> CreateEmployeesAsync(
        UserManager<ApplicationUser> userManager,
        List<Department> departments,
        Random random)
    {
        var employees = new List<(ApplicationUser, DateTime)>();
        var today = DateTime.Today;
        var twoYearsAgo = today.AddYears(-2);

        for (int i = 0; i < 50; i++)
        {
            var firstName = FirstNames[i % FirstNames.Length];
            var lastName = LastNames[i % LastNames.Length];
            
            var suffix = i >= FirstNames.Length ? (i / FirstNames.Length + 1).ToString() : "";
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}{suffix}@company.pl";

            var department = departments[random.Next(departments.Count)];
            
            var maxDaysBack = (today - twoYearsAgo).Days;
            var minDaysBack = 180;
            var daysBack = random.Next(minDaysBack, maxDaysBack);
            var hireDate = today.AddDays(-daysBack);

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName + suffix,
                EmailConfirmed = true,
                IsActive = random.NextDouble() > 0.05,
                DepartmentId = department.Id
            };

            var result = await userManager.CreateAsync(user, "Employee123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, RoleEmployee);
                employees.Add((user, hireDate));
            }
        }

        return employees;
    }

    private static List<WorkShift> CreateShiftsForEmployees(
        List<(ApplicationUser User, DateTime HireDate)> employees,
        List<Department> departments,
        Random random)
    {
        var shifts = new List<WorkShift>();
        var today = DateTime.Today;

        foreach (var (user, hireDate) in employees)
        {
            var department = departments.FirstOrDefault(d => d.Id == user.DepartmentId);
            var hourlyRate = department?.HourlyRate ?? 50m;

            var currentDate = hireDate;
            var endDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);

            while (currentDate <= endDate)
            {
                if ((currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                    && random.NextDouble() < 0.8)
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                if (random.NextDouble() < 0.7)
                {
                    var shiftType = random.Next(4);
                    DateTime startTime, endTime;

                    switch (shiftType)
                    {
                        case 0:
                            startTime = currentDate.AddHours(6);
                            endTime = currentDate.AddHours(14);
                            break;
                        case 1:
                            startTime = currentDate.AddHours(8);
                            endTime = currentDate.AddHours(16);
                            break;
                        case 2:
                            startTime = currentDate.AddHours(14);
                            endTime = currentDate.AddHours(22);
                            break;
                        default:
                            startTime = currentDate.AddHours(22);
                            endTime = currentDate.AddDays(1).AddHours(6);
                            break;
                    }

                    if (random.NextDouble() < 0.15)
                    {
                        endTime = endTime.AddHours(random.Next(1, 3));
                    }

                    shifts.Add(new WorkShift
                    {
                        UserId = user.Id,
                        StartTime = startTime,
                        EndTime = endTime,
                        HourlyRateSnapshot = hourlyRate
                    });
                }

                currentDate = currentDate.AddDays(1);
            }
        }

        return shifts;
    }

    private static List<Bonus> CreateBonusesForEmployees(
        List<(ApplicationUser User, DateTime HireDate)> employees,
        Random random)
    {
        var bonuses = new List<Bonus>();
        var today = DateTime.Today;

        var bonusReasons = new[]
        {
            "Outstanding performance this quarter",
            "Successful project completion",
            "Employee of the month",
            "Going above and beyond",
            "Exceptional customer service",
            "Innovation award",
            "Team collaboration excellence",
            "Meeting annual targets",
            "Leadership recognition",
            "Special achievement award"
        };

        foreach (var (user, hireDate) in employees)
        {
            if (random.NextDouble() > 0.6) continue;

            var bonusCount = random.Next(1, 5);
            
            for (int i = 0; i < bonusCount; i++)
            {
                var daysSinceHire = (today - hireDate).Days;
                if (daysSinceHire <= 30) continue;

                var bonusDate = hireDate.AddDays(random.Next(30, Math.Max(31, daysSinceHire)));
                
                if (random.NextDouble() < 0.1)
                {
                    bonusDate = today.AddDays(random.Next(1, 60));
                }

                bonuses.Add(new Bonus
                {
                    UserId = user.Id,
                    Amount = random.Next(3, 51) * 100,
                    DateGranted = bonusDate,
                    Reason = bonusReasons[random.Next(bonusReasons.Length)]
                });
            }
        }

        return bonuses;
    }

    private static List<LeaveRequest> CreateRequestsForEmployees(
        List<(ApplicationUser User, DateTime HireDate)> employees,
        List<Department> departments,
        Random random)
    {
        var requests = new List<LeaveRequest>();
        var today = DateTime.Today;

        foreach (var (user, hireDate) in employees)
        {
            if (random.NextDouble() > 0.7) continue;

            var requestCount = random.Next(1, 6);

            for (int i = 0; i < requestCount; i++)
            {
                var daysSinceHire = (today - hireDate).Days;
                if (daysSinceHire < 14) continue;

                var requestDate = hireDate.AddDays(random.Next(14, Math.Max(15, daysSinceHire)));
                var createdAt = requestDate.AddDays(-random.Next(1, 14));

                var isVacation = random.NextDouble() < 0.8;

                if (isVacation)
                {
                    var vacationLength = random.Next(1, 15);
                    var startDate = requestDate;
                    var endDate = startDate.AddDays(vacationLength);

                    RequestStatus status;
                    string? adminComment = null;

                    if (endDate < today.AddDays(-7))
                    {
                        status = random.NextDouble() < 0.85 ? RequestStatus.Approved : RequestStatus.Rejected;
                        if (status == RequestStatus.Rejected)
                        {
                            adminComment = GetRandomRejectionReason(random);
                        }
                    }
                    else if (startDate > today.AddDays(7))
                    {
                        status = random.NextDouble() < 0.7 ? RequestStatus.Pending : RequestStatus.Approved;
                    }
                    else
                    {
                        var roll = random.NextDouble();
                        status = roll < 0.3 ? RequestStatus.Pending : 
                                 roll < 0.9 ? RequestStatus.Approved : RequestStatus.Rejected;
                        if (status == RequestStatus.Rejected)
                        {
                            adminComment = GetRandomRejectionReason(random);
                        }
                    }

                    requests.Add(new LeaveRequest
                    {
                        UserId = user.Id,
                        Type = RequestType.Vacation,
                        Status = status,
                        StartDate = startDate,
                        EndDate = endDate,
                        CreatedAt = createdAt,
                        AdminComment = adminComment
                    });
                }
                else
                {
                    var otherDepartments = departments.Where(d => d.Id != user.DepartmentId).ToList();
                    if (!otherDepartments.Any()) continue;

                    var targetDept = otherDepartments[random.Next(otherDepartments.Count)];
                    
                    var status = random.NextDouble() < 0.5 ? RequestStatus.Pending :
                                 random.NextDouble() < 0.7 ? RequestStatus.Approved : RequestStatus.Rejected;

                    string? adminComment = null;
                    if (status == RequestStatus.Rejected)
                    {
                        adminComment = "Transfer not possible at this time due to staffing requirements.";
                    }
                    else if (status == RequestStatus.Approved)
                    {
                        adminComment = "Transfer approved. Please coordinate with your new manager.";
                    }

                    requests.Add(new LeaveRequest
                    {
                        UserId = user.Id,
                        Type = RequestType.DepartmentChange,
                        Status = status,
                        StartDate = requestDate,
                        EndDate = requestDate,
                        CreatedAt = createdAt,
                        TargetDepartmentId = targetDept.Id,
                        AdminComment = adminComment
                    });
                }
            }
        }

        return requests;
    }

    private static string GetRandomRejectionReason(Random random)
    {
        var reasons = new[]
        {
            "Insufficient coverage during requested period.",
            "Critical project deadline conflicts with requested dates.",
            "Too many team members already on leave.",
            "Please resubmit with adjusted dates.",
            "Business needs require your presence during this period."
        };
        return reasons[random.Next(reasons.Length)];
    }

    // ============================================================================
    // END OF DEVELOPMENT HELPER METHODS
    // ============================================================================

    private static async Task AssignEmployeeRoleToUsersWithoutRole(UserManager<ApplicationUser> userManager)
    {
        var allUsers = userManager.Users.ToList();
        
        foreach (var user in allUsers)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                await userManager.AddToRoleAsync(user, RoleEmployee);
            }
        }
    }

    private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task CreateAdminUserIfNotExists(UserManager<ApplicationUser> userManager)
    {
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FirstName = "Admin",
                LastName = "System",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, AdminPassword);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleAdmin);
            }
        }
    }
}
