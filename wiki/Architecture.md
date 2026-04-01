# Architecture

Technical architecture of Romarr, forked from [Sonarr](https://github.com/Sonarr/Sonarr).

## System Architecture

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Browser    │     │  API Client  │     │   Prowlarr   │
│  React SPA   │     │  (scripts)   │     │  (indexers)  │
└──────┬───────┘     └──────┬───────┘     └──────┬───────┘
       │                    │                    │
       ▼                    ▼                    ▼
┌────────────────────────────────────────────────────────┐
│                    Romarr Host                         │
│                  ASP.NET Core                           │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐             │
│  │ Static   │  │ REST API │  │ SignalR  │             │
│  │ Files    │  │ v3 / v5  │  │ Hub     │             │
│  └──────────┘  └────┬─────┘  └────┬─────┘             │
│                     │              │                    │
│  ┌─────────────────────────────────────────────────┐   │
│  │              Core Business Logic                 │   │
│  │  ┌─────────┐ ┌──────────┐ ┌───────────────┐    │   │
│  │  │  Games  │ │ Download │ │ Notifications │    │   │
│  │  │  ROMs   │ │ Clients  │ │    Import     │    │   │
│  │  │Platform │ │ Indexers │ │  Housekeeping │    │   │
│  │  └─────────┘ └──────────┘ └───────────────┘    │   │
│  └──────────────────────┬──────────────────────────┘   │
│                         │                               │
│  ┌──────────────────────┴──────────────────────────┐   │
│  │            Data Access (Dapper + SQLite)          │   │
│  │  ┌──────────────┐  ┌────────────────────────┐   │   │
│  │  │ Repositories │  │ FluentMigrator (223)   │   │   │
│  │  │ TableMapping │  │ Schema Migrations      │   │   │
│  │  └──────────────┘  └────────────────────────┘   │   │
│  └──────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────┘
       │                    │                    │
       ▼                    ▼                    ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   SQLite /   │     │  File System │     │  Download    │
│  PostgreSQL  │     │  (ROM files) │     │  Clients     │
└──────────────┘     └──────────────┘     └──────────────┘
```

## Component Overview

### Romarr.Host
ASP.NET Core application host. Configures middleware pipeline, authentication, static files, API routing, and SignalR.

### Romarr.Http
HTTP middleware layer. Handles authentication, CORS, error formatting, and request/response pipeline.

### Romarr.Api.V3 / V5
REST API controllers. Each controller maps to a resource (Game, Rom, Calendar, Command, etc.) and delegates to Core services.

### Romarr.Core
All business logic. Contains:
- **Domain models** (`Games/`): Game, Rom, Platform, RomFile
- **Services**: GameService, RomService, DownloadService, etc.
- **Repositories**: Data access via Dapper
- **Providers**: Download clients, indexers, notifications, import lists
- **Housekeeping**: Scheduled cleanup tasks (orphan files, old history, etc.)
- **Datastore**: Database context, table mapping, migrations

### Quality System
Sonarr video qualities (720p, 1080p, HDTV, Bluray) are legacy aliases for "Unknown". Romarr uses three quality levels: Unknown, Bad, Verified. ROMs are categorized by release type (Retail, Digital, Homebrew, Prototype) and modification status (Original, Patched, Hack, Translation).

### Romarr.Common
Shared utilities across all projects: logging (NLog), HTTP client, disk operations, exceptions, extensions.

### Romarr.SignalR
SignalR hub for real-time push notifications to connected clients (browser, API consumers).

### Romarr.Update
Self-update mechanism. Downloads and applies updates from configured update source.

## Database

### Engine
SQLite by default. PostgreSQL supported for larger deployments.

### ORM
Dapper micro-ORM with convention-based column mapping. Model properties must match database column names exactly.

### Table Mapping
`TableMapping.cs` maps C# classes to database table names:

```csharp
Mapper.Entity<Game>("Series");      // Game class → Series table (Sonarr legacy)
Mapper.Entity<Rom>("Episodes");     // Rom class → Episodes table (Sonarr legacy)
Mapper.Entity<RomFile>("EpisodeFiles"); // RomFile class → EpisodeFiles table (Sonarr legacy)
```

Tables keep original Sonarr names because 223 FluentMigrator migrations define the historical schema using those names. Video qualities like 720p, 1080p, HDTV, and Bluray are legacy aliases that map to "Unknown" quality. Romarr's actual qualities are: Unknown, Bad, and Verified.

### Migrations
Sequential numbered migrations in `Datastore/Migration/`. Each migration runs exactly once. Never modify existing migrations — always add new ones.

## Frontend

### Stack
- React 18 with TypeScript
- CSS Modules for scoped styling
- webpack 5 for bundling
- Custom Redux-like state management

### Structure
The SPA is organized by feature:
- `Game/` — Game list, detail, editor
- `AddGame/` — Add new game wizard
- `Rom/` — ROM detail views
- `Platform/` — Platform views
- `Calendar/` — Calendar view
- `Activity/` — Queue, history, blocklist
- `Settings/` — Configuration pages
- `System/` — Status, tasks, logs

### API Communication
Frontend communicates with the backend via:
- REST API (`/api/v3/`) for CRUD operations
- SignalR WebSocket for real-time updates

## Dependency Injection

DryIoc container. Services are registered automatically by convention (interfaces → implementations). Override by explicit registration in `CompositionRoot.cs`.

## Scheduling

Background tasks use a custom scheduler:
- RSS feed sync (configurable interval, default 15 min)
- Refresh metadata (daily by default)
- Housekeeping (daily cleanup tasks)
- Health checks (periodic)
- Download monitoring (frequent polling)
