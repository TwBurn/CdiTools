# CD-i Tools

This repository contains various tools that can be used in the development for CD-i applications and games.

## Contents

| Name  | Folder | Language | Description |
|---|---|---|---|
| CD-i Toolkit | `/CDiTools/` | .Net 5.0/C# | Various tools for Audio/Video conversion|
| NobLDtk | `/NobLDtk/` | .Net 5.0/C# | Nobelia Level convertor|

## CD-i Toolkit

### ImageToPalette
This tool extracts a color palette from an image.

| Parameter | Default Value | Description |
|---|---|---|
| Pos. 0 | | Path of the image file to be processed. (Required) |
| Pos. 1 | | Path of the output file. (Required) |
| `-f`, `--fill` | false | Fill palette to 128 colors. |
| `-m`, `--mode` | p | p = Binary Palette <br/>a = Plane A<br/>b = Plane B<br/>c = C Code (Not yet implemented)<br/>j = JSON |

### ImageToClut
This tool converts an image to a CLUT binary file using a given palette.

| Parameter | Default Value | Description |
|---|---|---|
| Pos. 0 | | Path of the image file to be processed. (Required) |
| Pos. 1 | | Path of the output file. (Required) |
| `-p`, `--palette` | | Path of palette file to use. (Required) |

> Currently this tool only outputs CLUT7 images

### AdpcmEdit
This tool performs various operations on an AIFF ADPCM audio file that has been converted for CD-i use.

| Parameter | Default Value | Description |
|---|---|---|
| Pos. 0 | | Path of the ADPCM audio (ACM) file to be processed. (Required) |
| Pos. 1 | | Path of the output file. (Required) |
| `-h`, `--header` | false | Write AIFF header.|
| `-k`, `--kill20` | false | Run Kill20 (Strip out CD block alignment bytes). |
| `-l`, `--left` | false | Write Left channel only (Mutes Right channel). |
| `-r` `--right` | false | Write Right channel only (Mutes Left channel). |

## NobLDtk
Level converter for Nobelia. Converts Nobelia LDTK world file to the Nobelia level format.