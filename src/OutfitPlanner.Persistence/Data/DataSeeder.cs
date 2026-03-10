using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
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
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if already seeded
            if (await _context.Outfits.AnyAsync())
            {
                _logger.LogInformation("Database already seeded, skipping...");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // Seed sample users
            await SeedUsersAsync();

            // Seed sample outfits with images
            await SeedOutfitsAsync();

            // Seed validation polls
            await SeedPollsAsync();

            // Seed trending outfits
            await SeedTrendingOutfitsAsync();

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
        // Get existing users from Identity
        var users = await _userManager.Users.Take(5).ToListAsync();
        
        if (users.Count == 0)
        {
            _logger.LogWarning("No users found. Please register a user first.");
            return;
        }

        _logger.LogInformation("Found {Count} users in the system", users.Count);
    }

    private async Task SeedOutfitsAsync()
    {
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

        // Create 5 sample outfits
        for (int i = 0; i < 5; i++)
        {
            var user = users[i % users.Count];
            
            var outfit = new Outfit
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = GetOutfitName(i),
                Occasion = (OccasionType)(i % 5),
                Season = (Season)(i % 4),
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
        var users = await _userManager.Users.Take(3).ToListAsync();
        if (users.Count == 0) return;

        var outfits = await _context.Outfits.Take(6).ToListAsync();
        if (outfits.Count < 2) return;

        var polls = new List<ValidationPoll>();
        var random = new Random(42);

        var pollQuestions = new[]
        {
            "Which outfit should I wear for a job interview?",
            "Best outfit for a casual Friday?",
            "What should I wear for a dinner date?",
            "Outfit for a weekend brunch?",
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

                // Add some votes
                var voteCount = random.Next(1, 8);
                for (int v = 0; v < voteCount; v++)
                {
                    option.Votes.Add(new Vote
                    {
                        Id = Guid.NewGuid(),
                        PollId = poll.Id,
                        OptionId = option.Id,
                        VoterId = users[random.Next(users.Count)].Id,
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

    private static string GetOutfitName(int index)
    {
        return index switch
        {
            0 => "Casual Friday Look",
            1 => "Weekend Warrior",
            2 => "Date Night Style",
            3 => "Office Professional",
            4 => "Weekend Brunch",
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
}
