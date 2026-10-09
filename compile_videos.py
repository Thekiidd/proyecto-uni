import os, subprocess, math
from PIL import Image

base_dir = r"D:\gamess\My project (9)"
vid_dir = os.path.join(base_dir, "Videos")
os.makedirs(vid_dir, exist_ok=True)
ffmpeg_exe = r"C:\Users\lbati\AppData\Local\Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-8.1.2-full_build\bin\ffmpeg.exe"

# Importar las funciones de render
import render_showcase as rs

# 1. GENERAR GAMEPLAY LEVEL 01 (60 frames)
print("Generando 60 frames de Gameplay Level 01...")
frames_gameplay = []
temp_dir_game = os.path.join(base_dir, "TempGame")
os.makedirs(temp_dir_game, exist_ok=True)

for i in range(60):
    progress = i / 60.0
    falin_x = progress * 6.5
    gem_phase = i * 0.15
    f_img = rs.render_level_01(falin_offset_x=falin_x, gem_phase=gem_phase)
    frame_path = os.path.join(temp_dir_game, f"frame_{i:04d}.png")
    f_img.save(frame_path)
    # Redimensionar para GIF ligero
    frames_gameplay.append(f_img.resize((960, 540), Image.Resampling.LANCZOS))

# Guardar GIF nativo con PIL
gif_game_path = os.path.join(vid_dir, "Gameplay_Level_01_Bosque.gif")
frames_gameplay[0].save(gif_game_path, save_all=True, append_images=frames_gameplay[1:], duration=35, loop=0)
print("GIF de Gameplay guardado:", gif_game_path)

# Compilar MP4 con FFmpeg usando subprocess
mp4_game_path = os.path.join(vid_dir, "Gameplay_Level_01_Bosque.mp4")
subprocess.run([
    ffmpeg_exe, "-y",
    "-framerate", "30",
    "-i", os.path.join(temp_dir_game, "frame_%04d.png"),
    "-c:v", "libx264",
    "-pix_fmt", "yuv420p",
    mp4_game_path
], check=True)
print("MP4 de Gameplay guardado:", mp4_game_path)

for f in os.listdir(temp_dir_game): os.remove(os.path.join(temp_dir_game, f))
os.rmdir(temp_dir_game)

# 2. GENERAR PREVIEW MAIN MENU (40 frames)
print("Generando 40 frames de Main Menu...")
frames_menu = []
temp_dir_menu = os.path.join(base_dir, "TempMenu")
os.makedirs(temp_dir_menu, exist_ok=True)

for i in range(40):
    hover = 0 if i < 20 else -1
    m_img = rs.render_main_menu(btn_hover_idx=hover)
    frame_path = os.path.join(temp_dir_menu, f"frame_{i:04d}.png")
    m_img.save(frame_path)
    frames_menu.append(m_img.resize((960, 540), Image.Resampling.LANCZOS))

gif_menu_path = os.path.join(vid_dir, "Preview_MainMenu.gif")
frames_menu[0].save(gif_menu_path, save_all=True, append_images=frames_menu[1:], duration=50, loop=0)
print("GIF de Main Menu guardado:", gif_menu_path)

mp4_menu_path = os.path.join(vid_dir, "Preview_MainMenu.mp4")
subprocess.run([
    ffmpeg_exe, "-y",
    "-framerate", "20",
    "-i", os.path.join(temp_dir_menu, "frame_%04d.png"),
    "-c:v", "libx264",
    "-pix_fmt", "yuv420p",
    mp4_menu_path
], check=True)
print("MP4 de Main Menu guardado:", mp4_menu_path)

for f in os.listdir(temp_dir_menu): os.remove(os.path.join(temp_dir_menu, f))
os.rmdir(temp_dir_menu)

print("¡Todos los videos y GIFs han sido compilados con éxito!")
