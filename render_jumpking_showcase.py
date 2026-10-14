import os
from PIL import Image, ImageDraw, ImageFont

base_dir = r"D:\gamess\My project (9)"
ss_dir = os.path.join(base_dir, "Screenshots")
art_dir = r"C:\Users\lbati\.gemini\antigravity\brain\65c5e88e-c6fe-404b-acce-8b8e379c8f06"
os.makedirs(ss_dir, exist_ok=True)

w, h = 1920, 1080
img = Image.new("RGBA", (w, h), (16, 22, 34, 255))
draw = ImageDraw.Draw(img)

try:
    font_title = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 44)
    font_sub = ImageFont.truetype(r"C:\Windows\Fonts\georgiab.ttf", 22)
    font_hud = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 24)
    font_small = ImageFont.truetype(r"C:\Windows\Fonts\arial.ttf", 17)
    font_badge = ImageFont.truetype(r"C:\Windows\Fonts\arialbd.ttf", 20)
except Exception:
    font_title = font_sub = font_hud = font_small = font_badge = ImageFont.load_default()

# Cargar sprites en PERFIL LATERAL (East)
falin_idle_path = os.path.join(base_dir, "Assets", "Sprites", "PixelLab", "Falin", "Idle", "animations", "idle", "east", "frame_000.png")
falin_walk_path = os.path.join(base_dir, "Assets", "Sprites", "PixelLab", "Falin", "Idle", "animations", "walking", "east", "frame_002.png")
falin_jump_path = os.path.join(base_dir, "Assets", "Sprites", "PixelLab", "Falin", "Idle", "animations", "running", "east", "frame_000.png")
mia_idle_path = os.path.join(base_dir, "Assets", "Sprites", "PixelLab", "Mia", "Idle", "animations", "idle", "east", "frame_000.png")
altar_path = os.path.join(base_dir, "Assets", "Sprites", "Village", "Art", "Props", "Ancient_Stone_Circle.png")

falin_idle = Image.open(falin_idle_path).convert("RGBA").resize((80, 80), Image.Resampling.NEAREST) if os.path.exists(falin_idle_path) else None
falin_walk = Image.open(falin_walk_path).convert("RGBA").resize((80, 80), Image.Resampling.NEAREST) if os.path.exists(falin_walk_path) else None
falin_jump = Image.open(falin_jump_path).convert("RGBA").resize((80, 80), Image.Resampling.NEAREST) if os.path.exists(falin_jump_path) else None
mia_idle = Image.open(mia_idle_path).convert("RGBA").resize((70, 70), Image.Resampling.NEAREST) if os.path.exists(mia_idle_path) else None
altar_img = Image.open(altar_path).convert("RGBA").resize((160, 160), Image.Resampling.NEAREST) if os.path.exists(altar_path) else None

screen_w = 560
screen_h = 740
y_top = 180

screens = [
    ("PANTALLA 1: Base del Bosque (Calibrada)", 60, "Salto inicial suave (~35% de carga alcanza Plat 1)", (22, 38, 28)),
    ("PANTALLA 4: Acantilados de Viento", 680, "Salto diagonal preciso • Sin aturdimiento al caer", (18, 30, 46)),
    ("PANTALLA 8: La Cima Sagrada (Altar)", 1300, "¡Alcanza la cima y rescata a tu compañera!", (32, 18, 44))
]

for title, sx, desc, bg_color in screens:
    draw.rectangle([sx, y_top, sx + screen_w, y_top + screen_h], fill=bg_color, outline=(80, 110, 150), width=4)
    draw.rectangle([sx, y_top, sx + 25, y_top + screen_h], fill=(42, 52, 68))
    draw.rectangle([sx + screen_w - 25, y_top, sx + screen_w, y_top + screen_h], fill=(42, 52, 68))
    draw.rectangle([sx, y_top, sx + screen_w, y_top + 45], fill=(14, 18, 28))
    draw.text((sx + 20, y_top + 10), title, font=font_badge, fill=(255, 215, 0))
    draw.text((sx + 30, y_top + screen_h - 35), desc, font=font_small, fill=(200, 215, 230))

# --- Pantalla 1: Plataformas calibradas ---
# Suelo
draw.rectangle([60 + 25, y_top + screen_h - 80, 60 + screen_w - 25, y_top + screen_h - 50], fill=(85, 75, 60), outline=(130, 115, 95), width=2)
# Plat 1 alcanzable (X = -3.2, Y = 3.2)
draw.rectangle([95, y_top + 530, 260, y_top + 555], fill=(80, 95, 70), outline=(130, 155, 110), width=2)
draw.text((105, y_top + 505), "⭐ PLAT 1 (Fácil acceso ~35%)", font=font_small, fill=(255, 235, 120))
# Plat 2 (X = 2.8, Y = 5.3)
draw.rectangle([340, y_top + 400, 500, y_top + 425], fill=(80, 95, 70), outline=(130, 155, 110), width=2)
# Plat 3 (X = -2.6, Y = 7.5)
draw.rectangle([130, y_top + 270, 280, y_top + 295], fill=(80, 95, 70), outline=(130, 155, 110), width=2)
# Plat 4 (Salida)
draw.rectangle([360, y_top + 130, 520, y_top + 155], fill=(80, 95, 70), outline=(130, 155, 110), width=2)

if falin_idle:
    # Falin en perfil sobre el suelo
    img.paste(falin_idle, (280, y_top + screen_h - 150), falin_idle)
if falin_jump:
    # Falin saltando en perfil hacia Plat 1
    img.paste(falin_jump, (150, y_top + 560), falin_jump)
    # Parábola
    draw.line([(280, y_top + screen_h - 110), (220, y_top + 530), (170, y_top + 520)], fill=(255, 220, 80), width=3)

# --- Pantalla 4 ---
draw.rectangle([760, y_top + 580, 920, y_top + 605], fill=(68, 78, 92), outline=(115, 130, 150), width=2)
draw.rectangle([1020, y_top + 430, 1180, y_top + 455], fill=(68, 78, 92), outline=(115, 130, 150), width=2)
draw.rectangle([740, y_top + 280, 890, y_top + 305], fill=(68, 78, 92), outline=(115, 130, 150), width=2)
if mia_idle:
    img.paste(mia_idle, (800, y_top + 515), mia_idle)
    draw.text((780, y_top + 485), "🌸 Mía (Ágil)", font=font_small, fill=(255, 190, 240))

# --- Pantalla 8 (Cima) ---
draw.rectangle([1300 + 40, y_top + 480, 1300 + screen_w - 40, y_top + 510], fill=(115, 100, 140), outline=(185, 170, 225), width=3)
if altar_img:
    img.paste(altar_img, (1500, y_top + 330), altar_img)
    draw.text((1440, y_top + 280), "✨ ¡ALTAR DE RESCATE! ✨", font=font_badge, fill=(255, 230, 120))

# --- HUD Superior Elegante ---
draw.rectangle([260, 35, 1660, 115], fill=(10, 16, 28, 240), outline=(65, 95, 145), width=3)
draw.text((300, 58), "Piso 1 / 8: Base del Bosque", font=font_hud, fill=(255, 225, 90))
draw.text((820, 58), "🐾 Falin (Fuerza) | [TAB] Mía", font=font_hud, fill=(255, 180, 60))
draw.text((1300, 58), "⏱️ 01:20  |  🦘 Saltos: 8  |  💥 Caídas: 0", font=font_hud, fill=(255, 255, 255))

# Franja inferior de controles
draw.rectangle([0, 990, w, 1080], fill=(10, 14, 24))
draw.text((200, 1005), "🕹️ Mantén [ESPACIO] para cargar fuerza • Suelta para saltar en parábola • [A / D] o Flechas para dirección • [TAB] Alternar Falin / Mía", font=font_sub, fill=(215, 230, 255))
draw.text((200, 1045), "✅ Salto calibrado (fácil alcance de la primera plataforma con ~35% de carga) • Sin aturdimiento al caer • Sprites de PixelLab en perfil lateral", font=font_small, fill=(150, 180, 210))

out_ss = os.path.join(ss_dir, "Captura_JumpKing_Ascenso.png")
out_art = os.path.join(art_dir, "Captura_JumpKing_Ascenso.png")
img.save(out_ss)
img.save(out_art)
print("Preview actualizado con éxito!")
