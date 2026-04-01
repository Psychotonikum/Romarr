# ROM Organization

## How Romarr Organizes Files

Romarr stores ROM files in a structured folder hierarchy where the root folder contains platform folders, each named after a console or system:

```
{Root Folder}/
└── {Platform}/
    └── {Game Title} ({Region}).ext
```

Example for simple (non-updateable) systems:
```
/roms/
├── nes/
│   ├── Super Mario Bros (USA).nes
│   └── Super Mario Bros (Japan).nes
│   └── The Legend of Zelda (USA).nes
├── snes/
│       └── The Legend of Zelda - A Link to the Past (USA).sfc
└── genesis/
        └── Sonic the Hedgehog (USA, Europe).md
```

For updateable systems (e.g. Nintendo Switch eShop titles), game folders contain `Base/`, `DLC/`, and `Updates/` subdirectories:

```
/roms/
└── switch/
    └── Super Smash Bros.™ Ultimate [01006A800016E000]/
        ├── Base/
        │   └── Super Smash Bros.™ Ultimate [01006A800016E000] [BASE][v0].xcz
        ├── DLC/
        │   └── Super Smash Bros.™ Ultimate - Spirit Board Challenge Pack 7 [01006A800016F081] [DLC][v0].nsp
        │   └── Super Smash Bros.™ Ultimate Challenger Pack 8 [01006A800016F009] [DLC][v0].nsp
        └── Updates/
             └── Super Smash Bros.™ Ultimate [01006A800016E000] [UPDATE][v1966080].nsp
```

## Naming Conventions

### Configuring Naming

Go to **Settings > Media Management** to customize naming.

### Available Tokens

#### Game Tokens
| Token | Example |
|-------|---------|
| `{Game Title}` | Super Mario Bros |
| `{Game CleanTitle}` | Super Mario Bros |
| `{Game TitleYear}` | Super Mario Bros (1985) |
| `{Game TitleThe}` | Legend of Zelda, The |

#### Platform Tokens
| Token | Example |
|-------|---------|
| `{Platform}` | nes |
| `{Platform:00}` | 01 |

#### ROM Tokens
| Token | Example |
|-------|---------|
| `{Rom Title}` | USA Rev A |
| `{Rom CleanTitle}` | USA Rev A |
| `{Rom:00}` | 01 |

#### Quality Tokens
| Token | Example |
|-------|---------|
| `{Quality Title}` | Verified |
| `{Quality Full}` | [Verified] |

### Example Schemes

**Standard:**
```
{Game Title} ({Rom Title})
→ Super Mario Bros (USA Rev A).nes
```

**Detailed:**
```
{Game Title} ({Rom Title}) [{Quality Full}]
→ Super Mario Bros (USA Rev A) [Verified].nes
```

**With platform in filename:**
```
{Game Title} - {Platform} - {Rom Title}
→ Super Mario Bros - nes - USA Rev A.nes
```

## File Renaming

### Automatic Renaming

When Romarr downloads a new ROM, it automatically renames it according to your naming scheme.

### Manual Rename

To rename existing files:

1. Go to the game's detail page
2. Click **Files**
3. Select files to rename
4. Click **Rename** and confirm

### Bulk Rename

1. Go to **Games > Mass Editor**
2. Select games
3. Use **Rename Files** from the actions menu

## Root Folders

Root folders define base directories for your ROM storage.

### Adding Root Folders

**Settings > Media Management > Root Folders**

You can have multiple root folders for different storage locations:
- `/roms/active` — Current favorites
- `/roms/archive` — Archived collections
- `/external/roms` — External drive

### Folder Permissions

Romarr needs read and write access to root folders. On Linux:

```bash
# Ensure the Romarr user owns the ROM directory
sudo chown -R romarr:romarr /roms

# Or add the Romarr user to the appropriate group
sudo usermod -aG media romarr
```

## Importing Existing Files

### Library Import

1. Go to **Games > Library Import**
2. Select the root folder containing existing ROMs
3. Romarr scans the folder structure and matches files to games
4. Review the suggestions and adjust any incorrect matches
5. Click **Import Selected**

### Manual Import

For individual files or folders:

1. Go to **Activity > Import** (or use the manual import feature)
2. Browse to the file or folder
3. Select the target game, platform, and ROM
4. Click **Import**

## Recycle Bin

When Romarr replaces a ROM with an upgrade, the old file is moved to the recycle bin (if configured).

Configure in **Settings > Media Management > File Management**:
- **Recycling Bin**: Path to the recycle bin folder
- **Recycling Bin Cleanup**: Days before files are permanently deleted (0 = never auto-delete)
