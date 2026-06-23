# Fix: Save AI Outfit Suggestions Persistently

## Problem Statement
When users return to old AI chat sessions, outfit suggestions show only names without images. The suggestions are generated with image data during chat, but when persisted to the database, the outfit suggestion data (including item image URLs) is not saved.

## Root Cause
In `ChatService.cs:PersistSessionAsync()`, the AI response message is saved to `ChatMessage.Metadata` without outfit suggestions. The `Metadata` property exists on `ChatMessage` entity but is never populated with the `OutfitSuggestions` from the LLM response.

## Solution: Save Outfit Suggestions to Database + Enable "Save Outfit" Action

### Backend Changes

1. **Modify `ChatService.cs:PersistSessionAsync`** to save outfit suggestions in the metadata:
   - Serialize `OutfitSuggestions` and `SuggestedActions` to the `Metadata` JSON field when saving AI messages
   - This preserves the suggestion data for later retrieval

2. **Implement actual "Save Outfit" functionality** in `ChatCommandHandler.cs:HandleSaveOutfit`:
   - Create a real `Outfit` entity in the database
   - Add `OutfitItem` records linking to the clothing items
   - Generate combined image using `IImageCombinationService`
   - Return proper response with saved outfit details

### Frontend Changes

3. **Update AI reducer** (`ai.reducer.ts`) to properly parse saved metadata on load (already partially done, but ensure consistency)

4. **Implement outfit save API call** in `ai.datasource.ts`:
   - Add endpoint to call `api/outfits/{outfitId}/combined-image` to get/generate outfit preview

5. **Fix image URL resolution** in `ai-assistant.component.ts`:
   - Ensure `resolveImageUrl` works for all image URL formats (relative vs absolute)

## Implementation Order

1. Fix persistence in `ChatService.cs` to store outfit suggestions in Metadata
2. Implement actual "Save Outfit" backend logic in `ChatCommandHandler.cs`
3. Verify frontend display works correctly for persisted sessions
4. Test that "Save Outfit" action creates a real saved outfit

## Files to Modify

- `src/OutfitPlanner.Infrastructure/Services/AI/ChatService.cs` - Save outfit suggestions to metadata
- `src/OutfitPlanner.Application/Features/AI/Handlers/Commands/ChatCommandHandler.cs` - Implement actual save outfit logic
- `src/outfit-planner-ui/src/app/data/datasources/ai.datasource.ts` - Add combined image endpoint call
- `src/outfit-planner-ui/src/app/presentation/pages/ai-assistant/ai-assistant.component.ts` - Wire up save action