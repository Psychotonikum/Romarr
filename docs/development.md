# Development Guide

## Architecture

Romarr is a fork of [Sonarr](https://github.com/Sonarr/Sonarr), adapted for ROM/game management. The architecture follows the Servarr pattern:

```

              Web Browser (SPA)              │
          React 18 / TypeScript              │

           REST API (v3 / v5)               │
         Romarr.Api.V3 / V5               │

         ASP.NET Core Host                  │
      Romarr.Host / Romarr.Http          │

         Core Business Logic                │
           Romarr.Core                     │
  ┌─────────┬──────────┬─────────────────┐  │
  │  Games  │ Download │  Notifications  │  │
  │  ROMs   │ Clients  │  Import Lists   │  │
  │Platform │ Indexers  │  Housekeeping   │  │
  └─────────┴──────────┴─────────────────┘  │

          Data Access Layer                 │
   Dapper ORM + FluentMigrator + SQLite     │

```

## Prerequisites

- .NET 10 SDK (10.0.103+)
- Node.js 20+
- Yarn 1.x
- Git

## Quick Setup

```bash
# Automated setup (Debian/Ubuntu):
sudo bash scripts/setup-dev.sh

# Or manually:
dotnet restore src/Romarr.sln
dotnet build src/Romarr.sln
yarn install
yarn build
```

## Building & Running

```bash
# Backend build (Debug, Posix)
dotnet msbuild -restore src/Romarr.sln -p:Configuration=Debug -p:Platform=Posix

# Run
./_output/net10.0/Romarr

# Frontend dev mode (hot reload on port 9797)
yarn start
```

## Running Tests

```bash
# Unit tests only (recommended — excludes integration tests that need external services)
dotnet test src/Romarr.sln --filter 'Category!=IntegrationTest&Category!=AutomationTest&Category!=ManualTest'

# Single test project
dotnet test src/Romarr.Core.Test/Romarr.Core.Test.csproj --no-build

# With code coverage
dotnet test src/Romarr.sln --settings src/coverlet.runsettings --filter 'Category!=IntegrationTest'

# Integration tests (require running Romarr instance and network access)
dotnet test src/Romarr.Integration.Test/Romarr.Integration.Test.csproj
```

**Note**: Running `dotnet test` without `--filter` will include integration tests that attempt to start a Romarr server and make HTTP calls to external services. These will hang if the build output or network is unavailable.

## IGDB / Twitch API Setup

Game metadata comes from IGDB via the Twitch API. To enable game search:

1. Create a Twitch developer account at https://dev.twitch.tv/
2. Register an application to get a Client ID and Secret
3. In Romarr: **Settings > Metadata Source** — enter the credentials
4. For local development, store credentials in `.env.local` (git-ignored):

```
TWITCH_CLIENT_ID=your_client_id
TWITCH_CLIENT_SECRET=your_client_secret
```

## Domain Model

Romarr reuses the Sonarr database schema. The C# class names differ from the table names:

| C# Class | DB Table | Sonarr Equivalent | Description |
|----------|----------|-------------------|-------------|
| `Game` | `Series` | Series | A game title |
| `Platform` | (computed) | Season | A gaming platform (NES, SNES, etc.) |
| `Rom` | `Episodes` | Episode | An individual ROM/game file |
| `RomFile` | `EpisodeFiles` | EpisodeFile | Physical file on disk |

Properties that map to database columns use the **original Sonarr column names** (e.g., `TvdbId`, `SeriesId`, `SeasonNumber`). `TableMapping.cs` maps C# classes to their DB tables.

### Quality System

Sonarr video qualities (720p, 1080p, HDTV, Bluray, etc.) are legacy aliases that all map to "Unknown" quality. Romarr's actual qualities are:
- **Unknown** — Unverified dump
- **Bad** — Known bad dump
- **Verified** — Verified against No-Intro/Redump databases

### Release Types

ROMs are categorized by release type: Retail, Digital, Homebrew, Prototype.

### Modifications

ROM files can have modification tags: Original, Patched, Hack, Translation.

## Database Migrations

Migrations live in `src/Romarr.Core/Datastore/Migration/` and use FluentMigrator.

**Rules:**
- Never rename existing table or column names
- New migrations get the next sequential number
- Test migrations with both SQLite and PostgreSQL

```csharp
[Migration(232)]
public class add_your_feature : RomarrMigrationBase
{
    protected override void MainDbUpgrade()
    {
        // migration code
    }
}
```

## Frontend

The frontend is a React SPA in `frontend/src/`:

```bash
yarn start        # Dev server with hot reload
yarn build        # Production build
yarn lint         # ESLint
yarn stylelint    # CSS linting
```

The SPA communicates with the backend via REST API (`/api/v3`) and SignalR WebSocket for real-time updates.
