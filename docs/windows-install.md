# Windows installation

## Using several headsets

The encoder service opens a session for each connected headset using the driver. Each headset keeps its codec and quality settings. The panel selects newly connected headphones, and Windows controls the playback output. To upgrade an existing installation, extract the release ZIP linked below and run its **install.cmd**.

**Requirements:** Windows 11 x64 22H2 or later, classic Bluetooth A2DP, administrator access and a supported headset. This fork was tested with Sony WH-1000XM2 and a Realtek adapter on Windows 11 Pro 26H2. The upstream project also supports JBL Live 780NC. The release bundles the encoder and settings app. Build tools are only needed for source builds.

1. Download **LdacA2dp-Windows-x64.zip** under **Assets** on the [fork release](https://github.com/AleReb/ldac-a2dp-windows/releases/tag/v0.2.324.1927-multi.1). Compare `Get-FileHash .\LdacA2dp-Windows-x64.zip -Algorithm SHA256` with **SHA256SUMS.txt**. Extract the entire archive.
2. This build uses a **local test certificate**. Loading it requires Windows test signing, a system-wide change, with Secure Boot off. Check Secure Boot in `msinfo32`. Before changing boot settings, make sure you can access your BitLocker recovery key if encryption is enabled. If you accept this configuration, run `bcdedit /set testsigning on` in an administrator terminal and restart. Managed devices and some anti-cheat software may restrict test mode. [Microsoft guidance](https://learn.microsoft.com/en-us/windows-hardware/drivers/install/the-testsigning-boot-configuration-option)
3. Pair and connect the headphones in Windows Bluetooth settings. For Sony WH-1000XM2 use sound quality priority. Run **install.cmd** from the extracted folder and approve the administrator prompt. It trusts the included public test certificate in the machine Root and TrustedPublisher stores, installs the driver, registers **LdacSvc** and adds the Start menu shortcut.
4. Select the headphones as your stereo audio output, play music and open **LDAC A2DP**. Select the headphones in the left column. Choose **LDAC** under **Codec settings → Codec** and check **Live status → Active codec**, **Streaming** and **Bitrate**.
5. To compare with LDAC off, select **SBC**, wait for **Active codec** to change and replay the same passage at the same volume. Select LDAC to switch back.

## Troubleshooting

- **HQ selected, SQ active:** adaptive bitrate may lower quality. Disable **Automatically reduce quality when the Bluetooth signal is weak** to test fixed HQ. Restore adaptive bitrate or select SQ if audio stutters or the dropped packet counter increases.
- **Empty Live status:** play audio, refresh the list, select the correct device and check `Get-Service LdacSvc`. **Reconnect device** renegotiates an active session. Reconnect offline headphones through Windows Bluetooth settings first.
- **No supported device during installation:** pair your supported headphones and rerun **install.cmd**. A different model can require additional INF hardware IDs and testing.
- **Diagnostics:** see `%ProgramData%\LdacA2dp\ldacsvc.log` or run `& "$env:ProgramFiles\LdacA2dp\ldacsvc.exe" status` in PowerShell. Redact Bluetooth addresses before sharing logs.

## Uninstall

Run **uninstall.cmd** from the extracted package or uninstall **LDAC A2DP** in Windows Settings → Apps. Reconnect your headset to use its Windows driver. Device settings remain in `%ProgramData%\LdacA2dp`. If this driver was your only reason for enabling test signing, run `bcdedit /set testsigning off` as administrator and restart. Restore Secure Boot in UEFI if you disabled it for this test.

See [NOTICE](../NOTICE) for upstream and third-party credits, including Sony's LDAC certification notice.
