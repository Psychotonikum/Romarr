# Configuration Reference

Romarr stores its configuration in `config.xml` inside the application data directory.

## Config File Locations

| OS | Path |
|----|------|
| Linux | `~/.config/Romarr/config.xml` |
| macOS | `~/.config/Romarr/config.xml` |
| Windows | `%AppData%\Romarr\config.xml` |
| Docker | `/config/config.xml` |

## Settings Reference

### Server

| Setting | Default | Description |
|---------|---------|-------------|
| `Port` | `9797` | HTTP port for the web UI and API |
| `SslPort` | `9898` | HTTPS port (when SSL is enabled) |
| `BindAddress` | `*` | IP address to bind to. Use `*` for all interfaces, `127.0.0.1` for localhost only |
| `EnableSsl` | `False` | Enable HTTPS |
| `SslCertPath` | | Path to SSL certificate (.pfx) |
| `SslCertPassword` | | Password for the SSL certificate |
| `UrlBase` | | URL base for reverse proxy setups (e.g., `/romarr`) |
| `LaunchBrowser` | `True` | Open browser on startup (desktop only) |

### Security

| Setting | Default | Description |
|---------|---------|-------------|
| `ApiKey` | (auto-generated) | API authentication key. Required for all API calls |
| `AuthenticationMethod` | `None` | Authentication type: `None`, `Basic`, `Forms`, `External` |
| `AuthenticationRequired` | `Enabled` | Whether authentication is required |
| `Branch` | `main` | Update branch |

### Logging

| Setting | Default | Description |
|---------|---------|-------------|
| `LogLevel` | `info` | Log verbosity: `trace`, `debug`, `info`, `warn`, `error`, `fatal` |
| `LogSql` | `False` | Log SQL queries (debug only) |
| `ConsoleLogLevel` | | Override log level for console output |
| `LogSizeLimit` | `1` | Maximum log file size in MB before rotation |

### Database

| Setting | Default | Description |
|---------|---------|-------------|
| Database engine | SQLite | Default embedded database |
| PostgreSQL | | Supported via connection string for larger deployments |

#### PostgreSQL Configuration

Set these environment variables:

```bash
Romarr__Postgres__Host=localhost
Romarr__Postgres__Port=5432
Romarr__Postgres__User=romarr
Romarr__Postgres__Password=secret
Romarr__Postgres__MainDb=romarr-main
Romarr__Postgres__LogDb=romarr-log
```

## Command-Line Arguments

```bash
dotnet Romarr.dll [options]
```

| Argument | Description |
|----------|-------------|
| `--port=PORT` | Override the configured port |
| `--data=/path` | Override the app data directory |
| `--nobrowser` | Don't open browser on startup |
| `--debug` | Enable debug logging |

## Environment Variables

| Variable | Description |
|----------|-------------|
| `Romarr__Server__Port` | Override port |
| `Romarr__Server__UrlBase` | Override URL base |
| `Romarr__Log__Level` | Override log level |
| `Romarr__Update__Branch` | Override update branch |

## Naming Configuration

Romarr supports configurable file naming via Settings > Media Management.

### Standard ROM Format

Tokens available:

| Token | Example | Description |
|-------|---------|-------------|
| `{Game Title}` | Super Mario Bros | Game name |
| `{Game CleanTitle}` | Super Mario Bros | Cleaned game name |
| `{Game TitleYear}` | Super Mario Bros (1985) | Game name with year |
| `{Platform}` | nes | Platform name |
| `{Platform:00}` | 01 | Zero-padded platform number |
| `{Rom Title}` | USA Rev A | ROM title |
| `{Rom:00}` | 01 | Zero-padded ROM number |
| `{Quality Title}` | Verified | Quality name |
| `{Quality Full}` | [Verified] | Quality with brackets |

### Example Naming Scheme

For simple systems:
```
{Platform}/{Game Title} ({Rom Title})
```

Produces:
```
nes/Super Mario Bros (USA Rev A).nes
```

For updateable systems (e.g. Switch eShop):
```
switch/Super Smash Bros Ultimate [01006A800016E000]/
├── Base/
│   └── Super Smash Bros Ultimate [01006A800016E000] [BASE][v0].xcz
├── DLC/
│   └── Super Smash Bros Ultimate Challenger Pack 8 [01006A800016F009] [DLC][v0].nsp
└── Updates/
    └── Super Smash Bros Ultimate [01006A800016E000] [UPDATE][v1966080].nsp
```

## Quality Definitions

Quality profiles determine which ROM versions are preferred. Sonarr video qualities (720p, 1080p, HDTV, Bluray, etc.) are legacy aliases that all map to "Unknown". Romarr's actual qualities (ranked lowest to highest):

1. **Unknown** — Unverified dump
2. **Bad** — Known bad dump
3. **Verified** — Verified against No-Intro/Redump databases

### Release Types

ROMs are also categorized by release type: Retail, Digital, Homebrew, Prototype.

### Modifications

ROM files can carry modification tags: Original, Patched, Hack, Translation.

Configure custom quality profiles in **Settings > Profiles**.
