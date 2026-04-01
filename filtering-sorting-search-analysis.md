# Filtering, Sorting & Search Analysis Report

## Executive Summary

After analyzing the Outfit Planner application, I've identified significant **architectural inconsistencies** and **performance anti-patterns** in how filtering, sorting, and search are implemented. The system exhibits a **mixed pattern** where some operations happen on the frontend (client-side) and others on the backend (server-side), leading to scalability risks and inconsistent user experiences.

---

## 1. DETECTION - Complete Inventory

### 🔴 FRONTEND-ONLY Filtering/Sorting (Anti-pattern)

| Feature | Location | Implementation | Risk Level |
|---------|----------|----------------|------------|
| **Wardrobe Dashboard Filtering** | `wardrobe-dashboard.component.ts` | Client-side computed signal filters ALL items by category, color, occasion, search query | **HIGH** |
| **Outfits Dashboard Filtering** | `outfits-dashboard.component.ts` | Client-side computed signal filters ALL outfits by occasion, season, search query + sorting | **HIGH** |
| **Community Feed Poll Filtering** | `community-feed.component.ts` | Client-side computed signal filters polls by status (active/closed) | **MEDIUM** |
| **Wardrobe Stats** | `wardrobe.selectors.ts` | Client-side reduce operations for total cost calculations | **LOW** |

### 🟢 BACKEND-ONLY Filtering/Sorting (Correct Pattern)

| Feature | Location | Implementation |
|---------|----------|----------------|
| **Global Search** | `SearchController.cs` → `SearchService.cs` | Full server-side search with facets, pagination, multi-filter support |
| **Trending Outfits** | `TrendingController.cs` → `TrendingOutfitRepository.cs` | Server-side pagination + sorting by trending score |
| **Feed Posts** | `FeedController.cs` → `FeedPostRepository.cs` | Cursor-based pagination, server-side visibility filtering |
| **Calendar Events** | `CalendarController.cs` | Server-side date-range filtering (year/month) |
| **Outfit Suggestions** | `GenerateOutfitSuggestionsQueryHandler.cs` | Server-side filtering by occasion/season/weather + relevance sorting |
| **Comments** | `FeedController.cs` | Cursor-based pagination for post comments |
| **Followers/Following** | `UserController.cs` | Cursor-based pagination |

### 🟡 HYBRID (Mixed Responsibility)

| Feature | Frontend | Backend | Issue |
|---------|----------|---------|-------|
| **Wardrobe by Category** | Fetches ALL items | Has `GetByCategory` endpoint | Frontend ignores backend filtering |
| **Search Results Tab Filter** | Tabs filter already-fetched results | Supports type param | Double filtering, inconsistent counts |

---

## 2. DATA FLOW ANALYSIS

### ❌ Problematic Pattern: Wardrobe Dashboard

```typescript
// wardrobe-dashboard.component.ts
allItems = toSignal(store.select(selectAllItems)); // Fetches EVERYTHING

items = computed(() => {
  let filtered = this.allItems().filter((item) => {
    // ALL filtering happens client-side on full dataset
    const categoryMatch = activeCat === 'All' || mappedCategory === activeCat;
    const colorMatch = activeColor === 'All' || item.primaryColor?.includes(activeColor);
    const occasionMatch = activeOccasion === 'All' || itemCategory === mappedOccasion;
    return categoryMatch && colorMatch && occasionMatch;
  });
  return filtered;
});
```

**Request/Response Pattern:**
- **Request:** `GET /api/wardrobe` (no query params)
- **Response:** `List<ClothingItemListDto>` - ALL user's clothing items
- **Frontend:** Downloads everything, filters in memory

### ❌ Problematic Pattern: Outfits Dashboard

```typescript
// outfits-dashboard.component.ts
filteredOutfits = computed(() => {
  let result = this.outfitsSignal();
  if (occasion) result = result.filter(o => o.occasion?.toLowerCase() === occasion.toLowerCase());
  if (season) result = result.filter(o => o.season?.toLowerCase() === mappedSeason.toLowerCase());
  if (query) result = result.filter(o => o.name?.toLowerCase().includes(query));
  // Sorting also client-side
  switch (sort) {
    case 'mostWorn': result = [...result].sort((a, b) => b.timesWorn - a.timesWorn);
    case 'name': result = [...result].sort((a, b) => a.name.localeCompare(b.name));
  }
});
```

**Request/Response Pattern:**
- **Request:** `GET /api/outfits` (no query params)
- **Response:** `List<OutfitDto>` - ALL user's outfits
- **Frontend:** Downloads everything, filters/sorts in memory

### ✅ Correct Pattern: Global Search

```typescript
// search.service.ts - Frontend
search(query: string, filters?: SearchFilters, page: number = 1): Observable<SearchResults> {
  let params = new HttpParams()
    .set('q', query)
    .set('page', page.toString())
    .set('pageSize', '20');
  
  if (filters?.categories?.length) {
    params = params.set('categories', filters.categories.join(','));
  }
  // ... more filters
  return this.http.get<SearchApiResponse>(this.baseUrl, { params });
}
```

```csharp
// SearchService.cs - Backend
public async Task<SearchResultDto> SearchAsync(string userId, SearchRequest request)
{
    var outfitQuery = _context.Outfits
        .Where(o => o.UserId == userId && o.Status != OutfitStatus.Deleted);
    
    // Apply season filter
    if (request.Seasons?.Any() == true)
    {
        outfitQuery = outfitQuery.Where(o => seasonEnums.Contains(o.Season));
    }
    // ... more filters
    
    var skip = (request.Page - 1) * request.PageSize;
    var outfits = await outfitQuery
        .OrderBy(o => o.Name)
        .Skip(skip)
        .Take(request.PageSize)
        .ToListAsync();
}
```

**Request/Response Pattern:**
- **Request:** `GET /api/search?q={query}&categories={cats}&seasons={seasons}&page=1&pageSize=20`
- **Response:** `SearchResultDto` with paginated results + facets
- **Frontend:** Receives only matching results, already filtered/paginated

---

## 3. PERFORMANCE & SCALABILITY EVALUATION

### 🚨 Critical Issues

| Issue | Impact | Evidence |
|-------|--------|----------|
| **Unbounded Data Loading** | Users with 1000+ wardrobe items will download massive payloads | `GetAll()` returns ALL items without pagination |
| **Memory Pressure** | Client-side filtering creates multiple array copies | `[...result].sort()` creates shallow copies on every filter change |
| **Wasted Bandwidth** | Transferring data that's immediately filtered out | Season filter on wardrobe loads all items even when filtered to 1% |
| **CPU Spikes** | Computed signals re-run filtering on every interaction | Every keystroke in search triggers full re-filter of entire dataset |
| **Inconsistent Pagination** | Frontend pagination doesn't match backend cursor pagination | No unified approach across features |

### 📊 Scalability Risk Assessment

| Feature | Current Approach | Risk with 1000 items | Risk with 10,000 items |
|---------|------------------|---------------------|----------------------|
| Wardrobe Dashboard | Frontend filter | Medium (slow UI) | Critical (browser crash) |
| Outfits Dashboard | Frontend filter/sort | Medium (slow UI) | Critical (browser crash) |
| Global Search | Backend filter | Low | Low |
| Feed | Backend cursor pagination | Low | Low |
| Trending | Backend pagination | Low | Low |

---

## 4. CONSISTENCY & ARCHITECTURE ANALYSIS

### 🔴 Architectural Inconsistencies Found

1. **Mixed Pagination Strategies**
   - Feed uses cursor-based pagination (correct for infinite scroll)
   - Trending uses offset pagination (page/pageSize)
   - Wardrobe/Outfits have NO pagination

2. **Mixed Filter Responsibility**
   - Search: Backend filters + returns facets
   - Wardrobe: Frontend filters (ignores backend category endpoint)
   - Outfits: Frontend filters (ignores no backend filter endpoint exists)
   - Polls: Frontend filters by status (should be backend)

3. **Duplicate Filter Logic**
   - Category filtering exists in both `SearchService.cs` and `wardrobe-dashboard.component.ts`
   - Season filtering in both backend and frontend
   - Search query matching in both layers

4. **Missing API Capabilities**
   - `WardrobeController` has `GetByCategory` endpoint but frontend doesn't use it
   - `OutfitsController` has NO filter endpoints at all
   - Price filtering commented out in backend due to Value Object limitations

---

## 5. BEST PRACTICE ASSESSMENT

### When to Use Backend vs Frontend Filtering

| Scenario | Recommendation | Current State |
|----------|----------------|---------------|
| Large datasets (>100 items) | **Backend** | ❌ Violated (Wardrobe/Outfits) |
| Faceted search with counts | **Backend** | ✅ Correct (Global Search) |
| Real-time search (type-ahead) | **Backend** with debounce | ✅ Correct (500ms debounce) |
| Sorting large lists | **Backend** | ❌ Violated (Outfits dashboard) |
| Cross-field filtering | **Backend** | ❌ Violated (Wardrobe multi-filter) |
| Quick toggle filters (small dataset) | Frontend acceptable | ✅ Acceptable (Poll status) |
| Data export (CSV) | Backend | N/A |

### Recommended Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      ANGULAR FRONTEND                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Search     │  │   Wardrobe   │  │   Outfits    │       │
│  │   (Minimal)  │  │   (Display)  │  │   (Display)  │       │
│  │  No filtering│  │  No filtering│  │  No filtering│       │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘       │
└─────────┼─────────────────┼─────────────────┼───────────────┘
          │                 │                 │
          ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                        BACKEND API                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   /search    │  │  /wardrobe   │  │  /outfits    │       │
│  │  Full filter │  │  Query param │  │  Query param │       │
│  │   + facets   │  │   filters    │  │   filters    │       │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘       │
└─────────┼─────────────────┼─────────────────┼───────────────┘
          │                 │                 │
          ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                      DATABASE                                │
│              (Indexed queries with pagination)               │
└─────────────────────────────────────────────────────────────┘
```

---

## 6. REFACTORING PLAN

### Phase 1: Critical Fixes (High Priority)

#### 6.1 Wardrobe Dashboard Backend Enhancement

**New API Endpoint:**
```csharp
// WardrobeController.cs
[HttpGet("filtered")]
public async Task<ActionResult<PagedResult<ClothingItemListDto>>> GetFiltered(
    [FromQuery] string? category,
    [FromQuery] string? color,
    [FromQuery] string? occasion,
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var userId = GetUserId();
    var query = new GetFilteredClothingItemsQuery 
    { 
        UserId = userId,
        Category = category,
        Color = color,
        Occasion = occasion,
        SearchQuery = search,
        Page = page,
        PageSize = pageSize
    };
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

**Frontend Changes:**
```typescript
// wardrobe-dashboard.component.ts
// Remove: items = computed(() => { ... client-side filtering ... })
// Replace with:
items = toSignal(this.store.select(selectFilteredItems));

setCategory(category: string) {
  this.activeCategory.set(category);
  this.store.dispatch(WardrobeActions.loadFilteredItems({ 
    filters: { category, color: this.activeColor(), occasion: this.activeOccasion() }
  }));
}
```

#### 6.2 Outfits Dashboard Backend Enhancement

**New API Endpoint:**
```csharp
// OutfitsController.cs
[HttpGet("filtered")]
public async Task<ActionResult<PagedResult<OutfitDto>>> GetFiltered(
    [FromQuery] string? occasion,
    [FromQuery] string? season,
    [FromQuery] string? search,
    [FromQuery] string? sortBy, // recent, mostWorn, name
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var userId = GetUserId();
    var query = new GetFilteredOutfitsQuery { ... };
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

**Frontend Changes:**
```typescript
// outfits-dashboard.component.ts
// Remove: filteredOutfits = computed(() => { ... })
// Replace with backend-driven filtering via store effects
```

#### 6.3 Community Feed Poll Status Filter

**Current:** Frontend filters all loaded polls
**Fix:** Add status filter to backend query

```csharp
// PollsController.cs
[HttpGet]
public async Task<ActionResult<List<ValidationPollDto>>> GetMyPolls(
    [FromQuery] PollStatus? status, // Add this
    [FromQuery] int page = 1, 
    [FromQuery] int pageSize = 20)
```

### Phase 2: API Unification (Medium Priority)

#### 6.4 Standardize Pagination

**Create unified pagination parameters:**
```csharp
// Application/Common/PaginationRequest.cs
public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

// For infinite scroll scenarios:
public class CursorPaginationRequest
{
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
}
```

#### 6.5 Fix Price Filtering

**Problem:** Value Object prevents EF Core translation
**Solution:** Add shadow property or DTO projection

```csharp
// Option 1: Use projection
var items = await _context.ClothingItems
    .Where(c => c.PurchasePrice.Amount >= minPrice) // Won't work
    .Select(c => new { c.Id, c.Name, Price = c.PurchasePrice.Amount }) // Still problematic
    
// Option 2: Add decimal column (recommended)
public class ClothingItem 
{
    public Money PurchasePrice { get; set; } = null!;
    [Column("purchase_price_amount")] // Shadow property
    public decimal PurchasePriceAmount { get; set; }
}
```

### Phase 3: Performance Optimizations (Low Priority)

#### 6.6 Add Database Indexes

```sql
-- For wardrobe filtering
CREATE INDEX idx_clothing_items_user_category ON clothing_items(user_id, category);
CREATE INDEX idx_clothing_items_user_color ON clothing_items(user_id, primary_color);
CREATE INDEX idx_clothing_items_user_active ON clothing_items(user_id, is_active);

-- For outfits filtering  
CREATE INDEX idx_outfits_user_occasion ON outfits(user_id, occasion);
CREATE INDEX idx_outfits_user_season ON outfits(user_id, season);
CREATE INDEX idx_outfits_user_status ON outfits(user_id, status);
```

#### 6.7 Implement Response Caching Headers

```csharp
// Add to filtered endpoints
[ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "category", "color", "page" })]
```

---

## 7. CODE-LEVEL EVIDENCE

### Anti-pattern Evidence: Wardrobe Dashboard

**File:** `src/outfit-planner-ui/src/app/presentation/pages/wardrobe-dashboard/wardrobe-dashboard.component.ts`
**Lines:** 82-142

```typescript
// Lines 82-142: Frontend filtering of complete dataset
items = computed(() => {
  let filtered = this.allItems().filter((item) => {
    // Category filter with mapping
    const categoryMap: { [key: string]: string } = {
      footwear: 'Shoes', top: 'Tops', /* ... */ 
    };
    const itemType = item.type?.toLowerCase() || '';
    const mappedCategory = categoryMap[itemType] || itemType;
    const categoryMatch = activeCat === 'All' || mappedCategory === activeCat;
    
    // Color filter
    const colorMatch = activeColor === 'All' || 
      item.primaryColor?.toLowerCase().includes(activeColor.toLowerCase());
    
    // Occasion filter
    const occasionMatch = activeOccasion === 'All' || 
      itemCategory === mappedOccasion.toLowerCase();
    
    // Season filter - DISABLED (backend has no season field)
    const seasonMatch = true; 
    
    return categoryMatch && colorMatch && occasionMatch && seasonMatch;
  });

  // Search filter - additional client-side filtering
  const searchValue = this.searchQuery();
  if (searchValue.trim()) {
    const q = searchValue.toLowerCase();
    filtered = filtered.filter(
      (item) =>
        item.name.toLowerCase().includes(q) ||
        item.type.toLowerCase().includes(q) ||
        item.brand?.toLowerCase().includes(q)
    );
  }
  return filtered;
});
```

### Correct Pattern Evidence: Global Search

**File:** `src/OutfitPlanner.Infrastructure/Services/SearchService.cs`
**Lines:** 44-120

```csharp
// Lines 44-120: Server-side filtering with proper query building
private async Task SearchOutfitsAsync(string userId, SearchRequest request, string query, SearchResultDto result, CancellationToken cancellationToken)
{
    var outfitQuery = _context.Outfits
        .AsNoTracking()
        .Where(o => o.UserId == userId && o.Status != OutfitStatus.Deleted)
        .AsQueryable();

    // Apply text search
    if (!string.IsNullOrWhiteSpace(query))
    {
        outfitQuery = outfitQuery.Where(o =>
            EF.Functions.Like(o.Name.ToLower(), $"%{query}%"));
    }

    // Apply season filter
    if (request.Seasons?.Any() == true)
    {
        var seasonEnums = request.Seasons
            .Select(s => Enum.TryParse<Season>(s, true, out var season) ? (Season?)season : null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();
        outfitQuery = outfitQuery.Where(o => seasonEnums.Contains(o.Season));
    }

    // Apply pagination at database level
    var skip = (request.Page - 1) * request.PageSize;
    var outfits = await outfitQuery
        .OrderBy(o => o.Name)
        .Skip(skip)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);
}
```

### Unused Backend Capability Evidence

**File:** `src/OutfitPlanner.Api/Controllers/WardrobeController.cs`
**Lines:** 47-58

```csharp
// Lines 47-58: Backend supports category filtering but frontend doesn't use it
[HttpGet("category/{category}")]
[ProducesResponseType(typeof(List<ClothingItemListDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<List<ClothingItemListDto>>> GetByCategory(string category)
{
    var userId = GetUserId();
    var items = await _mediator.Send(new GetClothingItemsByCategoryRequest
    {
        UserId = userId,
        Category = category
    });
    return Ok(items);
}
```

---

## 8. SUMMARY & RECOMMENDATIONS

### Immediate Actions Required

1. **CRITICAL:** Move Wardrobe dashboard filtering to backend
   - Add pagination to prevent unbounded data loading
   - Create filtered query endpoint
   - Update frontend to use backend filtering

2. **CRITICAL:** Move Outfits dashboard filtering/sorting to backend
   - Add filtered query endpoint with sort support
   - Remove client-side computed filtering

3. **HIGH:** Add pagination to all list endpoints
   - Wardrobe: Add page/pageSize parameters
   - Outfits: Add page/pageSize parameters
   - Polls: Use existing pagination consistently

### Architectural Principles to Adopt

1. **"Filter at the Source"** - Always filter as close to the database as possible
2. **"Paginate Everywhere"** - No endpoint should return unbounded lists
3. **"Consistent Patterns"** - Use the same pagination/filtering approach across all features
4. **"Frontend Display Only"** - Frontend should receive pre-filtered, paginated data

### Files to Modify

| Priority | File | Change |
|----------|------|--------|
| P0 | `WardrobeController.cs` | Add filtered/paginated endpoint |
| P0 | `OutfitsController.cs` | Add filtered/paginated endpoint |
| P0 | `wardrobe-dashboard.component.ts` | Remove client-side filtering |
| P0 | `outfits-dashboard.component.ts` | Remove client-side filtering |
| P1 | `wardrobe.effects.ts` | Add filtered load effect |
| P1 | `outfit.effects.ts` | Add filtered load effect |
| P2 | `ClothingItem` entity | Add price amount column for filtering |
| P2 | Database | Add indexes for filter columns |

---

## Conclusion

This analysis reveals a system that works for small datasets but will fail at scale. The refactoring plan prioritizes the most critical issues while maintaining backward compatibility during the transition. The **Global Search implementation serves as the reference pattern** - it correctly handles filtering, pagination, and faceted search entirely on the backend.