using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;


namespace GymmanagmentSystem
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GymDbcontext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            try
            {
                await SeedPlansAsync(context, logger);
                await SeedCategoriesAsync(context, logger);
                await SeedTrainersAsync(context, logger);
                await SeedMembersAsync(context, logger);
                await SeedWeightProgressRecordsAsync(context, logger);
                await SeedRolesAsync(roleManager, logger);
                await SeedAdminUserAsync(userManager, logger);
                await SeedManagerUsersAsync(userManager, logger);
                await SeedUpcomingSessionsAsync(context, logger); // must be LAST — needs Trainers + Categories
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roles = { "Admin", "Manager", "Member", "Trainer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Created role: {Role}", role);
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            var adminEmail = "admin@gymmanagement.com";
            var existing = await userManager.FindByEmailAsync(adminEmail);

            if (existing != null)
            {
                // Reset password every startup to ensure it's correct
                var token = await userManager.GeneratePasswordResetTokenAsync(existing);
                var resetResult = await userManager.ResetPasswordAsync(existing, token, "Admin@1234");

                if (resetResult.Succeeded)
                    logger.LogInformation("Admin password reset successfully.");
                else
                    logger.LogError("Failed to reset admin password: {Errors}",
                        string.Join(", ", resetResult.Errors.Select(e => e.Description)));

                // Ensure admin has Admin role
                if (!await userManager.IsInRoleAsync(existing, "Admin"))
                {
                    await userManager.AddToRoleAsync(existing, "Admin");
                    logger.LogInformation("Admin role assigned to existing user.");
                }

                return;
            }

            var admin = new AppUser
            {
                FullName = "System Admin",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Admin user created: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        private static async Task SeedManagerUsersAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            var managers = new[]
            {
                new { FullName = "Operations Manager", Email = "manager1@gymmanagement.com", Password = "Manager@1234" },
                new { FullName = "Front Desk Manager", Email = "manager2@gymmanagement.com", Password = "Manager@1234" }
            };

            foreach (var managerInfo in managers)
            {
                var existing = await userManager.FindByEmailAsync(managerInfo.Email);

                if (existing != null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(existing);
                    var resetResult = await userManager.ResetPasswordAsync(existing, token, managerInfo.Password);

                    if (resetResult.Succeeded)
                        logger.LogInformation("Manager password reset successfully for {Email}.", managerInfo.Email);
                    else
                        logger.LogError("Failed to reset manager password for {Email}: {Errors}",
                            managerInfo.Email,
                            string.Join(", ", resetResult.Errors.Select(e => e.Description)));

                    if (!await userManager.IsInRoleAsync(existing, "Manager"))
                    {
                        await userManager.AddToRoleAsync(existing, "Manager");
                        logger.LogInformation("Manager role assigned to existing user: {Email}", managerInfo.Email);
                    }

                    continue;
                }

                var manager = new AppUser
                {
                    FullName = managerInfo.FullName,
                    UserName = managerInfo.Email,
                    Email = managerInfo.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(manager, managerInfo.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Manager");
                    logger.LogInformation("Manager user created: {Email}", managerInfo.Email);
                }
                else
                {
                    logger.LogError("Failed to create manager user {Email}: {Errors}",
                        managerInfo.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private static async Task SeedPlansAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Plans.Any())
            {
                logger.LogInformation("Plans already seeded — skipping.");
                return;
            }

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "plans.json");
            if (!File.Exists(jsonPath))
            {
                logger.LogWarning("plans.json not found at {Path}", jsonPath);
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            var planDtos = JsonSerializer.Deserialize<List<PlanSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (planDtos == null || !planDtos.Any())
            {
                logger.LogWarning("No plans found in plans.json");
                return;
            }

            var plans = planDtos.Select(p => new Plans
            {
                Name = p.Name,
                Description = p.Description,
                DurationInDays = p.DurationDays,
                Price = p.Price,
                IsActive = p.IsActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            await context.Plans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} plans.", plans.Count);
        }

        private static async Task SeedCategoriesAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Category.Any())
            {
                logger.LogInformation("Categories already seeded — skipping.");
                return;
            }

            var categories = new List<Category>
            {
                new() { CategoryName = Categories.Cardio,   CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { CategoryName = Categories.Strength, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { CategoryName = Categories.Training, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { CategoryName = Categories.Yoga,     CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };

            await context.Category.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} categories.", categories.Count);
        }

        private static async Task SeedUpcomingSessionsAsync(GymDbcontext context, ILogger logger)
        {
            // Count upcoming sessions
            var upcomingCount = context.Session
                .Count(s => s.StartDate > DateTime.Now);

            if (upcomingCount >= 7)
            {
                logger.LogInformation("7 upcoming sessions already exist — skipping.");
                return;
            }

            var trainers = context.Trainer.ToList();
            var categories = context.Category.ToList();

            if (!trainers.Any() || !categories.Any())
            {
                logger.LogWarning("No trainers or categories found — skipping session seeding.");
                return;
            }

            // Mix of session times
            var sessionTimes = new[]
            {
        (hour: 7,  duration: 1),  // 7 AM - 8 AM
        (hour: 9,  duration: 2),  // 9 AM - 11 AM
        (hour: 11, duration: 1),  // 11 AM - 12 PM
        (hour: 14, duration: 2),  // 2 PM - 4 PM
        (hour: 16, duration: 1),  // 4 PM - 5 PM
        (hour: 18, duration: 2),  // 6 PM - 8 PM
        (hour: 20, duration: 1),  // 8 PM - 9 PM
    };

            var random = new Random();
            var sessionsToAdd = new List<Session>();
            var startDate = DateTime.Now.Date.AddDays(1); // Start from tomorrow

            for (int i = 0; i < 7; i++)
            {
                // Pick random category
                var category = categories[random.Next(categories.Count)];

                // Find trainers that match this category via specialty mapping
                var matchingTrainers = trainers.Where(t =>
                    GetMatchingCategory(t.Specialty) == category.CategoryName).ToList();

                // Fallback to any trainer if no matching trainer found
                var trainer = matchingTrainers.Any()
                    ? matchingTrainers[random.Next(matchingTrainers.Count)]
                    : trainers[random.Next(trainers.Count)];

                var time = sessionTimes[i];
                var sessionDate = startDate.AddDays(i);
                var sessionStart = sessionDate.AddHours(time.hour);
                var sessionEnd = sessionStart.AddHours(time.duration);

                sessionsToAdd.Add(new Session
                {
                    Description = $"{category.CategoryName} training session",
                    Capacity = random.Next(10, 26), // 10 to 25 slots
                    StartDate = sessionStart,
                    EndDate = sessionEnd,
                    TrainerId = trainer.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await context.Session.AddRangeAsync(sessionsToAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} upcoming sessions.", sessionsToAdd.Count);
        }

        private static Categories GetMatchingCategory(Specialty specialty)
        {
            return specialty switch
            {
                Specialty.GeneralFitness => Categories.Training,
                Specialty.Yoga => Categories.Yoga,
                Specialty.Boxing => Categories.Cardio,
                Specialty.CrossFit => Categories.Strength,
                _ => Categories.Training
            };
        }
        private static async Task SeedTrainersAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Trainer.Any())
            {
                logger.LogInformation("Trainers already seeded — skipping.");
                return;
            }

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "trainers.json");
            if (!File.Exists(jsonPath))
            {
                logger.LogWarning("trainers.json not found at {Path}", jsonPath);
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            var trainerDtos = JsonSerializer.Deserialize<List<TrainerSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (trainerDtos == null || !trainerDtos.Any())
            {
                logger.LogWarning("No trainers found in trainers.json");
                return;
            }

            var trainers = trainerDtos.Select(t => new Trainer
            {
                Name = t.Name,
                Email = t.Email,
                Phone = t.Phone,
                DateOFBirth = DateOnly.Parse(t.DateOfBirth),
                Gender = (Gender)t.Gender,
                Specialty = (Specialty)t.Specialty,
                Address = new Address
                {
                    BuildingNumber = t.BuildingNumber,
                    Street = t.Street,
                    City = t.City
                },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            await context.Trainer.AddRangeAsync(trainers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} trainers.", trainers.Count);
        }
        private static async Task SeedMembersAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Member.Any())
            {
                logger.LogInformation("Members already seeded — skipping.");
                return;
            }

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "members.json");
            if (!File.Exists(jsonPath))
            {
                logger.LogWarning("members.json not found at {Path}", jsonPath);
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            var memberDtos = JsonSerializer.Deserialize<List<MemberSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (memberDtos == null || !memberDtos.Any())
            {
                logger.LogWarning("No members found in members.json");
                return;
            }

            var maleAvatars = new[] { "/images/avatars/male-avatar-1.png", "/images/avatars/male-avatar-2.png" };
            var femaleAvatar = "/images/avatars/female-avatar-1.png";
            int maleIndex = 0;

            var members = memberDtos.Select(m =>
            {
                var gender = (Gender)m.Gender;
                string photo;

                if (gender == Gender.Male)
                {
                    photo = maleAvatars[maleIndex % maleAvatars.Length];
                    maleIndex++;
                }
                else
                {
                    photo = femaleAvatar;
                }

                return new Member
                {
                    Name = m.Name,
                    Email = m.Email,
                    Phone = m.Phone,
                    DateOFBirth = DateOnly.Parse(m.DateOfBirth),
                    Gender = gender,
                    Photo = photo,
                    Address = new Address
                    {
                        BuildingNumber = m.BuildingNumber,
                        Street = m.Street,
                        City = m.City
                    },
                    HealthRecord = new HealthRecord
                    {
                        Height = m.Height,
                        Weight = m.Weight,
                        BloodType = m.BloodType,
                        Note = m.Note,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
            }).ToList();

            await context.Member.AddRangeAsync(members);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} members.", members.Count);
        }

        private static async Task SeedWeightProgressRecordsAsync(GymDbcontext context, ILogger logger)
        {
            if (context.WeightProgressRecords.Any())
            {
                logger.LogInformation("Weight progress records already seeded — skipping.");
                return;
            }

            var healthRecords = context.HealthRecord.ToList();
            if (!healthRecords.Any())
            {
                logger.LogWarning("No health records found — skipping weight progress seeding.");
                return;
            }

            var progressRecords = healthRecords.Select(record => new WeightProgressRecord
            {
                MemberId = record.MemberId,
                Weight = record.Weight,
                RecordedAt = record.CreatedAt == default ? DateTime.Now : record.CreatedAt,
                Note = "Imported from current health record",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            await context.WeightProgressRecords.AddRangeAsync(progressRecords);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} weight progress records.", progressRecords.Count);
        }

        public class MemberSeedDto
        {
            public string Name { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Phone { get; set; } = default!;
            public string DateOfBirth { get; set; } = default!;
            public int Gender { get; set; }
            public int BuildingNumber { get; set; }
            public string Street { get; set; } = default!;
            public string City { get; set; } = default!;
            public decimal Height { get; set; }
            public decimal Weight { get; set; }
            public string BloodType { get; set; } = default!;
            public string? Note { get; set; }
        }

        public class PlanSeedDto
        {
            public string Name { get; set; } = default!;
            public string Description { get; set; } = default!;
            public int DurationDays { get; set; }
            public decimal Price { get; set; }
            public bool IsActive { get; set; }
        }
        public class TrainerSeedDto
        {
            public string Name { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Phone { get; set; } = default!;
            public string DateOfBirth { get; set; } = default!;
            public int Gender { get; set; }
            public int Specialty { get; set; }
            public int BuildingNumber { get; set; }
            public string Street { get; set; } = default!;
            public string City { get; set; } = default!;
        }
    }
}
