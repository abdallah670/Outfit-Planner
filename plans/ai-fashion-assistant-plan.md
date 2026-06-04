# Outfit-Planner: AI Fashion Assistant — Implementation Plan

> **Date:** 2026-05-23  
> **Scope:** Conversational AI fashion assistant — C# only (no Python)  
> **Source:** Extracted from `platform-comprehensive-review.md` (Section 3.3 + Phase 4)  
> **Dependencies:** Phase 1 (Auth/Roles) must be complete, Phase 4 AI builds on existing wardrobe/weather/services

---

## Table of Contents

1. [Vision & User Stories](#1-vision--user-stories)
2. [Architecture Overview](#2-architecture-overview)
3. [Why C# Only](#3-why-c-only)
4. [Backend Implementation](#4-backend-implementation)
5. [Frontend Implementation](#5-frontend-implementation)
6. [Implementation Phases](#6-implementation-phases)
7. [Existing Assets & Integration Points](#7-existing-assets--integration-points)

---

## 1. Vision & User Stories

### Core Vision

A conversational AI fashion assistant — like ChatGPT for your wardrobe. The user types natural language questions and gets answers based on **their own clothing items**, wear history, weather forecast, and style profile.

**NOT** a form-based outfit generator — this is a chat interface.

### User Stories — What Users Can Ask

| User Query | AI Response |
|------------|-------------|
| *"What should I wear for a rainy job interview tomorrow?"* | "I checked your wardrobe + tomorrow's weather (rain, 18°C). Your **Navy Blazer** matches well with **Grey Dress Pants** and **Brown Oxfords**. Add the **Beige Trench Coat** for rain protection. Style score: 88/100." |
| *"I have a date tonight, what goes with my blue jacket?"* | "Your **Blue Denim Jacket** pairs perfectly with the **White Linen Shirt** and **Dark Jeans** you have. Add **White Sneakers** for a casual date look. Want something more formal? Try the **Navy Blazer** instead." |
| *"Create a 5-day vacation outfit pack for a beach trip"* | "Based on your wardrobe + beach weather (28°C, sunny): **Day 1:** Linen Shirt + Shorts + Sandals. **Day 2:** White T-Shirt + Swim Trunks + Flip Flops. **Day 3:** Polo + Chinos + Boat Shoes. ... Need visuals too?" |
| *"What's missing in my wardrobe?"* | "Analysis of your 24 items: You have 8 tops but only 2 pairs of pants. Consider adding **Chinos** (versatile) and a **Light Jacket**. Your color palette is 60% neutral — try adding a **bold accessory**." |
| *"Rate this outfit I'm planning"* | "You want to wear: **Red Hoodie + Green Cargo Pants + White Sneakers**. Color harmony: 65/100 (red/green is complementary). Occasion fit: 72/100. Overall: 68/100. Try swapping the hoodie for a **Black T-Shirt** for a cleaner look." |
| *"My style has changed — what does my data say?"* | "In the last 3 months, you've worn **Streetwear** 60% of the time vs 20% before. Your most-worn colors shifted from neutrals to earth tones. Your style evolution score: **Casual → Streetwear**. Want me to suggest new items?" |

### Target Outcome

| Current State | Target State |
|---------------|--------------|
| "Today's Pick" selects a **random outfit** | AI-powered suggestion based on weather, style, wear history |
| No intelligence in outfit suggestions | Context-aware, personalized recommendations |
| No chat interface | Conversational UI at `/ai-assistant` |
| No color/style analysis | Color harmony scoring + style compatibility engine |
| All form-based | Natural language input |

---

## 2. Architecture Overview

### C#-Only AI Pipeline

```
┌──────────────────────────────────────────────────────────────┐
│                 Angular Frontend — Chat UI                     │
│  ┌────────────────────────────────────────────────────────┐   │
│  │  💬 AI Fashion Assistant (/ai-assistant)                │   │
│  │                                                         │   │
│  │  User: "What should I wear for a rainy interview?"     │   │
│  │  ┌────────────────────────────────────────────────┐    │   │
│  │  │ AI: "Based on your wardrobe + tomorrow's       │    │   │
│  │  │ weather (rain, 18°C)...                        │    │   │
│  │  │                                                │    │   │
│  │  │ 👔 Navy Blazer  👖 Grey Dress Pants           │    │   │
│  │  │ 👞 Brown Oxfords  🧥 Beige Trench Coat         │    │   │
│  │  │                                                │    │   │
│  │  │ Style Score: 88/100 ✅  [Save as Outfit]       │    │   │
│  │  └────────────────────────────────────────────────┘    │   │
│  │                                                         │   │
│  │  ┌────────────────────────────────────────────────┐    │   │
│  │  │  What should I wear?              [Send ➤]     │    │   │
│  │  └────────────────────────────────────────────────┘    │   │
│  │                                                         │   │
│  │  [Quick Suggestions:]                                   │   │
│  │  [Date night?] [Casual Friday] [Beach trip]            │   │
│  │  [What's missing?] [Rate my outfit]                    │   │
│  └────────────────────────────────────────────────────────┘   │
└──────────────────────────┬────────────────────────────────────┘
                           │ POST /api/ai/chat
                           │ { message, userId }
┌──────────────────────────▼────────────────────────────────────┐
│              .NET Backend — All C# Services                     │
│  ┌────────────────────────────────────────────────────────┐   │
│  │  AIChatController (/api/ai/chat)                       │   │
│  │  └─ ChatService (orchestrator)                         │   │
│  │                                                         │   │
│  │  ChatService Orchestration Pipeline:                    │   │
│  │                                                         │   │
│  │  Step 1: IntentClassifier                               │   │
│  │  ├─ Calls OpenAI/OpenRouter API via C# SDK              │   │
│  │  ├─ Prompt: Classify this message into:                 │   │
│  │  │  outfit_suggestion | outfit_rating | wardrobe_analysis│   │
│  │  │  trip_planning | style_query | general                │   │
│  │  └─ Returns: { intent, occasion?, weather?, items? }    │   │
│  │                                                         │   │
│  │  Step 2: WardrobeContextBuilder                         │   │
│  │  ├─ Queries user's wardrobe via DbContext (C#)          │   │
│  │  ├─ Filters by occasion/weather/season from intent      │   │
│  │  ├─ Checks WearEvents for recent usage                  │   │
│  │  ├─ Gets weather forecast from existing WeatherService  │   │
│  │  ├─ Gets UserStyleProfile from DbContext                │   │
│  │  └─ Returns: structured wardrobe context                │   │
│  │                                                         │   │
│  │  Step 3: ColorHarmonyService (C# pure math)             │   │
│  │  ├─ Converts hex colors → HSV                           │   │
│  │  ├─ Applies color wheel rules (monochromatic, etc.)     │   │
│  │  ├─ Returns: harmony score + explanation                │   │
│  │  └─ Packages as structured data for LLM prompt          │   │
│  │                                                         │   │
│  │  Step 4: StyleCompatibilityService (C# weighted math)   │   │
│  │  ├─ Scores occasion match (30%)                         │   │
│  │  ├─ Scores weather fit (20%)                            │   │
│  │  ├─ Scores color harmony from Step 3 (25%)              │   │
│  │  ├─ Scores style cohesion (15%)                         │   │
│  │  ├─ Scores layering logic (10%)                         │   │
│  │  └─ Returns: total score + breakdown                    │   │
│  │                                                         │   │
│  │  Step 5: OutfitCombinationService (C#)                  │   │
│  │  ├─ Generates valid combinations from filtered items    │   │
│  │  ├─ Ensures: 1 top + 1 bottom + 1 footwear ± outerwear │   │
│  │  ├─ Scores each via StyleCompatibilityService           │   │
│  │  ├─ Ranks by score, diversifies by style                │   │
│  │  └─ Returns: top 3 outfit combinations                  │   │
│  │                                                         │   │
│  │  Step 6: LLMResponseGenerator                           │   │
│  │  ├─ Builds structured prompt with:                      │   │
│  │  │  - User's original message                          │   │
│  │  │  - Detected intent                                  │   │
│  │  │  - Wardrobe context (filtered items)                │   │
│  │  │  - Top 3 outfit combinations with scores            │   │
│  │  │  - Color harmony + style breakdown                  │   │
│  │  │  - Chat history (last 5 messages)                   │   │
│  │  ├─ Calls OpenAI/OpenRouter via C# OpenAI SDK          │   │
│  │  └─ Returns: natural language response                 │   │
│  │                                                         │   │
│  │  Step 7: ChatHistoryCache                               │   │
│  │  ├─ Stores conversation history in IMemoryCache         │   │
│  │  ├─ TTL: 30 minutes since last message                  │   │
│  │  └─ Persists for context across messages               │   │
│  └────────────────────────────────────────────────────────┘   │
│                                                               │
│  NuGet Packages (new):                                        │
│  ├─ OpenAI — official C# SDK for LLM API calls               │
│  ├─ SkiaSharp — image analysis (dominant color extraction)   │
│  └─ Microsoft.Extensions.AI — standardized AI abstractions   │
└──────────────────────────────────────────────────────────────┘
```

### New Domain Entities

```csharp
// ChatSession — represents a conversation
public class ChatSession : BaseEntity
{
    public string UserId { get; set; }
    public string Title { get; set; }          // Auto-generated from first message
    public string Status { get; set; }         // Active, Archived
    public int MessageCount { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    
    // Navigation
    public ICollection<ChatMessage> Messages { get; set; }
}

// ChatMessage — individual message in a conversation
public class ChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public string SenderId { get; set; }       // User or "assistant"
    public string Content { get; set; }        // Message text
    public string Role { get; set; }           // "user" or "assistant"
    public string Intent { get; set; }         // Classified intent (optional)
    public string Metadata { get; set; }       // JSON: outfit suggestions, scores, etc.
    
    // Navigation
    public ChatSession Session { get; set; }
}
```

### New Files: Backend

| File | Path | Purpose |
|------|------|---------|
| `ChatSession.cs` | `src/OutfitPlanner.Domain/Entities/` | Chat session entity |
| `ChatMessage.cs` | `src/OutfitPlanner.Domain/Entities/` | Chat message entity |
| `ChatSessionConfiguration.cs` | `src/OutfitPlanner.Persistence/Configurations/` | EF Core config |
| `ChatMessageConfiguration.cs` | `src/OutfitPlanner.Persistence/Configurations/` | EF Core config |
| `IChatSessionRepository.cs` | `src/OutfitPlanner.Application/Contracts/Persistence/` | Repository interface |
| `ChatSessionRepository.cs` | `src/OutfitPlanner.Persistence/Repositories/` | Repository implementation |
| `IAiService.cs` | `src/OutfitPlanner.Application/Contracts/Services/` | AI service interface |
| `IIntentClassifier.cs` | `src/OutfitPlanner.Application/Features/AI/Interfaces/` | Intent classification interface |
| `IWardrobeContextBuilder.cs` | `src/OutfitPlanner.Application/Features/AI/Interfaces/` | Wardrobe context builder interface |
| `IColorHarmonyService.cs` | `src/OutfitPlanner.Application/Features/AI/Interfaces/` | Color harmony service interface |
| `IStyleCompatibilityService.cs` | `src/OutfitPlanner.Application/Features/AI/Interfaces/` | Style compatibility interface |
| `IOutfitCombinationService.cs` | `src/OutfitPlanner.Application/Features/AI/Interfaces/` | Outfit combination interface |
| `ILLMResponseGenerator.cs` | `src/OutfitPlanner.Application/Features/AI/Interfaces/` | LLM response generator interface |
| `IChatHistoryCache.cs` | `src/OutfitPlanner.Application/Features/AI/Interfaces/` | Chat history cache interface |
| `IntentClassifier.cs` | `src/OutfitPlanner.Application/Features/AI/Services/` | LLM-based intent classification |
| `WardrobeContextBuilder.cs` | `src/OutfitPlanner.Application/Features/AI/Services/` | Queries + filters wardrobe |
| `ColorHarmonyService.cs` | `src/OutfitPlanner.Application/Features/AI/Services/` | Pure C# color math |
| `StyleCompatibilityService.cs` | `src/OutfitPlanner.Application/Features/AI/Services/` | Weighted scoring engine |
| `OutfitCombinationService.cs` | `src/OutfitPlanner.Application/Features/AI/Services/` | Combination generator |
| `LLMResponseGenerator.cs` | `src/OutfitPlanner.Application/Features/AI/Services/` | Structured prompt + LLM call |
| `ChatHistoryCache.cs` | `src/OutfitPlanner.Application/Features/AI/Services/` | IMemoryCache wrapper |
| `ChatService.cs` | `src/OutfitPlanner.Application/Features/AI/Services/` | Orchestrator pipeline |
| `AIChatController.cs` | `src/OutfitPlanner.Api/Controllers/` | API endpoints |
| `ChatSessionDto.cs` | `src/OutfitPlanner.Application/DTOs/AI/` | Chat session DTOs |
| `ChatMessageDto.cs` | `src/OutfitPlanner.Application/DTOs/AI/` | Chat message DTOs |
| `AISettings.cs` | `src/OutfitPlanner.Application/Models/` | Already exists — update with LLM config |

### New Files: Frontend

| File | Path | Purpose |
|------|------|---------|
| `chat.entity.ts` | `src/domain/entities/` | ChatSession, ChatMessage interfaces |
| `chat.repository.ts` | `src/domain/repositories/` | Repository interface |
| `chat.usecases.ts` | `src/domain/usecases/` | Use cases |
| `chat.datasource.ts` | `src/data/datasources/` | API calls |
| `chat.repository.impl.ts` | `src/data/repositories/` | Repository implementation |
| `chat.actions.ts` | `src/core/state/ai/` | NgRx actions |
| `chat.reducer.ts` | `src/core/state/ai/` | NgRx reducer |
| `chat.effects.ts` | `src/core/state/ai/` | NgRx effects |
| `chat.selectors.ts` | `src/core/state/ai/` | NgRx selectors |
| `index.ts` | `src/core/state/ai/` | State barrel export |
| `ai-assistant.component.ts` | `src/presentation/pages/ai-assistant/` | Chat page component |
| `ai-assistant.component.html` | `src/presentation/pages/ai-assistant/` | Chat page template |
| `ai-assistant.component.scss` | `src/presentation/pages/ai-assistant/` | Chat page styles |
| `chat-message.component.ts` | `src/presentation/components/chat/` | Message bubble component |
| `chat-message.component.html` | `src/presentation/components/chat/` | Message bubble template |
| `chat-message.component.scss` | `src/presentation/components/chat/` | Message bubble styles |
| `outfit-card-inline.component.ts` | `src/presentation/components/chat/` | Inline outfit card |
| `outfit-card-inline.component.html` | `src/presentation/components/chat/` | Inline outfit card template |
| `chat-typing-indicator.component.ts` | `src/presentation/components/chat/` | Typing indicator |
| `chat-typing-indicator.component.scss` | `src/presentation/components/chat/` | Typing indicator styles |

---

## 3. Why C# Only

| Concern | Python Microservice | C# Only |
|---------|-------------------|---------|
| **Deployment** | Need Docker + container orchestration | Just one .NET app to deploy |
| **Latency** | Network call between .NET → Python adds ~5-20ms | In-process, no network overhead |
| **Auth** | Need JWT validation in both services | Single auth layer |
| **Monitoring** | Two services to log, trace, monitor | One service, one log stream |
| **Expertise** | Need C# + Python knowledge | All C# |
| **Maintenance** | Two build pipelines | One build pipeline |
| **LLM SDK** | OpenAI has official C# SDK too | Same quality as Python |
| **Color Math** | Pure math — trivial in any language | C# handles math equally well |
| **Image Processing** | OpenCV (Python) vs SkiaSharp (C#) | SkiaSharp is mature, cross-platform |

---

## 4. Backend Implementation

### 4.1 ColorHarmonyService — Pure C# Math

**File:** `src/OutfitPlanner.Application/Features/AI/Services/ColorHarmonyService.cs`

```csharp
public class ColorHarmonyService : IColorHarmonyService
{
    public ColorHarmonyResult CalculateHarmony(IEnumerable<string> hexColors)
    {
        // 1. Convert hex → HSV
        var hsvColors = hexColors.Select(HexToHsv).ToList();
        
        // 2. Score based on color wheel rules:
        //    - Monochromatic: same hue, different saturation/value  → 90-100
        //    - Complementary: opposite hues (180° apart)           → 80-90
        //    - Analogous: adjacent hues (30-60° apart)            → 70-80
        //    - Triadic: evenly spaced (120° apart)                → 60-70
        //    - Random: no clear relationship                      → 0-50
        
        // 3. Penalize clashes (e.g., same saturation/brightness = visual noise)
        // 4. Return score (0-100) + explanation
        
        return new ColorHarmonyResult
        {
            Score = harmonyScore,
            Scheme = detectedScheme,   // "Monochromatic", "Complementary", etc.
            Explanation = explanation  // "Your Navy (#000080) and Grey (#808080) create..."
        };
    }
    
    private HsvColor HexToHsv(string hex) { /* pure math */ }
}

public class HsvColor
{
    public double Hue { get; set; }        // 0-360
    public double Saturation { get; set; } // 0-100
    public double Value { get; set; }      // 0-100
    public string Hex { get; set; }
}
```

### 4.2 StyleCompatibilityService — Weighted Scoring

**File:** `src/OutfitPlanner.Application/Features/AI/Services/StyleCompatibilityService.cs`

```csharp
public class StyleCompatibilityService : IStyleCompatibilityService
{
    public StyleScoreResult CalculateScore(
        IEnumerable<ClothingItem> items, 
        OutfitContext context)
    {
        // Weighted scoring:
        // - Occasion match: 30%  (Casual outfit for casual occasion)
        // - Weather fit:    20%  (Light fabrics for hot weather)
        // - Color harmony:  25%  (From ColorHarmonyService)
        // - Style cohesion: 15%  (All items match user's style profile)
        // - Layering logic: 10%  (Valid layering combinations)
        
        return new StyleScoreResult
        {
            TotalScore = weightedTotal,
            Breakdown = new Dictionary<string, double>
            {
                ["Occasion Match"] = occasionScore,
                ["Weather Fit"] = weatherScore,
                ["Color Harmony"] = harmonyScore,
                ["Style Cohesion"] = cohesionScore,
                ["Layering Logic"] = layeringScore
            }
        };
    }
}
```

### 4.3 IntentClassifier — LLM-Powered

**File:** `src/OutfitPlanner.Application/Features/AI/Services/IntentClassifier.cs`

```csharp
public class IntentClassifier : IIntentClassifier
{
    private readonly OpenAIClient _openAi;
    
    public async Task<IntentResult> ClassifyAsync(string message)
    {
        // Prompt: "Classify this fashion-related message into one of:
        // outfit_suggestion, outfit_rating, wardrobe_analysis,
        // trip_planning, style_query, general
        // Return JSON: { intent, occasion?, weather?, items? }"
        
        var result = await _openAi.Chat.Completions.CreateAsync(prompt);
        return JsonSerializer.Deserialize<IntentResult>(result);
    }
}
```

### 4.4 ChatService — Orchestrator Pipeline

**File:** `src/OutfitPlanner.Application/Features/AI/Services/ChatService.cs`

```csharp
public class ChatService : IChatService
{
    public async Task<ChatResponse> ProcessMessageAsync(string userId, string message, Guid? sessionId)
    {
        // Step 1: Classify intent
        var intent = await _intentClassifier.ClassifyAsync(message);
        
        // Step 2: Build wardrobe context
        var context = await _wardrobeContextBuilder.BuildAsync(userId, intent);
        
        // Step 3: Calculate color harmony
        var harmony = _colorHarmonyService.CalculateHarmony(context.SelectedColors);
        
        // Step 4: Score combinations
        var combinations = _outfitCombinationService.Generate(context, harmony);
        
        // Step 5: Generate LLM response
        var response = await _llmResponseGenerator.GenerateAsync(
            message, intent, context, combinations, sessionId);
        
        // Step 6: Cache history
        await _chatHistoryCache.SaveAsync(sessionId, message, response);
        
        return new ChatResponse
        {
            Message = response.Text,
            OutfitSuggestions = combinations.Take(3).Select(c => new OutfitSuggestionDto
            {
                Items = c.Items,
                Score = c.Score,
                Breakdown = c.Breakdown
            }),
            SuggestedActions = response.Actions // "Save as Outfit", "Try different", etc.
        };
    }
}
```

### 4.5 AIChatController

**File:** `src/OutfitPlanner.Api/Controllers/AIChatController.cs`

```
POST   /api/ai/chat                    → ProcessMessage (send + get response)
GET    /api/ai/chat/sessions           → List user's chat sessions
GET    /api/ai/chat/sessions/{id}      → Get session messages (cursor-paginated)
DELETE /api/ai/chat/sessions/{id}      → Delete/archive session
POST   /api/ai/chat/sessions/{id}/save-outfit/{combinationIndex} → Save suggested outfit
```

### 4.6 Configuration — Update AISettings

**File:** `src/OutfitPlanner.Application/Models/AISettings.cs` (already exists — extend it)

```csharp
public class AISettings
{
    // Existing
    public string Provider { get; set; }      // "OpenAI" or "OpenRouter"
    public string Model { get; set; }         // "gpt-4o" or "gpt-4o-mini"
    public string ApiKey { get; set; }
    public string Endpoint { get; set; }      // https://api.openai.com or https://openrouter.ai
    
    // New
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 1000;
    public int ChatHistoryMaxMessages { get; set; } = 10;
    public int CacheMinutes { get; set; } = 30;
}
```

### 4.7 Database Migration

Add migration for `ChatSession` and `ChatMessage` tables:

```sql
CREATE TABLE ChatSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId NVARCHAR(450) NOT NULL,
    Title NVARCHAR(200),
    Status NVARCHAR(20) DEFAULT 'Active',
    MessageCount INT DEFAULT 0,
    LastActivityAt DATETIMEOFFSET,
    CreatedAt DATETIMEOFFSET NOT NULL
);

CREATE TABLE ChatMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SessionId UNIQUEIDENTIFIER NOT NULL,
    SenderId NVARCHAR(450) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Intent NVARCHAR(50),
    Metadata NVARCHAR(MAX),
    CreatedAt DATETIMEOFFSET NOT NULL,
    FOREIGN KEY (SessionId) REFERENCES ChatSessions(Id)
);
```

### 4.8 Dependency Injection

**File:** `src/OutfitPlanner.Infrastructure/DependencyInjection.cs`

```csharp
// AI Services
services.AddScoped<IIntentClassifier, IntentClassifier>();
services.AddScoped<IWardrobeContextBuilder, WardrobeContextBuilder>();
services.AddScoped<IColorHarmonyService, ColorHarmonyService>();
services.AddScoped<IStyleCompatibilityService, StyleCompatibilityService>();
services.AddScoped<IOutfitCombinationService, OutfitCombinationService>();
services.AddScoped<ILLMResponseGenerator, LLMResponseGenerator>();
services.AddSingleton<IChatHistoryCache, ChatHistoryCache>();
services.AddScoped<IChatService, ChatService>();

// OpenAI Client
services.AddOpenAIClient(configuration["AI:ApiKey"]);
```

---

## 5. Frontend Implementation

### 5.1 NgRx State — AI Module

**Actions:**

| Action | Trigger | Effect |
|--------|---------|--------|
| `sendMessage` | User sends chat message | Calls POST /api/ai/chat, dispatches `messageSent` |
| `messageSent` | API response received | Adds assistant response to chat state |
| `loadSessions` | User opens AI page | Calls GET /api/ai/chat/sessions |
| `sessionsLoaded` | Sessions loaded | Sets sessions in state |
| `loadSessionMessages` | User clicks a session | Calls GET /api/ai/chat/sessions/{id} |
| `sessionMessagesLoaded` | Messages loaded | Sets messages in state |
| `saveOutfitSuggestion` | User clicks "Save as Outfit" | Calls POST .../save-outfit/{index} |
| `outfitSaved` | Save successful | Shows success toast |
| `clearChat` | User clears chat | Resets current session state |

**State Shape:**

```typescript
interface AiState {
  currentSession: ChatSession | null;
  sessions: ChatSession[];
  messages: ChatMessage[];
  isLoading: boolean;
  isTyping: boolean;  // Show typing indicator
  error: string | null;
}
```

### 5.2 Chat UI — AiAssistantComponent is mathc to /Design/ai.html

**Layout:**
```
┌─────────────────────────────────────────────┐
│  💬 AI Fashion Assistant          [⚙] [✕]   │
├─────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────┐│
│  │ AI: "Based on your wardrobe + weather  ││
│  │ tomorrow (rain, 18°C)...               ││
│  │                                         ││
│  │ 👔 Navy Blazer   👖 Grey Pants         ││
│  │ 👞 Brown Oxfords  🧥 Trench Coat       ││
│  │                                         ││
│  │ Style Score: 88/100  [💾 Save Outfit]  ││
│  └─────────────────────────────────────────┘│
│                                             │
│  ┌─────────────────────────────────────────┐│
│  │ User: What should I wear for a rainy   ││
│  │ job interview tomorrow?                ││
│  └─────────────────────────────────────────┘│
│                                             │
│  ┌─────────────────────────────────────────┐│
│  │ AI: "Great choice! The Navy Blazer     ││
│  │ pairs well with..."                    ││
│  └─────────────────────────────────────────┘│
│                                             │
│  [📋 Date night?] [🏖 Beach trip]          │
│  [📊 What's missing?] [⭐ Rate my outfit]  │
│                                             │
├─────────────────────────────────────────────┤
│  [💬 Type your fashion question...] [Send ➤]│
└─────────────────────────────────────────────┘
```

**Features:**
- Message bubbles with distinct user/AI styling
- Typing indicator while AI generates response
- Inline outfit cards with clothing item images
- "Save as Outfit" button per suggestion
- Quick suggestion buttons (predefined prompts)
- Scroll-to-bottom on new messages
- Session history sidebar (optional, desktop)

### 5.3 Route Registration

```typescript
// In app.routes.ts
{
  path: 'ai-assistant',
  component: AiAssistantComponent,
  canActivate: [authGuard],
  title: 'AI Fashion Assistant'
}
```

### 5.4 Floating "Ask AI" Button

Add a floating action button on:
- **Home page** — quick access to AI assistant
- **Wardrobe page** — "Ask about this item"
- **Outfit builder** — "Let AI help you build"

```html
<button class="ask-ai-fab" (click)="openAiAssistant()">
  <span class="ai-icon">✨</span>
  <span class="ai-label">Ask AI</span>
</button>
```

---

## 6. Implementation Phases

### Phase 1: Foundation — C# AI Services (Week 1)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Install NuGet packages: `OpenAI`, `SkiaSharp`, `Microsoft.Extensions.AI`. Create `Services/AI/` directory with all interfaces. Create `ChatSession` and `ChatMessage` entities + EF configuration | 🟢 Easy |
| **Day 2** | Implement `ColorHarmonyService` — HSV conversion, color wheel rules, harmony scoring (pure C# math). Unit test with known color combinations | 🟡 Medium |
| **Day 3** | Implement `StyleCompatibilityService` — weighted scoring: occasion (30%), weather (20%), color harmony (25%), style cohesion (15%), layering (10%). Unit test | 🟡 Medium |
| **Day 4** | Implement `OutfitCombinationService` — generates valid combinations from filtered wardrobe (1 top + 1 bottom + 1 footwear ± outerwear), scores each, ranks top 3 | 🟡 Medium |
| **Day 5** | Create migration for ChatSession/ChatMessage tables. Wire up repositories. Test with EF Core | 🟡 Medium |

### Phase 2: LLM Integration & Pipeline (Week 2)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Configure OpenAI/OpenRouter client. Implement `IntentClassifier` — call LLM to classify message intent into: outfit_suggestion, outfit_rating, wardrobe_analysis, trip_planning, style_query, general | 🟡 Medium |
| **Day 2** | Implement `WardrobeContextBuilder` — queries wardrobe via DbContext, filters by occasion/weather/season, checks WearEvents for recent usage, gets weather forecast from existing WeatherService, gets UserStyleProfile | 🟡 Medium |
| **Day 3** | Implement `LLMResponseGenerator` — builds structured prompts with wardrobe context + scored outfits, calls OpenAI SDK, returns natural language response. This is the hardest part — prompt engineering matters | 🔴 Hard |
| **Day 4** | Implement `ChatHistoryCache` — stores/retrieves conversation history using `IMemoryCache` with 30-minute TTL. Create `ChatService` — orchestrates the full pipeline (Steps 1-7) | 🟡 Medium |
| **Day 5** | Create `AIChatController` with all endpoints. Update `AISettings` model. Register all services in DI. End-to-end API test with Postman | 🟡 Medium |

### Phase 3: Frontend Implementation (Week 3)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Create AI NgRx state module (actions, reducer, effects, selectors). Create chat datasource, repository, use cases | 🟡 Medium |
| **Day 2** | Create `AiAssistantComponent` — full chat page at `/ai-assistant` with message bubble list, input field, send button | 🟡 Medium |
| **Day 3** | Create `ChatMessageComponent` — message bubble with user/AI styling. Create typing indicator. Add scroll-to-bottom behavior | 🟡 Medium |
| **Day 4** | Add quick suggestion buttons (predefined prompts). Add inline outfit card preview component with "Save as Outfit" button | 🟡 Medium |
| **Day 5** | Add session history sidebar. Wire up session CRUD (list, load, delete). Wire up "Save as Outfit" → calls outfit creation API | 🟡 Medium |

### Phase 4: Integration & Polish (Week 4)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Replace random "Today's Pick" with AI-powered suggestion. Home page shows AI-recommended outfit instead of random | 🟡 Medium |
| **Day 2** | Add floating "Ask AI" button on Home, Wardrobe, and Outfit Builder pages. Modal or slide-out panel for quick access | 🟡 Medium |
| **Day 3** | Add SkiaSharp dominant color extraction on clothing image upload — auto-tag primary color. Display color tags in wardrobe | 🟡 Medium |
| **Day 4** | End-to-end testing: send message → classify → build context → generate outfits → score → LLM response → display in UI → save outfit | 🟡 Medium |
| **Day 5** | Error handling: graceful fallbacks for LLM errors (cached responses, error messages). Loading states. Empty state for new users | 🟡 Medium |

---

## 7. Existing Assets & Integration Points

### Backend Assets Available

| Asset | How It's Used |
|-------|---------------|
| `WeatherService` | Provides real-time weather data for context-aware suggestions |
| `UserStyleProfile` | Style preferences, colors, fit — feeds into StyleCompatibilityService |
| `WearEvent` | Recent wear history — wardrobe context builder filters recently worn items |
| `ClothingItem` | Full item data (color, type, category, brand, fabric) — all used in outfit combinations |
| `Outfit` | Existing outfits — AI can reference/suggest existing combinations |
| `ColorHarmonyService` | Placeholder — will be the new C# pure math service |
| `AISettings` | Existing model — will be extended with LLM config |
| `OutfitImageProcessingService` | Image processing — used for thumbnail generation in outfit cards |

### Frontend Assets Available

| Asset | How It's Used |
|-------|---------------|
| `authGuard` | Route protection for `/ai-assistant` |
| Existing NgRx patterns | Follow same pattern for AI state module |
| Existing outfit cards | Reuse styling for inline outfit card component |
| `sweetalert2` | Toast notifications for save success/error |
| Existing form validation patterns | Reuse for any AI-related forms |

### Integration Points

| Feature | Integration |
|---------|-------------|
| **Today's Pick → AI Suggestion** | Replace `GetTodaysPickHandler.cs` random logic with AI-powered suggestion or use AI fallback |
| **Save as Outfit** | Call existing `CreateOutfitCommand` from AI chat context |
| **Weather Data** | Already fetched by `WeatherService` — just inject into `WardrobeContextBuilder` |
| **User Style Profile** | Already exists in DB — `WardrobeContextBuilder` queries via `IUserRepository` |

---

> **Key Decision: C# Only (No Python)**  
> Everything runs inside the existing .NET project. LLM calls use the OpenAI C# SDK. Color math and style scoring are pure C#. Image analysis uses SkiaSharp. Chat history uses IMemoryCache.
>
> **Dependencies:** Phase 1 (Auth/Roles) should be complete before starting AI work, but AI can begin independently of Phases 2-3 if needed.