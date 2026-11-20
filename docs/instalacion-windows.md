# Instalar LDAC A2DP en Windows

## Varios auriculares

El servicio abre una sesión por cada auricular conectado que utilice el controlador. Cada uno conserva su códec y calidad. El panel selecciona el que acabas de conectar y Windows controla por dónde se reproduce el audio. Para actualizar una instalación anterior, extrae el ZIP de la versión enlazada más abajo y ejecuta su **install.cmd**.

Esta edición incluye un panel en inglés y los identificadores Bluetooth de los Sony WH-1000XM2 y JBL Live 780NC. En el equipo de desarrollo se verificó transmisión LDAC HQ a 990 kbps y reconexión con los Sony, un adaptador Realtek y Windows 11 Pro 26H2 x64. El proyecto original documenta sus pruebas con los JBL. La tasa alcanzada depende del enlace.

## Antes de instalar

- Windows 11 x64, versión 22H2 o posterior, Bluetooth clásico A2DP y una cuenta de administrador. El instalador incluye el servicio y el panel. El PC de destino no necesita Visual Studio, .NET SDK ni WDK.
- Esta compilación lleva una **firma local de prueba**. Windows necesita el modo de prueba en cada arranque en el que vayas a usar el controlador. Es un cambio de seguridad de todo el equipo. Algunos equipos administrados y programas antitrampas lo impiden. Utiliza esta versión solo en un PC donde puedas aceptar ese cambio.
- Comprueba **Estado de arranque seguro** en `msinfo32`. Si está activado, esta compilación requiere desactivarlo en el firmware UEFI. Si usas BitLocker o Cifrado de dispositivo, comprueba antes que puedes recuperar tu clave: cambiar el arranque puede hacer que Windows la solicite. [Documentación de Microsoft](https://learn.microsoft.com/es-es/windows-hardware/drivers/install/the-testsigning-boot-configuration-option)

## Descargar e instalar

1. Abre la [versión del fork](https://github.com/AleReb/ldac-a2dp-windows/releases/tag/v0.2.324.1927-multi.1). En **Assets**, descarga **LdacA2dp-Windows-x64.zip**. Los archivos **Source code** contienen el código para desarrolladores. Para verificar el ZIP, ejecuta `Get-FileHash .\LdacA2dp-Windows-x64.zip -Algorithm SHA256` desde su carpeta y compara el resultado con **SHA256SUMS.txt** de la misma versión.
2. En Terminal **como administrador**, ejecuta `bcdedit /set testsigning on` y reinicia. Si Windows indica que Secure Boot protege el valor, revisa los requisitos anteriores.
3. Empareja y conecta los auriculares en **Configuración → Bluetooth y dispositivos**. En los Sony elige la prioridad de calidad de sonido si cambiaste antes ese ajuste. [Guía de Sony](https://helpguide.sony.net/mdr/wh1000xm2/v1/es/contents/TP0001513476.html)
4. Usa **Extraer todo** sobre el ZIP. En la carpeta extraída, ejecuta **install.cmd**, acepta el aviso de administrador y espera el mensaje **Done**. El instalador importa el certificado público de prueba en los almacenes locales de raíces de confianza y editores de confianza, instala el controlador y registra el servicio **LdacSvc**.
5. Selecciona **Auriculares** de los Sony o JBL como salida estéreo de Windows. Reproduce música y abre **LDAC A2DP** desde Inicio. Selecciona tus auriculares en la columna izquierda y **LDAC** en **Codec settings → Codec**.

## Confirmar y comparar

En **Live status**, comprueba **Active codec = LDAC**, **Streaming = Active** y **Bitrate**. Para escuchar la diferencia con LDAC desactivado, elige **SBC**, espera que **Active codec** cambie y repite el mismo fragmento al mismo volumen. Para volver, elige LDAC. El perfil de llamadas usa su propio controlador.

- **HQ seleccionado, SQ en vivo.** Con el ajuste automático activado, HQ es el máximo. Para probar HQ fijo, desmarca **Automatically reduce quality when the Bluetooth signal is weak**. Si oyes cortes o aumenta **Dropped packets**, vuelve al ajuste automático o elige SQ.
- **Live status vacío.** Reproduce audio, pulsa **Refresh list** y selecciona el dispositivo correcto. Comprueba la salida estéreo de Windows y `Get-Service LdacSvc` en PowerShell. **Reconnect device** renegocia una sesión de audio activa. Para unos auriculares desconectados, reconecta primero desde Bluetooth en Windows.
- **El instalador no encuentra los auriculares.** Empareja el modelo compatible y vuelve a ejecutar **install.cmd**. Otros modelos con LDAC pueden requerir identificadores adicionales en el INF y pruebas con ese hardware.
- **Diagnóstico.** El registro está en `%ProgramData%\LdacA2dp\ldacsvc.log`. En PowerShell puedes ejecutar `& "$env:ProgramFiles\LdacA2dp\ldacsvc.exe" status`. Los registros pueden incluir direcciones Bluetooth. Ocúltalas antes de publicarlos.

## Desinstalar y restaurar el arranque

Ejecuta **uninstall.cmd** desde la carpeta extraída o abre **Configuración → Aplicaciones → Aplicaciones instaladas → LDAC A2DP → Desinstalar**. Reconecta los auriculares y elige su salida de Windows. Los ajustes se conservan en `%ProgramData%\LdacA2dp`. Si habilitaste el modo de prueba solo para este controlador, ejecuta `bcdedit /set testsigning off` como administrador y reinicia. Si desactivaste Secure Boot para esta prueba, restaura su configuración anterior en UEFI teniendo a mano la clave de recuperación.

## Procedencia

Fork de [yariktop133/ldac-a2dp-windows](https://github.com/yariktop133/ldac-a2dp-windows). El codificador LDAC proviene de Sony a través de AOSP. Consulta [LICENSE](../LICENSE) y [NOTICE](../NOTICE) para atribución y la nota de Sony sobre certificación de productos. Este proyecto es independiente de Sony y Microsoft.
