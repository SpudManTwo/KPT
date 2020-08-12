# S-KPT Patcher

S-KPT Patcher is a C-Sharp tool designed solely for the purpose of applying a fan-translated English patch to the game *Kokoro Connect: Yochi Random* for the PSP. It is a fork of the original KPT. The patch is distributed separately in the [Releases](https://github.com/SpudManTwo/KPT/releases) section of this project.

It is possible, but not advisable, to use this tool to patch other PSP ISOs. This tool is specifically designed for use with the fan translation patch for *Kokoro Connect: Yochi Random*.


## Prerequisites

* To run the patcher, you must have [Microsoft .NET Framework 4.6.1 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net461) installed

* The [latest English patch](https://github.com/SpudManTwo/KPT/releases) (look for the .kpt file)

* A legitimate *Kokoro Connect: Yochi Random* ISO


## Installation

1. Download the latest release of S-KPT Patcher and the English patch from the [Releases](https://github.com/SpudManTwo/KPT/releases) section.

2. Extract the .zip archive into wherever you'd like to store the patcher. You can delete the patcher after you've finished patching your ISO.

3. Download and extract all the contents of latest version of the English patch, into the *same folder* as S-KPT Patcher.


## Usage

1. Start S-KPT Patcher by launching KPT.exe from the folder where you extracted the patcher. Your antivirus may throw up a flag, but ask it to trust the patcher.

2. Click the "Easy Patch" Button.

3. Select the path to the Kokoro Connect Yochi Random ISO file, the path to the English patch (look for a .kpt file), and the folder where you would like the patched ISO contents to be placed in the next dialog box.

4. The patcher will unpack the ISO's contents and replace them with the English patch. It will NOT overwrite your original ISO copy; the Japanese ISO will still be there after the new patched one is created.

5. Wait a few seconds while the patcher does the rest of the work.

6. When it is complete, you may close the patcher.

7. (optional) If you'd like to pack the unpacked and patched ISO contents into an .iso, .cso, or similiar image, then use a tool such as (UMDGen)[https://umdgen.en.lo4d.com/windows] to pack the patched ISO contents into a single ISO file.


## Developers Only

1. Start S-KPT Patcher by launching KPT.exe from the folder where you extracted the patcher.

5. Create a new Project.

6. Open the newly created project.

7. Dump the contents of your *Kokoro Connect: Yochi Random* ISO.

8. Unpack the ISO contents.

9. Dump the strings.

10. Extract the images.

11. Go into the directory of your newly created project and navigate to the Editable Game Files.

12. Replace the image files with English translated/edited ones.

13. Add the English translation to the the specified column in all the CSV files in the "St000" folder.

14. Select the Repacking tab in the patcher.

15. Load the strings.

16. Load the images.

17. Repack the ISO.

18. Copy the Repacked folder to wherever you'd like to store the translated game's contents.

19. (optional) Use a tool such as UMDGen to master the translated game's contents into a single ISO file.


## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

If you'd like to help with translating the game, please reach out to myself, defan752, or ask about helping out in the [Kokoro Connect Discord](https://discord.gg/PDDUkar).


## Acknowledgements

**Nuget Packages:**

[DiscUtils](https://github.com/DiscUtils/DiscUtils)
[SharpYAML](https://github.com/xoofx/SharpYaml)
[CSVHelper](https://joshclose.github.io/CsvHelper/)


**Other Code Repos**

[LibCPK/CPKTools](https://github.com/wmltogether/CriPakTools)
[Pgftools](https://github.com/tpunix/pgftool)


**dl471's code (Thank you, you are a hero!)**

[DLL for pgftools](https://github.com/dl471/pgftool)
[C# Wrapper for DLL](https://github.com/dl471/libpgf-csharp)
[KPT](https://github.com/dl471/KPT)


**Tools:**

GimConv


**Github CI:**

[shindouj](https://github.com/shindouj/KPT)
