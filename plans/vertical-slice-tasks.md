# Outfit Planner — Comprehensive Vertical Slice Task List

This document provides a **detailed, step-by-step implementation plan** organized by **Vertical Slices**. Each feature is completed in full (DTOs → CQRS → Validators → Tests → Controller → Frontend State → UI) before moving to the next entity.

> ✅ = Done | ❌ = Pending
> _Referenced against [sequential-tasks.md](file:///c:/Meno/Projects/Outfit Planner/plans/sequential-tasks.md)_

---

## 🟢 Section 0: Foundation (COMPLETED)

### 0.1 Domain Layer

- [x] Domain entities: `User`, `ClothingItem`, `ClothingTag`, `Outfit`, `OutfitItem`, `OutfitFeedback`, `StyleRule`, `ValidationPoll`, `PollOption`, `Vote`, `WearEvent`
- [x] Enums: `ClothingType`, `OccasionType`, `Season`, `StylePreference`, `PrivacyLevel`, `FabricType`, `PollStatus`, `OutfitStatus`, `ItemRole`
- [x] Value objects: `Money`, `Color`, `Temperature`

### 0.2 Persistence & Infrastructure

- [x] SQL Server `AppDbContext`, Fluent API configs, Migrations
- [x] Generic + 15 Specific Repositories
- [x] JWT Auth, Email, Image Processing, Exception & Logging Middleware

### 0.3 Wardrobe Backend Complete

- [x] **Tasks 5–20**: All Wardrobe CQRS (Queries, Commands, DTOs, Validators, Handlers)
- [x] **Tasks 36–43**: `WardrobeController` fully wired to MediatR

### 0.4 Auth & Angular Core

- [x] **Tasks 63–66**: AuthGuard, AuthInterceptor, AuthService
- [x] **Tasks 115–117**: Unit & Integration tests for Wardrobe

---

## 👔 Section 1: Wardrobe — Frontend Completion

_The Wardrobe backend is done. This section finishes the frontend vertical slice._

---

### 1.1 State Management Setup

#### Task 77 — Install and Configure NgRx Store

- [x] Install packages: `@ngrx/store`, `@ngrx/effects`, `@ngrx/store-devtools`
- [x] Register `provideStore()` and `provideEffects()` in `app.config.ts`
- [x] Register `provideStoreDevtools()` for development debugging
- **Files**:
  - `src/outfit-planner-ui/src/app/app.config.ts` — Add NgRx providers
  - `package.json` — New dependencies

#### Task 78 — Create `auth` NgRx State Slice

- [x] Create `auth.actions.ts` — `login`, `loginSuccess`, `loginFailure`, `logout`, `refreshToken`
- [x] Create `auth.reducer.ts` — State: `{ user: User | null, token: string | null, isAuthenticated: boolean, loading: boolean, error: string | null }`
- [x] Create `auth.effects.ts` — Side effects calling `AuthService.login()`, `AuthService.register()`, `AuthService.refreshToken()`
- [x] Create `auth.selectors.ts` — `selectUser`, `selectToken`, `selectIsAuthenticated`, `selectAuthLoading`
- **Directory**: `src/outfit-planner-ui/src/app/core/state/auth/`

#### Task 79 — Create `wardrobe` NgRx State Slice

- [x] Create `wardrobe.actions.ts`:
  - `loadClothingItems` / `loadClothingItemsSuccess` / `loadClothingItemsFailure`
  - `loadClothingItemById` / `loadClothingItemByIdSuccess`
  - `loadClothingItemsByCategory` / `loadClothingItemsByCategorySuccess`
  - `createClothingItem` / `createClothingItemSuccess`
  - `updateClothingItem` / `updateClothingItemSuccess`
  - `deleteClothingItem` / `deleteClothingItemSuccess`
  - `recordWear` / `recordWearSuccess`
- [x] Create `wardrobe.reducer.ts` — State: `{ items: ClothingItem[], selectedItem: ClothingItem | null, loading: boolean, error: string | null, filter: { category: string | null } }`
- [x] Create `wardrobe.effects.ts` — Side effects calling `ClothingItemDataSource` methods for each action
- [x] Create `wardrobe.selectors.ts` — `selectAllItems`, `selectSelectedItem`, `selectLoading`, `selectItemsByCategory`, `selectWardrobeStats`
- **Directory**: `src/outfit-planner-ui/src/app/core/state/wardrobe/`

---

### 1.2 Frontend UI Pages

#### Task 87 — Wardrobe Dashboard Page

- [x] Create component `wardrobe-dashboard.component.ts`
- [x] Display clothing items in a responsive grid of cards (image, name, type, color swatch, wear count)
- [x] Category filter tabs/buttons using `ClothingType` enum values (`Top`, `Bottom`, `Dress`, `Outerwear`, `Footwear`, `Accessory`, etc.)
- [x] Search bar filtering by `Name`, `Brand`, `Category`
- [x] Sorting options: Most Worn, Least Worn, Recently Added, Name A-Z
- [x] FAB button to navigate to Add Clothing Item
- [x] Dispatch `loadClothingItems` on init, subscribe to `selectAllItems`
- **API**: `GET /api/wardrobe` and `GET /api/wardrobe/category/{category}`
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/wardrobe-dashboard/`

#### Task 88 — Add Clothing Item Page

- [x] Create component `add-clothing-item.component.ts`
- [x] Multi-step form or single-page form with sections:
  - **Basic Info**: `Name` (text), `Type` (dropdown — `ClothingType`), `Category` (text), `Brand` (text), `Size` (text)
  - **Colors**: `PrimaryColor` (color picker), `SecondaryColors` (multi-select)
  - **Details**: `Fabric` (dropdown — `FabricType`), `PurchasePrice` (number + currency), `PurchaseDate` (datepicker), `Condition` (dropdown: good/fair/poor)
  - **Image**: File upload component (accepts `.jpg`, `.jpeg`, `.png`, `.webp`, max 10MB)
  - **Tags**: Chip input for `ClothingTag` names
- [x] Client-side validation matching backend `CreateClothingItemDtoValidator`
- [x] Dispatch `createClothingItem` action on submit → navigate to dashboard on success
- **API**: `POST /api/wardrobe` with `CreateClothingItemDto` body
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/add-clothing-item/`

#### Task 89 — Edit Clothing Item Page

- [x] Create component `edit-clothing-item.component.ts`
- [x] Pre-populate form with existing data from `GET /api/wardrobe/{id}`
- [x] Same form fields as Add page, using `UpdateClothingItemDto`
- [x] Dispatch `updateClothingItem` action on submit
- **API**: `GET /api/wardrobe/{id}` (load) → `PUT /api/wardrobe/{id}` (save)
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/edit-clothing-item/`

#### Task 90 — Clothing Item Detail Page

- [x] Create component `clothing-item-detail.component.ts`
- [x] Hero section: Large image with thumbnail gallery (original, medium, large sizes)
- [x] Info panel: Name, Type, Category, Brand, Size, Colors, Fabric, Condition, Price
- [x] Stats: Wear Count, Last Worn date, Last Washed date, Cost per Wear
- [x] Wear History timeline (list of `WearEvent` dates)
- [x] Action buttons: "Wear Now" (quick wear), "Edit", "Delete" (with confirmation dialog)
- [x] "Wear Now" dispatches `recordWear` → `POST /api/wardrobe/{id}/wear/quick`
- [x] "Delete" dispatches `deleteClothingItem` → `DELETE /api/wardrobe/{id}`
- **API**: `GET /api/wardrobe/{id}`
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/clothing-item-detail/`
### will be implmented in future
#### Task 91 — Wardrobe Analytics Component

- [ ] Create component `wardrobe-analytics.component.ts`
- [ ] Dashboard cards: Total Items, Most Worn Item, Least Worn Item, Items by Category (bar chart), Cost Analysis
- [ ] Calculate from `selectAllItems` selector data
- [ ] Embed as a tab or section in the Wardrobe Dashboard
- **Directory**: `src/outfit-planner-ui/src/app/presentation/components/wardrobe-analytics/`

#### Task 92 — Wardrobe Routes

- [x] Add routes to `app.routes.ts`:
  - `/wardrobe` → `WardrobeDashboardComponent`
  - `/wardrobe/add` → `AddClothingItemComponent`
  - `/wardrobe/:id` → `ClothingItemDetailComponent`
  - `/wardrobe/:id/edit` → `EditClothingItemComponent`
- [x] Protect all routes with `AuthGuard`
- **File**: `src/outfit-planner-ui/src/app/app.routes.ts`

---

## 👗 Section 2: Outfit Management — Full Stack

_Backend CQRS → Tests → Controller → Frontend State → UI_

---

### 2.1 DTOs

#### Task 21 — Create Outfit DTOs

- [x] Create `OutfitDto` in `Application/DTOs/Outfit/OutfitDto.cs`:
  - Properties: `Id` (Guid), `UserId` (string), `Name` (string), `Occasion` (string — mapped from `OccasionType`), `WeatherCondition` (string), `Season` (string — mapped from `Season`), `ComfortRating` (int?), `StyleRating` (int?), `LastWorn` (DateTimeOffset?), `TimesWorn` (int), `Status` (string — mapped from `OutfitStatus`), `Items` (List\<OutfitItemDto\>), `CreatedAt` (DateTimeOffset)
- [x] Create `OutfitListDto` in `Application/DTOs/Outfit/OutfitListDto.cs`:
  - Lightweight: `Id`, `Name`, `Occasion`, `Season`, `TimesWorn`, `LastWorn`, `Status`, `ItemCount` (int), `ThumbnailUrl` (string — first item's thumbnail)
- [x] Create `OutfitItemDto` in `Application/DTOs/Outfit/OutfitItemDto.cs`:
  - Properties: `Id` (Guid), `ClothingItemId` (Guid), `ClothingItemName` (string), `ClothingItemImageUrl` (string), `Role` (string — from `ItemRole`), `LayeringOrder` (int), `IsEssential` (bool)
- [x] Create `CreateOutfitDto` in `Application/DTOs/Outfit/CreateOutfitDto.cs`:
  - Properties: `Name` (string, required), `Occasion` (string, required — must be valid `OccasionType`), `WeatherCondition` (string), `Season` (string, required — must be valid `Season`), `Items` (List\<CreateOutfitItemDto\>, required, min 1 item)
- [x] Create `CreateOutfitItemDto`:
  - Properties: `ClothingItemId` (Guid, required), `Role` (string — `ItemRole`), `LayeringOrder` (int), `IsEssential` (bool)
- [x] Create `UpdateOutfitDto` in `Application/DTOs/Outfit/UpdateOutfitDto.cs`:
  - Properties: `Name` (string?), `Occasion` (string?), `WeatherCondition` (string?), `Season` (string?), `ComfortRating` (int?), `StyleRating` (int?), `Items` (List\<CreateOutfitItemDto\>?)
- **Directory**: `src/OutfitPlanner.Application/DTOs/Outfit/`

---

### 2.2 AutoMapper Profiles

#### Update MappingProfile

- [x] Add to `MappingProfile.cs`:
  ```
  #region Outfit Mappings
  CreateMap<Outfit, OutfitDto>().ReverseMap();
  CreateMap<Outfit, OutfitListDto>()
      .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count))
      .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Items.FirstOrDefault().ClothingItem.ThumbnailUrl));
  CreateMap<OutfitItem, OutfitItemDto>()
      .ForMember(d => d.ClothingItemName, o => o.MapFrom(s => s.ClothingItem.Name))
      .ForMember(d => d.ClothingItemImageUrl, o => o.MapFrom(s => s.ClothingItem.ImageUrl));
  #endregion
  ```
- **File**: `src/OutfitPlanner.Application/Profiles/MappingProfile.cs`

---

### 2.3 CQRS Queries

#### Task 22 — `GetOutfitsRequest` + Handler

- [x] Create `GetOutfitsRequest.cs` implementing `IRequest<List<OutfitListDto>>`
  - Properties: `UserId` (string)
- [x] Create `GetOutfitsRequestHandler.cs` in `Handlers/Queries/`
  - Inject: `IOutfitRepository`, `IMapper`
  - Logic: Call `_outfitRepository.GetByUserIdAsync(request.UserId)`, map to `List<OutfitListDto>`
- **Directory**: `src/OutfitPlanner.Application/Features/Outfits/Requests/Queries/` and `src/OutfitPlanner.Application/Features/Outfits/Handlers/Queries/`

#### Task 23 — `GetOutfitByIdRequest` + Handler

- [x] Create `GetOutfitByIdRequest.cs` implementing `IRequest<OutfitDto>`
  - Properties: `Id` (Guid), `UserId` (string)
- [x] Create `GetOutfitByIdRequestHandler.cs`
  - Inject: `IOutfitRepository`, `IMapper`
  - Logic: Call `_outfitRepository.GetWithItemsByIdAsync(request.Id)`, verify `UserId` matches, map to `OutfitDto`
  - Throw `NotFoundException` if not found, `UnauthorizedAccessException` if user mismatch

---

### 2.4 CQRS Commands

#### Task 24 — `CreateOutfitCommand` + Handler

- [x] Create `CreateOutfitCommand.cs` implementing `IRequest<BaseCommandResponse>`
  - Properties: `UserId` (string), `Request` (CreateOutfitDto)
- [x] Create `CreateOutfitCommandHandler.cs`
  - Inject: `IOutfitRepository`, `IOutfitItemRepository`, `IClothingItemRepository`, `IMapper`, `IValidator<CreateOutfitCommand>`
  - Logic:
    1. Validate command via FluentValidation
    2. Verify each `ClothingItemId` in `Items` exists and belongs to `UserId`
    3. Create new `Outfit` entity, parse `OccasionType` and `Season` enums from strings
    4. Create `OutfitItem` entities for each item
    5. Save via repository, return `BaseCommandResponse` with new `Id`
- [x] Create `CreateOutfitCommandValidator.cs` — Validate `Name` not empty, `Occasion` is valid enum, `Season` is valid enum, `Items` has at least 1 item
- **Directory**: `src/OutfitPlanner.Application/Features/Outfits/Requests/Commands/` and `src/OutfitPlanner.Application/Features/Outfits/Handlers/Commands/`

#### Task 25 — `UpdateOutfitCommand` + Handler

- [x] Create `UpdateOutfitCommand.cs` implementing `IRequest<OutfitDto>`
  - Properties: `Id` (Guid), `UserId` (string), `Request` (UpdateOutfitDto)
- [x] Create `UpdateOutfitCommandHandler.cs`
  - Logic:
    1. Fetch outfit by Id, verify ownership
    2. Update only non-null fields from `UpdateOutfitDto`
    3. If `Items` provided, clear existing `OutfitItem` entries and re-create
    4. Save and return mapped `OutfitDto`
- [x] Create `UpdateOutfitCommandValidator.cs`

#### Task 26 — `DeleteOutfitCommand` + Handler

- [x] Create `DeleteOutfitCommand.cs` implementing `IRequest<BaseCommandResponse>`
  - Properties: `Id` (Guid), `UserId` (string)
- [x] Create `DeleteOutfitCommandHandler.cs`
  - Logic: Fetch outfit, verify ownership, set `Status = OutfitStatus.Deleted` (soft delete), save

#### Task 27 — `RecordOutfitWearCommand` + Handler

- [x] Create `RecordOutfitWearCommand.cs` implementing `IRequest<BaseCommandResponse>`
  - Properties: `UserId` (string), `OutfitId` (Guid), `WornAt` (DateTimeOffset)
- [x] Create `RecordOutfitWearCommandHandler.cs`
  - Logic:
    1. Fetch outfit, verify ownership
    2. Create `WearEvent` entity with `OutfitId`, `UserId`, `WornAt`
    3. Update `Outfit.LastWorn` and increment `Outfit.TimesWorn`
    4. Also update each clothing item's `LastWorn` and `WearCount`

#### Task 28 — `GenerateOutfitSuggestionsQuery` + Handler

- [x] Create `GenerateOutfitSuggestionsQuery.cs` implementing `IRequest<List<OutfitDto>>`
  - Properties: `UserId` (string), `Occasion` (string?), `Season` (string?), `WeatherCondition` (string?)
- [x] Create `GenerateOutfitSuggestionsQueryHandler.cs`
  - **Placeholder algorithm**: Fetch user's clothing items, group by `ClothingType`, randomly combine one Top + one Bottom + optional Outerwear + optional Footwear + optional Accessory. Return 3 suggestions as `OutfitDto` objects (not persisted until user saves).

---

### 2.5 Unit Tests for Outfit Handlers

- [x] Create `OutfitTests.cs` in `tests/OutfitPlanner.Application.IntegrationTests/Outfits/`
- [x] Test cases (follow `ClothingItemTests.cs` pattern with in-memory DB):
  - `CreateOutfit_ShouldSucceed_WithValidData`
  - `CreateOutfit_ShouldFail_WhenNameIsEmpty`
  - `CreateOutfit_ShouldFail_WhenNoItemsProvided`
  - `GetOutfitById_ShouldReturnOutfit_WithItems`
  - `GetOutfitById_ShouldThrow_WhenNotFound`
  - `GetOutfitById_ShouldThrow_WhenWrongUser`
  - `UpdateOutfit_ShouldSucceed_WhenValid`
  - `DeleteOutfit_ShouldSoftDelete`
  - `RecordOutfitWear_ShouldUpdateCounters`
  - `GetOutfits_ShouldReturnOnlyUserOutfits`

---

### 2.6 `OutfitsController` API Endpoints

#### Tasks 44–51 — Implement OutfitsController

- [x] Inject `IMediator` and `ILogger<OutfitsController>`, add `[Authorize]`
- [x] **Task 44** — `GET /api/outfits`: `GetAll()` → dispatches `GetOutfitsRequest { UserId }`, returns `List<OutfitListDto>`
- [x] **Task 45** — `GET /api/outfits/{id:guid}`: `GetById(Guid id)` → dispatches `GetOutfitByIdRequest { Id, UserId }`, returns `OutfitDto`
- [x] **Task 46** — `POST /api/outfits/generate`: `GenerateSuggestions([FromQuery] string? occasion, string? season)` → dispatches `GenerateOutfitSuggestionsQuery`, returns `List<OutfitDto>`
- [x] **Task 47** — `POST /api/outfits`: `Create([FromBody] CreateOutfitDto)` → dispatches `CreateOutfitCommand`, returns `CreatedAtAction` with `BaseCommandResponse`
- [x] **Task 48** — `PUT /api/outfits/{id:guid}`: `Update(Guid id, [FromBody] UpdateOutfitDto)` → dispatches `UpdateOutfitCommand`, returns `OutfitDto`
- [x] **Task 49** — `DELETE /api/outfits/{id:guid}`: `Delete(Guid id)` → dispatches `DeleteOutfitCommand`, returns `NoContent`
- [x] **Task 50** — `GET /api/outfits/today`: `GetToday()` → dispatches `GenerateOutfitSuggestionsQuery` with today's weather/season, returns single `OutfitDto`
- [x] **Task 51** — `POST /api/outfits/{id:guid}/wear`: `RecordWear(Guid id)` → dispatches `RecordOutfitWearCommand`, returns `BaseCommandResponse`
- **File**: `src/OutfitPlanner.Api/Controllers/OutfitsController.cs`

---

### 2.7 Integration Tests for OutfitsController

- [ ] Create `OutfitControllerTests.cs` in `tests/` using `WebApplicationFactory`
- [ ] Test: Create → Get → Update → Delete → Verify soft delete

---

### 2.8 Frontend — Outfit Domain & Data Layer

#### Task 71 — Create `OutfitUseCases`

- [ ] Create `outfit.usecases.ts` in `app/domain/usecases/`
  - Methods: `getOutfits()`, `getOutfitById(id)`, `createOutfit(dto)`, `updateOutfit(id, dto)`, `deleteOutfit(id)`, `recordWear(id)`, `generateSuggestions(params)`

#### Task 73 — Create `OutfitDataSource` + `OutfitRepositoryImpl`

- [ ] Create `outfit.datasource.ts` in `app/data/datasources/`
  - HTTP calls to: `GET /api/outfits`, `GET /api/outfits/{id}`, `POST /api/outfits`, `PUT /api/outfits/{id}`, `DELETE /api/outfits/{id}`, `POST /api/outfits/{id}/wear`, `POST /api/outfits/generate`
- [ ] Create `outfit.repository.impl.ts` in `app/data/repositories/`
  - Implements `OutfitRepository` interface, delegates to `OutfitDataSource`

#### Task 80 — Create `outfits` NgRx State Slice

- [ ] Actions: `loadOutfits`, `loadOutfitById`, `createOutfit`, `updateOutfit`, `deleteOutfit`, `recordOutfitWear`, `generateSuggestions` (+ success/failure variants)
- [ ] Reducer state: `{ outfits: Outfit[], selectedOutfit: Outfit | null, suggestions: Outfit[], loading: boolean, error: string | null }`
- [ ] Effects calling `OutfitDataSource`
- [ ] Selectors: `selectAllOutfits`, `selectSelectedOutfit`, `selectSuggestions`, `selectOutfitsLoading`
- **Directory**: `src/outfit-planner-ui/src/app/core/state/outfits/`

---

### 2.9 Frontend — Outfit UI Pages

#### Task 93 — Outfits Dashboard

- [ ] Grid of outfit cards showing: Name, Occasion badge, Season badge, Item count, Times Worn, combined thumbnail of items
- [ ] Filter by Occasion and Season
- [ ] FAB to create new outfit
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/outfits-dashboard/`

#### Task 94 — Outfit Builder

- [ ] Interactive page: Left panel shows user's wardrobe items (from `selectAllItems`), right panel shows the outfit being built
- [ ] Drag & drop or click-to-add clothing items
- [ ] Set outfit metadata: Name, Occasion (dropdown), Season (dropdown)
- [ ] Assign `ItemRole` (Primary/Secondary/Accent) and `LayeringOrder` per item
- [ ] Preview assembled outfit visually
- [ ] Submit dispatches `createOutfit`
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/outfit-builder/`

#### Task 95 — Daily Suggestion Page

- [ ] Show 1–3 suggested outfits for today
- [ ] Each suggestion as a card with constituent items displayed
- [ ] "Save This Outfit" button → dispatches `createOutfit`
- [ ] "Wear This Today" button → saves + dispatches `recordOutfitWear`
- [ ] Uses `POST /api/outfits/generate` or `GET /api/outfits/today`
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/daily-suggestion/`

#### Task 97 — Outfit Routes

- [ ] `/outfits` → `OutfitsDashboardComponent`
- [ ] `/outfits/build` → `OutfitBuilderComponent`
- [ ] `/outfits/today` → `DailySuggestionComponent`
- [ ] `/outfits/:id` → `OutfitDetailComponent` (reuse pattern from clothing detail)
- **File**: `src/outfit-planner-ui/src/app/app.routes.ts`

---

## 🗳️ Section 3: Social Validation — Full Stack

---

### 3.1 DTOs

#### Task 29 — Create Social DTOs

- [ ] Create `ValidationPollDto` in `Application/DTOs/Social/ValidationPollDto.cs`:
  - Properties: `Id` (Guid), `UserId` (string), `Question` (string), `Context` (string), `ExpiresAt` (DateTimeOffset), `Status` (string — from `PollStatus`), `Options` (List\<PollOptionDto\>), `TotalVotes` (int — computed), `CreatedAt` (DateTimeOffset)
- [ ] Create `PollOptionDto` in `Application/DTOs/Social/PollOptionDto.cs`:
  - Properties: `Id` (Guid), `OutfitId` (Guid?), `Description` (string), `DisplayOrder` (int), `VoteCount` (int — computed from `Votes.Count`), `OutfitThumbnail` (string? — from outfit's first item)
- [ ] Create `VoteDto` in `Application/DTOs/Social/VoteDto.cs`:
  - Properties: `Id` (Guid), `PollId` (Guid), `OptionId` (Guid), `VoterId` (string), `Rating` (int 1–5), `Comment` (string?), `IsAnonymous` (bool), `CreatedAt` (DateTimeOffset)
- [ ] Create `CreatePollDto`:
  - Properties: `Question` (string, required), `Context` (string?), `ExpiresAt` (DateTimeOffset, required, must be in future), `Options` (List\<CreatePollOptionDto\>, required, min 2 options)
- [ ] Create `CreatePollOptionDto`:
  - Properties: `OutfitId` (Guid?), `Description` (string, required), `DisplayOrder` (int)
- [ ] Create `CastVoteDto`:
  - Properties: `OptionId` (Guid, required), `Rating` (int, 1–5), `Comment` (string?), `IsAnonymous` (bool)
- **Directory**: `src/OutfitPlanner.Application/DTOs/Social/`

---

### 3.2 AutoMapper Profile Update

- [ ] Add to `MappingProfile.cs`:
  ```
  #region Social Mappings
  CreateMap<ValidationPoll, ValidationPollDto>()
      .ForMember(d => d.TotalVotes, o => o.MapFrom(s => s.Votes.Count));
  CreateMap<PollOption, PollOptionDto>()
      .ForMember(d => d.VoteCount, o => o.MapFrom(s => s.Votes.Count));
  CreateMap<Vote, VoteDto>();
  #endregion
  ```

---

### 3.3 CQRS Queries & Commands

#### Task 30 — `GetPollsRequest` + Handler

- [ ] `GetPollsRequest` implementing `IRequest<List<ValidationPollDto>>`
  - Properties: `UserId` (string)
- [ ] Handler: Call `_validationPollRepository.GetByUserIdAsync(userId)`, include `Options` and `Votes`, map to DTOs
- **Repositories used**: `IValidationPollRepository` (`GetByUserIdAsync`, `GetActivePollsAsync`)

#### Task 31 — `GetPollByIdRequest` + Handler

- [ ] `GetPollByIdRequest` implementing `IRequest<ValidationPollDto>`
  - Properties: `Id` (Guid), `UserId` (string)
- [ ] Handler: Fetch poll with options and votes, map to DTO

#### Task 32 — `CreatePollCommand` + Handler

- [ ] `CreatePollCommand` implementing `IRequest<BaseCommandResponse>`
  - Properties: `UserId` (string), `Request` (CreatePollDto)
- [ ] Handler:
  1. Validate (`Question` not empty, `ExpiresAt` in future, min 2 options)
  2. Create `ValidationPoll` entity + child `PollOption` entities
  3. If `OutfitId` provided on options, verify outfits exist and belong to user
  4. Save and return `BaseCommandResponse`
- [ ] Create `CreatePollCommandValidator.cs`

#### Task 33 — `VoteOnPollCommand` + Handler

- [ ] `VoteOnPollCommand` implementing `IRequest<BaseCommandResponse>`
  - Properties: `UserId` (string), `PollId` (Guid), `Request` (CastVoteDto)
- [ ] Handler:
  1. Verify poll exists and is `PollStatus.Active` and not expired
  2. Verify option belongs to this poll
  3. Check user hasn't already voted on this poll
  4. Create `Vote` entity (with `VoterId = UserId`, `PollId`, `OptionId`, `Rating`, `Comment`, `IsAnonymous`)
  5. Save and return response
- [ ] Create `VoteOnPollCommandValidator.cs` — `Rating` between 1–5, `OptionId` required
- **Repositories used**: `IValidationPollRepository`, `IPollOptionRepository`, `IVoteRepository` (`GetByPollIdAsync`, `GetByOptionIdAsync`)

---

### 3.4 Unit Tests for Social Handlers

- [ ] Create `SocialTests.cs` in `tests/OutfitPlanner.Application.IntegrationTests/Social/`
- [ ] Test cases:
  - `CreatePoll_ShouldSucceed_WithValidData`
  - `CreatePoll_ShouldFail_WhenLessThanTwoOptions`
  - `CreatePoll_ShouldFail_WhenExpiredInPast`
  - `VoteOnPoll_ShouldSucceed_WhenPollActive`
  - `VoteOnPoll_ShouldFail_WhenPollExpired`
  - `VoteOnPoll_ShouldFail_WhenAlreadyVoted`
  - `GetPolls_ShouldReturnUserPolls`
  - `GetPollById_ShouldReturnWithVoteCounts`

---

### 3.5 `SocialController` API Endpoints

#### Tasks 52–56 — Implement SocialController

- [ ] Inject `IMediator`, `ILogger<SocialController>`, add `[Authorize]`
- [ ] **Task 52** — `GET /api/social/polls`: `GetPolls()` → `GetPollsRequest`, returns `List<ValidationPollDto>`
- [ ] **Task 53** — `GET /api/social/polls/{id:guid}`: `GetPollById(Guid id)` → `GetPollByIdRequest`, returns `ValidationPollDto`
- [ ] **Task 54** — `POST /api/social/polls`: `CreatePoll([FromBody] CreatePollDto)` → `CreatePollCommand`, returns `CreatedAtAction`
- [ ] **Task 55** — `POST /api/social/polls/{id:guid}/vote`: `Vote(Guid id, [FromBody] CastVoteDto)` → `VoteOnPollCommand`, returns `BaseCommandResponse`
- [ ] **Task 56** — `GET /api/social/trends/local`: **Placeholder** — return hardcoded trending data for now
- **File**: `src/OutfitPlanner.Api/Controllers/SocialController.cs`

---

### 3.6 Frontend — Social Domain & Data Layer

#### Tasks 67–69 — Social Domain Interfaces

- [ ] Create `validation-poll.entity.ts` in `app/domain/entities/` — `ValidationPoll`, `PollOption`, `Vote` TypeScript interfaces
- [ ] Create `wear-event.entity.ts` in `app/domain/entities/` — `WearEvent` interface
- [ ] Create `social.repository.ts` in `app/domain/repositories/` — `getPolls()`, `getPollById(id)`, `createPoll(dto)`, `vote(pollId, dto)`

#### Task 74 — Social Data Layer

- [ ] Create `social.datasource.ts` — HTTP calls to `/api/social/polls/*`
- [ ] Create `social.repository.impl.ts` — Implements `SocialRepository`

#### Task 72 — `SocialUseCases`

- [ ] Create `social.usecases.ts` — `getPolls()`, `getPollById(id)`, `createPoll(dto)`, `voteOnPoll(pollId, dto)`

#### Task 81 — `social` NgRx State Slice

- [ ] Actions: `loadPolls`, `loadPollById`, `createPoll`, `vote` + success/failure
- [ ] State: `{ polls: ValidationPoll[], selectedPoll: ValidationPoll | null, loading: boolean, error: string | null }`
- [ ] Effects + Selectors

---

### 3.7 Frontend — Social UI Pages

#### Task 98 — Community Feed

- [ ] Grid/list of active polls: Question, expiry countdown, total votes, option thumbnails
- [ ] Filter: Active / Closed / My Polls
- [ ] FAB to create new poll
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/community-feed/`

#### Task 99 — Create Poll Page

- [ ] Form: Question (text), Expiry (datetime picker), Options (dynamic list — each with Description + optional outfit picker)
- [ ] Min 2 options, max 6 options
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/create-poll/`

#### Task 100 — Poll Detail / Vote Page

- [ ] Show poll question, options with outfit images if available
- [ ] Vote interface: Select option, set rating (1–5 stars), optional comment, anonymous toggle
- [ ] After voting: Show results with vote counts / percentages as bar chart
- **Directory**: `src/outfit-planner-ui/src/app/presentation/pages/poll-detail/`

#### Task 101 — Social Routes

- [ ] `/social` → `CommunityFeedComponent`
- [ ] `/social/create` → `CreatePollComponent`
- [ ] `/social/polls/:id` → `PollDetailComponent`
- **File**: `src/outfit-planner-ui/src/app/app.routes.ts`

---

## ☁️ Section 4: Weather Integration — Full Stack

---

### 4.1 Backend

#### Task 34 — `GetCurrentWeatherQuery` + Handler

- [ ] Create `WeatherDto` in `Application/DTOs/Weather/WeatherDto.cs`:
  - Properties: `Temperature` (double), `Condition` (string), `Humidity` (int), `WindSpeed` (double), `Icon` (string), `City` (string), `Description` (string)
- [ ] Create `GetCurrentWeatherQuery` implementing `IRequest<WeatherDto>`
  - Properties: `City` (string?), `Latitude` (double?), `Longitude` (double?)
- [ ] Handler: **Mock implementation** — return hardcoded weather data. Later replace with OpenWeatherMap API call.
- [ ] Create `IWeatherService` interface in `Application/Contracts/Infrastructure/`
- [ ] Implement `MockWeatherService` in `Infrastructure/Services/`
- **Directory**: `src/OutfitPlanner.Application/Features/Weather/`

#### Task 35 — `GetWeatherForecastQuery` + Handler

- [ ] Create `WeatherForecastDto`: `Date` (DateOnly), `HighTemp` (double), `LowTemp` (double), `Condition` (string), `Icon` (string)
- [ ] Create `GetWeatherForecastQuery` implementing `IRequest<List<WeatherForecastDto>>`
  - Properties: `City` (string?), `Days` (int, default 5)
- [ ] Handler: Mock — return 5 days of fake forecast data

#### Tasks 57–58 — `WeatherController`

- [ ] **Task 57** — `GET /api/weather/current?city={city}`: Returns `WeatherDto`
- [ ] **Task 58** — `GET /api/weather/forecast?city={city}&days={days}`: Returns `List<WeatherForecastDto>`
- **File**: `src/OutfitPlanner.Api/Controllers/WeatherController.cs`

---

### 4.2 Frontend

#### Tasks 70 & 75 — Weather Data Layer

- [x] Create `weather.repository.ts` interface in `app/domain/repositories/`
- [x] Create `weather.datasource.ts` — HTTP calls to `/api/weather/*`
- [x] Create `weather.repository.impl.ts`

#### Task 82 — `weather` NgRx State Slice

- [x] State: `{ current: WeatherDto | null, forecast: WeatherForecastDto[], loading: boolean }`
- [x] Actions, Effects, Selectors

#### Task 96 — Weather Display Component

- [x] Reusable component showing: temperature, condition icon, city name, humidity, wind
- [x] Used in Daily Suggestion page and Dashboard
- **Directory**: `src/outfit-planner-ui/src/app/presentation/components/weather-display/`

---

## 🏁 Section 5: Layout, Polish & Deployment

---

### 5.1 Global Architecture

#### Task 65 — `ErrorInterceptor`

- [ ] Create `error.interceptor.ts` in `app/core/interceptors/`
- [ ] Intercept HTTP errors: 401 → redirect to login, 403 → show forbidden, 404 → show not found, 500 → show generic error toast
- [ ] Register in `app.config.ts`

#### Task 76 — Auth Data Source (if not already done)

- [ ] Create `auth.datasource.ts` — `login(email, password)`, `register(...)`, `refreshToken(token)`
- [ ] Wire to `POST /api/auth/login`, `POST /api/auth/register`, `POST /api/auth/refresh`

#### Tasks 83–86 — Wire Auth UI

- [ ] **Task 83**: Wire Login page form submit → dispatch `login` action → call `POST /api/auth/login` → store token
- [ ] **Task 84**: Wire Register page → dispatch `register` action → call `POST /api/auth/register`
- [ ] **Task 85**: Implement token refresh — auto-refresh when token expires via `AuthInterceptor`
- [ ] **Task 86**: Apply `AuthGuard` to all `/wardrobe/**`, `/outfits/**`, `/social/**` routes

---

### 5.2 Layout & Navigation

#### Task 102 — Main Layout Component

- [ ] Sidebar (desktop) / Bottom nav (mobile) + main content area
- [ ] Nav items: Dashboard, Wardrobe, Outfits, Social, Weather, Profile
- [ ] Active route highlighting
- **Directory**: `src/outfit-planner-ui/src/app/presentation/layouts/main-layout/`

#### Task 103 — Navigation Component

- [ ] Responsive: Hamburger menu on mobile, persistent sidebar on desktop
- [ ] Icons for each section

#### Task 104 — Home Dashboard

- [ ] Overview cards: "Your Wardrobe" (item count), "Today's Outfit" (suggestion), "Active Polls" (count), "Weather" (current)
- [ ] Quick actions: Add Item, Build Outfit, Create Poll
- [ ] Recent activity feed

#### Task 105 — User Profile & Settings

- [ ] Display user info (name, email, style preferences)
- [ ] Edit style preferences (`StylePreference` enum values)
- [ ] Account settings (change password, privacy level)

---

### 5.3 Integration & API Polish

- [ ] **Task 106**: Verify all frontend services are connected to backend
- [ ] **Task 107**: Global error toasts using Angular Material Snackbar or similar
- [ ] **Task 108**: Loading spinners and skeleton screens on every page
- [ ] **Task 109**: Optimistic updates for quick wear, voting

---

### 5.4 UI/UX Refinement

- [ ] **Task 110**: Mobile-first responsive breakpoints for all pages
- [ ] **Task 111**: CSS transitions on route changes, card hovers, button clicks
- [ ] **Task 112**: ARIA labels, keyboard navigation, focus traps on modals
- [ ] **Task 113**: CSS custom properties design system (colors, spacing, typography, shadows)
- [ ] **Task 114**: Dark mode toggle with prefers-color-scheme support

---

### 5.5 Final Testing & Delivery

- [ ] **Task 118**: Angular component tests (Wardrobe Dashboard, Outfit Builder, Poll Detail)
- [ ] **Task 119**: E2E test: Login → Add Item → Build Outfit → Wear → Create Poll → Vote
- [ ] **Task 120**: All Swagger endpoint docs verified with XML comments
- [ ] **Task 121**: Deployment guide (Azure App Service / Docker)
- [ ] **Task 122**: README with local setup instructions
- [ ] **Task 123**: GitHub Actions CI/CD pipeline (build, test, deploy)
