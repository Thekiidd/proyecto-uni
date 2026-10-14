import os, math
from PIL import Image, ImageDraw, ImageFont

base_dir = r"D:\gamess\My project (9)"
ss_dir = os.path.join(base_dir, "Screenshots")
art_dir = r"C:\Users\lbati\.gemini\antigravity\brain\65c5e88e-c6fe-404b-acce-8b8e379c8f06"
os.makedirs(ss_dir, exist_ok=True)

# Canvas 1920x1080
w, h = 1920, 1080
img = Image.new("RGBA", (w, h), (18, 24, 38, 255))
draw = ImageDraw.Draw(img)

# Fuentes
try:
    font_title = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 44)
    font_sub = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 24)
    font_hud = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 26)
    font_small = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 18)
    font_badge = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 20)
except Exception:
    font_title = font_sub = font_hud = font_small = font_badge = ImageFont.load_default()

# Cargar sprites
falin_path = os.path.join(base_dir, "Assets", "Sprites", "PixelLab", "Falin", "Idle", "rotations", "south.png")
mia_path = os.path.join(base_dir, "Assets", "Sprites", "PixelLab", "Mia", "Idle", "rotations", "south.png")
altar_path = os.path.join(base_dir, "Assets", "Sprites", "Village", "Art", "Props", "Ancient_Stone_Circle.png")
plat_path = os.path.join(base_dir, "Assets", "Sprites", "Tiles", "Stone_Platform.png")

falin_img = Image.open(falin_path).convert("RGBA").resize((70, 70), Image.Resampling.NEAREST) if os.path.exists(falin_path) else None
mia_img = Image.open(mia_path).convert("RGBA").resize((60, 60), Image.Resampling.NEAREST) if os.path.exists(mia_path) else None
altar_img = Image.open(altar_path).convert("RGBA").resize((160, 160), Image.Resampling.NEAREST) if os.path.exists(altar_path) else None

# Dibujar 3 Pantallas representativas del ascenso (Piso 1, Piso 4, Piso 8 Cima)
screen_w = 560
screen_h = 750
y_top = 180

screens_data = [
    ("PANTALLA 1: Base del Bosque", 60, "Aprende el salto cargado con [ESPACIO]", (25, 45, 30)),
    ("PANTALLA 4: Acantilados de Viento", 680, "Salto diagonal preciso y rebotes en pared", (20, 32, 50)),
    ("PANTALLA 8: La Cima Sagrada", 1300, "¡El Altar místico y el rescate sin checkpoints!", (35, 20, 48))
]

for title, sx, desc, bg_color in screens_data:
    # Marco de la pantalla
    draw.rectangle([sx, y_top, sx + screen_w, y_top + screen_h], fill=bg_color, outline=(80, 110, 150), width=4)
    # Paredes laterales
    draw.rectangle([sx, y_top, sx + 25, y_top + screen_h], fill=(45, 55, 70))
    draw.rectangle([sx + screen_w - 25, y_top, sx + screen_w, y_top + screen_h], fill=(45, 55, 70))
    # Encabezado
    draw.rectangle([sx, y_top, sx + screen_w, y_top + 45], fill=(15, 20, 30))
    draw.text((sx + 20, y_top + 10), title, font=font_badge, fill=(255, 215, 0))
    # Subtítulo inferior
    draw.text((sx + 35, y_top + screen_h - 35), desc, font=font_small, fill=(200, 215, 230))

# --- Detalles Pantalla 1 ---
# Suelo
draw.rectangle([60 + 25, y_top + screen_h - 70, 60 + screen_w - 25, y_top + screen_h - 50], fill=(90, 80, 65), outline=(130, 115, 95), width=2)
# Plataformas intermedias
draw.rectangle([130, y_top + 520, 320, y_top + 540], fill=(80, 95, 70), outline=(120, 140, 105), width=2)
draw.rectangle([340, y_top + 380, 520, y_top + 400], fill=(80, 95, 70), outline=(120, 140, 105), width=2)
draw.rectangle([160, y_top + 240, 360, y_top + 260], fill=(80, 95, 70), outline=(120, 140, 105), width=2)
draw.rectangle([380, y_top + 110, 560, y_top + 130], fill=(80, 95, 70), outline=(120, 140, 105), width=2)

if falin_img:
    img.paste(falin_img, (180, y_top + screen_h - 138), falin_img)
    # Barra de carga de salto
    draw.rectangle([165, y_top + screen_h - 160, 275, y_top + screen_h - 146], fill=(20, 20, 20), outline=(255, 255, 255))
    draw.rectangle([167, y_top + screen_h - 158, 245, y_top + screen_h - 148], fill=(50, 230, 80)) # Carga 70%
    draw.text((170, y_top + screen_h - 180), "CARGANDO 70%", font=font_small, fill=(50, 230, 80))

# Parábola de salto dibujada
points = [(215, y_top + screen_h - 140), (220, y_top + 590), (235, y_top + 530)]
draw.line(points, fill=(255, 230, 100), width=3)

# --- Detalles Pantalla 4 (Rebote en pared) ---
draw.rectangle([760, y_top + 600, 920, y_top + 620], fill=(70, 80, 95), outline=(110, 125, 145), width=2)
draw.rectangle([1020, y_top + 450, 1180, y_top + 470], fill=(70, 80, 95), outline=(110, 125, 145), width=2)
draw.rectangle([740, y_top + 300, 890, y_top + 320], fill=(70, 80, 95), outline=(110, 125, 145), width=2)
draw.rectangle([980, y_top + 140, 1140, y_top + 160], fill=(70, 80, 95), outline=(110, 125, 145), width=2)

if mia_img:
    img.paste(mia_img, (1180, y_top + 340), mia_img) # Mia rebotando en la pared derecha!
    # Flechas de rebote
    draw.line([(1060, y_top + 450), (1200, y_top + 360)], fill=(255, 100, 100), width=3)
    draw.line([(1200, y_top + 360), (1110, y_top + 280)], fill=(100, 200, 255), width=3)
    draw.text((1050, y_top + 260), "💥 REBOTE EN PARED", font=font_badge, fill=(100, 220, 255))

# --- Detalles Pantalla 8 (Cima Sagrada) ---
draw.rectangle([1300 + 40, y_top + 480, 1300 + screen_w - 40, y_top + 510], fill=(120, 105, 145), outline=(190, 175, 230), width=3)
if altar_img:
    img.paste(altar_img, (1500, y_top + 330), altar_img)
    # Aura mágica de rescate
    draw.text((1440, y_top + 280), "✨ ¡ALTAR DE RESCATE! ✨", font=font_badge, fill=(255, 230, 120))
    if falin_img and mia_img:
        img.paste(falin_img, (1480, y_top + 415), falin_img)
        img.paste(mia_img, (1580, y_top + 425), mia_img)

# --- HUD Superior (Jump King) ---
draw.rectangle([200, 30, 1720, 125], fill=(12, 18, 30, 240), outline=(70, 100, 150), width=3)
draw.text((240, 50), "Piso 1 / 8: Base del Bosque", font=font_hud, fill=(255, 220, 80))
draw.text((780, 50), "🐾 Falin (Fuerza) | [TAB] Cambiar a Mía", font=font_hud, fill=(255, 180, 60))
draw.text((1350, 50), "⏱️ 02:45  |  🦘 Saltos: 14  |  💥 Caídas: 2", font=font_hud, fill=(255, 255, 255))

# Subtítulo explicativo inferior
draw.rectangle([0, 970, w, 1080], fill=(10, 14, 24))
draw.text((120, 990), "🎮 MODO JUMP KING (KIMAYA): Salto táctico cargado • Sin control en el aire • Rebote en paredes • Caídas sin checkpoints hasta el inicio", font=font_sub, fill=(220, 235, 255))
draw.text((120, 1030), "Controles: Mantén [ESPACIO] para cargar fuerza • [A / D] o Flechas para dirección • [TAB] Alternar Falin / Mía", font=font_small, fill=(160, 185, 210))

# Guardar en Screenshots y Artifacts
out_ss = os.path.join(ss_dir, "Captura_JumpKing_Ascenso.png")
out_art = os.path.join(art_dir, "Captura_JumpKing_Ascenso.png")
img.save(out_ss)
img.save(out_art)
print(f"Showcase Jump King guardado en: {out_ss}")
