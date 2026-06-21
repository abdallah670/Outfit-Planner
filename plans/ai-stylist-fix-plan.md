# AI Stylist — Diagnostic & Fix Plan

**Status:** 🔴 Broken end‑to‑end
**Date:** 2026‑06‑08
**Scope:** `OutfitPlanner.Api` (controllers, AI services), `outfit-planner-ui` (NgRx AI state + AI assistant page)

---

## 1. Executive Summary

The AI Stylist is **not reaching the LLM** in production today. Every user message falls into a hard‑coded fallback path that produces the same canned responses ("outfit jeans", "tell me more about hi"). On top of that:

- The chat‑session layer is half‑built server‑side and completely unwired client‑side.
- There is a **missing NgRx effect file** that should translate HTTP calls into `Success`/`Failure` actions.
- The front end never sends the `sessionId` back to the server, so every message becomes a brand‑new session.
- The reducer adds a duplicate empty user message and a malformed assistant message (`senderId: <guid>`).

**Who is right?** The user. The "model" is not bad — the model is never called. The integration is broken.

---

## 2. Root Cause Mapping (Symptom → File → Line)

| # | Symptom | File | Cause |
|---|---------|------|-------|
| 1 | "Outfit jeans" / "Top with jeans" for every outfit request | `src/OutfitPlanner.Infrastructure/Services/AI/LLMResponseGenerator.cs` L36‑45, 83‑88 | API key missing → exception caught → `GenerateFallbackResponse` returns canned text |
| 2 | "hi" → "tell me more about hi" | `LLMResponseGenerator.cs` L162 | `_` branch of fallback uses raw user message in canned template |
| 3 | Wardrobe never referenced | `LLMResponseGenerator.cs` L147‑170 + `ChatService.cs` L51 | Fallback ignores wardrobe; real LLM call requires API key; wardrobe only loaded for 3 specific intents |
| 4 | First message doesn't create a session | `src/OutfitPlanner.Persistence/...` + `ChatService.cs` L90, 108‑153 | Fire‑and‑forget `_ = PersistSessionAsync(...)` with `catch{}` swallows all errors → no DB row |
| 5 | Seeded sessions don't appear | `src/OutfitPlanner.Api/Controllers/AiChatController.cs` L40‑45 | `GetSessions()` returns hard‑coded `new List<object>()`; never queries the repository |
| 6 | Front end shows no reply / no sessions | Missing file: `src/outfit-planner-ui/src/app/core/state/ai/ai.effects.ts` | No effect dispatches `loadSessionsSuccess` / `sendMessageSuccess` |
| 7 | Every message = new session | `src/outfit-planner-ui/src/app/presentation/pages/ai-assistant/ai-assistant.component.ts` L84 | `sendMessage` dispatched **without** `sessionId` |
| 8 | Empty user bubble + assistant with garbled senderId | `src/outfit-planner-ui/src/app/core/state/ai/ai.reducer.ts` L18‑22 | `sendMessageSuccess` adds `{ content: '', senderId: 'user' }` user stub and `{ senderId: response.sessionId, … }` assistant (senderId should be `'ai'`) |
| 9 | `currentSessionId` null on first message | `ai.reducer.ts` `appendMessage` L51 | Uses `state.currentSessionId || ''` for `sessionId` field of new messages |

---

## 3. Confirmed Issues (Priority Order)

| # | Severity | Area | Issue |
|---|----------|------|-------|
| 1 | 🔴 Critical | Backend | No API key → real LLM never called |
| 2 | 🔴 Critical | Backend | `GetSessions` endpoint is a stub returning `[]` |
| 3 | 🔴 Critical | Frontend | Missing `AiEffects` (NgRx effect file) |
| 4 | 🔴 Critical | Frontend | `sendMessage` dispatched without `sessionId` |
| 5 | 🟠 High | Frontend | Reducer adds empty user stub + malformed assistant message |
| 6 | 🟠 High | Backend | `PersistSessionAsync` silently swallows exceptions |
| 7 | 🟡 Medium | Backend | Wardrobe context only loaded for 3 intents |
| 8 | 🟡 Medium | Backend | No greeting branch for "hi" / short messages |
| 9 | 🟡 Medium | Backend | Frontend only sends first 5 wardrobe items in prompt (acceptable) |

---

## 4. Fix Plan

### Phase 1 — Backend (server‑side correctness)

#### 4.1 `AiChatController.cs` — implement real session endpoints
- Replace stub `GetSessions` with call to `IChatSessionRepository.GetByUserAsync(userId)`.
- Add `GET /api/ai/sessions/{id}/messages` returning persisted `ChatMessage`s.
- Add a `[HttpGet("health")]` for quick smoke testing.

#### 4.2 `ChatService.cs` — fix persistence & default context
- Stop fire‑and‑forgetting `PersistSessionAsync`; await it (or move to a `BackgroundJob` that logs failures).
- Set `session.CreatedAt = DateTimeOffset.UtcNow` on new sessions.
- Replace silent `catch{}` with `Console.Error.WriteLine` or `ILogger`.
- Default `needsWardrobe = true`; allow LLM to decide whether to use it.
- Add a `_` (greeting) intent branch in the fallback that returns a friendly welcome referencing the user's name (from `context.UserId` or a future `DisplayName` lookup).

#### 4.3 `LLMResponseGenerator.cs` — refactor to support Gemini (and OpenAI) + greeting branch
**Provider:** the user has a **Gemini** key, so we need to wire Google's `generativelanguage` API. The current `LLMResponseGenerator` is hard‑wired to OpenAI's `chat/completions` schema, which will not work with Gemini.

Concrete changes:
- Add a `Provider` field to `AISettings` (`"Gemini"` or `"OpenAI"`, default `"Gemini"`).
- In `LLMResponseGenerator`, branch on `_settings.Provider`:
  - **Gemini path:**
    - Endpoint becomes `https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent?key={ApiKey}`.
    - No `Authorization` header; the key is in the query string.
    - Request body shape:
      ```json
      {
        "systemInstruction": { "parts": [{ "text": "<system prompt>" }] },
        "contents": [
          { "role": "user",      "parts": [{ "text": "<history + new user message>" }] },
          { "role": "model",     "parts": [{ "text": "<prior assistant message>" }] }
        ],
        "generationConfig": { "maxOutputTokens": 1024, "temperature": 0.7 }
      }
      ```
    - Response body shape: read `candidates[0].content.parts[0].text`.
    - For history, alternate `user` / `model` roles (Gemini rejects `assistant` and rejects consecutive same‑role turns — merge them with a newline if needed).
  - **OpenAI path:** keep the existing `messages[].content` format for backward compatibility.
- Add a "greeting" branch to `GenerateFallbackResponse` for `greeting` / `general` intents.
- On missing API key, log a clear warning and still return a useful fallback (don't throw).
- Include `intent.Intent`, `intent.Occasion`, `intent.WeatherCondition`, and a one‑line wardrobe summary in the **system** prompt, not just the user prompt.

`AISettings.cs` will get a new field:
```csharp
public string Provider { get; set; } = "Gemini";   // "Gemini" | "OpenAI"
public string ModelName { get; set; } = "gemini-1.5-flash";  // default for free tier
```
`appsettings.json` under `AI` will get:
```json
{
  "Provider": "Gemini",
  "ApiKey": "<USER_KEY_GOES_HERE>",
  "ModelName": "gemini-1.5-flash",
  "Endpoint": "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent",
  "MaxTokens": 1024,
  "Temperature": 0.7
}
```
**Tell the user to paste the Gemini key in `appsettings.json` under `AI:ApiKey`, or set the env var `GEMINI_API_KEY` (we'll also wire that env‑var read path).**

#### 4.4 `WardrobeContextBuilder.cs` — verify DB query
- Confirm it returns the **user's** items, not all items.
- Surface item count + a brief description ("You have 23 items: 6 tops, 4 bottoms, 3 dresses, 5 footwear, 5 accessories").
- Confirm `PrimaryColor` matches the entity (might need `Colors` array instead of a string).

#### 4.5 `IChatSessionRepository` — add missing methods
- `Task<List<ChatSession>> GetByUserAsync(string userId, int skip, int take)` (paged).
- `Task<List<ChatMessage>> GetMessagesBySessionAsync(Guid sessionId, int skip, int take)`.
- Confirm these are implemented in `ChatSessionRepository` (concrete).

---

### Phase 2 — Frontend (client‑side wiring)

#### 4.6 Create `src/outfit-planner-ui/src/app/core/state/ai/ai.effects.ts` (NEW FILE)
- `loadSessions$` effect: on `loadSessions` → call `AiDataSource.getSessions()` → dispatch `loadSessionsSuccess` (or `loadSessionsFailure`).
- `sendMessage$` effect: on `sendMessage` → call `AiDataSource.sendMessage(msg, sessionId)` → dispatch `sendMessageSuccess` (or `sendMessageFailure`).
- Register `AiEffects` in the `EffectsModule.forFeature([AiEffects])` array of the relevant module (check `app.config.ts` / `app.module.ts`).

#### 4.7 `ai-assistant.component.ts` — pass sessionId, fix `newSession`
- On send, select `currentSessionId` from the store and pass it:
  ```ts
  this.currentSessionId$.pipe(take(1)).subscribe(sid => {
    this.store.dispatch(AiActions.sendMessage({ message: msg, sessionId: sid ?? undefined }));
  });
  ```
- `newSession()` currently dispatches `clearCurrentSession` with `userId: ''`. Fix to use the actual user id from `selectUser`.

#### 4.8 `ai.reducer.ts` — fix `sendMessageSuccess`
- Do **not** add an empty user message (the component already dispatched `appendMessage`).
- Set assistant `senderId` to `'ai'`.
- Use `response.sessionId` for the `sessionId` field of the assistant message.

#### 4.9 `ai-assistant.component.html` — render sessions sidebar
- Add a side panel listing `sessions$` (most recent first).
- On click, call `AiActions.loadSessionMessages({ sessionId })` (new action).
- Show a clear "Welcome" empty state with the quick‑suggestion chips when `messages.length === 0`.

---

### Phase 3 — Polish & UX

#### 4.10 Better prompts
- Move wardrobe summary into the **system** prompt.
- Pass the last 3 exchanges (not just the last 3 messages) for better context continuity.

#### 4.11 Greeting handler
- In `IntentClassifier`, add a `greeting` intent with keywords `["hi", "hello", "hey", "good morning", "good evening"]`.
- In `GenerateFallbackResponse`, return: `"Hi there! 👋 I'm your AI fashion assistant. Ask me for outfit ideas, a wardrobe review, or trip packing help."`

#### 4.12 Telemetry / logging
- Log every LLM call: prompt, response, latency, token usage.
- Add a `/api/ai/health` endpoint that reports API key configured (yes/no), recent error count, and a sample completion.

---

## 5. Required User Inputs

| Question | Status / Why |
|----------|-----|
| Provider & key — confirmed: **Gemini** | Needed for #4.3 to switch the request/response format. User must paste the key into `appsettings.json` under `AI:ApiKey` (or set the `GEMINI_API_KEY` env var) before this is fully wired. |
| Preferred Gemini model — confirmed: **`gemini-1.5-flash`** (free tier, testing only) | Default model name. Will be set in `appsettings.json` and used in the `LLMResponseGenerator` request. |
| Auto‑create a session on the first message — confirmed | Backend `ChatService` must create a new `ChatSession` row when no `sessionId` is sent, using the user's first message as the **title** (truncated to 100 chars). Front end should treat any response with a new `sessionId` as the "current" session and re‑send it on subsequent messages. |
| Was the `AiEffects` file ever created, or has it never existed? | Tells me whether to search git history or create fresh. Will assume **never existed** and create from scratch unless user says otherwise. |
| Should the AI read the user's display name? | Tells me whether to add a `DisplayName` lookup in `WardrobeContextBuilder`. Will assume **no** (use friendly generic greeting) unless user says otherwise. |

---

## 6. Implementation Order (Recommended)

1. **Backend stub fix** (`AiChatController.GetSessions`) — unblocks session UI.
2. **NgRx effects** (`ai.effects.ts`) — unblocks everything UI‑side.
3. **Reducer fix** (`sendMessageSuccess`) — fixes duplicate/empty messages.
4. **Component fix** (pass `sessionId`) — fixes "new session per message" bug.
5. **Persistence fix** (`PersistSessionAsync`) — fixes missing seeded sessions on refresh.
6. **Greeting intent** — fixes "hi" UX.
7. **API key wiring** (env var) — turns on the real LLM (last because it depends on user having a key).
8. **Polish** (sidebar, better prompts, health endpoint).

---

## 7. Verification Checklist

After implementing the above, verify:

- [ ] `dotnet build` succeeds with 0 errors.
- [ ] `npm run build` succeeds with 0 errors.
- [ ] Logging in to the AI assistant loads previously seeded sessions.
- [ ] Sending "hi" returns a friendly greeting (not "tell me more about hi").
- [ ] Sending "suggest an outfit" uses wardrobe items by name.
- [ ] Sending a 2nd message in the same session stays in the same session (check server log: only one `ChatSession` row per conversation).
- [ ] Reloading the page restores the last conversation from the DB.
- [ ] "New session" button clears messages and starts a new DB session row.
- [ ] When the API key is missing, the UI shows a friendly fallback (not a 500 error).

---

## 8. Out of Scope (Future Improvements)

- Streaming responses (SSE) for the LLM.
- Image generation for outfit previews.
- Voice input/output.
- Personalization based on past sessions.
- Multi‑language support.
