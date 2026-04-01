using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.DTOs.Search;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Infrastructure.Services;
using OutfitPlanner.Persistence;
using Xunit;

namespace OutfitPlanner.Application.UnitTests.Search;

public class SearchServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SearchService _searchService;
    private readonly string _userId = "test-user-id";

    public SearchServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
        _searchService = new SearchService(_context, cache);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Seed outfits
        var outfits = new List<Outfit>
        {
            new Outfit
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                Name = "Casual Friday",
             
                Season = Season.Summer,
                Occasion = OccasionType.Casual,
                Status = OutfitStatus.Active,
                ComfortRating = 5,
                StyleRating = 4,
                ImageUrl = "/images/outfit1.jpg"
            },
            new Outfit
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                Name = "Business Meeting",
            
                Season = Season.Winter,
                Occasion = OccasionType.Formal,
                Status = OutfitStatus.Active,
                ComfortRating = 4,
                StyleRating = 5,
                ImageUrl = "/images/outfit2.jpg"
            },
            new Outfit
            {
                Id = Guid.NewGuid(),
                UserId = "other-user",
                Name = "Other User Outfit",
                Season = Season.Spring,
                Occasion = OccasionType.Casual,
                Status = OutfitStatus.Active,
                ComfortRating = 3,
                StyleRating = 3
            }
        };

        // Seed clothing items
        var clothingItems = new List<ClothingItem>
        {
            new ClothingItem
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                Name = "Blue Cotton Shirt",
                Category = "Tops",
                Brand = "Nike",
                PrimaryColor = "Blue",
                Type = ClothingType.Top,
                Fabric = FabricType.Cotton,
                IsActive = true,
                ImageUrl = "/images/shirt.jpg"
            },
            new ClothingItem
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                Name = "Black Jeans",
                Category = "Bottoms",
                Brand = "Levi's",
                PrimaryColor = "Black",
                Type = ClothingType.Bottom,
                Fabric = FabricType.Denim,
                IsActive = true,
                ImageUrl = "/images/jeans.jpg"
            },
            new ClothingItem
            {
                Id = Guid.NewGuid(),
                UserId = "other-user",
                Name = "Other User Item",
                Category = "Tops",
                Brand = "Adidas",
                PrimaryColor = "Red",
                Type = ClothingType.Top,
                Fabric = FabricType.Cotton,
                IsActive = true
            },
            new ClothingItem
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                Name = "Inactive Item",
                Category = "Accessories",
                Brand = "Gucci",
                PrimaryColor = "Gold",
                Type = ClothingType.Accessory,
                Fabric = FabricType.Leather,
                IsActive = false
            }
        };

        _context.Outfits.AddRange(outfits);
        _context.ClothingItems.AddRange(clothingItems);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ShouldReturnAllUserItems()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.All
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Outfits.Should().HaveCount(2); // Only user's outfits
        result.WardrobeItems.Should().HaveCount(2); // Only active user's items
        result.TotalResults.Should().Be(4);
    }

    [Fact]
    public async Task SearchAsync_WithQuery_ShouldReturnMatchingItems()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "casual",
            Type = SearchType.All
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Outfits.Should().HaveCount(1);
        result.Outfits.First().Name.Should().Be("Casual Friday");
        result.WardrobeItems.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithCategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.Wardrobe,
            Categories = new List<string> { "Tops" }
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.WardrobeItems.Should().HaveCount(1);
        result.WardrobeItems.First().Category.Should().Be("Tops");
    }

    [Fact]
    public async Task SearchAsync_WithSeasonFilter_ShouldFilterBySeason()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.Outfits,
            Seasons = new List<string> { "Winter" }
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Outfits.Should().HaveCount(1);
        result.Outfits.First().Season.Should().Be("Winter");
    }

    [Fact]
    public async Task SearchAsync_WithColorFilter_ShouldFilterByColor()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.Wardrobe,
            Color = "Blue"
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.WardrobeItems.Should().HaveCount(1);
        result.WardrobeItems.First().PrimaryColor.Should().Be("Blue");
    }

    [Fact]
    public async Task SearchAsync_WithTypeFilter_Outfits_ShouldReturnOnlyOutfits()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.Outfits
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Outfits.Should().HaveCount(2);
        result.WardrobeItems.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithTypeFilter_Wardrobe_ShouldReturnOnlyWardrobe()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.Wardrobe
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Outfits.Should().BeEmpty();
        result.WardrobeItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ShouldNotReturnOtherUsersItems()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.All
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Outfits.Should().NotContain(o => o.Name == "Other User Outfit");
        result.WardrobeItems.Should().NotContain(i => i.Name == "Other User Item");
    }

    [Fact]
    public async Task SearchAsync_ShouldNotReturnInactiveItems()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.Wardrobe
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.WardrobeItems.Should().NotContain(i => i.Name == "Inactive Item");
    }

    [Fact]
    public async Task SearchAsync_ShouldCalculateRelevanceScore()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "Casual Friday",
            Type = SearchType.Outfits
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Outfits.Should().HaveCount(1);
        result.Outfits.First().RelevanceScore.Should().Be(100.0); // Exact match
    }

    [Fact]
    public async Task GetRecentSearchesAsync_WithNoSearches_ShouldReturnEmptyList()
    {
        // Act
        var result = await _searchService.GetRecentSearchesAsync(_userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveRecentSearchAsync_ShouldAddSearch()
    {
        // Arrange
        var query = "summer outfits";

        // Act
        await _searchService.SaveRecentSearchAsync(_userId, query);
        var result = await _searchService.GetRecentSearchesAsync(_userId);

        // Assert
        result.Should().Contain(query);
    }

    [Fact]
    public async Task SaveRecentSearchAsync_WithExistingSearch_ShouldMoveToTop()
    {
        // Arrange
        await _searchService.SaveRecentSearchAsync(_userId, "first search");
        await _searchService.SaveRecentSearchAsync(_userId, "second search");

        // Act
        await _searchService.SaveRecentSearchAsync(_userId, "first search");
        var result = await _searchService.GetRecentSearchesAsync(_userId);

        // Assert
        result.First().Should().Be("first search");
    }

    [Fact]
    public async Task ClearRecentSearchesAsync_ShouldClearAllSearches()
    {
        // Arrange
        await _searchService.SaveRecentSearchAsync(_userId, "search 1");
        await _searchService.SaveRecentSearchAsync(_userId, "search 2");

        // Act
        await _searchService.ClearRecentSearchesAsync(_userId);
        var result = await _searchService.GetRecentSearchesAsync(_userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSuggestionsAsync_WithShortQuery_ShouldReturnEmptyList()
    {
        // Act
        var result = await _searchService.GetSuggestionsAsync(_userId, "a");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldIncludeFacets()
    {
        // Arrange
        var request = new SearchRequest
        {
            Query = "",
            Type = SearchType.All
        };

        // Act
        var result = await _searchService.SearchAsync(_userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Facets.Should().NotBeNull();
        result.Facets.Should().ContainKey("category_Tops");
        result.Facets.Should().ContainKey("category_Bottoms");
    }
}
