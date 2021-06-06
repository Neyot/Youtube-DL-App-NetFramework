# Youtube-DL-App
[![GitHub license](https://img.shields.io/github/license/Neyot/Youtube-DL-App)](https://github.com/Neyot/Youtube-DL-App/blob/main/LICENSE)
[![GitHub release](https://img.shields.io/github/v/release/Neyot/Youtube-DL-App?include_prereleases)](https://github.com/Neyot/Youtube-DL-App/releases)
[![GitHub all downloads](https://img.shields.io/github/downloads/Neyot/Youtube-DL-App/total)](https://github.com/Neyot/Youtube-DL-App/releases)

[Youtube-DL](https://github.com/ytdl-org/youtube-dl) C# Wrapper with a WPF GUI.

# Current Features
- [x] Download audio (.mp3) from a given Youtube link to a specific output folder.
- [x] A console window that shows the output from **youtube-dl.exe** which can be opened with the program by using the "--console" switch.
- [x] Automatically search for a valid **youtube-dl.exe** executable or failing that, allow the user to specify the location of the executable.
- [x] Automatic build process in Powershell using msbuild with semi-automatic version incrementing.

# TODO List (Planned Features)
- [ ] Choosing the output file type.
- [ ] A set of checkboxes to enable/disable different **youtube-dl.exe** arguments/settings.
- [ ] Download video in a chosen format from a given Youtube link to a specific output folder.
- [ ] Split the audio and video downloaders into separate tabs using WPF TabControl.
- [ ] Add a separate Settings window which more clearly helps the user select their **youtube-dl.exe** executable.
- [ ] In the Settings window, list the other executables that **youtube-dl.exe** requires and have a tick or a cross next to them to indicate whether the requirement for that binary is filled (and possibly an info window/hover-over which instructs the user how/where to download the required executable).
