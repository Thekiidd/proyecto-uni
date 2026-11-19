# 🐾 Kimaya: El Bosque del Eco (`proyecto-uni`)

Videojuego 2D Pixel-Art desarrollado en **Unity 6 (`6000.4.11f1`)** con arte generado mediante **PixelLab AI**, música original estilo retro 16-bit y físicas de precisión vertical inspiradas en *Jump King*.

> 🌐 **¿Buscas los recursos para la página web?**  
> Todos los **GIFs animados transparentes, sprites 4x, spritesheets CSS, ilustraciones de historia, guion PDF y música MP3** están listos en la carpeta 👉 **[`/WebAssets_Perros`](WebAssets_Perros/README.md)** (incluye el catálogo visual interactivo [`CATALOGO_WEB_INTERACTIVO.html`](WebAssets_Perros/CATALOGO_WEB_INTERACTIVO.html) y el dashboard [`WebLeaderboard/index.html`](WebLeaderboard/index.html)).

---

## 📖 Sinopsis e Historia Oficial

Durante un paseo tranquilo, **Kiara** (la hermana mayor), **Falin** y **Mía** persiguen una pelota roja que se desvía hacia un misterioso tronco hueco iluminado en el bosque. Al cruzarlo, despiertan en **El Bosque del Eco**, un reino ancestral corrompido por **El Zarzal Sombrío**. Cuando unas raíces espinosas emergen del suelo para atacarlas, **Kiara** salta frente a sus hermanas para protegerlas y es capturada hacia las alturas del **Sendero de las Raíces**.

Guiadas por **Maya**, el Hada Guardiana del Círculo de Hadas, **Falin** y **Mía** deben restaurar las **4 Runas del Círculo**, ascender por los **5 pisos de la Torre del Zarzal** y rescatar a Kiara para volver a casa.

- 🎬 **Video Oficial de la Historia (`1080p` con música y SFX):** [`Videos/Video_Historia_Kimaya_Evento.mp4`](Videos/Video_Historia_Kimaya_Evento.mp4)
- 📄 **Guion Narrativo y Diálogos (PDF):** [`WebAssets_Perros/5_Historia_y_Arte_Oficial/Guion_Narrativo_y_Dialogos_Kimaya_Oficial.pdf`](WebAssets_Perros/5_Historia_y_Arte_Oficial/Guion_Narrativo_y_Dialogos_Kimaya_Oficial.pdf)

![Mosaico Video Historia](Screenshots/Captura_Video_Historia_Mosaico.png)

---

## 🐕 Personajes Actualizados

| Personaje | Raza / Rol | Personalidad y Habilidades | Atributos en *El Ascenso* (Torre 5 Pisos) |
| :--- | :--- | :--- | :--- |
| **Falin** | **Border Collie** (Negro/Blanco, arnés azul)<br>*Protagonista Jugable (Hermana Mediana)* | Impulsiva, valiente y protectora.<br>**Habilidad en Hub:** `Ladrido Protector` (`[ESPACIO]` / `[F]`). | **Especialista en salto vertical potente:**<br>`g = 3.15` · `Vy = 17.80` · `Vx = 6.25`<br>Altura máx.: `5.12 u` · Alcance: `7.20 u` |
| **Mía** | **Schnauzer Miniatura** (Gris/Plata, arnés rojo)<br>*Protagonista Jugable (Hermana Menor)* | Observadora, analítica y ágil.<br>**Habilidad en Hub:** `Olfato Mágico` (`[ESPACIO]` / `[F]`). | **Especialista en agilidad y alcance horizontal:**<br>`g = 2.90` · `Vy = 17.10` · `Vx = 6.35`<br>Altura máx.: `5.13 u` · Alcance: `7.74 u` |
| **Kiara** | **Schnauzer** (Gris claro, collar morado)<br>*Hermana Mayor / Objetivo de Rescate* | Responsable y protectora. Se sacrifica para salvar a Falin y Mía del Zarzal Sombrío. | Cautiva en el **Altar de la Cumbre (Piso 5)**. Al rescatarla se reúne con sus hermanas en el Hub. |
| **Maya** | **Hada Guardiana del Círculo**<br>*NPC Guía en el Santuario (`Hub`)* | Guardiana ancestral del Círculo de Hadas. Orienta a las hermanas sobre las 4 Runas y el Sendero de las Raíces. | Interactuable en el Hub con `[E]` (diálogo con retrato animado y voz RPG). |
| **El Zarzal Sombrío** | **Fuerza Corruptora del Bosque**<br>*Antagonista Ambiental* | Invade el bosque con raíces oscuras y trampas de espinas moradas. | Trampas de picos (`KimayaSpikeHazard`) en los Pisos 1–5 que empujan al jugador hacia el vacío. |

---

## 🏰 Modos y Escenas del Juego

### 1. 🌲 Santuario del Círculo de Hadas (`Assets/Scenes/Hub.unity`)
![Santuario Hub 3 Hermanas](Screenshots/Captura_Hub_Santuario_Maya_y_Hermanas.png)
- **Dúo Dinámico en 4 Direcciones (`KimayaHubDuoController`):** Camina y corre (`Shift`) por el santuario con **Falin** y **Mía**; tu compañera te sigue automáticamente. Presiona `TAB` o `Q` para alternar de líder en cualquier momento.
- **Diálogo con el Hada Maya (`KimayaStoryFairy`):** Acércate a Maya y presiona `E` para conversar sobre la historia y el estado de las runas.
- **Círculo Rúnico y Portales (`KimayaHubRunicCircle`):** HUD superior izquierdo con el estado de las **4 Runas** y portal verde activo hacia **El Ascenso**.
- **Colisiones Naturales y Menú del Santuario (`[ESC]`):** Límites físicos en acantilados, pinos y árboles perimetrales, más menú de pausa con acceso a **Continuar**, **Ajustes** y **Menú Principal**.
- **Estado Post-Rescate:** Una vez completada la torre, **Kiara** aparece en el centro del Círculo junto a Falin y Mía, y el portal permite volver a jugar para mejorar tu récord de tiempo.

### 2. 🧗‍♂️ El Ascenso: Torre del Zarzal de 5 Pisos (`Assets/Scenes/Ascenso_Kimaya.unity`)
![El Ascenso Falin Border Collie](Screenshots/Captura_Ascenso_Kimaya_Falin_BorderCollie.png)
- **Física Táctica Estilo *Jump King* (`KimayaJumpKingController`):**
  - **Salto Vertical o Direccional:** Mantén `ESPACIO` para cargar la barra inferior. Si sueltas `ESPACIO` sin presionar `A` ni `D`, saltas **recto hacia arriba**; si mantienes `A` o `D`, saltas con parábola fija hacia ese lado.
  - **Rebote en Muros y Troncos:** Chocar lateralmente en el aire invierte tu velocidad horizontal con chispas pixel-art y sonido de impacto.
  - **Resbalón Realista en Bordes (`ApplyLedgeOverhangSlip`):** Evita que el personaje quede flotando con medio cuerpo fuera de la plataforma.
- **Trampas del Zarzal Sombrío (`KimayaSpikeHazard`):** Espinas moradas que aplican retroceso físico (*knockback*) hacia el vacío, parpadeo de daño y sonido de impacto.
- **3 Scooby Galletas y Medallas de Tiempo (`KimayaPixelHUD`):**
  - Coleccionables ocultos en rutas de riesgo de los Pisos 1, 3 y 4.
  - Cronómetro con medallas al rescatar a Kiara: 🥇 **Oro (`< 01:45`)**, 🥈 **Plata (`< 03:00`)**, 🥉 **Bronce (`>= 03:00`)**.
- **Pantallas de Carga Animadas (`KimayaSceneTransition`) y Partículas (`KimayaPixelVFX`):** Cortinillas con Falin o Mía corriendo en pixel-art, consejos dinámicos, polvo al saltar/aterrizar y letreros al descubrir cada nuevo piso.

---

## 🎵 Música Oficial y Efectos de Sonido Retro 16-Bit (`KimayaAudio.cs`)

- **Banda Sonora Oficial (`Assets/Resources/Music/`):**
  - 🌅 `Title_Screen_Sunrise.mp3` — Menú Principal (`MainMenu`) y Prólogo Ilustrado (`Prologue_Story`).
  - 🌿 `Under_the_Emerald_Boughs.mp3` — Santuario (`Hub`), Torre del Zarzal (`Ascenso_Kimaya`) y Final (`Ending`), con transición suave (*crossfade*).
- **Efectos de Sonido Retro 16-Bit:**
  - Pulso ascendente al cargar salto (`ChargeTick`), brinco normal (`Jump`), brinco cargado (`PowerJump`), golpe/rebote en muro (`WallBounce`), silbido de caída libre (`FallWhistle`), impacto grave de caída fuerte/*splat* (`HeavyFall`), golpe en espinas (`Damage`), recolección de galleta (`Checkpoint`), pasos (`Step`), ladrido (`Bark`) y olfato (`Smell`).
- **Control en Tiempo Real (`KimayaSettingsController`):** Barras independientes de **Música (`-`/`+`)** y **Efectos SFX (`-`/`+`)** además de Calidad Gráfica (`Baja / Media / Alta`), Resolución y Pantalla Completa, persistentes en `PlayerPrefs`.

---

## 🕹️ Controles Principales

| Acción | Teclado | Escena |
| :--- | :--- | :--- |
| **Moverse** | `A` `D` `W` `S` o Flechas | Santuario (`Hub`) |
| **Correr** | `Shift` + Movimiento | Santuario (`Hub`) |
| **Caminar en Plataforma** | `A` / `D` o Flechas `←` / `→` | Torre (`Ascenso_Kimaya`) |
| **Cargar y Saltar (Recto o Diagonal)** | Mantener `ESPACIO` (+ opcional `A`/`D`) y soltar | Torre (`Ascenso_Kimaya`) |
| **Cambiar entre Falin y Mía** | `TAB` / `Q` | Santuario (`Hub`) y Torre |
| **Habilidad (Ladrido / Olfato)** | `ESPACIO` o `F` | Santuario (`Hub`) |
| **Hablar con Maya / Entrar al Portal** | `E` | Santuario (`Hub`) |
| **Menú de Pausa y Ajustes** | `ESC` | Santuario (`Hub`) y Torre |

---

## 📁 Estructura del Repositorio para el Equipo

- [`Assets/Scripts/Kimaya/`](Assets/Scripts/Kimaya/) — Código fuente C# de físicas, torre de 5 pisos, dúo del Hub, audio, HUD, partículas y transiciones.
- [`Assets/Resources/`](Assets/Resources/) — Sprites de Falin, Mía, Kiara, Hada Maya, Zarzal, música oficial MP3 y fuentes pixel-art.
- [`WebAssets_Perros/`](WebAssets_Perros/README.md) — **Kit completo para el equipo Web** (573 archivos: GIFs animados, PNGs 4x, spritesheets CSS, ilustraciones de historia, guion PDF, música MP3 y catálogo HTML).
- [`WebLeaderboard/index.html`](WebLeaderboard/index.html) — Salón de la Fama / Leaderboard web con avatares pixel-art de Falin, Mía y Kiara.
- [`Mapas/`](Mapas/README.md) — Mapas del Hub y portales en alta resolución.
- [`Screenshots/`](Screenshots/) y [`Videos/`](Videos/) — Capturas actualizadas y video de historia para exposición (`Video_Historia_Kimaya_Evento.mp4`).
