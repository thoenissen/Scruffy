# Scruffy Project Overview

## Purpose
Discord bot and web application for managing Guild Wars 2 raid appointments, user registrations, and points tracking.

## Tech Stack
- .NET 10, C#, Blazor (WebApp)
- Entity Framework Core (data access via repository pattern)
- Discord bot integration (Scruffy.ServiceHosts.Discord)
- Solution: Scruffy.sln

## Project Structure
- **Scruffy.WebApp** - Blazor web application (raid commit pages, etc.)
- **Scruffy.Services** - Business logic, Discord dialog elements
- **Scruffy.Data** - Data layer, entities, repositories, enumerations
- **Scruffy.Commands** - Discord bot commands
- **Scruffy.ServiceHosts.Discord** - Discord bot hosting
- **Scruffy.ManualTesting** - Manual testing

## Code Style
- Uses `#region` blocks for organizing code sections
- Custom analyzer rule: logical `!` operator is disallowed; use `== false` instead
- XML doc comments on fields and public members
- Multi-line `<summary>` tags required
- Switch expression opening `{` on new line
- Object initializer `{` on new line below `new`
