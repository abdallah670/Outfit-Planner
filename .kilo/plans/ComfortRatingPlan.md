# ComfortRating Implementation Plan

## Goal
Add `ComfortRating` to outfit creation with a default value of 5, ensure mappings are complete, validators enforce 1–5 range, DB column is non-nullable with default 5, and UI surfaces the field in outfit creation and detail flows.

## Current State (verified)
- `Outfit` entity (`src/OutfitPlanner.Domain/Entities/Outfit.cs`) already has `int? ComfortRating { get; set; }`
- `UpdateOutfitDto` already has `int? ComfortRating { get; set; }`
- `CreateOutfitDto` does **not** have ComfortRating
- `OutfitDto` already has `int? ComfortRating { get; set; }`
- `MappingProfile`: `CreateMap<CreateOutfitDto, Outfit>()` does **not** map ComfortRating (so it stays null on create)
- `OutfitConfiguration` (`src/OutfitPlanner.Persistence/Configurations/OutfitConfiguration.cs`) has no ComfortRating config
- UI `outfit.entity.ts` does **not** expose `comfortRating`
- UI `outfit-builder.component.ts` (`saveOutfit`) does **not** send `comfortRating`
- UI `schedule-outfit-modal.component.ts` (`createOutfitFromItems` / `createOutfitWithPhoto`) does **not** send comfort rating

## Proposals

### 1. Make ComfortRating non-nullable with default 5
- Entity: change `int? ComfortRating` → `int ComfortRating { get; set; } = 5;`
- DB: add EF migration to set non-nullable, configure `.HasDefaultValue(5)` in `OutfitConfiguration`.
- This guarantees every outfit has a comfort rating, aligns with "default is 5".

### 2. Update CreateOutfitDto
- Add `public int ComfortRating { get; set; } = 5;`
- Frontend: add `comfortRating: 5` to create payloads.
- **Behavior note**: both `createOutfitWithImage` and `scheduleOutfitModal` POST to `/api/outfits` without a comfort rating field. Because the entity default is `5` and the DTO default is `5`, Omitting ComfortRating in those requests will result in `ComfortRating = 5` in the database.

### 3. Validators
- `CreateOutfitCommandValidator`: add rule for `ComfortRating` between 1 and 5.
- `UpdateOutfitCommandValidator`: add rule for `ComfortRating` between 1 and 5 when provided.

### 4. MappingProfile
- Update `CreateMap<CreateOutfitDto, Outfit>()` to map ComfortRating.
- Update `CreateMap<Outfit, OutfitDto>()` to map ComfortRating explicitly (currently implicit; add explicitly for clarity).
- Ensure reverse map `OutfitDto -> Outfit` maps ComfortRating if used in updates.

### 5. UI changes
- `outfit.entity.ts`: add `comfortRating?: number;`
- `outfit-builder.component.ts`: include `comfortRating` in `newOutfit` when saving.
- `schedule-outfit-modal.component.ts`:
  - Add comfort rating input to `createOutfitForm`.
  - Include `comfortRating` in `createOutfitRequest` and `FormData` for photo upload.
- Other places that construct `CreateOutfitDto` or `UpdateOutfitDto` payloads (search UI codebase for `/outfits` POST/PUT) should include comfortRating.

### 6. Backend handlers
- No handler changes needed because AutoMapper will map the new DTO property to the entity property once mapping is configured.
- `CreateOutfitCommandHandler` overrides item creation manually but the entity itself is mapped first, so default 5 on entity + DTO mapping covers it.

## Open Questions
1. **Range enforcement**: Should ComfortRating be strictly 1–5 integers, or allow decimals? Recommendation: `int` with `InclusiveBetween(1, 5)` to match typical 1–5 star UI.
2. **Editability**: Should users be able to update ComfortRating via `UpdateOutfitDto` (already supported), or is read-only after creation? Recommendation: allow updates through existing `UpdateOutfitCommand`.

## Risk / Edge Cases
- Existing outfits in DB have `ComfortRating` as nullable; migration will set them all to 5.
- Frontend components that display `OutfitDto` may need null-safe handling if any consumer hasn't been updated; since we’re making it non-nullable, it’s safe.

## Files Changed Summary
| File | Change |
|------|--------|
| `src/OutfitPlanner.Domain/Entities/Outfit.cs` | `int?` → `int` with default 5 |
| `src/OutfitPlanner.Application/DTOs/Outfit/CreateOutfitDto.cs` | + `ComfortRating` |
| `src/OutfitPlanner.Application/DTOs/Outfit/OutfitDto.cs` | explicit map (optional) |
| `src/OutfitPlanner.Application/Profiles/MappingProfile.cs` | map ComfortRating on create + entity→dto |
| `src/OutfitPlanner.Application/Features/Outfits/Requests/Commands/Validators/CreateOutfitCommandValidator.cs` | validate 1–5 |
| `src/OutfitPlanner.Application/Features/Outfits/Requests/Commands/Validators/UpdateOutfitCommandValidator.cs` | validate 1–5 when provided |
| `src/OutfitPlanner.Persistence/Configurations/OutfitConfiguration.cs` | `.HasDefaultValue(5)` |
| `src/outfit-planner-ui/src/app/domain/entities/outfit.entity.ts` | + `comfortRating` |
| `src/outfit-planner-ui/src/app/presentation/pages/outfit-builder/outfit-builder.component.ts` | send comfortRating |
| `src/outfit-planner-ui/src/app/presentation/components/calendar/schedule-outfit-modal/schedule-outfit-modal.component.ts` | + form field + payload |
| `src/outfit-planner-ui/src/app/presentation/pages/social/create-outfit-post/create-outfit-post.component.ts` | send comfortRating if it creates outfits |
| `src/OutfitPlanner.Api/Controllers/OutfitsController.cs` | ensure Swagger/model reflects ComfortRating (no code change if DTO auto-updates) |
