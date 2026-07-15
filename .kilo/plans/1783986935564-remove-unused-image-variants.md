# Remove Unused Image Variants (keep only original path)

## Context

Every clothing-item upload currently writes **4 files** to disk via
`LocalFileStorageService.UploadImageAsync`:
`original`, `_thumb`, `_medium`, `_large` (generated in
`ImageProcessingService.ProcessImageAsync`).

Investigation shows the frontend **renders** images with the **original** path
everywhere, but `ThumbnailUrl` is still generated, stored, and lightly consumed:

- `WardrobeController` (lines 141–142, 181–182) stores both `ImageUrl` (original)
  and `ThumbnailUrl` (thumbnail); it **never stores or reads `MediumPath`/`LargePath`**.
- `ThumbnailUrl` is used (secondary only): `OutfitListDto.ThumbnailUrl` is mapped
  from the first item's `ThumbnailUrl` (`MappingProfile.cs:115`); in the UI
  `wardrobe.service.ts:131` URL-fixes `thumbnailUrl`, and
  `wardrobe-dashboard.component.ts:106` uses it as a fallback
  (`imageUrl || thumbnailUrl || placeholder`). `outfit-card` `itemThumbnailUrls`
  uses `clothingItemImageUrl` (the original), NOT `thumbnailUrl`.
- **`_medium` and `_large` are the only fully-dead files** — zero readers anywhere.
- `GetThumbnailUrl` has **no callers** anywhere in the codebase.
- Outfit images are a separate path (`OutfitImageGeneratorService`) that combines
  item originals into a single image — not affected by this change.

Per decision: keep only the original path. The thumbnail file will no longer be
generated, so `ThumbnailUrl` becomes empty unless explicitly re-mapped from the
original (see step 6).

`ThumbnailUrl` is still mapped for outfit lists in `MappingProfile.cs:115`, and
the `ClothingItem.ThumbnailUrl` DB column exists but would become empty.

**Decision (user):** keep only the original path; stop generating the extra variants.

## Goal

On upload, generate and persist only the original image. Remove the dead
thumbnail/medium/large generation and the now-unused plumbing, while keeping
outfit thumbnails resolving from the original image.

## Changes (backend — `src/OutfitPlanner.Api`, `src/OutfitPlanner.Infrastructure`, `src/OutfitPlanner.Application`)

1. **`Infrastructure/Services/ImageProcessingService.cs`** (`ProcessImageAsync`):
   - Remove generation of `result.Thumbnail`, `result.Medium`, `result.Large`.
   - Keep only `result.Original` (JPEG-converted from the stream).

2. **`Application/Models/ProcessedImage.cs`**:
   - Remove `Thumbnail`, `Medium`, `Large` properties; keep `Original`.
   - Simplify `Dispose()` to dispose only `Original`.

3. **`Application/Models/ImageUploadResult.cs`**:
   - Remove `ThumbnailPath`, `MediumPath`, `LargePath`.
   - Update `Successful(...)` to take only `originalPath` (drop the three path args).

4. **`Infrastructure/Services/LocalFileStorageService.cs`** (`UploadImageAsync`):
   - Save only the original file; return `ImageUploadResult.Successful(originalPath, ...)`.
   - Remove `_Thumb`/`_Medium`/`_Large` suffix constants and the unused
     `GetThumbnailUrl` method.
   - Remove `GetThumbnailUrl` from `IImageStorageService` (no callers).

5. **`Api/Controllers/WardrobeController.cs`** (Create + Update clothing item):
   - Set **both** `request.ImageUrl` and `request.ThumbnailUrl` to
     `uploadResult.OriginalPath`.
     Rationale: repointing `ThumbnailUrl` to the original keeps every existing
     consumer working unchanged (fallback + URL-normalization) with no UI edits.
   - There is no medium/large field on `ClothingItem`, so nothing else to set.

6. **`Application/Profiles/MappingProfile.cs:115`** — no change required.
   `OutfitListDto.ThumbnailUrl` already maps from `ClothingItem.ThumbnailUrl`,
   which now holds the original path, so it resolves correctly.

## Changes (frontend — `src/outfit-planner-ui`)

7. **No frontend changes required.** With `ThumbnailUrl` repointed to the original,
   `wardrobe-dashboard.component.ts:106` (`imageUrl || thumbnailUrl || placeholder`)
   and `wardrobe.service.ts:131` (URL-fix) behave identically.
   Optional cleanup: the `thumbnailUrl ||` fallback in `wardrobe-dashboard` can be
   dropped later, but it is not necessary.

## Optional cleanup

8. `Infrastructure/Configuration/ImageStorageSettings.cs` `ThumbnailSettings`
   (Thumbnail/Medium/Large sizes + qualities) is now unused — may be removed
   (extra JSON keys in `appsettings.json` are ignored, so no breakage).
9. Existing orphaned `_thumb`/`_medium`/`_large` files on disk from prior uploads
   can be left as-is (new uploads won't create them). Optionally add a one-off
   cleanup script if disk space matters.

## Validation

- `dotnet build` for the API/Infrastructure/Application projects.
- Run tests: `OutfitPlanner.Application.UnitTests`, `OutfitPlanner.Application.IntegrationTests`.
- Manual: upload a clothing item → `uploads/{userId}/{imageId}/` contains exactly
  one `.jpg`; item displays in wardrobe and daily-pick; outfit card still shows an
  image (mapped from original).

## Risks / notes

- No DB migration needed: `ThumbnailUrl` column is kept (just left empty).
- DTOs (`ClothingItemDto`, `ClothingItemListDto`, `OutfitListDto`) keep their
  `ThumbnailUrl` field, now populated with the original path — so no consumer
  breaks and no frontend edits are needed.
- Profile pictures go through `UploadProfilePictureAsync` (no variants) — unaffected.
