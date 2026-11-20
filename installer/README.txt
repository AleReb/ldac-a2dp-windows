LDAC A2DP

This package provides the LdacA2dp Bluetooth audio and remote control driver,
the LdacSvc audio encoder and the LDAC A2DP settings app.
Supported models in this build: JBL Live 780NC and Sony WH-1000XM2.
Requires Windows 11 22H2 or later (x64).

Installation
1. Pair the headphones in Windows Settings > Bluetooth & devices.
2. Run install.cmd and accept the administrator prompt.
3. Select Headphones (your device name) as the Windows audio output.
   Open LDAC A2DP from the Start menu.

This build uses a test certificate. Windows requires test signing to load it.
Enabling test signing requires Secure Boot to be off. To enable test signing,
run "bcdedit /set testsigning on" as administrator and restart Windows.
The installer checks this setting before installing the driver.

Compare LDAC with SBC
Play the same passage at the same volume. Select your headphones in LDAC A2DP.
Under Codec settings, select LDAC or SBC. Allow playback to reconnect, then
check Live status > Active codec before listening. Choose LDAC to switch back.

Uninstall
Open Settings > Apps > LDAC A2DP > Uninstall, or run uninstall.cmd.
Windows resumes handling the headphones. Device settings remain in
%ProgramData%\LdacA2dp. Run uninstall.ps1 -RemoveSettings to remove them too.
To turn off test signing after uninstalling, run "bcdedit /set testsigning off"
as administrator and restart Windows.

Service log: %ProgramData%\LdacA2dp\ldacsvc.log
Service status: "%ProgramFiles%\LdacA2dp\ldacsvc.exe" status

Full instructions: docs/windows-install.md (English) and docs/instalacion-windows.md (Spanish).
