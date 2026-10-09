import os, math
from PIL import Image, ImageDraw, ImageFont, ImageFilter

base_dir = r"D:\gamess\My project (9)"
ss_dir = os.path.join(base_dir, "Screenshots")
vid_dir = os.path.join(base_dir, "Videos")
os.makedirs(ss_dir, exist_ok=True)
os.makedirs(vid_dir, exist_ok=True)

# Rutas de assets
bg_menu_path = os.path.join(base_dir, "Assets", "Sprites", "UI", "MainMenu_Background.png")
banner_path  = os.path.join(base_dir, "Assets", "Sprites", "UI", "Title_Banner.png")
btn_norm_path= os.path.join(base_dir, "Assets", "Sprites", "UI", "Button_Wood_Normal.png")
btn_hov_path = os.path.join(base_dir, "Assets", "Sprites", "UI", "Button_Wood_Hover.png")
paw_path     = os.path.join(base_dir, "Assets", "Sprites", "UI", "Icon_Paw.png")
panel_path   = os.path.join(base_dir, "Assets", "Sprites", "UI", "Panel_Wood_Frame.png")

bg_level_path= os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Kimaya_Level_Background.jpg")
tile_path    = os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Stone_Platform.png")
plank_path   = os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Wood_Plank.png")
fulcrum_path = os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Seesaw_Fulcrum.png")
crate_path   = os.path.join(base_dir, "Assets", "Sprites", "Village", "Art", "Props", "Crate_Medium_Closed.png")
gem_g_path   = os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Gem_Green.png")
gem_p_path   = os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Gem_Purple.png")
door_path    = os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Door_Portal.png")
dog_idle_path= os.path.join(base_dir, "Assets", "Sprites", "Dogs", "Great-Dane", "Great-Dane-idle.png")
dog_run_path = os.path.join(base_dir, "Assets", "Sprites", "Dogs", "Great-Dane", "Great-Dane-run.png")

# Fuentes
font_title = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 54)
font_sub   = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 22)
font_btn   = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 28)
font_timer = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 32)
font_hint  = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 16)
font_modal_h = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 32)
font_modal_b = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 20)

def draw_text_shadow(draw, pos, text, font, fill, shadow_fill=(20, 10, 5, 220), offset=(2, 2)):
    draw.text((pos[0] + offset[0], pos[1] + offset[1]), text, font=font, fill=shadow_fill)
    draw.text(pos, text, font=font, fill=fill)

# ==========================================
# 1. RENDERIZAR MAIN MENU
# ==========================================
def render_main_menu(btn_hover_idx=-1):
    canvas = Image.new("RGBA", (1920, 1080), (10, 20, 10, 255))
    if os.path.exists(bg_menu_path):
        bg = Image.open(bg_menu_path).convert("RGBA").resize((1920, 1080), Image.Resampling.LANCZOS)
        canvas.paste(bg, (0, 0))

    # Viñeta suave
    overlay = Image.new("RGBA", (1920, 1080), (10, 20, 15, 35))
    canvas = Image.alpha_composite(canvas, overlay)

    # Banner Superior (900x150 en X=510, Y=80)
    if os.path.exists(banner_path):
        banner = Image.open(banner_path).convert("RGBA").resize((900, 150), Image.Resampling.LANCZOS)
        canvas.paste(banner, (510, 70), banner)

    # Huellitas en el banner
    if os.path.exists(paw_path):
        paw = Image.open(paw_path).convert("RGBA").resize((52, 52), Image.Resampling.LANCZOS)
        canvas.paste(paw, (570, 120), paw)
        canvas.paste(paw, (1298, 120), paw)

    draw = ImageDraw.Draw(canvas)
    # Título "KIMAYA"
    bbox_t = font_title.getbbox("K I M A Y A")
    w_t = bbox_t[2] - bbox_t[0]
    draw_text_shadow(draw, (960 - w_t//2, 95), "K I M A Y A", font_title, (255, 224, 90, 255), shadow_fill=(35, 18, 5, 240), offset=(3, 3))

    # Subtítulo "El Bosque del Eco"
    bbox_s = font_sub.getbbox("El Bosque del Eco")
    w_s = bbox_s[2] - bbox_s[0]
    draw_text_shadow(draw, (960 - w_s//2, 162), "El Bosque del Eco", font_sub, (255, 250, 235, 240), shadow_fill=(35, 18, 5, 220), offset=(2, 2))

    # 4 Botones de madera centrados
    buttons = ["▶  JUGAR", "AJUSTES", "CÓMO JUGAR", "✕  SALIR"]
    btn_w, btn_h = 420, 68
    start_y = 390
    spacing = 78

    btn_img_norm = Image.open(btn_norm_path).convert("RGBA").resize((btn_w, btn_h), Image.Resampling.LANCZOS) if os.path.exists(btn_norm_path) else None
    btn_img_hov  = Image.open(btn_hov_path).convert("RGBA").resize((btn_w, btn_h), Image.Resampling.LANCZOS) if os.path.exists(btn_hov_path) else btn_img_norm

    for i, label in enumerate(buttons):
        by = start_y + i * spacing
        bx = 960 - btn_w // 2

        cur_btn_img = btn_img_hov if i == btn_hover_idx else btn_img_norm
        if cur_btn_img:
            canvas.paste(cur_btn_img, (bx, by), cur_btn_img)

        bbox_b = font_btn.getbbox(label)
        bw = bbox_b[2] - bbox_b[0]
        bh = bbox_b[3] - bbox_b[1]
        tx = 960 - bw // 2
        ty = by + (btn_h - bh) // 2 - 4

        txt_col = (255, 245, 180, 255) if i == btn_hover_idx else (255, 240, 215, 255)
        draw_text_shadow(draw, (tx, ty), label, font_btn, txt_col, shadow_fill=(40, 20, 8, 240), offset=(2, 2))

    return canvas

# ==========================================
# 2. RENDERIZAR LEVEL 01 (BOSQUE TEMPLE)
# ==========================================
def render_level_01(falin_offset_x=0, gem_phase=0.0):
    canvas = Image.new("RGBA", (1920, 1080), (10, 20, 15, 255))
    if os.path.exists(bg_level_path):
        bg = Image.open(bg_level_path).convert("RGBA").resize((1920, 1080), Image.Resampling.LANCZOS)
        canvas.paste(bg, (0, 0))

    draw = ImageDraw.Draw(canvas)

    # Coordenadas de Unity (-13 a +13 en X, -7.5 a +7.5 en Y) -> (0 a 1920, 1080 a 0)
    def to_screen(ux, uy):
        sx = int((ux + 13.5) / 27.0 * 1920)
        sy = int((7.6 - uy) / 15.2 * 1080)
        return sx, sy

    # Helper para dibujar plataformas con textura de piedra
    tile_img = Image.open(tile_path).convert("RGBA") if os.path.exists(tile_path) else None

    def draw_plat(ux, uy, uw, uh):
        x1, y1 = to_screen(ux - uw/2, uy + uh/2)
        x2, y2 = to_screen(ux + uw/2, uy - uh/2)
        pw, ph = x2 - x1, y2 - y1
        if tile_img and pw > 0 and ph > 0:
            tiled = Image.new("RGBA", (pw, ph))
            for tx in range(0, pw, tile_img.width):
                for ty in range(0, ph, tile_img.height):
                    tiled.paste(tile_img, (tx, ty))
            canvas.paste(tiled, (x1, y1), tiled)
        else:
            draw.rectangle([x1, y1, x2, y2], fill=(42, 60, 48, 255), outline=(75, 110, 80, 255), width=2)

    # Marco
    draw_plat(0, 7.3, 27, 1.2)      # Ceiling
    draw_plat(0, -7.3, 27, 1.2)     # Floor
    draw_plat(-12.8, 0, 1.2, 15.5)  # Left wall
    draw_plat(12.8, 0, 1.2, 15.5)   # Right wall

    # Pisos
    draw_plat(-9.2, 0, 6.2, 0.7)    # Mid left
    draw_plat(9.2, 0, 6.2, 0.7)     # Mid right
    draw_plat(-6.5, 4.2, 6, 0.6)    # Top left
    draw_plat(6.5, 4.2, 6, 0.6)     # Top right
    draw_plat(-3.2, 5.4, 2.2, 0.45) # Float 1
    draw_plat(0, 5.0, 2.2, 0.45)    # Float 2
    draw_plat(3.2, 5.4, 2.2, 0.45)  # Float 3
    draw_plat(-6.5, -3.6, 4.5, 0.6) # Low divider
    draw_plat(-4.5, -5.2, 0.6, 3.8) # Low vertical

    # Balancín Central (Seesaw)
    sx, sy = to_screen(0, 0.1)
    if os.path.exists(fulcrum_path):
        fulc = Image.open(fulcrum_path).convert("RGBA").resize((48, 48), Image.Resampling.LANCZOS)
        canvas.paste(fulc, (sx - 24, sy - 8), fulc)
    if os.path.exists(plank_path):
        plank = Image.open(plank_path).convert("RGBA").resize((480, 26), Image.Resampling.LANCZOS)
        # Rotar ligeramente según física
        angle = math.sin(gem_phase) * 6
        plank_rot = plank.rotate(angle, expand=True, resample=Image.Resampling.BICUBIC)
        canvas.paste(plank_rot, (sx - plank_rot.width//2, sy - 28), plank_rot)

    # Balancín Inferior
    sx2, sy2 = to_screen(3.5, -5.4)
    if os.path.exists(fulcrum_path):
        fulc = Image.open(fulcrum_path).convert("RGBA").resize((48, 48), Image.Resampling.LANCZOS)
        canvas.paste(fulc, (sx2 - 24, sy2 - 8), fulc)
    if os.path.exists(plank_path):
        plank2 = Image.open(plank_path).convert("RGBA").resize((420, 26), Image.Resampling.LANCZOS)
        angle2 = math.cos(gem_phase) * -5
        plank2_rot = plank2.rotate(angle2, expand=True, resample=Image.Resampling.BICUBIC)
        canvas.paste(plank2_rot, (sx2 - plank2_rot.width//2, sy2 - 28), plank2_rot)

    # Caja Empujable de madera
    cx, cy = to_screen(-7.0, 0.85)
    if os.path.exists(crate_path):
        crate = Image.open(crate_path).convert("RGBA").resize((75, 75), Image.Resampling.LANCZOS)
        canvas.paste(crate, (cx - 37, cy - 37), crate)

    # Placa de presión verde
    px, py = to_screen(-4.2, 0.45)
    plate_top_path = os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Plate_Top.png")
    plate_base_path= os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Plate_Base.png")
    if os.path.exists(plate_base_path):
        p_base = Image.open(plate_base_path).convert("RGBA").resize((85, 24), Image.Resampling.LANCZOS)
        canvas.paste(p_base, (px - 42, py - 12), p_base)
    if os.path.exists(plate_top_path):
        p_top = Image.open(plate_top_path).convert("RGBA").resize((75, 18), Image.Resampling.LANCZOS)
        canvas.paste(p_top, (px - 37, py - 20), p_top)

    # Puerta rúnica verde
    dx, dy = to_screen(11.8, -2.6)
    draw.rectangle([dx - 28, dy - 95, dx + 28, dy + 95], fill=(35, 140, 70, 220), outline=(120, 255, 170, 255), width=3)

    # Puerta de salida (Portal)
    vx, vy = to_screen(11.8, -5.5)
    if os.path.exists(door_path):
        door = Image.open(door_path).convert("RGBA").resize((85, 135), Image.Resampling.LANCZOS)
        canvas.paste(door, (vx - 42, vy - 67), door)

    # Gemas flotantes (Verdes y Moradas)
    gem_g = Image.open(gem_g_path).convert("RGBA").resize((38, 38), Image.Resampling.LANCZOS) if os.path.exists(gem_g_path) else None
    gem_p = Image.open(gem_p_path).convert("RGBA").resize((38, 38), Image.Resampling.LANCZOS) if os.path.exists(gem_p_path) else None

    green_gems = [(-3.2, 6.2), (0, 5.8), (3.2, 6.2), (-9.5, 1.2)]
    for gx, gy in green_gems:
        gx_s, gy_s = to_screen(gx, gy + math.sin(gem_phase + gx)*0.12)
        if gem_g: canvas.paste(gem_g, (gx_s - 19, gy_s - 19), gem_g)

    purple_gems = [(6.5, 5.0), (9.5, 1.2), (7.5, -4.2)]
    for px, py in purple_gems:
        px_s, py_s = to_screen(px, py + math.sin(gem_phase + px)*0.12)
        if gem_p: canvas.paste(gem_p, (px_s - 19, py_s - 19), gem_p)

    # Personaje Jugador (Falin / Gran Danés con sprite real)
    fx, fy = to_screen(-10.5 + falin_offset_x, 0.85)
    falin_sprite = Image.open(dog_run_path if falin_offset_x > 0 else dog_idle_path).convert("RGBA")
    # Si es spritesheet, tomar el primer frame
    fw = falin_sprite.width // 4 if falin_sprite.width > 200 else falin_sprite.width
    fh = falin_sprite.height
    frame = falin_sprite.crop((0, 0, fw, fh)).resize((110, 85), Image.Resampling.LANCZOS)
    canvas.paste(frame, (fx - 55, fy - 55), frame)

    # Banner del Cronómetro de Speedrun arriba al centro
    draw = ImageDraw.Draw(canvas)
    draw.rounded_rectangle([820, 20, 1100, 85], radius=14, fill=(22, 18, 12, 230), outline=(210, 175, 90, 240), width=2)
    draw.text((865, 30), "00:14.28", font=font_timer, fill=(255, 230, 110, 255))

    # Controls Hint abajo
    draw.text((490, 1030), "WASD / Flechas: Mover y Saltar  |  Empuja la caja a la placa verde para abrir la compuerta  |  ESC: Pausa", font=font_hint, fill=(255, 255, 255, 180))

    return canvas

# ==========================================
# 3. MODAL DE AJUSTES Y CÓMO JUGAR
# ==========================================
def render_modal(title_text, content_lines, back_btn_text):
    bg_menu = render_main_menu()
    dim = Image.new("RGBA", (1920, 1080), (5, 10, 8, 180))
    modal_canvas = Image.alpha_composite(bg_menu, dim)

    # Marco de madera (720x540)
    mx1, my1, mx2, my2 = 600, 270, 1320, 810
    if os.path.exists(panel_path):
        p_frame = Image.open(panel_path).convert("RGBA").resize((720, 540), Image.Resampling.LANCZOS)
        modal_canvas.paste(p_frame, (mx1, my1), p_frame)
    else:
        d = ImageDraw.Draw(modal_canvas)
        d.rounded_rectangle([mx1, my1, mx2, my2], radius=20, fill=(30, 42, 35, 250), outline=(210, 175, 90, 255), width=4)

    draw = ImageDraw.Draw(modal_canvas)
    # Título del modal
    draw_text_shadow(draw, (960 - font_modal_h.getbbox(title_text)[2]//2, 310), title_text, font_modal_h, (255, 235, 140, 255))

    # Contenido
    cy = 385
    for line in content_lines:
        draw.text((660, cy), line, font=font_modal_b, fill=(245, 245, 235, 240))
        cy += 40

    # Botón Volver
    bw, bh = 260, 56
    bx, by = 960 - bw//2, 720
    if os.path.exists(btn_norm_path):
        b_img = Image.open(btn_norm_path).convert("RGBA").resize((bw, bh), Image.Resampling.LANCZOS)
        modal_canvas.paste(b_img, (bx, by), b_img)
    draw_text_shadow(draw, (960 - font_btn.getbbox(back_btn_text)[2]//2, by + 12), back_btn_text, font_btn, (255, 245, 215, 255))

    return modal_canvas

# Guardar capturas estáticas PNG
print("Generando Screenshots de alta resolución...")
render_main_menu().save(os.path.join(ss_dir, "Captura_01_MainMenu.png"))
render_level_01().save(os.path.join(ss_dir, "Captura_02_Level_01_Bosque.png"))
render_modal("AJUSTES", ["Música:  [||||||||..] 80%", "Efectos de Sonido:  [||||||||||] 100%", "Modo de Pantalla:  Pantalla Completa", "Resolución:  1920 x 1080 (16:9)"], "< VOLVER").save(os.path.join(ss_dir, "Captura_03_Ajustes.png"))
render_modal("CÓMO JUGAR", ["• MOVIMIENTO: Teclas A / D o Flechas", "• SALTAR: Barra Espaciadora o Tecla W", "• BALANCINES: Inclinan con tu peso y cajas", "• PLACAS Y PUERTAS: Activa runas verdes", "• SPEEDRUN: Completa el templo en récord web!"], "ENTENDIDO").save(os.path.join(ss_dir, "Captura_04_ComoJugar.png"))

print("Screenshots generadas con éxito en:", ss_dir)

# ==========================================
# 4. GENERAR SECUENCIAS DE FOTOGRAMAS Y VIDEOS (MP4 + GIF)
# ==========================================
temp_f_dir = os.path.join(base_dir, "TempVideoFrames")
os.makedirs(temp_f_dir, exist_ok=True)

print("Renderizando fotogramas para video de Gameplay (Level 01)...")
for frame_idx in range(60):
    progress = frame_idx / 60.0
    falin_x = progress * 6.0 # Falin avanza a la derecha
    gem_phase = frame_idx * 0.15
    f_img = render_level_01(falin_offset_x=falin_x, gem_phase=gem_phase)
    f_img.save(os.path.join(temp_f_dir, f"frame_{frame_idx:04d}.png"))

print("Fotogramas de Gameplay listos. Compilando MP4 y GIF con FFmpeg...")
ffmpeg_exe = r"C:\Users\lbati\AppData\Local\Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-8.1.2-full_build\bin\ffmpeg.exe"

# 1. MP4 Gameplay
cmd_mp4 = f'"{ffmpeg_exe}" -y -framerate 30 -i "{temp_f_dir}\\frame_%04d.png" -c:v libx264 -pix_fmt yuv420p "{vid_dir}\\Gameplay_Level_01_Bosque.mp4"'
os.system(cmd_mp4)

# 2. GIF Gameplay (Optimizado para README / GitHub)
cmd_gif = f'"{ffmpeg_exe}" -y -framerate 20 -i "{temp_f_dir}\\frame_%04d.png" -vf "scale=960:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" "{vid_dir}\\Gameplay_Level_01_Bosque.gif"'
os.system(cmd_gif)

# 3. Main Menu Preview (Pulsando hover de botones)
print("Renderizando fotogramas para video de Main Menu...")
for f in os.listdir(temp_f_dir): os.remove(os.path.join(temp_f_dir, f))

for frame_idx in range(45):
    hover = 0 if frame_idx < 25 else 1
    m_img = render_main_menu(btn_hover_idx=hover)
    m_img.save(os.path.join(temp_f_dir, f"frame_{frame_idx:04d}.png"))

cmd_m_mp4 = f'"{ffmpeg_exe}" -y -framerate 24 -i "{temp_f_dir}\\frame_%04d.png" -c:v libx264 -pix_fmt yuv420p "{vid_dir}\\Preview_MainMenu.mp4"'
os.system(cmd_m_mp4)

cmd_m_gif = f'"{ffmpeg_exe}" -y -framerate 15 -i "{temp_f_dir}\\frame_%04d.png" -vf "scale=960:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" "{vid_dir}\\Preview_MainMenu.gif"'
os.system(cmd_m_gif)

# Limpieza temp
for f in os.listdir(temp_f_dir): os.remove(os.path.join(temp_f_dir, f))
os.rmdir(temp_f_dir)

print("¡Videos MP4 y GIFs generados exitosamente en:", vid_dir)
