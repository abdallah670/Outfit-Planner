
## 11. AI Fashion Assistant Page

### Page Structure
```
┌──────────────────────────────────────────────────────────────┐
│  💬 AI Fashion Assistant                        [⚙] [✕]    │
├────────────────────────────┬─────────────────────────────────┤
│  Session History           │  Messages Area                  │
│  (desktop only)            │                                 │
│                            │  ┌─────────────────────────┐    │
│  ┌──────────────────────┐  │  │ 🤖 AI: "Hello! I'm     │    │
│  │ Today                 │  │  │ your fashion assistant.│    │
│  │ ├ What to wear today?│  │  │ Ask me anything about  │    │
│  │ ├ Beach trip outfits │  │  │ your wardrobe!"        │    │
│  │                       │  │  └─────────────────────────┘    │
│  │ Yesterday             │  │                                 │
│  │ ├ Date night look    │  │  ┌─────────────────────────┐    │
│  │                       │  │  │ 😊 You: "What should   │    │
│  │ Last 7 Days           │  │  │ I wear for a rainy     │    │
│  │ ├ Wardrobe analysis  │  │  │ interview tomorrow?"   │    │
│  └──────────────────────┘  │  └─────────────────────────┘    │
│                            │                                 │
│                            │  ┌─────────────────────────┐    │
│                            │  │ 🤖 AI: "Based on your   │    │
│                            │  │ wardrobe + weather...   │    │
│                            │  │                         │    │
│                            │  │ 👔 Navy Blazer          │    │
│                            │  │ 👖 Grey Dress Pants     │    │
│                            │  │ 👞 Brown Oxfords        │    │
│                            │  │ 🧥 Beige Trench Coat    │    │
│                            │  │                         │    │
│                            │  │ Style Score: 88/100 ✅  │    │
│                            │  │             [Save Outfit]│    │
│                            │  └─────────────────────────┘    │
│                            │                                 │
│                            │  ┌─────────────────────────┐    │
│                            │  │ 🤖 AI is typing...      │    │
│                            │  │ ⚫ ⚫ ⚫                 │    │
│                            │  └─────────────────────────┘    │
│                            ├─────────────────────────────────┤
│                            │  [Date night?] [Casual Friday]   │
│                            │  [Beach trip] [What's missing?]  │
│                            ├─────────────────────────────────┤
│                            │  💬 Type your question... [Send] │
└────────────────────────────┴─────────────────────────────────┘
```

### Component Architecture
```typescript
- AiAssistantComponent (main chat page, route: /ai-assistant)
- ChatMessageComponent (message bubble: user right / AI left)
- TypingIndicatorComponent (animated dots during AI response)
- OutfitCardPreviewComponent (inline outfit suggestion with items)
- QuickSuggestionChipsComponent (predefined prompt buttons)
- SessionHistorySidebarComponent (optional desktop list)
```

---

## 12. Support Floating Widget (User Side)

### Page Structure
```
┌─────────────────────────────────────────────────────┐
│  [💬]  ← Floating FAB button (bottom-right, all pages)
└─────────────────────────────────────────────────────┘
                    ↓ Click opens slide-out panel

┌────────────────────────────────────────────────────┐
│  💬 Need help?                       [−] [✕]       │
├────────────────────────────────────────────────────┤
│  ┌────────────────────────────────────────────┐    │
│  │ 🤖 Bot: "Hi! I'm your support assistant.   │    │
│  │ How can I help you today?"                 │    │
│  │                                            │    │
│  │ Quick answers:                             │    │
│  │ [🔑 Reset password]  [🐛 Report bug]       │    │
│  │ [🚩 Report user]      [💬 Talk to admin]    │    │
│  │                                            │    │
│  │ Or type your question below..."            │    │
│  └────────────────────────────────────────────┘    │
│                                                    │
│  ┌────────────────────────────────────────────┐    │
│  │ Type your message...              [Send]   │    │
│  └────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────┘
```

### Component Architecture
```typescript
- SupportFabComponent (global floating button with unread badge)
- SupportWidgetComponent (slide-out chat panel container)
- SupportBotMessageComponent (AI chatbot bubble)
- SupportQuickReplyChipsComponent (predefined answer buttons)
```

---

## 13. Support Tickets List (User Side)

### Page Structure
```
┌──────────────────────────────────────────────────────────────┐
│  🎫 My Support Tickets                        [+ New Ticket] │
├──────────────────────────────────────────────────────────────┤
│  [All] [Open] [In Progress] [Resolved] [Closed]             │
├──────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────┐│
│  │ ! HIGH: Login issue after email change                  ││
│  │  Account • Opened 2h ago • Assigned to: John A.        ││
│  │  ──────────────────────────────────────────────         ││
│  │  Status: ● InProgress • 3 messages                     ││
│  ├─────────────────────────────────────────────────────────┤│
│  │   MED: Can't upload clothing photo                      ││
│  │  Technical • Opened 1d ago                              ││
│  │  ──────────────────────────────────────────────         ││
│  │  Status: ○ Open • 1 message                             ││
│  ├─────────────────────────────────────────────────────────┤│
│  │   LOW: Feature suggestion for color filter              ││
│  │  Other • Opened 3d ago                                  ││
│  │  ──────────────────────────────────────────────         ││
│  │  Status: ● Resolved • 5 messages                        ││
│  └─────────────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────────┘
```

### Component Architecture
```typescript
- SupportTicketsComponent (list page, route: /support/tickets)
- TicketCardComponent (priority icon + subject + status + meta)
- TicketFilterTabsComponent (status filter pills)
- NewTicketDialogComponent (create ticket modal form)
```

---

## 14. Ticket Detail (User Side)

### Page Structure
```
┌──────────────────────────────────────────────────────────────┐
│  🎫 #142: Login issue                [Cancel Ticket] [← Back]│
│  Status: ● InProgress   Priority: ! High                     │
│  Assigned to: John A. (Admin)                                │
├──────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────┐│
│  │ 📌 Originally reported:                                ││
│  │ "I can't log in after changing my email. It keeps      ││
│  │  saying invalid credentials."                          ││
│  └─────────────────────────────────────────────────────────┘│
│                                                              │
│  ─── Messages ───                                            │
│                                                              │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ 🛡️ Admin John A.: "I've reset your account. Try        ││
│  │ logging in with your new email. Let me know if it       ││
│  │ works."                         2:30 PM ✓ Read          ││
│  └─────────────────────────────────────────────────────────┘│
│                                                              │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ 😊 You: "It worked! Thank you."   2:45 PM               ││
│  └─────────────────────────────────────────────────────────┘│
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  [Type your message...]                          [Send]      │
└──────────────────────────────────────────────────────────────┘
```

### Component Architecture
```typescript
- TicketDetailComponent (detail page, route: /support/tickets/:id)
- TicketHeaderComponent (id + subject + status + priority badges)
- OriginalDescriptionCardComponent (pinned original report)
- SupportMessageBubbleComponent (message with sender avatar + time)
- TicketChatInputComponent (text input + send button)
```

---

## 15. Content Report Modal

### Page Structure
```
┌────────────────────────────────────────────────────┐
│  🚩 Report Content                                 │
├────────────────────────────────────────────────────┤
│  Why are you reporting this?                       │
│                                                    │
│  Reason:                                           │
│  ○ Spam                                            │
│  ● Harassment                                      │
│  ○ Inappropriate                                   │
│  ○ Copyright violation                             │
│  ○ Other: [________________]                       │
│                                                    │
│  Description (optional):                           │
│  ┌────────────────────────────────────────────┐   │
│  │ This user has been posting offensive       │   │
│  │ comments on my outfits repeatedly.         │   │
│  └────────────────────────────────────────────┘   │
│                                                    │
│  [Cancel]  [Submit Report]                         │
└────────────────────────────────────────────────────┘
```

### Component Architecture
```typescript
- ReportContentModalComponent (reusable dialog component)
- ReportReasonSelectorComponent (radio group for reason)
- ReportDescriptionInputComponent (optional text area)
```

---

## 16. Admin Support Dashboard

### Page Structure
```
┌──────────────────────────────────────────────────────────────┐
│  🎫 Support Dashboard                                        │
├──────────────────────────────────────────────────────────────┤
│  ┌────────┐  ┌────────┐  ┌────────┐  ┌────────┐            │
│  │ 12     │  │ 5 In   │  │ 8 Today│  │ 4.2h   │            │
│  │ Open   │  │Progress│  │Resolved│  │Avg Time│            │
│  └────────┘  └────────┘  └────────┘  └────────┘            │
├──────────────────────────────────────────────────────────────┤
│  [All] [Open] [In Progress] [Urgent] [Assigned to Me]       │
├──────────────────────────────────────────────────────────────┤
│  ┌──────┬────────────┬──────────┬────────┬────────┬──────┐  │
│  │  #   │ Subject    │ User     │ Status │ Prio   │ Time │  │
│  ├──────┼────────────┼──────────┼────────┼────────┼──────┤  │
│  │ #142 │ Login issue│ john_d   │ ● InP  │ ! High │ 2h   │  │
│  │ #141 │ Bug report │ sarah_m  │ ○ Open │ ◉ Med  │ 5h   │  │
│  │ #140 │ Report usr │ bob_k    │ ○ Open │ !! Urg │ 30m  │  │
│  │ #139 │ Can't upl. │ emma_l   │ ● InP  │ ◉ Med  │ 1d   │  │
│  └──────┴────────────┴──────────┴────────┴────────┴──────┘  │
├──────────────────────────────────────────────────────────────┤
│  Live Chat Monitor (SignalR)                                 │
│  ┌─────────────────────────────────────────────────────────┐│
│  │ 💬 User #142 is online...                      [Open]   ││
│  │ 💬 User #139 was active 5m ago                          ││
│  └─────────────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────────┘
```

### Component Architecture
```typescript
- AdminSupportDashboardComponent (route: /admin/support)
- SupportMetricCardsComponent (open/in-progress/resolved/avg-time)
- TicketsTableComponent (sortable data table)
- LiveChatMonitorComponent (online status + quick-open)
- AdminTicketDetailComponent (route: /admin/support/:id)
- AdminLiveChatComponent (route: /admin/support/live, split-pane chat)
```

---

## Design System Reference

Reuse the **same design tokens** from `plans/design.md`:

| Token | Value |
|-------|-------|
| Background | #FAFAFA |
| Surface | #FFFFFF |
| Primary | #F8B4C4 |
| Secondary | #9CAF88 |
| Text Primary | #2D3436 |
| Text Secondary | #636E72 |
| Border | #DFE6E9 |
| Error | #E17055 |
| Warning | #FDCB6E |
| Font | Inter |
| Border Radius Cards | 12px |
| Border Radius Buttons | 8px |

### Admin-Specific Design Tokens

| Element | Value |
|---------|-------|
| Sidebar width | 240px |
| Sidebar bg | #1a1a2e or dark navy |
| Sidebar text | #FFFFFF |
| Sidebar active | #F8B4C4 |
| Header height | 60px |
| Table row hover | rgba(248, 180, 196, 0.08) |
| Status Open | #FDCB6E (yellow) |
| Status InProgress | #74B9FF (blue) |
| Status Resolved | #9CAF88 (green) |
| Priority Low | #636E72 (grey) |
| Priority Medium | #FDCB6E (yellow) |
| Priority High | #E17055 (orange) |
| Priority Urgent | #D63031 (red) |

### AI Chat Specific Tokens

| Element | Value |
|---------|-------|
| AI bubble bg | #F0F0F5 |
| User bubble bg | #F8B4C4 |
| AI bubble align | left |
| User bubble align | right |
| Chat max width | 720px |
| Suggestion chip | outlined pill |
| Input bar height | 56px |

---

*Document Version: 1.0*
*Last Updated: May 2026*