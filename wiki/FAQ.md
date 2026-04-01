# Frequently Asked Questions

## General

### What is Romarr?

Romarr is a ROM management system for retro gaming enthusiasts. It automates downloading, organizing, and renaming game ROMs. It's built on the same architecture as Sonarr and Radarr, so the UI and workflow will feel familiar.

### What port does Romarr use?

The default port is **9797**. You can change it in `config.xml` or via the `--port` command-line argument.

### Is Romarr free?

Yes. Romarr is open source under the GNU GPL v3 license.

### What platforms does Romarr run on?

- Linux (Debian, Ubuntu, CentOS, etc.)
- Windows 10+
- macOS 12+
- Docker
- Raspberry Pi (ARM)

---

## Setup

### How do I change the port?

Edit your `config.xml`:

```xml
<Config>
  <Port>9797</Port>
</Config>
```

Or start with: `dotnet Romarr.dll --port=9797`

### How do I set up authentication?

Go to **Settings > General > Security** and choose an authentication method:
- **None** — No login required (not recommended if exposed to the internet)
- **Basic** — Username/password via HTTP Basic Auth
- **Forms** — Login form in the web UI

### Where is the config file?

| OS | Path |
|----|------|
| Linux | `~/.config/Romarr/config.xml` |
| macOS | `~/.config/Romarr/config.xml` |
| Windows | `%AppData%\Romarr\config.xml` |
| Docker | `/config/config.xml` |

### How do I reset my API key?

Delete the `<ApiKey>` line from `config.xml` and restart Romarr. A new key will be auto-generated.

### Can I use PostgreSQL instead of SQLite?

Yes. Set these environment variables:

```bash
Romarr__Postgres__Host=localhost
Romarr__Postgres__Port=5432
Romarr__Postgres__User=romarr
Romarr__Postgres__Password=secret
Romarr__Postgres__MainDb=romarr-main
Romarr__Postgres__LogDb=romarr-log
```

---

## Games & ROMs

### What's the domain mapping from Sonarr?

Romarr reuses the Sonarr database schema internally but maps TV concepts to gaming:

| Sonarr Term | Romarr Term | Description |
|-------------|-------------|-------------|
| Series | Game | A game title |
| Season | Platform | A gaming platform (NES, SNES, etc.) |
| Episode | ROM | An individual ROM/game file |
| Episode File | ROM File | Physical file on disk |
| Video qualities (720p, 1080p, HDTV, Bluray) | Legacy aliases for "Unknown" | Actual qualities: Unknown, Bad, Verified |

### How are ROMs organized on disk?

By default: `{Root Folder}/{Game Title}/Platform {Number}/`

Example:
```
/roms/Super Mario Bros/Platform 01/Super Mario Bros (USA).nes
```

You can customize naming in **Settings > Media Management**.

### Can I import ROMs I already have?

Yes. Use **Games > Library Import** or the manual import feature to scan existing folders and match ROM files to games.

### What happens when a better ROM becomes available?

If your quality profile has a cutoff above the current file's quality, Romarr will automatically download the upgrade and replace the old file (keeping a copy in the recycle bin if configured).

---

## Download Clients

### Which download clients are supported?

**Usenet**: SABnzbd, NZBGet, NZBVortex
**Torrent**: qBittorrent, Transmission, Deluge, rTorrent, uTorrent, Vuze, Aria2, Flood

### My download client isn't connecting

1. Verify the host, port, and API key in **Settings > Download Clients**
2. Click **Test** to check the connection
3. Ensure the client is running and accessible from the Romarr machine
4. Check firewall rules

---

## Troubleshooting

### Romarr won't start

- Check if port 9797 is already in use: `lsof -i :9797`
- Check logs at `~/.config/Romarr/logs/`
- Ensure .NET 10 runtime is installed: `dotnet --info`

### The web UI is blank/white

- Clear your browser cache (Ctrl+Shift+R)
- Rebuild the frontend: `yarn build`
- Check browser console for JavaScript errors

### Database is locked

Only one instance of Romarr can access the database at a time. Check for:
- Multiple Romarr processes: `ps aux | grep -i romarr`
- "Database is locked" errors in logs

Kill any duplicate processes and restart.

### Downloads aren't being imported

1. Check **Activity > Queue** for stuck items
2. Verify file permissions — Romarr must be able to read the download folder and write to the ROM folder
3. Check **System > Status** for health warnings
4. Ensure the download client's "completed download" folder is accessible

### I can't connect to the API

- Include `X-Api-Key` header in all requests
- Find your key at **Settings > General > Security**
- Verify the URL: `http://localhost:9797/api/v3/system/status`
