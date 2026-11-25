# Prueba manual del control por celular

El juego, al dar Play, muestra un QR. El celular lo escanea y se vuelve el mando. Esta guía es para comprobarlo a mano, con Unity abierto y un teléfono en la misma wifi.

No hace falta `npm`, ni el servidor de la página, ni abrir https://kimaya-companion.pages.dev como mando. Esa dirección solo sirve después, para mirar el cuadro de honor.

## Antes de empezar

1. En la carpeta del juego: `git pull` de `main` en https://github.com/Thekiidd/proyecto-uni. El control con QR está desde el commit `68ffc19`.
2. Abrir el proyecto con Unity `6000.4.11f1`. La primera vez importa los scripts nuevos; hay que esperar a que termine.
3. Apagar la API de Spring (`kimaya-api`). Usa el puerto `8080`, el mismo del mando. Si sigue encendida, el juego no recibe el celular.
4. Celular y computadora en la misma red wifi. Los datos móviles del teléfono tienen que estar apagados.
5. Si Windows pregunta si Unity puede usar la red, permitirla en redes privadas.

## Cómo probarlo

1. Abrir la escena `Assets/Scenes/Ascenso_Kimaya.unity`.
2. Dar Play.
3. A la izquierda debe salir el recuadro **Escanea para jugar**, el QR y, debajo, una dirección que empieza por `http://` y la IP de la computadora, puerto `8090`. Ejemplo: `http://192.168.0.20:8090/?ip=192.168.0.20&port=8080`.
4. En la consola de Unity deben aparecer dos líneas:
   - `Mando listo en ws://...:8080/mando`
   - `Control listo en http://...`
5. Con la cámara del celular, escanear el QR. El navegador abre el mando solo.
6. Si la cámara falla, escribir en el navegador del celular la dirección que está debajo del QR. Tiene que ser la IP de la computadora, no `127.0.0.1`.
7. Al conectar, el QR desaparece y arriba queda el aviso **Control conectado**.
8. El celular pide un nombre.
   - **Listo** guarda ese nombre con la partida.
   - **Sin nombre** deja la cuenta por defecto, Falin.
   - Letras, números, espacio y guion. Máximo 24 caracteres.
9. Probar el mando en la torre:
   - Izquierda y Derecha mueven.
   - Mantener **A** carga el salto. Soltar **A** salta.
   - **START** pausa.
10. El teclado sigue funcionando igual si nadie usa el celular.
11. Jugar hasta un game over o hasta rescatar a Kiara. Solo entonces se guarda la partida.
12. Abrir https://kimaya-companion.pages.dev y mirar el cuadro de honor. La fila nueva debe decir **Cuenta** y el nombre que se escribió. Si se jugó solo con teclado, o se eligió **Sin nombre**, sigue diciendo **Cuenta Falin**.

El mismo QR aparece en cualquier escena, porque el mando arranca al dar Play. El nombre solo se guarda al terminar una partida del ascenso.

## Si algo no sale

| Qué se ve | Qué hacer |
| --- | --- |
| Aviso de que no se pudo abrir el puerto `8080` | Cerrar Spring u otro programa que use ese puerto y volver a dar Play. |
| Aviso de que no se pudo servir el control | Falta `Assets/StreamingAssets/KimayaPwa`. Volver a hacer `git pull`. |
| El QR muestra `127.0.0.1` | La computadora no está en una wifi. El celular no puede usar esa dirección. |
| El celular dice que no pudo conectar con el servidor | Casi siempre es la IP o el firewall. Revisar los dos puntos de abajo. |
| El celular no abre la página | Otra wifi, o el firewall bloqueó a Unity. Permitir Unity en redes privadas y públicas. |
| La página abre y se queda conectando | Se entró a la página de Cloudflare, o el puerto `8080` está bloqueado. Hay que usar la dirección del QR. |
| El nombre no aparece en el cuadro de honor | La partida no terminó, se cerró el celular antes del game over, o se eligió **Sin nombre**. El nombre se olvida cuando el celular se desconecta. |

El mismo nombre, escrito otra vez, reutiliza la misma cuenta. No crea un jugador nuevo.

### Si el celular dice «no se pudo conectar con el servidor»

1. **La IP del QR debe ser la de la wifi.** Debajo del QR se lee la dirección, por ejemplo `http://10.1.13.54:8090`. Si empieza con `172.27.` o `192.168.56.`, es un adaptador virtual (WSL, Hyper-V, VirtualBox) y el celular no puede llegar a ella. El juego ya descarta esos adaptadores; en la consola de Unity aparece la línea `Redes encontradas para el mando` con la que eligió. Si la PC tiene cable y wifi a la vez, el celular debe estar en la misma red que la IP elegida.
2. **El firewall de Windows.** Si la wifi está marcada como red **Pública**, Windows bloquea las conexiones entrantes. Abrir PowerShell como administrador y ejecutar una vez:

```powershell
New-NetFirewallRule -DisplayName "Kimaya mando" -Direction Inbound -Protocol TCP -LocalPort 8080,8090 -Action Allow -Profile Any
```

3. Probar antes de usar el mando: abrir `http://IP-DEL-QR:8090` en el navegador del celular. Si carga la pantalla del control, la red está bien.
4. En iPhone, si Safari pregunta por la **red local**, aceptar. Si ya se negó, ir a Ajustes → Privacidad y seguridad → Red local y activarla para Safari.
