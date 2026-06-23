# Implement Chat Session History and Pagination

This plan addresses the issue where clicking a chat session does not load its messages, and adds pagination to load the last 20 messages at a time with a "Load More" feature.

## User Review Required

Please review the backend approach to pagination. I'll modify the `GetSessionMessages` endpoint to support `page` and `pageSize` parameters.

## Proposed Changes

### Backend (`OutfitPlanner.Api` & `OutfitPlanner.Persistence`)

#### [MODIFY] [IChatSessionRepository.cs](file:///f:/Meno/Outfit-Planner/src/OutfitPlanner.Application/Contracts/Persistence/IChatSessionRepository.cs)
- Add a new method `Task<List<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId, int skip, int take);` to efficiently query messages.

#### [MODIFY] [ChatSessionRepository.cs](file:///f:/Meno/Outfit-Planner/src/OutfitPlanner.Persistence/Repositories/ChatSessionRepository.cs)
- Implement `GetMessagesBySessionIdAsync` to query `ChatMessage` directly, order by `CreatedAt` descending (to get the latest), apply `Skip()` and `Take()`, and then order back by `CreatedAt` ascending for the UI.

#### [MODIFY] [AiChatController.cs](file:///f:/Meno/Outfit-Planner/src/OutfitPlanner.Api/Controllers/AiChatController.cs)
- Update `GetSessionMessages(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)` to use the new repository method.

### Frontend (`outfit-planner-ui`)

#### [MODIFY] Core Services (`ai.datasource.ts`, `ai.repository.ts`, `ai.usecases.ts`, `ai.repository.impl.ts`)
- Update `getSessionMessages(sessionId: string, page: number = 1, pageSize: number = 20): Observable<ChatMessage[]>` to send the pagination query parameters.

#### [MODIFY] State Management (`ai.actions.ts`, `ai.reducer.ts`, `ai.state.ts`, `ai.effects.ts`)
- **Actions**: Add `selectSession`, `loadMessages`, `loadMessagesSuccess` (which will include a boolean `hasMore` or just infer it from the length), and `loadMessagesFailure`.
- **State**: Add `currentPage` and `hasMoreMessages` properties.
- **Reducer**: 
  - On `selectSession`: Set `currentSessionId`, reset page to 1, clear messages.
  - On `loadMessagesSuccess`: If `page === 1`, replace messages. If `page > 1`, prepend the older messages to the array. Update `hasMoreMessages` based on the response array length.
- **Effects**: Map `loadMessages` to the usecase and dispatch success/failure.

#### [MODIFY] UI Component (`ai-assistant.component.ts` & `ai-assistant.component.html`)
- **HTML**: Add `(click)="selectSession(session.id)"` to the session list item in the sidebar.
- **HTML**: Add a "Load More Messages" button inside `.messages-container` when `hasMoreMessages$` is true.
- **TS**: Add the `selectSession` and `loadMoreMessages` handlers that dispatch the appropriate actions.

## Verification Plan

### Automated Tests
- Run `dotnet build` to ensure all C# code compiles correctly.
- Run `npm run build` in the UI directory to ensure Angular compiles correctly.

### Manual Verification
- Test creating a session and sending multiple messages.
- Verify clicking on a session successfully displays its messages.
- Verify that only up to 20 messages load initially.
- Verify clicking "Load More" fetches and prepends the older messages successfully.
