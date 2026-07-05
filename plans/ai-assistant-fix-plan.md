# AI Assistant Fix Plan — Image Upload, Triple Render & Suggestion Cards

This plan fixes 3 bugs in the AI Assistant feature and updates the outfit suggestion card UI to match `Design/ai.html`.

---
## Bug 0: Save outfit is not work

## Bug 1: Outfit suggestions rendered 3 times (NG0955)

### Root Cause
`ai-assistant.component.html` uses `@for` with `track` on non-unique values:
```html
@for (action of message.suggestedActions; track action)
@for (suggestion of message.outfitSuggestions; track suggestion.rank)
```

Angular 21's `@for` requires unique tracking keys. When `suggestedActions` contains duplicate strings (e.g., `["Save outfit", "Save outfit"]`) or `suggestion.rank` values collide, Angular throws **NG0955** and re-runs change detection **3 times**, causing the suggestion cards to render three times before succeeding.

### Fix
**File**: `ai-assistant.component.html`

Replace all `track <property>` with `track $index` on every `@for` loop:

| Current | Fix |
|---------|-----|
| `track action` | `track $index` |
| `track suggestion.rank` | `track $index` |
| `track message.id` | `track $index` |
| `track f.file.name` (image preview) | `track $index` |

---

## Bug 2: Image preview broken after file selection

### Root Cause
`ai-assistant.component.ts` — `onFileSelected()` method (line 90–109) uses `FileReader.readAsDataURL()` which fires `reader.onload` asynchronously **outside Angular's zone**. The `this.attachedFiles.push()` call doesn't trigger change detection, so the DOM never updates with the preview `<img>`.

### Fix
**File**: `ai-assistant.component.ts`

1. Inject `ChangeDetectorRef` in constructor
2. Call `this.cdRef.detectChanges()` inside `reader.onload` after push

Changes:
```typescript
// In constructor
constructor(
  private store: Store,
  private cdRef: ChangeDetectorRef  // ADD
) {}

// In onFileSelected() — inside reader.onload
reader.onload = () => {
  this.attachedFiles.push({
    file: file,
    preview: reader.result as string
  });
  this.cdRef.detectChanges();  // ADD
};
```

---

## Bug 3: Images not sent with the message

### Root Cause
`sendMessage()` dispatches `AiActions.sendMessage` but may not include the `images: File[]` parameter. The `AiActions.sendMessage` action payload definition might be missing `images` field.

### Fix
**File**: `ai-assistant.component.ts` — `sendMessage()` method

Ensure images are passed:
```typescript
const files = this.attachedFiles.map(f => f.file);
this.store.dispatch(AiActions.sendMessage({
  message: this.message,
  sessionId: this.selectedSessionId,
  images: files  // ADD
}));
```

**File**: `ai.actions.ts`

Ensure the `sendMessage` action accepts `images: File[]`:
```typescript
sendMessage: props<{ message: string; sessionId?: string; images?: File[] }>(),
```

---

## Bug 4: Old chat sessions show no outfit images

### Root Cause
`ai.repository.impl.ts` — `enrichMessage()` parses stored JSON metadata but uses wrong property casing. The backend `ChatService.cs` serializes with anonymous types that may produce PascalCase (`Id`, `Name`, `ImageUrl`) but the mapping expects specific casing.

### Fix
**File**: `ai.repository.impl.ts`

Update `enrichMessage()` to handle both PascalCase and camelCase:
```typescript
items: s.items?.map((item: any) => ({
  id: item.Id ?? item.id,
  name: item.Name ?? item.name,
  type: item.Type ?? item.type,
  imageUrl: item.ImageUrl ?? item.imageUrl ?? '',
  hexColor: item.HexColor ?? item.hexColor ?? '#ccc'
)) ?? []
**File**: `ai-assistant.component.html`

Replace the raw text suggestion rendering with a card component matching `Design/ai.html`. The "Save Outfit" button lives **only** inside the card footer — it is removed from `suggestedActions` chips to avoid duplication.

```
┌──────────────────────────────────────────────┐
│ ✦ Recommended Outfit           Outfit Name   │
├────────────────┬─────────────────────────────┤
│ [thumbnail 44px]│ [thumbnail 44px]           │
│ OUTERWEAR      │ SUITING                     │
│ The Madison    │ Classic Navy                │
│ Trench         │ Wool Blazer                 │
├────────────────┼─────────────────────────────┤
│ [thumbnail 44px]│ [thumbnail 44px]           │
│ TROUSERS       │ FOOTWEAR                    │
│ Grey Dress     │ Brown Calf                  │
│ Trousers       │ Leather Oxfords             │
├────────────────┴─────────────────────────────┤
│ ( 88 ) Style Score    [ 💾 Save Outfit ]     │ ← Save button ONLY here
└──────────────────────────────────────────────┘

[Date night?] [Casual Friday] [Beach trip] [What's missing?]  ← chips (no "Save outfit")
```

**Important**: The `suggestedActions` chips below the card must filter out any action that matches "save" (case-insensitive) since the Save button is now in the card footer.

Key HTML structure to add:
```html
<div class="bg-surface border border-border rounded-md overflow-hidden">
  <!-- Header -->
  <div class="px-4 py-3 border-b border-border flex items-center justify-between bg-background">
    <div class="flex items-center gap-2">
      <span class="text-secondary">[wand icon]</span>
      <span class="text-xs font-semibold text-text-primary uppercase tracking-wider">Recommended Outfit</span>
    </div>
    <span class="text-xs text-muted-foreground">Outfit Name</span>
  </div>
  
  <!-- 2-column grid of items -->
  <div class="grid grid-cols-2">
    <div *ngFor="let item of suggestion.items" class="p-4 flex gap-3 items-center border-b border-border border-r border-border">
      <div class="w-11 h-11 rounded-md overflow-hidden bg-background border border-border flex-shrink-0">
        <img [src]="item.imageUrl || 'assets/placeholder.png'" class="w-full h-full object-cover">
      </div>
      <div class="min-w-0 flex-1">
        <span class="text-[10px] font-bold uppercase tracking-wider text-secondary block">{{ item.type }}</span>
        <p class="text-xs font-semibold text-text-primary truncate mt-0.5">{{ item.name }}</p>
      </div>
    </div>
  </div>
  
  <!-- Footer with score + save button -->
  <div class="px-4 py-3 bg-background border-t border-border flex items-center justify-between">
    <div class="flex items-center gap-2.5">
      <div class="w-8 h-8 rounded-full border-2 border-secondary/30 bg-secondary/10 flex items-center justify-center">
        <span class="text-xs font-bold text-secondary">{{ suggestion.totalScore }}</span>
      </div>
      <div>
        <p class="text-xs font-semibold text-text-primary">Style Score</p>
        <p class="text-[10px] text-muted-foreground">Silhouette & weather match</p>
      </div>
    </div>
    <button class="flex items-center gap-1.5 bg-secondary text-white text-xs font-semibold px-4 py-2 rounded-sm">
      <span>[bookmark icon]</span>
      <span>Save Outfit</span>           <!-- Save button ONLY in card footer -->
    </button>
  </div>
</div>
```

**Filter `suggestedActions` to remove "save" duplicates**: In the template where `suggestedActions` chips are rendered, filter out any action containing "save":
```html
@for (action of message.suggestedActions?.filter(a => !a.toLowerCase().includes('save')); track $index) {
  <button class="chip">{{ action }}</button>
}
```

---

## Implementation Order

| Step | Bug | File(s) | Estimated Time |
|------|-----|---------|---------------|
| 1 | Bug 1 — Triple render | `ai-assistant.component.html` | 5 min |
| 2 | Bug 2 — Image preview | `ai-assistant.component.ts` | 5 min |
| 3 | Bug 3 — Send images | `ai-assistant.component.ts`, `ai.actions.ts` | 5 min |
| 4 | Bug 5 — Suggestion card UI + Save button dedup | `ai-assistant.component.html`, `.scss` | 30 min |
| 5 | Bug 4 — Metadata parsing | `ai.repository.impl.ts` | 10 min |

**Total**: ~55 min

---

## Verification Checklist

- [ ] Open AI assistant → send message → outfit suggestion appears **once** (not 3x)
- [ ] DevTools console shows **zero** NG0955 or Angular errors
- [ ] Select image file → **preview appears immediately** without needing to send message
- [ ] Send message with images → backend receives `UploadedImages` with data
- [ ] Attach 2+ images → all sent correctly, no missing files
- [ ] New session: outfit suggestion card shows **thumbnails**, **type labels**, **item names**
- [ ] Style score badge and Save button visible on suggestion card
- [ ] Return to old session → suggestion cards from past conversations show images
- [ ] "Save Outfit" appears **only** in the card footer, not in `suggestedActions` chips
- [ ] `suggestedActions` chips render without duplicates or "save" texts (track by $index)
