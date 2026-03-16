# Fix ASP.NET Identity.External Scheme Duplicate Registration

## Status: In Progress

### Plan Steps:

- [x] Create TODO.md
- [x] Update src/OutfitPlanner.Api/Program.cs
  - [x] Remove explicit `authBuilder.AddCookie("Identity.External")` block
  - [x] Remove `ConfigureApplicationCookie()` with conflicting name
- [ ] Test application startup (`dotnet run` in src/OutfitPlanner.Api)
- [ ] Verify OAuth flow (if credentials configured)
- [ ] Mark complete

### Current Issue:

```
Scheme already exists: Identity.External
```

Caused by triple registration of scheme from AddIdentity() + explicit registrations.
