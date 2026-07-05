# Fix AI Outfit Save Functionality

## Problem Summary

The "Save Outfit" button fails because:
1. Template doesn't pass `suggestion` data to `executeAction`
2. Frontend sends `OutfitSuggestion` as JSON string in FormData
3. ASP.NET Core `[FromForm]` cannot auto-deserialize `List<OutfitSuggestionDto>` from JSON string
4. Backend receives null and `HandleSaveOutfit` returns "No valid wardrobe items to save"

## Solution

### Option A: Simple Fix - Use ClothingItemIds Directly (Recommended)

Change the frontend datasource to send item IDs as an array (which `[FromForm]` handles natively).

### Step 1: Update Backend ChatCommand.cs
**File**: `src/OutfitPlanner.Application/Features/AI/Requests/Commands/ChatCommand.cs`

Either:
- Remove `OutfitSuggestion` property (use existing `ClothingItemIds`)
- Or keep it but use `[JsonPropertyName]` for proper deserialization

Since `ClothingItemIds` already exists and `HandleSaveOutfit` supports it, we can just use that.

### Step 2: Update Frontend ai.datasource.ts
**File**: `src/outfit-planner-ui/src/app/data/datasources/ai.datasource.ts`

Replace lines 22-24:
```typescript
// BEFORE:
if (outfitSuggestion) {
  formData.append('OutfitSuggestion', JSON.stringify(outfitSuggestion));
}

// AFTER:
if (outfitSuggestion?.items?.length) {
  outfitSuggestion.items.forEach(item => {
    if (item.id) formData.append('ClothingItemIds', item.id);
  });
}
```

### Step 3: Update Frontend Template ai-assistant.component.html
**File**: `src/outfit-planner-ui/src/app/presentation/pages/ai-assistant/ai-assistant.component.html`

Line 140:
```html
<!-- BEFORE -->
<button class="btn-save-outfit" (click)="executeAction('Save Outfit')">

<!-- AFTER -->
<button class="btn-save-outfit" (click)="executeAction('Save Outfit', suggestion)">
```

### Step 4: Update Backend ChatCommandHandler.cs (Already Done)
The `HandleSaveOutfit` method already handles `request.ClothingItemIds` as priority fallback.

## Verification Steps

1. Click "Save Outfit" on a suggestion card
2. Verify: outfit saved successfully
3. Check saved outfits list shows the new outfit