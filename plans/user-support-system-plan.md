# Outfit-Planner: User Support & Communication System — Implementation Plan

> **Date:** 2026-05-23  
> **Scope:** Complete support ecosystem — live chat, ticketing, AI chatbot, admin queue  
> **Source:** Extracted from `platform-comprehensive-review.md` (Section 5)  
> **Dependencies:** Phase 1 (Auth/Roles) + Phase 2 (Admin Panel) must be complete  
> **Integration:** Extends AdminController from Phase 2 admin panel work

---

## Table of Contents

1. [Vision & User Stories](#1-vision--user-stories)
2. [Architecture Overview](#2-architecture-overview)
3. [New Domain Entities](#3-new-domain-entities)
4. [Backend Implementation](#4-backend-implementation)
5. [Frontend Implementation — User Side](#5-frontend-implementation--user-side)
6. [Frontend Implementation — Admin Side](#6-frontend-implementation--admin-side)
7. [AI Support Chatbot](#7-ai-support-chatbot)
8. [SignalR Real-Time Chat](#8-signalr-real-time-chat)
9. [Implementation Phases](#9-implementation-phases)

---

## 1. Vision & User Stories

### Core Vision

A complete support ecosystem where users can communicate with platform admins, submit complaints/tickets, get automated help from a support chatbot, and track resolution status.

**ALL IN C#** — no third-party support tools. Everything custom-built inside the existing .NET project.

### What Users Should Be Able to Do

| Action | How It Works |
|--------|-------------|
| *"I need help with my account"* | Opens a support chat → user types message → admin sees it in admin panel → real-time back and forth |
| *"Report inappropriate content"* | User clicks "Report" on a post → chooses reason → creates a support ticket with reference |
| *"I have a complaint about another user"* | User fills a complaint form → creates a moderated ticket → admin reviews and takes action |
| *"Can you help me recover my wardrobe?"* | User opens support chat → AI chatbot triages → if complex, escalates to human admin |
| *"Track my support request"* | User views `/support/tickets` → sees all their tickets with status (Open/In Progress/Resolved/Closed) |

### What Admins Should Be Able to Do

| Action | How It Works |
|--------|-------------|
| *"View all support tickets"* | Admin dashboard shows ticket queue with filters |
| *"Respond to a user in real-time"* | Admin live chat via SignalR — sees messages instantly, types response |
| *"Assign tickets to other admins"* | Change assigned admin on a ticket |
| *"Resolve or close tickets"* | Update ticket status with resolution notes |
| *"Monitor support metrics"* | Dashboard shows avg response time, backlog, tickets/day |

### Target Outcome

| Current State | Target State |
|---------------|--------------|
| No support system exists | Complete support ecosystem |
| No user-to-admin communication | Live chat + ticket system |
| No content reporting | Report buttons + moderated tickets |
| No support chatbot | AI chatbot with escalation |
| No admin support tools | Full admin support dashboard |

---

## 2. Architecture Overview

```
                    ┌─────────────────────────────────────────┐
                    │  💬 Support Chat Component              │
                    │  (/support/chat or floating widget)     │
                    │  [Type your message...] [Send]          │
                    │─────────────────────────────────────────│
                    │  🎫 Support Tickets                     │
                    │  (/support/tickets)                     │
                    │  ┌─────────┬─────────┬─────────┐       │
                    │  │ Open    │ In Prog │ Resolved │       │
                    │  │ Ticket  │ Ticket  │ Ticket  │       │
                    │  │ #142    │ #139    │ #135    │       │
                    │  └─────────┴─────────┴─────────┘       │
                    │─────────────────────────────────────────│
                    │  🤖 AI Support Chatbot                  │
                    │  Handles common queries automatically   │
                    │  "How do I reset my password?"          │
                    │  → Auto-responds with guide             │
                    │  Escalates to human if unresolved       │
                    └─────────────────────────────────────────┘
                                    │
┌───────────────────────────────────┴──────────────────────────┐
│              .NET Backend — Support System                     │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  SupportController (/api/support)                      │  │
│  │  ├─ POST /api/support/tickets — Create ticket          │  │
│  │  ├─ GET  /api/support/tickets — List my tickets        │  │
│  │  ├─ GET  /api/support/tickets/{id} — Ticket detail     │  │
│  │  ├─ POST /api/support/chat/send — Send chat message   │  │
│  │  ├─ GET  /api/support/chat/messages — Get messages    │  │
│  │  ├─ POST /api/support/tickets/{id}/close — Close      │  │
│  │  └─ POST /api/support/chatbot — Talk to AI chatbot    │  │
│  │                                                         │  │
│  │  AdminController (extensions)                           │  │
│  │  ├─ GET  /api/admin/support/tickets — All tickets      │  │
│  │  ├─ PUT  /api/admin/support/tickets/{id}/status        │  │
│  │  ├─ POST /api/admin/support/chat/respond — Admin reply │  │
│  │  └─ GET  /api/admin/support/stats — Support metrics    │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  SignalR Hub (/hubs/support)                           │  │
│  │  ├─ JoinTicketGroup(ticketId) — User joins ticket room │  │
│  │  ├─ JoinAdminGroup() — Admin joins global support room │  │
│  │  ├─ SendMessage(ticketId, text) — Real-time message    │  │
│  │  └─ ReceiveMessage(ticketId, message) — Push to clients│  │
│  └────────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Support Services                                       │  │
│  │  ├─ TicketService         — Ticket CRUD + workflow      │  │
│  │  ├─ SupportChatService    — Chat message management     │  │
│  │  ├─ SupportBotService     — AI chatbot logic            │  │
│  │  └─ SupportNotification   — Push/email on ticket update │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

### Communication Flow

```
┌──────────┐    SignalR    ┌───────────┐    SignalR    ┌───────────┐
│  User    │◄────────────►│  SignalR  │◄────────────►│   Admin   │
│  Browser │               │   Hub     │               │  Browser  │
│          │               │           │               │           │
│ Chat     │  HTTP/REST    │ /hubs/    │  HTTP/REST    │ Admin     │
│ Widget   │◄────────────►│ support   │◄────────────►│ Dashboard │
└──────────┘               └─────┬─────┘               └───────────┘
                                 │
                    ┌────────────┴────────────┐
                    │  .NET Backend           │
                    │  SignalR Hub            │
                    │  TicketService          │
                    │  SupportChatService     │
                    │  SupportBotService      │
                    └─────────────────────────┘
```

---

## 3. New Domain Entities

### 3.1 SupportTicket

```csharp
public class SupportTicket : BaseEntity
{
    public string UserId { get; set; }                 // Who created the ticket
    public string Subject { get; set; }                // Short description
    public string Description { get; set; }            // Full details
    public SupportCategory Category { get; set; }      // Account, Technical, Content, UserComplaint, Billing, Other
    public SupportPriority Priority { get; set; }      // Low, Medium, High, Urgent
    public SupportStatus Status { get; set; }          // Open, InProgress, Resolved, Closed, Cancelled
    public string AssignedToId { get; set; }           // Admin assigned to handle
    public string ReferenceType { get; set; }          // "FeedPost", "User", "Poll", "Outfit", etc.
    public string ReferenceId { get; set; }            // ID of the reported content
    public DateTimeOffset? ResolvedAt { get; set; }
    public string ResolutionNotes { get; set; }        // How it was resolved
    public bool IsEscalated { get; set; }              // Flagged as urgent/unresolved
    
    // Navigation
    public User User { get; set; }
    public User AssignedTo { get; set; }
    public ICollection<SupportMessage> Messages { get; set; }
}

public enum SupportCategory
{
    Account,
    Technical,
    Content,
    UserComplaint,
    Billing,
    Other
}

public enum SupportPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum SupportStatus
{
    Open,
    InProgress,
    Resolved,
    Closed,
    Cancelled
}
```

### 3.2 SupportMessage

```csharp
public class SupportMessage : BaseEntity
{
    public Guid TicketId { get; set; }
    public string SenderId { get; set; }               // User or Admin ID
    public string Content { get; set; }                 // Message text
    public bool IsFromAdmin { get; set; }               // True if admin responded
    public bool HasAttachment { get; set; }
    public string AttachmentUrl { get; set; }           // Screenshot, etc.
    public bool IsRead { get; set; }                    // Read status for admin
    
    // Navigation
    public SupportTicket Ticket { get; set; }
    public User Sender { get; set; }
}
```

### 3.3 SupportBotLog (Optional — for analytics)

```csharp
public class SupportBotLog : BaseEntity
{
    public string UserId { get; set; }
    public string UserMessage { get; set; }
    public string BotResponse { get; set; }
    public bool WasEscalated { get; set; }
    public Guid? TicketId { get; set; }                 // Created ticket if escalated
    public string Intent { get; set; }                  // Classified intent
    public bool WasResolved { get; set; }               // User satisfied?
}
```

### 3.4 ContentReport (reused from Admin Phase 2)

The `ContentReport` entity from Phase 2 admin panel work is referenced here:

```csharp
public class ContentReport : BaseEntity
{
    public string ReporterId { get; set; }              // User who reported
    public string ReportedUserId { get; set; }          // User being reported (optional)
    public string ReferenceType { get; set; }           // "FeedPost", "User", "Poll", "Comment"
    public string ReferenceId { get; set; }
    public ReportReason Reason { get; set; }            // Spam, Harassment, Inappropriate, Other
    public string Description { get; set; }
    public ReportStatus Status { get; set; }            // Pending, Reviewed, Resolved, Dismissed
    public string ReviewedById { get; set; }            // Admin who reviewed
    public string Resolution { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}

public enum ReportReason { Spam, Harassment, Inappropriate, Copyright, Other }
public enum ReportStatus { Pending, Reviewed, Resolved, Dismissed }
```

### 3.5 Database Migration SQL

```sql
-- SupportTickets table
CREATE TABLE SupportTickets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId NVARCHAR(450) NOT NULL,
    Subject NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Category INT NOT NULL DEFAULT 0,       -- enum
    Priority INT NOT NULL DEFAULT 1,       -- enum
    Status INT NOT NULL DEFAULT 0,         -- enum
    AssignedToId NVARCHAR(450) NULL,
    ReferenceType NVARCHAR(50) NULL,
    ReferenceId NVARCHAR(450) NULL,
    ResolvedAt DATETIMEOFFSET NULL,
    ResolutionNotes NVARCHAR(MAX) NULL,
    IsEscalated BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (AssignedToId) REFERENCES AspNetUsers(Id)
);

-- SupportMessages table
CREATE TABLE SupportMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TicketId UNIQUEIDENTIFIER NOT NULL,
    SenderId NVARCHAR(450) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    IsFromAdmin BIT NOT NULL DEFAULT 0,
    HasAttachment BIT NOT NULL DEFAULT 0,
    AttachmentUrl NVARCHAR(500) NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL,
    FOREIGN KEY (TicketId) REFERENCES SupportTickets(Id) ON DELETE CASCADE,
    FOREIGN KEY (SenderId) REFERENCES AspNetUsers(Id)
);

-- SupportBotLogs table (optional)
CREATE TABLE SupportBotLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId NVARCHAR(450) NOT NULL,
    UserMessage NVARCHAR(MAX) NOT NULL,
    BotResponse NVARCHAR(MAX) NOT NULL,
    WasEscalated BIT NOT NULL DEFAULT 0,
    TicketId UNIQUEIDENTIFIER NULL,
    Intent NVARCHAR(50) NULL,
    WasResolved BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL
);

-- Indexes
CREATE INDEX IX_SupportTickets_UserId ON SupportTickets(UserId);
CREATE INDEX IX_SupportTickets_Status ON SupportTickets(Status);
CREATE INDEX IX_SupportTickets_AssignedToId ON SupportTickets(AssignedToId);
CREATE INDEX IX_SupportMessages_TicketId ON SupportMessages(TicketId);
```

---

## 4. Backend Implementation

### 4.1 New Files

| File | Path | Purpose |
|------|------|---------|
| `SupportTicket.cs` | `src/OutfitPlanner.Domain/Entities/` | Support ticket entity |
| `SupportMessage.cs` | `src/OutfitPlanner.Domain/Entities/` | Support message entity |
| `SupportBotLog.cs` | `src/OutfitPlanner.Domain/Entities/` | Bot interaction log (optional) |
| `ContentReport.cs` | `src/OutfitPlanner.Domain/Entities/` | Content report entity (if not created in Phase 2) |
| `SupportTicketConfiguration.cs` | `src/OutfitPlanner.Persistence/Configurations/` | EF Core config |
| `SupportMessageConfiguration.cs` | `src/OutfitPlanner.Persistence/Configurations/` | EF Core config |
| `ISupportTicketRepository.cs` | `src/OutfitPlanner.Application/Contracts/Persistence/` | Repository interface |
| `SupportTicketRepository.cs` | `src/OutfitPlanner.Persistence/Repositories/` | Repository implementation |
| `ISupportMessageRepository.cs` | `src/OutfitPlanner.Application/Contracts/Persistence/` | Repository interface |
| `SupportMessageRepository.cs` | `src/OutfitPlanner.Persistence/Repositories/` | Repository implementation |
| `TicketDto.cs` | `src/OutfitPlanner.Application/DTOs/Support/` | Ticket DTOs |
| `TicketMessageDto.cs` | `src/OutfitPlanner.Application/DTOs/Support/` | Message DTOs |
| `CreateTicketCommand.cs` | `src/OutfitPlanner.Application/Features/Support/Commands/` | Create ticket handler |
| `UpdateTicketStatusCommand.cs` | `src/OutfitPlanner.Application/Features/Support/Commands/` | Update status handler |
| `AssignTicketCommand.cs` | `src/OutfitPlanner.Application/Features/Support/Commands/` | Assign admin handler |
| `CloseTicketCommand.cs` | `src/OutfitPlanner.Application/Features/Support/Commands/` | Close ticket handler |
| `SendMessageCommand.cs` | `src/OutfitPlanner.Application/Features/Support/Commands/` | Send message handler |
| `GetUserTicketsQuery.cs` | `src/OutfitPlanner.Application/Features/Support/Queries/` | List user's tickets |
| `GetTicketDetailQuery.cs` | `src/OutfitPlanner.Application/Features/Support/Queries/` | Ticket detail + messages |
| `GetAdminTicketsQuery.cs` | `src/OutfitPlanner.Application/Features/Support/Queries/` | Admin: all tickets |
| `GetSupportStatsQuery.cs` | `src/OutfitPlanner.Application/Features/Support/Queries/` | Support metrics |
| `SupportController.cs` | `src/OutfitPlanner.Api/Controllers/` | User-facing support endpoints |
| `AdminController.cs` (extend) | `src/OutfitPlanner.Api/Controllers/` | Add admin support endpoints |
| `SupportChatService.cs` | `src/OutfitPlanner.Application/Services/` | Chat logic (optional, can be in handler) |
| `SupportBotService.cs` | `src/OutfitPlanner.Application/Services/` | AI chatbot logic |
| `SupportHub.cs` | `src/OutfitPlanner.Api/Hubs/` | SignalR hub for real-time |
| `ISupportHubClient.cs` | `src/OutfitPlanner.Application/Contracts/SignalR/` | SignalR client interface |

### 4.2 SupportController Endpoints

```
User-Facing (requires auth):
  POST   /api/support/tickets                    → CreateTicket (with optional ReferenceType/ReferenceId)
  GET    /api/support/tickets                    → ListUserTickets (paginated, filterable by status)
  GET    /api/support/tickets/{id}               → GetTicketDetail (with messages)
  POST   /api/support/tickets/{id}/messages      → SendMessage (user adds to ticket)
  PUT    /api/support/tickets/{id}/cancel        → CancelTicket (user cancels their ticket)
  POST   /api/support/chatbot                    → TalkToBot (AI support chatbot)
  POST   /api/support/chatbot/escalate           → EscalateToHuman (creates ticket from bot)

Admin (requires admin role):
  GET    /api/admin/support/tickets              → GetAllTickets (filterable: status, category, priority, date)
  GET    /api/admin/support/tickets/{id}         → GetTicketDetail
  PUT    /api/admin/support/tickets/{id}/status  → UpdateTicketStatus
  PUT    /api/admin/support/tickets/{id}/assign  → AssignTicket (to admin)
  POST   /api/admin/support/tickets/{id}/messages→ RespondToTicket (admin responds)
  GET    /api/admin/support/stats                → GetSupportStats (metrics)
  GET    /api/admin/support/stats/daily          → GetDailyStats (time-series)
```

### 4.3 CreateTicket — Example Handler

```csharp
public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, TicketDto>
{
    private readonly ISupportTicketRepository _ticketRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    
    public async Task<TicketDto> Handle(CreateTicketCommand request, CancellationToken ct)
    {
        var ticket = new SupportTicket
        {
            UserId = _currentUser.UserId,
            Subject = request.Subject,
            Description = request.Description,
            Category = request.Category,
            Priority = DeterminePriority(request.Category),
            Status = SupportStatus.Open,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            IsEscalated = request.Priority == SupportPriority.Urgent
        };
        
        await _ticketRepo.AddAsync(ticket);
        
        // Notify admins via SignalR about new ticket
        await _hubContext.Clients.Group("Admins")
            .SendAsync("NewTicket", _mapper.Map<TicketDto>(ticket));
        
        return _mapper.Map<TicketDto>(ticket);
    }
    
    private SupportPriority DeterminePriority(SupportCategory category)
    {
        return category switch
        {
            SupportCategory.Account => SupportPriority.High,
            SupportCategory.UserComplaint => SupportPriority.High,
            SupportCategory.Content => SupportPriority.Medium,
            SupportCategory.Technical => SupportPriority.Medium,
            SupportCategory.Billing => SupportPriority.Medium,
            _ => SupportPriority.Low
        };
    }
}
```

### 4.4 Ticket Status Workflow

```
Created ──→ Open ──→ InProgress ──→ Resolved ──→ Closed
  │                    │               │
  └────────────────────┴───────────────┘
  User can cancel → Cancelled
```

**Status transitions:**
- **Open** → Admin picks up ticket, sets InProgress
- **InProgress** → Admin marks as Resolved with notes
- **Resolved** → User confirms or reopens via message
- **Closed** → Auto-closed after 7 days, or admin closes
- **Cancelled** → User cancels their own ticket anytime

### 4.5 SupportChatService

```csharp
public class SupportChatService
{
    private readonly ISupportTicketRepository _ticketRepo;
    private readonly ISupportMessageRepository _messageRepo;
    private readonly IHubContext<SupportHub> _hubContext;
    
    public async Task<SupportMessage> SendMessageAsync(
        Guid ticketId, string senderId, string content, bool isFromAdmin)
    {
        var message = new SupportMessage
        {
            TicketId = ticketId,
            SenderId = senderId,
            Content = content,
            IsFromAdmin = isFromAdmin,
            IsRead = !isFromAdmin  // Admin messages are auto-read for user
        };
        
        await _messageRepo.AddAsync(message);
        
        // Real-time delivery via SignalR
        var groupName = $"ticket-{ticketId}";
        await _hubContext.Clients.Group(groupName)
            .SendAsync("ReceiveMessage", MapToDto(message));
        
        // Update ticket last activity
        var ticket = await _ticketRepo.GetByIdAsync(ticketId);
        ticket.LastActivityAt = DateTimeOffset.UtcNow;
        await _ticketRepo.UpdateAsync(ticket);
        
        return message;
    }
}
```

### 4.6 SignalR Hub — SupportHub

```csharp
[Authorize]
public class SupportHub : Hub<ISupportHubClient>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRoleService _roleService;
    
    public override async Task OnConnectedAsync()
    {
        var userId = _currentUser.UserId;
        
        // Admins join the global admin group
        if (await _roleService.IsAdminAsync(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }
        
        await base.OnConnectedAsync();
    }
    
    // User joins their ticket room
    public async Task JoinTicketRoom(Guid ticketId)
    {
        var userId = _currentUser.UserId;
        var canJoin = await _supportTicketRepo
            .UserOwnsTicketAsync(ticketId, userId);
            
        if (canJoin || await _roleService.IsAdminAsync(userId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"ticket-{ticketId}");
        }
    }
    
    // Admin joins the admin support room (receives all new tickets)
    public async Task JoinAdminRoom()
    {
        if (await _roleService.IsAdminAsync(_currentUser.UserId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId, "Admins");
        }
    }
    
    // Send message via SignalR (handled by HTTP for reliability, but SignalR for presence)
    public async Task SendMessage(Guid ticketId, string content)
    {
        // Validate, save, and broadcast
        var message = await _chatService.SendMessageAsync(
            ticketId, _currentUser.UserId, content, isFromAdmin: false);
        
        // Broadcast to ticket room
        await Clients.Group($"ticket-{ticketId}")
            .ReceiveMessage(message);
    }
}

public interface ISupportHubClient
{
    Task ReceiveMessage(TicketMessageDto message);
    Task TicketStatusUpdated(TicketDto ticket);
    Task NewTicketNotification(TicketDto ticket);
    Task AdminTyping(bool isTyping);
}
```

### 4.7 Dependency Injection

```csharp
// In src/OutfitPlanner.Infrastructure/DependencyInjection.cs

// Support Services
services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
services.AddScoped<ISupportMessageRepository, SupportMessageRepository>();
services.AddScoped<ISupportChatService, SupportChatService>();
services.AddScoped<ISupportBotService, SupportBotService>();

// MediatR Handlers
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateTicketCommand>());

// SignalR Hub
services.AddSignalR();

// In Program.cs
app.MapHub<SupportHub>("/hubs/support");
```

---

## 5. Frontend Implementation — User Side

### 5.1 New NgRx State — Support Module

**Actions:**

| Action | Trigger | Effect |
|--------|---------|--------|
| `loadTickets` | User opens /support/tickets | Calls GET /api/support/tickets |
| `ticketsLoaded` | API response | Sets tickets in state |
| `loadTicketDetail` | User opens a ticket | Calls GET /api/support/tickets/{id} |
| `ticketDetailLoaded` | API response | Sets ticket + messages |
| `createTicket` | User submits ticket form | Calls POST /api/support/tickets |
| `ticketCreated` | API response | Navigates to ticket detail |
| `sendMessage` | User sends chat message | Calls POST .../tickets/{id}/messages |
| `messageSent` | API response | Appends message to state |
| `cancelTicket` | User cancels ticket | Calls PUT .../tickets/{id}/cancel |
| `talkToBot` | User sends chatbot query | Calls POST /api/support/chatbot |
| `botResponded` | API response | Shows bot response |

**State Shape:**

```typescript
interface SupportState {
  tickets: SupportTicket[];
  currentTicket: SupportTicketDetail | null;
  messages: TicketMessage[];
  isLoading: boolean;
  isSending: boolean;
  error: string | null;
  botConversation: BotMessage[];
  isBotTyping: boolean;
}

interface SupportTicket {
  id: string;
  subject: string;
  category: SupportCategory;
  priority: SupportPriority;
  status: SupportStatus;
  createdAt: string;
  lastActivityAt: string;
  assignedToName: string | null;
}

interface SupportTicketDetail extends SupportTicket {
  description: string;
  referenceType: string | null;
  referenceId: string | null;
  resolvedAt: string | null;
  resolutionNotes: string | null;
  messages: TicketMessage[];
}

interface TicketMessage {
  id: string;
  senderId: string;
  senderName: string;
  content: string;
  isFromAdmin: boolean;
  hasAttachment: boolean;
  attachmentUrl: string | null;
  createdAt: string;
}
```

### 5.2 Support Chat Component — Floating Widget

```
┌──────────────────────────────────┐
│  💬 Need help?                    │
│  ──────────────────────────────── │
│  ┌────────────────────────────┐  │
│  │ 🤖 Bot: "How can I help? │  │
│  │                           │  │
│  │ Quick answers:            │  │
│  │ [🔑 Reset password]       │  │
│  │ [🐛 Report bug]           │  │
│  │ [🚩 Report user]           │  │
│  │ [💬 Talk to admin]        │  │
│  │                           │  │
│  │ Or type your question..." │  │
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │ Type your message... [Send]│  │
│  └────────────────────────────┘  │
└──────────────────────────────────┘
```

**Features:**
- Floating help button (bottom-right corner) available on all pages
- Opens in slide-out panel or modal
- AI chatbot handles initial queries
- Option to create ticket / talk to human admin
- Real-time updates via SignalR
- Unread message badge on help button

### 5.3 Support Tickets Page — /support/tickets

```
┌──────────────────────────────────────────────┐
│  🎫 My Support Tickets           [+ New]      │
├──────────────────────────────────────────────┤
│  [All] [Open] [In Progress] [Resolved] [Closed]│
├──────────────────────────────────────────────┤
│  ┌────────────────────────────────────────┐  │
│  │ ! HIGH: Login issue                    │  │
│  │  Account • Opened 2h ago • John A.    │  │
│  │  ──────────────────────────────────    │  │
│  │  Status: InProgress • 3 messages       │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │   MED: Can't add clothing photo        │  │
│  │  Technical • Opened 1d ago             │  │
│  │  ──────────────────────────────────    │  │
│  │  Status: Open • 1 message              │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │   LOW: Suggestion for new feature      │  │
│  │  Other • Opened 3d ago                 │  │
│  │  ──────────────────────────────────    │  │
│  │  Status: Resolved • 5 messages         │  │
│  └────────────────────────────────────────┘  │
└──────────────────────────────────────────────┘
```

### 5.4 Ticket Detail Page — /support/tickets/:id

```
┌──────────────────────────────────────────────┐
│  🎫 #142: Login issue              [Cancel]   │
│  Status: ● InProgress   Priority: ! High      │
│  Assigned to: John A. (Admin)                 │
├──────────────────────────────────────────────┤
│  ┌────────────────────────────────────────┐  │
│  │ 📌 Originally reported:               │  │
│  │ "I can't log in after changing my     │  │
│  │  email. It keeps saying invalid."     │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  ── Chat Messages ──                         │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │ [Admin] John A.: "I've reset your      │  │
│  │ account. Try logging in with your      │  │
│  │ new email. Let me know if it works."   │  │
│  │ 2:30 PM ✓ Read                         │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │ [You] "It worked! Thank you."          │  │
│  │ 2:45 PM                                │  │
│  └────────────────────────────────────────┘  │
│                                              │
├──────────────────────────────────────────────┤
│  [Type your message...]          [Send]       │
└──────────────────────────────────────────────┘
```

### 5.5 Route Registration

```typescript
// In app.routes.ts
{
  path: 'support',
  canActivate: [authGuard],
  children: [
    { path: '', redirectTo: 'tickets', pathMatch: 'full' },
    { path: 'tickets', component: SupportTicketsComponent, title: 'My Tickets' },
    { path: 'tickets/:id', component: TicketDetailComponent, title: 'Ticket Detail' }
  ]
}
```

### 5.6 Content Report Flow

When a user clicks "Report" on any content (post, comment, user profile):

```
1. User clicks [Report] button
2. Modal opens: "Why are you reporting this?"
   ┌───────────────────────────────────┐
   │  🚩 Report Content                │
   │                                   │
   │  Reason:                          │
   │  ○ Spam                           │
   │  ● Harassment                     │
   │  ○ Inappropriate                  │
   │  ○ Copyright                      │
   │  ○ Other: [______________]        │
   │                                   │
   │  Description (optional):          │
   │  [_____________________________]  │
   │                                   │
   │  [Cancel]  [Submit Report]        │
   └───────────────────────────────────┘
3. Submits → Creates ContentReport + auto-creates SupportTicket
4. User sees: "Report submitted. Support ticket #143 created."
5. Admin gets notified via SignalR
```

### 5.7 Support Floating Button — Global Component

```html
<!-- Floating help button — shown on all authenticated pages -->
<div class="support-fab" *ngIf="isAuthenticated">
  <button class="support-fab__button" (click)="toggleSupportPanel()">
    <span class="support-fab__icon">💬</span>
    <span class="support-fab__badge" *ngIf="unreadCount > 0">
      {{ unreadCount }}
    </span>
  </button>
  
  <!-- Slide-out support panel -->
  <div class="support-panel" *ngIf="isPanelOpen">
    <app-support-widget></app-support-widget>
  </div>
</div>
```

---

## 6. Frontend Implementation — Admin Side

### 6.1 NgRx State — Admin Support Module

**Actions:**

| Action | Trigger | Effect |
|--------|---------|--------|
| `loadAdminTickets` | Admin opens support page | Calls GET /api/admin/support/tickets |
| `loadTicketDetail` | Admin clicks ticket | Calls GET /api/admin/support/tickets/{id} |
| `updateTicketStatus` | Admin changes status | Calls PUT .../tickets/{id}/status |
| `assignTicket` | Admin assigns to self/other | Calls PUT .../tickets/{id}/assign |
| `respondToTicket` | Admin sends message | Calls POST .../tickets/{id}/messages |
| `loadSupportStats` | Admin opens stats tab | Calls GET /api/admin/support/stats |
| `receiveRealtimeMessage` | SignalR push | Appends message to ticket |
| `receiveNewTicket` | SignalR push | Adds ticket to queue |

### 6.2 Admin Support Dashboard — /admin/support

```
┌──────────────────────────────────────────────────────────────┐
│  🎫 Support Dashboard                                         │
├──────────────────────────────────────────────────────────────┤
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐        │
│  │ 12 Open │  │ 5 In    │  │ 8 Today │  │ 4.2h   │        │
│  │ Tickets │  │ Progress│  │Resolved │  │Avg Time│        │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘        │
├──────────────────────────────────────────────────────────────┤
│  [All] [Open] [In Progress] [Urgent] [Assigned to me]        │
├──────────────────────────────────────────────────────────────┤
│  ┌──────┬────────────┬──────────┬────────┬────────┬──────┐  │
│  │  #   │ Subject    │ User     │ Status │ Prio   │ Time │  │
│  ├──────┼────────────┼──────────┼────────┼────────┼──────┤  │
│  │ #142 │ Login issue│ john_d   │ ● InP  │ ! High │ 2h   │  │
│  │ #141 │ Bug report │ sarah_m  │ ● Open │ ◉ Med  │ 5h   │  │
│  │ #140 │ Report user│ bob_k    │ ● Open │ !! Urg │ 30m  │  │
│  │ #139 │ Can't upl. │ emma_l   │ ● InP  │ ◉ Med  │ 1d   │  │
│  └──────┴────────────┴──────────┴────────┴────────┴──────┘  │
├──────────────────────────────────────────────────────────────┤
│  Live Chat Monitor (SignalR)                                 │
│  ┌─────────────────────────────────────────────────────┐     │
│  │ 💬 User #142 is online...  [Open Chat]              │     │
│  │ 💬 User #139 was active 5m ago                      │     │
│  └─────────────────────────────────────────────────────┘     │
└──────────────────────────────────────────────────────────────┘
```

### 6.3 Admin Live Chat — /admin/support/live

```
┌──────────────────────────────────────────────────────────────┐
│  Live Support Chat                                            │
├──────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────────────────────────────┐ │
│  │ Active Chats │  │ Chat — Ticket #142                    │ │
│  │              │  │ User: john_d                          │ │
│  │ 🔵 #142 (2) │  │ ──────────────────────────────────   │ │
│  │   John D.    │  │ [Admin] You: "I've reset your        │ │
│  │              │  │ account..."                           │ │
│  │ ⚪ #139 (0)  │  │ [User] John: "It worked! Thank you"  │ │
│  │   Emma L.    │  │                                       │ │
│  │              │  │ ──────────────────────────────────   │ │
│  │              │  │ [Type your reply...] [Send]           │ │
│  └──────────────┘  └──────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

### 6.4 Admin Route Registration

```typescript
// In admin routes
{
  path: 'admin/support',
  component: AdminSupportDashboardComponent,
  canActivate: [authGuard, adminGuard],
  title: 'Support Dashboard'
},
{
  path: 'admin/support/:id',
  component: AdminTicketDetailComponent,
  canActivate: [authGuard, adminGuard],
  title: 'Ticket Detail'
},
{
  path: 'admin/support/live',
  component: AdminLiveChatComponent,
  canActivate: [authGuard, adminGuard],
  title: 'Live Chat'
},
{
  path: 'admin/support/stats',
  component: AdminSupportStatsComponent,
  canActivate: [authGuard, adminGuard],
  title: 'Support Stats'
}
```

---

## 7. AI Support Chatbot

### 7.1 Bot Capabilities

| User Query | Bot Response |
|------------|-------------|
| *"How do I reset my password?"* | "Go to Settings → Change Password. If you forgot it, click 'Forgot Password' on the login page and follow the email instructions." |
| *"I can't add a clothing item"* | "Make sure your image is under 10MB and in JPG/PNG format. If the issue persists, I'll create a ticket for you." |
| *"I want to report a user"* | "I'll create a support ticket for this. Please describe the issue in detail and an admin will review it within 24 hours." |
| *"My outfits disappeared"* | "Let me check... Your account shows 5 outfits saved. Are you looking in the right tab? If not, I'll escalate to an admin." |
| *"How does the AI assistant work?"* | "The AI Fashion Assistant can suggest outfits based on your wardrobe, weather, and style preferences. Just ask it anything about fashion!" |
| *"Talk to a human"* / *"I need an admin"* | "I'll transfer you to a human support agent. Please wait while I create a ticket." |

### 7.2 SupportBotService

```csharp
public class SupportBotService : ISupportBotService
{
    // Predefined responses for common intents
    private static readonly Dictionary<string, BotResponse> Responses = new()
    {
        ["password_reset"] = new(
            Intent: "password_reset",
            Response: "Go to Settings → Change Password. If you forgot your password, click 'Forgot Password' on the login page and follow the email instructions.",
            CanAutoResolve: true
        ),
        ["add_item"] = new(
            Intent: "add_item",
            Response: "Make sure your image is under 10MB and in JPG/PNG format. If the issue persists, I'll escalate this to an admin.",
            CanAutoResolve: false
        ),
        ["report_user"] = new(
            Intent: "report_user",
            Response: "I'll create a support ticket for this. Please provide details about the user and the issue.",
            CanAutoResolve: false,
            CreatesTicket: true
        ),
        ["talk_to_admin"] = new(
            Intent: "talk_to_admin",
            Response: "I'll transfer you to a human support agent. One moment please...",
            CanAutoResolve: false,
            CreatesTicket: true
        ),
        ["missing_items"] = new(
            Intent: "missing_items",
            Response: "Let me check your account... Your wardrobe shows {itemCount} items saved. Are you looking in the right category? If they're still missing, I'll escalate.",
            CanAutoResolve: false
        ),
        ["general"] = new(
            Intent: "general",
            Response: "I'm not sure about that. Let me connect you with a human support agent who can help.",
            CanAutoResolve: false,
            CreatesTicket: true
        )
    };
    
    public async Task<BotResult> ProcessMessageAsync(string userId, string message)
    {
        // 1. Try to classify intent using keyword matching or simple ML
        var intent = ClassifyIntent(message);
        
        // 2. If no match, use LLM (same OpenAI client as AI Assistant)
        if (intent == "unknown")
        {
            intent = await ClassifyWithLLMAsync(message);
        }
        
        // 3. Get response template
        if (Responses.TryGetValue(intent, out var template))
        {
            var response = template.Response;
            
            // Fill dynamic values
            if (intent == "missing_items")
            {
                var itemCount = await _wardrobeRepo.CountAsync(userId);
                response = response.Replace("{itemCount}", itemCount.ToString());
            }
            
            // 4. Create ticket if needed
            Guid? ticketId = null;
            if (template.CreatesTicket)
            {
                ticketId = await CreateTicketFromBotAsync(userId, message, intent);
            }
            
            // 5. Log interaction
            await _botLogRepo.AddAsync(new SupportBotLog
            {
                UserId = userId,
                UserMessage = message,
                BotResponse = response,
                WasEscalated = template.CreatesTicket || !template.CanAutoResolve,
                TicketId = ticketId,
                Intent = intent,
                WasResolved = template.CanAutoResolve
            });
            
            return new BotResult
            {
                Response = response,
                Intent = intent,
                CreatedTicketId = ticketId,
                CanAutoResolve = template.CanAutoResolve
            };
        }
        
        // 6. Fallback: create ticket
        var fallbackTicketId = await CreateTicketFromBotAsync(userId, message, "unknown");
        return new BotResult
        {
            Response = "I'm not sure how to help with that. I've created a support ticket and an admin will get back to you soon.",
            Intent = "unknown",
            CreatedTicketId = fallbackTicketId,
            CanAutoResolve = false
        };
    }
    
    private string ClassifyIntent(string message)
    {
        // Keyword-based classification
        var lower = message.ToLower();
        
        if (lower.Contains("password") || lower.Contains("reset") || lower.Contains("forgot"))
            return "password_reset";
        if (lower.Contains("add") && (lower.Contains("item") || lower.Contains("cloth")))
            return "add_item";
        if (lower.Contains("report") || lower.Contains("complaint"))
            return "report_user";
        if (lower.Contains("admin") || lower.Contains("human") || lower.Contains("agent"))
            return "talk_to_admin";
        if (lower.Contains("missing") || lower.Contains("disappeared") || lower.Contains("lost"))
            return "missing_items";
        
        return "unknown";
    }
    
    private async Task<string> ClassifyWithLLMAsync(string message)
    {
        // Use same OpenAI client as AI Fashion Assistant
        // or use a simpler/smaller model for support classification
        var prompt = $"Classify this support message into one of: " +
                     $"password_reset, add_item, report_user, talk_to_admin, missing_items, general.\n" +
                     $"Message: \"{message}\"\n" +
                     $"Return only the intent name.";
        
        var result = await _openAiClient.GetCompletionAsync(prompt);
        return result.Trim().ToLower();
    }
}
```

### 7.3 Bot Response UI

```
┌──────────────────────────────────────┐
│ 💬 Need help?                        │
├──────────────────────────────────────┤
│                                      │
│ 🤖 Hello! How can I help you today?  │
│                                      │
│ Quick options:                       │
│ ┌────────────────────────────────┐   │
│ │ 🔑 Reset my password          │   │
│ │ 🐛 Report a bug                │   │
│ │ 🚩 Report a user               │   │
│ │ 💬 Talk to an admin            │   │
│ └────────────────────────────────┘   │
│                                      │
│ ── Or type your question below ──    │
│                                      │
├──────────────────────────────────────┤
│ [Type your message...]    [Send]     │
└──────────────────────────────────────┘
```

### 7.4 Bot → Human Escalation Flow

```
User: "I need help with my account"
    │
    ▼
Bot: Classifies as "account_issue"
    │
    ├── Can auto-resolve? → Yes → Respond with guide
    │
    └── Can auto-resolve? → No
            │
            ▼
        "I'll create a support ticket for you.
         An admin will respond shortly."
            │
            ▼
        Creates SupportTicket (Status: Open)
            │
            ▼
        Admin receives SignalR notification
            │
            ▼
        Admin joins ticket chat → Real-time conversation
```

---

## 8. SignalR Real-Time Chat

### 8.1 Connection Setup (Frontend)

```typescript
// support-hub.service.ts
@Injectable({ providedIn: 'root' })
export class SupportHubService {
  private hubConnection: signalR.HubConnection;
  
  constructor(private authService: AuthService) {}
  
  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/support', {
        accessTokenFactory: () => this.authService.getToken()
      })
      .withAutomaticReconnect()
      .build();
    
    this.hubConnection.start().catch(err => console.error(err));
    
    this.hubConnection.on('ReceiveMessage', (message: TicketMessage) => {
      this.messageReceived.emit(message);
    });
    
    this.hubConnection.on('TicketStatusUpdated', (ticket: SupportTicket) => {
      this.ticketUpdated.emit(ticket);
    });
    
    this.hubConnection.on('NewTicketNotification', (ticket: SupportTicket) => {
      // Admin only — shows toast notification
      this.newTicketNotification.emit(ticket);
    });
  }
  
  async joinTicketRoom(ticketId: string): Promise<void> {
    await this.hubConnection.invoke('JoinTicketRoom', ticketId);
  }
  
  async joinAdminRoom(): Promise<void> {
    await this.hubConnection.invoke('JoinAdminRoom');
  }
  
  async sendMessage(ticketId: string, content: string): Promise<void> {
    await this.hubConnection.invoke('SendMessage', ticketId, content);
  }
}
```

### 8.2 SignalR Event Flow

```
User sends message via SignalR
    │
    ▼
SupportHub.SendMessage()
    │
    ├── Save message to DB
    ├── Broadcast to ticket group "ticket-{id}"
    │
    ▼
Admin receives message in real-time
    │
    ├── Admin types response
    │
    ▼
SupportHub.SendMessage() (isFromAdmin: true)
    │
    ├── Save to DB
    ├── Broadcast to ticket group
    │
    ▼
User receives admin response in real-time
    │
    ▼
Conversation continues...

New Ticket Created:
    │
    ▼
Server sends "NewTicketNotification" to "Admins" group
    │
    ▼
Admin dashboard updates queue in real-time
Admin sees toast: "New ticket #142 from john_d"
```

---

## 9. Implementation Phases

### Phase 1: Backend Foundation (Week 1)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Create `SupportTicket` and `SupportMessage` entities + EF Core configurations + migration | 🟢 Easy |
| **Day 2** | Create repositories (`ISupportTicketRepository`, `ISupportMessageRepository`) + implementations | 🟢 Easy |
| **Day 3** | Create `SupportController` with ticket CRUD endpoints (create, list, detail, cancel) | 🟡 Medium |
| **Day 4** | Create CQRS handlers: `CreateTicketCommand`, `GetUserTicketsQuery`, `GetTicketDetailQuery` | 🟡 Medium |
| **Day 5** | Create `SendMessageCommand`, `CloseTicketCommand`. Wire up AutoMapper profiles. Test all endpoints | 🟡 Medium |

### Phase 2: Admin Support Backend (Week 2)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Extend `AdminController` with support endpoints: list all tickets, get detail, update status, assign | 🟡 Medium |
| **Day 2** | Create admin CQRS handlers: `GetAdminTicketsQuery`, `UpdateTicketStatusCommand`, `AssignTicketCommand` | 🟡 Medium |
| **Day 3** | Create support stats endpoints: `GetSupportStatsQuery` (avg response time, backlog, tickets/day) | 🟡 Medium |
| **Day 4** | Create `SupportBotService` with keyword-based intent classification + predefined responses | 🟡 Medium |
| **Day 5** | Wire up `POST /api/support/chatbot` endpoint. Test bot with sample queries. Add LLM fallback classification | 🟡 Medium |

### Phase 3: SignalR Real-Time (Week 3)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Create `SupportHub` SignalR hub with group management (ticket rooms, admin group) | 🟡 Medium |
| **Day 2** | Create frontend `SupportHubService` — establish connection, handle reconnect, join rooms | 🟡 Medium |
| **Day 3** | Wire real-time message delivery: send message → save → broadcast to ticket group | 🔴 Hard |
| **Day 4** | Wire real-time ticket notifications: new ticket → push to admin group → admin dashboard updates | 🟡 Medium |
| **Day 5** | Add typing indicators, online/offline status for connected users. Test with multiple simultaneous chats | 🟡 Medium |

### Phase 4: Frontend — User Side (Week 4)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Create Support NgRx state module (actions, reducer, effects, selectors) | 🟡 Medium |
| **Day 2** | Create `SupportTicketsComponent` (/support/tickets) — list with filters and status tabs | 🟡 Medium |
| **Day 3** | Create `TicketDetailComponent` (/support/tickets/:id) — message timeline + input | 🟡 Medium |
| **Day 4** | Create `SupportWidgetComponent` — floating help button + slide-out panel with bot chat | 🟡 Medium |
| **Day 5** | Create `ContentReportModalComponent` — report form with reason selection. Wire to ticket creation | 🟡 Medium |

### Phase 5: Frontend — Admin Side (Week 5)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Create Admin Support NgRx state (actions, reducer, effects, selectors) | 🟡 Medium |
| **Day 2** | Create `AdminSupportDashboardComponent` (/admin/support) — ticket queue table with filters | 🟡 Medium |
| **Day 3** | Create `AdminTicketDetailComponent` (/admin/support/:id) — full ticket view with status controls | 🟡 Medium |
| **Day 4** | Create `AdminLiveChatComponent` (/admin/support/live) — real-time chat monitor with active conversations panel | 🔴 Hard |
| **Day 5** | Create `AdminSupportStatsComponent` (/admin/support/stats) — metrics dashboard with charts | 🟡 Medium |

### Phase 6: Polish & Integration (Week 6)

| Day | Tasks | Effort |
|-----|-------|--------|
| **Day 1** | Add push notifications for ticket updates (new message, status change) | 🟡 Medium |
| **Day 2** | Add email notifications for ticket updates (new ticket created, ticket resolved) | 🟡 Medium |
| **Day 3** | Add admin response templates (pre-written responses for common issues) | 🟢 Easy |
| **Day 4** | End-to-end testing: create ticket → bot triage → admin response → resolution → close | 🟡 Medium |
| **Day 5** | Error handling, loading states, empty states. Security audit (ensure only ticket owner sees their ticket) | 🟡 Medium |

---

## Integration with Existing Admin Panel

The support system integrates with the Phase 2 admin panel work:

| Admin Panel Feature | Support Integration |
|---------------------|---------------------|
| **Admin NgRx State** | Extend with support-related actions/state |
| **AdminLayoutComponent** | Add "Support" nav item with unread badge |
| **AdminDashboardComponent** | Add support stats cards (open tickets, avg response time) |
| **AdminController** | Add support endpoints alongside existing ones |
| **AuditLog** | Log all support actions (ticket created, assigned, resolved) |
| **User Management** | Link to user's support ticket history |

---

## Summary: Complete File Inventory

### Backend Files (30 new files)

| Layer | Count | Files |
|-------|-------|-------|
| **Domain Entities** | 3 | SupportTicket.cs, SupportMessage.cs, SupportBotLog.cs |
| **Persistence Config** | 2 | SupportTicketConfiguration.cs, SupportMessageConfiguration.cs |
| **Persistence Repos** | 2 | SupportTicketRepository.cs, SupportMessageRepository.cs |
| **App Contracts** | 4 | ISupportTicketRepository, ISupportMessageRepository, ISupportChatService, ISupportBotService |
| **App DTOs** | 4 | TicketDto.cs, TicketListDto.cs, TicketMessageDto.cs, BotResultDto.cs |
| **App Commands** | 5 | CreateTicket, SendMessage, UpdateTicketStatus, AssignTicket, CloseTicket |
| **App Queries** | 4 | GetUserTickets, GetTicketDetail, GetAdminTickets, GetSupportStats |
| **App Services** | 2 | SupportChatService.cs, SupportBotService.cs |
| **API Controllers** | 1 | SupportController.cs (AdminController extended separately) |
| **API Hubs** | 1 | SupportHub.cs |
| **API Contracts** | 1 | ISupportHubClient.cs |

### Frontend Files (25 new files)

| Layer | Count | Files |
|-------|-------|-------|
| **Domain Entities** | 3 | support-ticket.entity.ts, ticket-message.entity.ts, support-bot.entity.ts |
| **Domain Repos** | 1 | support.repository.ts |
| **Domain Use Cases** | 1 | support.usecases.ts |
| **Data Sources** | 1 | support.datasource.ts |
| **Data Repos** | 1 | support.repository.impl.ts |
| **NgRx State** | 5 | support.actions.ts, support.reducer.ts, support.effects.ts, support.selectors.ts, index.ts |
| **Services** | 1 | support-hub.service.ts (SignalR) |
| **Components** | 8 | SupportWidgetComponent, SupportTicketsComponent, TicketDetailComponent, ContentReportModal, AdminSupportDashboardComponent, AdminTicketDetailComponent, AdminLiveChatComponent, AdminSupportStatsComponent |

---

> **Dependencies:**
> - Phase 1 (Auth/Roles) — Admin role required for admin support endpoints
> - Phase 2 (Admin Panel) — Admin layout, NgRx state, and navigation structure reused
> - `AISettings` model — Shared with AI Assistant for LLM-based chatbot classification
> - SignalR — Must be added to Program.cs and configured in appsettings.json