# AI Integration Plan for Outfit-Planner

> **Goal**: Add intelligent, AI-powered outfit handling to the Outfit-Planner platform — from smart recommendations to image recognition and personal style learning.

---

## Table of Contents
1. [Current State Analysis](#1-current-state-analysis)
2. [AI Architecture Overview](#2-ai-architecture-overview)
3. [Phase 1: Python AI Microservice](#3-phase-1-python-ai-microservice)
4. [Phase 2: Core AI Services & Models](#4-phase-2-core-ai-services--models)
5. [Phase 3: Backend API Integration](#5-phase-3-backend-api-integration)
6. [Phase 4: Frontend AI Features](#6-phase-4-frontend-ai-features)
7. [Phase 5: Advanced Features](#7-phase-5-advanced-features)
8. [Technology Stack & Dependencies](#8-technology-stack--dependencies)
9. [Project Structure](#9-project-structure)
10. [Implementation Roadmap](#10-implementation-roadmap)

---

## 1. Current State Analysis

### What Exists Today
| Feature | Implementation | Status |
|---------|---------------|--------|
| Outfit Suggestions | Random selection from wardrobe | ❌ No AI |
| Today's Pick | Random outfit, no intelligence | ❌ No AI |
| Color Analysis | Hex codes stored, never analyzed | ❌ No AI |
| Style Compatibility | No compatibility scoring | ❌ Missing |
| Image Tagging | Manual only | ❌ Missing |
| Weather Integration | Weather fetched but not used for smart suggestions | ⚠️ Partial |
| Wear Patterns | Tracked but no insights generated | ❌ No AI |
| User Style Profile | Stored but never used for ML | ❌ No AI |

### Key Domain Entities for AI
```
ClothingItem → Color, Category, Type, Fabric, Brand, Images, Tags
Outfit → Items, Occasion, Season, Weather, Ratings, TimesWorn
WearEvent → Item, Weather, Duration, Rating, Notes
UserStyleProfile → Style, Colors, Fit, Comfort Priority
```

---

## 2. AI Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular Frontend                         │
│  [AI Outfit Generator] [Smart Suggestions] [Style Score]    │
└──────────────────────┬──────────────────────────────────────┘
                       │ HTTP / REST
┌──────────────────────▼──────────────────────────────────────┐
│               .NET Backend (OutfitPlanner.Api)                │
│  ┌────────────────────────────────────────────────────────┐  │
│  │        AI Integration Gateway (C# Service Layer)       │  │
│  │  - AIServiceClient (HTTP client to Python microservice)│  │
│  │  - ColorHarmonyService (fallback if Python unavailable)│  │
│  │  - RecommendationCache (caches AI results)             │  │
│  └──────────────────────┬─────────────────────────────────┘  │
└─────────────────────────┼────────────────────────────────────┘
                          │ HTTP / REST (localhost:8000)
┌─────────────────────────▼────────────────────────────────────┐
│               Python AI Microservice (FastAPI)                │
│  ┌────────────┐  ┌──────────────┐  ┌────────────────────┐   │
│  │ Image      │  │ Color        │  │ Style             │   │
│  │ Analyzer   │  │ Harmony      │  │ Compatibility     │   │
│  │ Service    │  │ Engine       │  │ Engine            │   │
│  └────────────┘  └──────────────┘  └────────────────────┘   │
│  ┌────────────┐  ┌──────────────┐  ┌────────────────────┐   │
│  │ Outfit     │  │ Personal     │  │ Trending          │   │
│  │ Generator  │  │ Style        │  │ Predictor         │   │
│  │ Service    │  │ Profiler     │  │ (ML model)        │   │
│  └────────────┘  └──────────────┘  └────────────────────┘   │
│  ┌────────────────────────────────────────────────────────┐  │
│  │             ML Models (scikit-learn / ONNX)            │  │
│  │  - Style Similarity Model                              │  │
│  │  - Outfit Rating Predictor                             │  │
│  │  - Occasion Classifier                                 │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## 3. Phase 1: Python AI Microservice

### 3.1 Project Setup

```
src/
└── OutfitPlanner.AI/                    # New Python microservice
    ├── requirements.txt
    ├── Dockerfile
    ├── main.py                          # FastAPI entry point
    ├── config.py                        # Configuration
    ├── services/
    │   ├── __init__.py
    │   ├── image_analyzer.py            # Color extraction, pattern detection
    │   ├── color_harmony.py             # Color wheel compatibility
    │   ├── style_compatibility.py       # Style matching engine
    │   ├── outfit_generator.py          # Smart outfit combination
    │   ├── personal_profiler.py         # User style learning
    │   └── trending_predictor.py        # ML-based trending prediction
    ├── models/
    │   ├── __init__.py
    │   ├── schemas.py                   # Pydantic models
    │   └── ml_models/                   # Trained ML models
    ├── utils/
    │   ├── __init__.py
    │   ├── color_utils.py               # HSV/RGB/HEX conversions
    │   └── fashion_rules.py             # Domain-specific rules
    └── tests/
```

### 3.2 requirements.txt

```txt
fastapi==0.115.0
uvicorn[standard]==0.30.0
scikit-learn==1.5.0
opencv-python-headless==4.10.0
pillow==10.4.0
numpy==1.26.0
pandas==2.2.0
httpx==0.27.0
pydantic==2.8.0
python-multipart==0.0.9
colorgram.py==2.0.0
webcolors==24.8.0
joblib==1.4.0
```

### 3.3 API Endpoints (FastAPI)

| Method | Endpoint | Description | Input | Output |
|--------|----------|-------------|-------|--------|
| POST | `/api/v1/analyze-clothing` | Analyze clothing image | Image file | Colors, category, style tags |
| POST | `/api/v1/color-harmony` | Score color compatibility | List of hex codes | Score (0-100), explanation |
| POST | `/api/v1/compatibility-score` | Score outfit compatibility | Clothing items + metadata | Score, breakdown, suggestions |
| POST | `/api/v1/generate-outfits` | Generate top outfit combinations | Wardrobe items + context | Ranked outfit list |
| POST | `/api/v1/personalized-pick` | ML-based daily pick | User profile + weather + wardrobe | Best outfit recommendation |
| POST | `/api/v1/analyze-style` | Analyze user's style | User's wardrobe and history | Style profile, preferences |
| GET  | `/api/v1/health` | Health check | - | Status |

---

## 4. Phase 2: Core AI Services & Models

### 4.1 Color Harmony Engine (color_harmony.py)

**Algorithm**:
1. Convert all hex colors → HSV (Hue, Saturation, Value)
2. Apply color harmony rules from color theory:

| Rule | Description | Score Boost |
|------|-------------|-------------|
| **Monochromatic** | Same hue, varying saturation/value | +30 |
| **Analogous** | Adjacent on color wheel (30-60°) | +25 |
| **Complementary** | Opposite on color wheel (180°) | +20 |
| **Triadic** | 120° apart on color wheel | +20 |
| **Tetradic** | Two complementary pairs | +15 |
| **Neutral Base** | Black/white/gray/beige + any | +10 |
| **Clashing** | Too close but not matching (15-25°) | -15 |
| **High Contrast Clash** | Multiple bright saturated colors | -20 |

**Implementation**:
```python
# Pseudocode for ColorHarmonyEngine
def score_color_compatibility(colors: List[str]) -> Dict:
    hsv_colors = [hex_to_hsv(c) for c in colors]
    
    # Check for neutral dominance
    neutral_count = sum(1 for c in hsv_colors if is_neutral(c))
    # Check hue differences
    hue_diffs = compute_hue_differences(hsv_colors)
    # Apply rules
    harmony_type = classify_harmony(hue_diffs)
    score = harmony_type.score + neutral_bonus(neutral_count)
    
    return {
        "score": min(100, score),
        "harmony_type": harmony_type.name,
        "dominant_color": find_dominant(hsv_colors),
        "suggested_accent": suggest_accent_color(hsv_colors)
    }
```

### 4.2 Style Compatibility Engine (style_compatibility.py)

**Factors Considered**:
1. **Occasion Match** (30% weight): Dress code vs outfit formality
2. **Weather Fit** (20% weight): Fabric type + coverage vs temperature
3. **Color Harmony** (25% weight): From Color Harmony Engine
4. **Style Cohesion** (15% weight): All items match a style category
5. **Layering Logic** (10% weight): Correct layering order

**Implementation**:
```python
def score_outfit_compatibility(outfit_data: Dict) -> Dict:
    occasion = outfit_data.get("occasion")
    weather = outfit_data.get("weather")
    items = outfit_data.get("items", [])
    
    scores = {}
    scores["occasion_match"] = score_occasion(items, occasion)
    scores["weather_fit"] = score_weather(items, weather)
    scores["color_harmony"] = score_colors([i["color"] for i in items])
    scores["style_cohesion"] = score_style_consistency(items)
    scores["layering"] = score_layering(items)
    
    total = weighted_average(scores, {
        "occasion_match": 0.30,
        "weather_fit": 0.20,
        "color_harmony": 0.25,
        "style_cohesion": 0.15,
        "layering": 0.10
    })
    
    return {
        "total_score": round(total, 2),
        "breakdown": scores,
        "compatible": total >= 60,
        "suggestions": generate_improvements(scores)
    }
```

### 4.3 Outfit Generator Service (outfit_generator.py)

**Algorithm**: Constraint-based generation + compatibility scoring

```
Input: Wardrobe items, occasion, weather, season, user preferences

Step 1: Filter items by season/weather
Step 2: Categorize into Top, Bottom, Footwear, Outerwear
Step 3: Generate all valid combinations (1 top + 1 bottom + 1 footwear ± outerwear)
Step 4: Score each combination (using Style Compatibility Engine)
Step 5: Filter by minimum score threshold
Step 6: Rank by score, diversify by style/color variety
Step 7: Return top N results (default 10)

Output: [Outfit1 (score: 92), Outfit2 (score: 88), ...]
```

### 4.4 Personal Style Profiler (personal_profiler.py)

**Learning from User Data**:
- **Preferred Colors**: Analyze most-worn colors from WearEvents
- **Style Preferences**: Most common occasion types, categories
- **Fabric Preferences**: Most common fabric types
- **Fit Preferences**: From UserStyleProfile + worn items
- **Brand Affinity**: Most worn brands
- **Seasonal Patterns**: Which items worn in which seasons

**User Style Fingerprint**:
```python
{
    "user_id": "guid",
    "color_palette": ["#3b82f6", "#1f2937", "#ffffff"],  # Top 3 colors
    "preferred_styles": ["Casual", "Streetwear"],
    "avoided_styles": ["Formal"],
    "fabric_preferences": ["Cotton", "Denim"],
    "weather_adaptability": 0.75,  # 0-1 scale
    "risk_taking": 0.3,  # How experimental with combinations
    "occasion_confidence": {"Casual": 0.9, "Formal": 0.4},
    "signature_look": "Casual denim with neutral tops"
}
```

### 4.5 ML Models (scikit-learn)

#### A. Style Similarity Model
```python
# Uses cosine similarity on style feature vectors
# Features: color_histogram, category_vector, fabric_vector, occasion_vector
# Trained on user's liked/saved outfits
from sklearn.metrics.pairwise import cosine_similarity

def find_similar_outfits(target_outfit, all_outfits, n=5):
    target_vec = extract_features(target_outfit)
    all_vecs = [extract_features(o) for o in all_outfits]
    similarities = cosine_similarity([target_vec], all_vecs)[0]
    top_indices = np.argsort(similarities)[-n:][::-1]
    return [all_outfits[i] for i in top_indices]
```

#### B. Outfit Rating Predictor
```python
# Predicts how a user would rate an outfit (1-5 stars)
# Features: occasion, weather, color_count, category_types, fabric_types, times_worn_avg
# Target: comfort_rating, style_rating
from sklearn.ensemble import RandomForestRegressor

model = RandomForestRegressor(n_estimators=100, max_depth=10)
# Trained on historical ratings from WearEvents + ValidationPoll votes
```

#### C. Occasion Classifier
```python
# Given clothing items, predict best occasion match
# Features: item_types, colors, fabrics, formality_score
from sklearn.linear_model import LogisticRegression

occasions = ["Casual", "Formal", "Business", "Sport", "Date", "Travel"]
classifier = LogisticRegression(multi_class='multinomial')
# Trained on labeled outfits from the database
```

---

## 5. Phase 3: Backend API Integration

### 5.1 New .NET Service Layer

```
src/OutfitPlanner.Application/
└── Services/
    └── AI/
        ├── IAIServiceClient.cs              # Interface
        ├── AIServiceClient.cs               # HTTP client to Python service
        ├── IColorHarmonyService.cs          # Interface (fallback)
        ├── ColorHarmonyService.cs           # C# local implementation
        ├── IRecommendationCache.cs          # Caching interface
        └── RecommendationCache.cs           # In-memory + Redis cache
```

### 5.2 New DTOs

```csharp
// src/OutfitPlanner.Application/DTOs/AI/

// Request DTOs
public class AnalyzeClothingRequest { IFormFile Image; }
public class CompatibilityRequest { List<Guid> ClothingItemIds; string? Occasion; WeatherDto? Weather; }
public class GenerateOutfitsRequest { Guid UserId; string? Occasion; string? Weather; int Count = 10; }
public class PersonalizedPickRequest { Guid UserId; WeatherDto? Weather; }

// Response DTOs
public class ColorAnalysisResult { string DominantColor; float Confidence; List<ColorInfo> Colors; }
public class CompatibilityResult { float TotalScore; Dictionary<string, float> Breakdown; List<string> Suggestions; }
public class GeneratedOutfitDto { Guid OutfitId; float Score; List<ClothingItemDto> Items; string Description; }
public class StyleProfileResult { string DominantStyle; List<string> ColorPalette; Dictionary<string, float> StyleScores; }
```

### 5.3 New API Controllers

```csharp
// src/OutfitPlanner.Api/Controllers/AIController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIController : ControllerBase
{
    // POST /api/ai/analyze-clothing → Upload image, get color/style analysis
    // POST /api/ai/generate-outfits → Generate smart outfit combinations
    // POST /api/ai/compatibility-score → Score existing outfit
    // GET  /api/ai/personalized-pick → ML-based daily pick
    // POST /api/ai/analyze-style → Analyze user's wardrobe style
}
```

### 5.4. Fallback Strategy

When Python microservice is unavailable:
1. Use C# `ColorHarmonyService` as fallback (basic color wheel rules)
2. Use random generation as last resort
3. Cache AI results to avoid repeated calls
4. Graceful degradation: AI features become suggestions, not requirements

---

## 6. Phase 4: Frontend AI Features

### 6.1 New UI Pages/Components

```
src/outfit-planner-ui/src/app/
└── presentation/
    └── pages/
        └── ai-outfit-generator/           # New page
            ├── ai-outfit-generator.component.ts
            ├── ai-outfit-generator.component.html
            └── ai-outfit-generator.component.scss
    └── components/
        └── shared/
            ├── ai-style-score/            # Style score badge
            │   ├── style-score.component.ts
            │   ├── style-score.component.html
            │   └── style-score.component.scss
            ├── ai-outfit-card/            # Enhanced outfit card with AI scores
            │   ├── ai-outfit-card.component.ts
            │   ├── ai-outfit-card.component.html
            │   └── ai-outfit-card.component.scss
            └── ai-suggestion-panel/       # AI suggestion sidebar
                ├── suggestion-panel.component.ts
                ├── suggestion-panel.component.html
                └── suggestion-panel.component.scss
```

### 6.2 Feature Descriptions

#### A. AI Outfit Generator Page (`/ai-generator`)
- Select occasion, weather, mood
- Click "Generate Outfits" → AI suggests top 10 combinations
- Each outfit shows: Style Score (0-100), Color Harmony breakdown
- Preview outfit with all items displayed
- "Add to Favorites" / "Save as Outfit" buttons
- "Explain why this works" → AI-generated fashion reasoning

**UI Mockup Flow**:
```
┌─────────────────────────────────────────────────┐
│  🔮 AI Outfit Generator                          │
│                                                   │
│  Occasion: [▼ Casual]  Weather: [☀️ Sunny 28°C]  │
│  Mood: [▼ Confident]  Season: [▼ Summer]         │
│                                                   │
│  [✨ Generate Outfits]                             │
│                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────┐ │
│  │ Outfit #1     │  │ Outfit #2     │  │ ...     │ │
│  │ Score: 92/100 │  │ Score: 88/100 │  │         │ │
│  │ [Items]       │  │ [Items]       │  │         │ │
│  │ [Save] [Why]  │  │ [Save] [Why]  │  │         │ │
│  └──────────────┘  └──────────────┘  └─────────┘ │
│                                                   │
│  ⚡ Quick Filters: [x] Casual  [ ] Formal         │
│  [x] Work  [ ] Date                                 │
└─────────────────────────────────────────────────┘
```

#### B. Smart "Complete the Look" Button
- On the wardrobe page, selecting 1-2 items shows "Complete the Look"
- AI suggests what's missing (e.g., "Add a jacket for work")
- Generates full outfit with selected + suggested items

#### C. Style Score Badge
- Every outfit shows: `🎯 Style Score: 85/100`
- Color-coded: Green (80+), Yellow (60-79), Red (<60)
- Hover tooltip shows breakdown

#### D. AI Daily Pick (Replaces Current)
- On home/dashboard page
- Shows "Today's AI Pick" with weather-aware reasoning
- "This outfit works because: Sunny day + Business meeting → Navy blazer matches perfectly"

#### E. Wardrobe Gap Analyzer
- "What's Missing in Your Wardrobe?"
- AI analyzes existing items and suggests purchases
- "You have 3 tops but only 1 pair of pants. Try adding more bottoms!"

### 6.3 New NgRx State

```typescript
// State: ai
interface AIState {
  generatedOutfits: GeneratedOutfit[];
  compatibilityScores: { [outfitId: string]: CompatibilityResult };
  styleProfile: StyleProfile | null;
  loading: boolean;
  error: string | null;
}

// Actions
class GenerateOutfits { constructor(public params: GenerateParams) {} }
class GenerateOutfitsSuccess { constructor(public outfits: GeneratedOutfit[]) {} }
class GetCompatibilityScore { constructor(public outfitId: string) {} }
class AnalyzeMyStyle {}
```

### 6.4 New Route

```typescript
{ path: 'ai-generator', component: AiOutfitGeneratorComponent, canActivate: [AuthGuard] },
```

---

## 7. Phase 5: Advanced Features

### 7.1 Trending Predictor (ML Model)
- **Goal**: Predict which outfits will trend based on:
  - Current fashion trends (seasonal)
  - User's past trending interactions
  - Similar users' preferences (collaborative filtering)
  - Weather patterns
- **Output**: "This outfit has +30% trending potential this week"

### 7.2 Style Evolution Tracker
- Track how user's style changes over time
- "Your style shifted from Streetwear → Smart Casual this month"
- Monthly style report with visualizations

### 7.3 AI Fashion Assistant (Chat)
- Natural language queries:
  - "What should I wear for a rainy job interview?"
  - "I have a date tonight, what goes with my blue jacket?"
  - "Create a vacation outfit pack for 5 days"
- Uses LLM (OpenAI / local model) + context from wardrobe

### 7.4 Virtual Try-On Preview (Future)
- Generate images of outfits on a model
- Show how items look together before assembling physically
- Requires: Stable Diffusion / DALL-E integration

---

## 8. Technology Stack & Dependencies

### Python Microservice
| Library | Version | Purpose |
|---------|---------|---------|
| FastAPI | 0.115+ | REST API framework |
| Uvicorn | 0.30+ | ASGI server |
| scikit-learn | 1.5+ | ML models (RandomForest, LogisticRegression) |
| OpenCV | 4.10+ | Image processing, color analysis |
| Pillow | 10.4+ | Image manipulation |
| NumPy | 1.26+ | Numerical operations |
| Pandas | 2.2+ | Data manipulation |
| Pydantic | 2.8+ | Data validation |
| joblib | 1.4+ | Model serialization |

### .NET Backend (New NuGet Packages)
| Package | Purpose |
|---------|---------|
| Microsoft.Extensions.Http.Polly | Resilience for AI service calls |
| Microsoft.Extensions.Caching.Memory | Cache AI results |

### Angular Frontend (New npm Packages)
| Package | Purpose |
|---------|---------|
| ngx-color-picker | Color picker for AI suggestions |
| chart.js + ng2-charts | Style analytics charts |

---

## 9. Project Structure (Final)

```
Outfit-Planner/
├── src/
│   ├── OutfitPlanner.AI/              ← NEW: Python microservice
│   │   ├── main.py
│   │   ├── config.py
│   │   ├── requirements.txt
│   │   ├── services/
│   │   │   ├── image_analyzer.py
│   │   │   ├── color_harmony.py
│   │   │   ├── style_compatibility.py
│   │   │   ├── outfit_generator.py
│   │   │   ├── personal_profiler.py
│   │   │   └── trending_predictor.py
│   │   ├── models/
│   │   │   ├── schemas.py
│   │   │   └── ml_models/
│   │   └── utils/
│   │       ├── color_utils.py
│   │       └── fashion_rules.py
│   │
│   ├── OutfitPlanner.Api/
│   │   └── Controllers/
│   │       └── AIController.cs        ← NEW: AI endpoints
│   │
│   ├── OutfitPlanner.Application/
│   │   └── Services/
│   │       └── AI/
│   │           ├── IAIServiceClient.cs
│   │           ├── AIServiceClient.cs
│   │           ├── IColorHarmonyService.cs
│   │           ├── ColorHarmonyService.cs
│   │           ├── IRecommendationCache.cs
│   │           └── RecommendationCache.cs
│   │
│   └── outfit-planner-ui/
│       └── src/app/
│           ├── presentation/
│           │   ├── pages/
│           │   │   └── ai-outfit-generator/    ← NEW
│           │   └── components/
│           │       └── shared/
│           │           ├── ai-style-score/     ← NEW
│           │           └── ai-suggestion-panel/ ← NEW
│           └── core/
│               └── state/ai/                   ← NEW: NgRx state
│                   ├── ai.actions.ts
│                   ├── ai.reducer.ts
│                   ├── ai.effects.ts
│                   └── ai.selectors.ts
```

---

## 10. Implementation Roadmap

### Phase 1: Foundation (Week 1)
```
Day 1-2:  Set up Python FastAPI microservice skeleton
          - main.py with all endpoints (stubs)
          - Dockerfile + docker-compose
          - Integration with .NET backend (AIServiceClient)

Day 3-4:  Implement ColorHarmony Engine (Python + C# fallback)
          - HSV color conversion utilities
          - Color wheel rule scoring
          - API endpoint: /api/v1/color-harmony

Day 5:    Implement basic Compatibility Scoring
          - Occasion matching logic
          - Weather-based filtering
          - API endpoint: /api/v1/compatibility-score
```

### Phase 2: Core AI (Week 2)
```
Day 1-2:  Image Analyzer Service
          - Extract dominant colors from uploaded images
          - Basic category detection from visual features
          - API endpoint: /api/v1/analyze-clothing

Day 3-4:  Outfit Generator
          - Constraint-based combination generation
          - Scoring + ranking pipeline
          - API endpoint: /api/v1/generate-outfits

Day 5:    Personal Style Profiler
          - Analyze user's wardrobe + wear history
          - Generate style fingerprint
          - API endpoint: /api/v1/analyze-style
```

### Phase 3: Backend Integration (Week 3)
```
Day 1-2:  .NET AIServiceClient
          - HTTP client with Polly retry policy
          - Deserialization + error handling
          - AI fallback to local C# implementation

Day 3:    AIController endpoints
          - All endpoints wired up
          - Authorization + validation

Day 4:    Recommendation Caching
          - In-memory cache with TTL
          - Cache invalidation strategy

Day 5:    Testing + edge cases
          - Unit tests for fallback logic
          - Integration tests for AI pipeline
```

### Phase 4: Frontend (Week 4)
```
Day 1-2:  NgRx AI state module
          - Actions, Reducer, Effects, Selectors
          - Data sources + repository

Day 3-4:  AI Outfit Generator page
          - UI with occasion/weather/mood selectors
          - Results display with style scores
          - Save outfit functionality

Day 5:    AI enhancements across existing pages
          - Style score badges on outfit cards
          - "Complete the Look" button in wardrobe
          - AI-powered daily pick on home page
```

### Phase 5: ML & Advanced (Week 5+)
```
Week 5:   Train ML models
          - Style Similarity Model (cosine similarity)
          - Outfit Rating Predictor (RandomForest)
          - Collect training data from existing ratings

Week 6:   Advanced features
          - Wardrobe Gap Analyzer
          - Trending Predictor
          - Style Evolution Tracker

Future:   AI Fashion Assistant (LLM integration)
          - Natural language outfit queries
          - ChatGPT / local LLM integration
          - Conversational outfit recommendations
```

---

## Appendix A: Color Harmony Reference

```
Color Wheel Degrees:
        Red (0°)
           \
      Orange(30°)  Red-Orange(15°)
           \
     Yellow(60°)
           \
     Green(120°)
           \
      Blue(240°)
           \
     Violet(270°)
           \
      Purple(300°)

Harmony Types:
  Monochromatic: Same hue (0° diff) → Elegant, cohesive
  Analogous: 30-60° apart → Harmonious, soothing
  Complementary: 180° apart → High contrast, bold
  Split-Complementary: Base + 150° + 210° → Balanced contrast
  Triadic: 120° apart each → Vibrant, balanced
  Tetradic: 60° + 180° + 240° → Rich, complex
  Square: 90° apart each → Dynamic, balanced
```

## Appendix B: Weather-to-Outfit Mapping

| Weather | Temperature | Recommended Fabrics | Recommended Colors |
|---------|-------------|-------------------|-------------------|
| ☀️ Sunny Hot | >30°C | Cotton, Linen, Silk | Light, pastel, white |
| ☀️ Warm | 25-30°C | Cotton, Linen | Bright, light |
| ⛅ Mild | 20-25°C | Cotton, Denim | Versatile |
| ☁️ Cool | 15-20°C | Denim, Polyester | Earth tones |
| 🌧️ Rainy | 10-15°C | Wool, Polyester | Dark, navy |
| ❄️ Cold | 5-10°C | Wool, Fleece | Dark, warm |
| 🥶 Freezing | <5°C | Wool, Down, Faux Fur | Dark, jewel tones |

---

*This document serves as the complete specification for adding AI capabilities to the Outfit-Planner platform. Each phase builds on the previous one, allowing for incremental delivery and testing.*