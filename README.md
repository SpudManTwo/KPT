# S-KPT

S-KPT is a fork of the original KPT and is designed solely for the purpose of patching Kokoro Connect Yoshi Random for the PSP. While it is possible to use this tool for other iso patching, I would not advise it as this tool is designed for use with the Fan Translation Patch for that specific game.

## Installation

S-KPT, like the original KPT, requires the .NET Framework 4.6.1 Runtime as distributed by Microsoft [here](https://dotnet.microsoft.com/download/dotnet-framework/net461)

For ease of use, it is recommended that you use the pre-compiled translation files as found (here)[https://drive.google.com/open?id=1YN8hdZqJq1xPCMvfAZToc_lOUc2poZD8]

Additionally, if you'd like to master the final game files into an .iso, .cso, or similiar, then I would personally recommend using (UMDGen)[https://umdgen.en.lo4d.com/windows].

Most importantly, **YOU MUST ALREADY HAVE A WORKING LEGITIMATE. I WILL NOT BE DISTRIBUTING THE ISO OR A PATCH THAT WORKS WITHOUT THE ISO. THAT IS PIRACY** 

## How To Use (Easy)

1. Ensure you have everything installed.
2. Download the latest release of S-KPT.
3. Extract the .zip archive into wherever you'd like to store the tool. (Don't worry, you can delete the tool after you've finished patching your iso).
4. Download and extract all the contents of latest version of the pre-compiled translation files.
5. Run KPT.exe from the folder where you extracted the tool into.
6. Click the Easy Patch Button.
7. Select the Kokoro Connect Yoshi Random ISO file in the file dialogue box.
8. Select the folder where you would like the patched iso contents to be placed in the next dialogue box.
9. Select the folder where you you extracted the pre-compiled translation files. The tool will not properly unless you select the correct directory here.
10. Wait a few seconds while the tool does the rest of the work.
11. (optional) Use a tool such as UMDGen to master the patched iso contents into a single iso file.

## How To Use (Hard)
1. Ensure you have everything installed.
2. Download the latest release of S-KPT.
3. Extract the .zip archive into wherever you'd like to store the tool.
4. Run KPT.exe from the folder where you extracted the tool into.
5. Create a new Project.
6. Open the newly created project.
7. Dump the contents of your Kokoro Connect Yoshi Random ISO
8. Unpack the ISO Files.
9. Dump the strings.
10. Extract the images.
11. Go into the directory of your newly created project and navigate to the Editable Game Files.
12. Replace the image files with english translated/edited ones.
13. Add the english translation to the the specified column in all the csv files in the St000 folder.
14. Switch to the Repacking tab in the tool.
15. Load the Strings
16. Load the Images.
17. Repack the iso files.
18. Copy the Repacked folder to wherever you'd like to store the translated game's contents
19. (optional) Use a tool such as UMDGen to master the translated game's contents into a single iso file.

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
