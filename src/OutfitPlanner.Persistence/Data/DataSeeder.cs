using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Domain.ValueObjects;
using OutfitPlanner.Persistence.Repositories;

namespace OutfitPlanner.Persistence.Data;

/// <summary>
/// Seeds initial data into the database for development and testing
/// </summary>
public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database if it hasn't been seeded yet
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Apply pending migrations
            await _context.Database.MigrateAsync();

            _logger.LogInformation("Starting database seeding...");

            // Seed sample users only if none exist
            if (!await _userManager.Users.AnyAsync())
            {
                await SeedUsersAsync();
            }
            else
            {
                _logger.LogInformation("Users already exist, skipping user seeding.");
            }

            // Other seed methods perform their own existence checks
            await SeedClothingItemsAsync();
            await SeedOutfitsAsync();
            await SeedPollsAsync();
            await SeedTrendingOutfitsAsync();
            await SeedNotificationsAsync();
            await SeedStyleProfilesAsync();

            _logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        // Check if there are any users
        if (await _userManager.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist, skipping user seeding.");
            return;
        }

        var users = new List<User>
        {
            new User { UserName = "StyleMaven92", Email = "stylemaven92@example.com", Name = "Style Maven 92" },
            new User { UserName = "Fashionista_A", Email = "alex@example.com", Name = "Alex Fashion" },
            new User { UserName = "ChicExplorer", Email = "chic@example.com", Name = "Chic Explorer" },
            new User { UserName = "TrendSetter", Email = "trend@example.com", Name = "Trend Setter" },
            new User { UserName = "UrbanVibes", Email = "urban@example.com", Name = "Urban Vibes" }
        };

        var password = "Password123!";

        foreach (var user in users)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Created user: {UserName}", user.UserName);
            }
            else
            {
                _logger.LogError("Failed to create user {UserName}: {Errors}", user.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        _logger.LogInformation("Seeded {Count} users", users.Count);
    }

    private async Task SeedClothingItemsAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        if (users.Count == 0)
        {
            _logger.LogWarning("No users found. Skipping clothing items seeding.");
            return;
        }

        // Check if any user already has clothing items
        var anyClothingItems = await _context.ClothingItems.AnyAsync();
        if (anyClothingItems)
        {
            _logger.LogInformation("Clothing items already exist, skipping clothing items seeding.");
            return;
        }

        var clothingItems = new List<ClothingItem>();
        var random = new Random(42); // Fixed seed for reproducibility

        foreach (var user in users)
        {
            // Create 2 tops, 2 bottoms, 2 footwear, 1 outerwear for each user
            var categories = new[] { "Top", "Top", "Bottom", "Bottom", "Footwear", "Footwear", "Outerwear" };
            foreach (var category in categories)
            {
                var clothingItem = new ClothingItem
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Name = $"{category} {random.Next(1, 10)}",
                    Category = category,
                    Type = ClothingType.Top, // Default type based on category
                    PrimaryColor = GetRandomColor(random),
                    Brand = $"Brand {random.Next(1, 5)}",
                    Size = GetRandomSize(random),
                    Fabric = FabricType.Cotton, // Default fabric
                    PurchaseDate = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                    PurchasePrice = Money.From((decimal)(random.NextDouble() * 100 + 10), "USD"),
                    ImageUrl = $"/uploads/clothing-images/{category.ToLower()}-{random.Next(1, 100)}.jpg",
                    Condition = "good",
                    IsActive = true
                };

                // Set Type based on category
                clothingItem.Type = category switch
                {
                    "Top" => ClothingType.Top,
                    "Bottom" => ClothingType.Bottom,
                    "Footwear" => ClothingType.Footwear,
                    "Outerwear" => ClothingType.Outerwear,
                    _ => ClothingType.Accessory
                };

                // Set random fabric
                var fabrics = new[] { FabricType.Cotton, FabricType.Polyester, FabricType.Wool, FabricType.Silk, FabricType.Denim };
                clothingItem.Fabric = fabrics[random.Next(fabrics.Length)];

                clothingItems.Add(clothingItem);
            }
        }

        await _context.ClothingItems.AddRangeAsync(clothingItems);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} clothing items", clothingItems.Count);
    }

    private async Task SeedOutfitsAsync()
    {
        // Check if outfits already exist
        if (await _context.Outfits.AnyAsync())
        {
            _logger.LogInformation("Outfits already exist, skipping outfits seeding.");
            return;
        }

        var users = await _userManager.Users.Take(3).ToListAsync();
        if (users.Count == 0) return;

        var clothingItems = await _context.ClothingItems
            .Where(c => users.Select(u => u.Id).Contains(c.UserId))
            .Take(20)
            .ToListAsync();

        if (clothingItems.Count < 4)
        {
            _logger.LogWarning("Not enough clothing items to create sample outfits. Please add more clothing items first.");
            return;
        }

        var outfits = new List<Outfit>();
        var random = new Random(42); // Fixed seed for reproducibility

        // Group clothing by category
        var tops = clothingItems.Where(c => c.Category == "Top").ToList();
        var bottoms = clothingItems.Where(c => c.Category == "Bottom").ToList();
        var footwear = clothingItems.Where(c => c.Category == "Footwear").ToList();
        var outerwear = clothingItems.Where(c => c.Category == "Outerwear").ToList();

        // Create 8 sample outfits (one for each occasion type)
        for (int i = 0; i < 8; i++)
        {
            var user = users[i % users.Count];
            
            var outfit = new Outfit
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = GetOutfitName(i),
                Occasion = (OccasionType)(i % 8),
                Season = (Season)(i % 5),
                WeatherCondition = GetWeatherCondition(i),
                ComfortRating = random.Next(3, 6),
                StyleRating = random.Next(3, 6),
                TimesWorn = random.Next(0, 10),
                Status = OutfitStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 30)),
                ImageUrl = $"/uploads/outfit-images/outfit-seed-{i + 1}.jpg"
            };

            // Add items to outfit
            var outfitItems = new List<OutfitItem>();

            // Add a top
            if (tops.Count > 0)
            {
                outfitItems.Add(new OutfitItem
                {
                    Id = Guid.NewGuid(),
                    OutfitId = outfit.Id,
                    ClothingItemId = tops[random.Next(tops.Count)].Id,
                    Role = ItemRole.Primary,
                    LayeringOrder = 1,
                    IsEssential = true,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            // Add bottom
            if (bottoms.Count > 0)
            {
                outfitItems.Add(new OutfitItem
                {
                    Id = Guid.NewGuid(),
                    OutfitId = outfit.Id,
                    ClothingItemId = bottoms[random.Next(bottoms.Count)].Id,
                    Role = ItemRole.Secondary,
                    LayeringOrder = 2,
                    IsEssential = true,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            // Add footwear
            if (footwear.Count > 0)
            {
                outfitItems.Add(new OutfitItem
                {
                    Id = Guid.NewGuid(),
                    OutfitId = outfit.Id,
                    ClothingItemId = footwear[random.Next(footwear.Count)].Id,
                    Role = ItemRole.Accent,
                    LayeringOrder = 3,
                    IsEssential = true,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            // Add outerwear sometimes
            if (outerwear.Count > 0 && i % 2 == 0)
            {
                outfitItems.Add(new OutfitItem
                {
                    Id = Guid.NewGuid(),
                    OutfitId = outfit.Id,
                    ClothingItemId = outerwear[random.Next(outerwear.Count)].Id,
                    Role = ItemRole.Secondary,
                    LayeringOrder = 0,
                    IsEssential = false,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            outfit.Items = outfitItems;
            outfits.Add(outfit);
        }

        await _context.Outfits.AddRangeAsync(outfits);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} sample outfits", outfits.Count);
    }

    private async Task SeedPollsAsync()
    {
        // Check if polls already exist
        if (await _context.ValidationPolls.AnyAsync())
        {
            _logger.LogInformation("Validation polls already exist, skipping polls seeding.");
            return;
        }

        var users = await _userManager.Users.Take(3).ToListAsync();
        if (users.Count == 0) return;

        var outfits = await _context.Outfits.Take(6).ToListAsync();
        if (outfits.Count < 2) return;

        var polls = new List<ValidationPoll>();
        var random = new Random(42);

        var pollQuestions = new[]
        {
            "Help me choose for the Weekend Brunch!",
            "Which outfit should I wear for a job interview?",
            "Best outfit for a casual Friday?",
            "What should I wear for a dinner date?",
            "What to wear for a virtual meeting?"
        };

        for (int i = 0; i < pollQuestions.Length; i++)
        {
            var user = users[i % users.Count];
            
            var poll = new ValidationPoll
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Question = pollQuestions[i],
                Context = "{\"occasion\": \"casual\", \"weather\": \"sunny\"}",
                Status = i < 3 ? PollStatus.Active : PollStatus.Closed,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(random.Next(1, 7)),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 10))
            };

            // Add 2-3 options for each poll
            var optionCount = random.Next(2, 4);
            var pollOptions = new List<PollOption>();

            for (int j = 0; j < optionCount && j < outfits.Count; j++)
            {
                var option = new PollOption
                {
                    Id = Guid.NewGuid(),
                    PollId = poll.Id,
                    OutfitId = outfits[(i + j) % outfits.Count].Id,
                    Description = $"Option {j + 1}",
                    DisplayOrder = j,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                // Add some votes (unique per user per option)
                var availableUsers = users.OrderBy(x => random.Next()).ToList();
                var voteCount = Math.Min(random.Next(1, 4), availableUsers.Count);
                
                for (int v = 0; v < voteCount; v++)
                {
                    option.Votes.Add(new Vote
                    {
                        Id = Guid.NewGuid(),
                        PollId = poll.Id,
                        OptionId = option.Id,
                        VoterId = availableUsers[v].Id,
                        Rating = random.Next(1, 6),
                        IsAnonymous = random.Next(2) == 0,
                        CreatedAt = DateTimeOffset.UtcNow.AddHours(-random.Next(1, 48))
                    });
                }

                pollOptions.Add(option);
            }

            poll.Options = pollOptions;
            polls.Add(poll);
        }

        await _context.ValidationPolls.AddRangeAsync(polls);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} validation polls", polls.Count);
    }

    private async Task SeedTrendingOutfitsAsync()
    {
        // Check if trending outfits already exist
        if (await _context.TrendingOutfits.AnyAsync())
        {
            _logger.LogInformation("Trending outfits already exist, skipping trending outfits seeding.");
            return;
        }

        var outfits = await _context.Outfits.Take(10).ToListAsync();
        var polls = await _context.ValidationPolls.Take(5).ToListAsync();

        if (outfits.Count == 0) return;

        var trendingOutfits = new List<TrendingOutfit>();
        var random = new Random(42);

        for (int i = 0; i < Math.Min(outfits.Count, 10); i++)
        {
            var poll = polls.ElementAtOrDefault(i % polls.Count);
            
            trendingOutfits.Add(new TrendingOutfit
            {
                Id = Guid.NewGuid(),
                OutfitId = outfits[i].Id,
                PollId = poll?.Id ?? Guid.Empty,
                VoteCount = random.Next(10, 200),
                ReactionCount = random.Next(5, 100),
                TrendingScore = random.Next(50, 100) / 10.0m,
                RankPosition = i + 1,
                Date = DateTime.UtcNow.Date,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _context.TrendingOutfits.AddRangeAsync(trendingOutfits);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} trending outfits", trendingOutfits.Count);
    }

    /// <summary>
    /// Seeds sample notifications for a specific user
    /// </summary>
   public async Task SeedNotificationsAsync(string? userId = null)
    {
        // If no user ID provided, try to get the first user
        if (string.IsNullOrEmpty(userId))   
        {
            var firstUser = await _userManager.Users.FirstOrDefaultAsync();
            if (firstUser == null)
            {
                _logger.LogWarning("No users found for notifications seeding");
                return;
            }
            userId = firstUser.Id;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for notifications seeding", userId);
            return;
        }

        // Check if notifications already exist for this user
        if (await _context.Notifications.AnyAsync(n => n.UserId == userId))
        {
            _logger.LogInformation("Notifications already exist for user {UserId}, skipping...", userId);
            return;
        }

        var notifications = new List<Notification>
        {
            // Today's notifications (unread)
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Reminder,
                Title = "Log your outfit for today",
                Message = "You scheduled \"Weekend Casual\" for today. Did you wear it?",
                ActionUrl = "/calendar",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Social,
                Title = "New like on your outfit",
                Message = "Emma W. liked your \"Office Chic\" outfit combination.",
                ActionUrl = "/outfits/1",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.System,
                Title = "Weekly Style Report Ready",
                Message = "Your style stats for last week are now available. You wore Blue Jeans 4 times!",
                ActionUrl = "/profile/stats",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            },

            // Yesterday's notifications (read and unread)
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Weather,
                Title = "Rain Forecast Alert",
                Message = "Rain is expected tomorrow. Don't forget your raincoat or umbrella!",
                ActionUrl = "/weather",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Social,
                Title = "Comment on \"Summer Dress\"",
                Message = "Sophie commented: \"Love this color on you! Where did you get it?\"",
                ActionUrl = "/outfits/2",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-3)
            },

            // Last week notifications (read)
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.System,
                Title = "Account Security",
                Message = "New login detected from Chrome on MacOS.",
                ActionUrl = "/settings/security",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Reminder,
                Title = "Outfit Reminder",
                Message = "You have \"Business Meeting\" scheduled for tomorrow at 2:00 PM",
                ActionUrl = "/calendar",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Social,
                Title = "New Follower",
                Message = "Michael T. started following you.",
                ActionUrl = "/community",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.System,
                Title = "Wear Count Update",
                Message = "You've worn your \"Blue Denim Jacket\" 10 times this month!",
                ActionUrl = "/wardrobe/1",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.Weather,
                Title = "Cold Weather Alert",
                Message = "Temperature dropping to 5°C tomorrow. Time to bundle up!",
                ActionUrl = "/weather",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} notifications for user {UserId}", notifications.Count, userId);
    }

    /// <summary>
    /// Seeds style profiles and custom rules for users
    /// </summary>
    private async Task SeedStyleProfilesAsync()
    {
        var users = await _userManager.Users.Take(3).ToListAsync();
        if (users.Count == 0)
        {
            _logger.LogWarning("No users found. Skipping style profiles seeding.");
            return;
        }

        // Check if style profiles already exist
        if (await _context.UserStyleProfiles.AnyAsync())
        {
            _logger.LogInformation("Style profiles already exist, skipping style profiles seeding.");
            return;
        }

        var random = new Random(42);
        var styles = new[] 
        { 
            StylePreference.Classic, 
            StylePreference.Streetwear, 
            StylePreference.Bohemian 
        };
        var fitPreferences = new[] { "Slim", "Regular", "Relaxed", "Oversized" };
        var colorPalettes = new[]
        {
            new[] { "Black", "White", "Gray", "Navy" },
            new[] { "Blue", "White", "Beige", "Brown" },
            new[] { "Earth Tones", "Olive", "Tan", "Rust" }
        };

        var styleProfiles = new List<UserStyleProfile>();

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            var styleProfile = new UserStyleProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Style = styles[i % styles.Length],
                PreferredColors = colorPalettes[i % colorPalettes.Length].ToList(),
                FitPreferences = fitPreferences[random.Next(fitPreferences.Length)],
                ComfortPriority = random.Next(50, 100),
                AcceptsTrends = random.Next(2) == 0
            };

            // Add some custom rules
            var ruleCount = random.Next(2, 4);
            for (int j = 0; j < ruleCount; j++)
            {
                var rule = new StyleRule
                {
                    Id = Guid.NewGuid(),
                    UserStyleProfileId = styleProfile.Id,
                    Name = GetRuleName(j),
                    Description = GetRuleDescription(j),
                    IsActive = true,
                    CriteriaJson = GetRuleCriteria(j)
                };
                styleProfile.CustomRules.Add(rule);
            }

            styleProfiles.Add(styleProfile);
        }

        await _context.UserStyleProfiles.AddRangeAsync(styleProfiles);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} style profiles with custom rules", styleProfiles.Count);
    }

    private static string GetRuleName(int index)
    {
        return index switch
        {
            0 => "No Black with Brown",
            1 => "Monochrome Days",
            2 => "Mix Metals",
            3 => "Casual Friday",
            _ => $"Style Rule {index + 1}"
        };
    }

    private static string GetRuleDescription(int index)
    {
        return index switch
        {
            0 => "Avoid wearing black items with brown items",
            1 => "Stick to one color family for outfit cohesion",
            2 => "Mix gold and silver accessories freely",
            3 => "Business casual is allowed on Fridays",
            _ => "Custom style rule"
        };
    }

    private static string GetRuleCriteria(int index)
    {
        return index switch
        {
            0 => "{\"colors\": [\"Black\", \"Brown\"], \"operator\": \"not_both\"}",
            1 => "{\"colors\": [\"same_family\"], \"operator\": \"monochrome\"}",
            2 => "{\"accessories\": [\"gold\", \"silver\"], \"operator\": \"mix\"}",
            3 => "{\"dressCode\": \"business_casual\", \"day\": \"friday\"}",
            _ => "{}"
        };
    }

    private static string GetOutfitName(int index)
    {
        return index switch
        {
            0 => "Casual Friday Look",
            1 => "Weekend Warrior",
            2 => "Date Night Style",
            3 => "Office Professional",
            4 => "Weekend Brunch",
            5 => "Business Meeting",
            6 => "Travel Comfort",
            7 => "Social Gathering",
            _ => $"Outfit {index + 1}"
        };
    }

    private static string GetWeatherCondition(int index)
    {
        return index switch
        {
            0 => "Sunny",
            1 => "Cloudy",
            2 => "Rainy",
            3 => "Windy",
            _ => "Clear"
        };
    }

    private static string GetRandomColor(Random random)
    {
        var colors = new[] { "Red", "Blue", "Green", "Black", "White", "Yellow", "Purple", "Pink", "Orange", "Gray" };
        return colors[random.Next(colors.Length)];
    }

    private static string GetRandomSize(Random random)
    {
        var sizes = new[] { "XS", "S", "M", "L", "XL", "XXL" };
        return sizes[random.Next(sizes.Length)];
    }
}
