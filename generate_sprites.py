import os
from PIL import Image, ImageDraw

out_dir = r"D:\gamess\My project (9)\Assets\Sprites\Tiles"
os.makedirs(out_dir, exist_ok=True)

# 1. Stone Platform Tile (64x64) with mossy top and brick bevels
def create_stone_tile():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Base stone body
    draw.rectangle([0, 0, 63, 63], fill=(42, 60, 48, 255))
    # Bevel highlight & shadow
    draw.line([(0, 0), (63, 0)], fill=(75, 110, 80, 255), width=2)
    draw.line([(0, 0), (0, 63)], fill=(55, 85, 60, 255), width=2)
    draw.line([(63, 0), (63, 63)], fill=(25, 38, 30, 255), width=2)
    draw.line([(0, 63), (63, 63)], fill=(20, 32, 24, 255), width=2)
    # Brick pattern
    draw.line([(0, 32), (63, 32)], fill=(28, 42, 33, 255), width=2)
    draw.line([(32, 0), (32, 32)], fill=(28, 42, 33, 255), width=2)
    draw.line([(16, 32), (16, 63)], fill=(28, 42, 33, 255), width=2)
    draw.line([(48, 32), (48, 63)], fill=(28, 42, 33, 255), width=2)
    # Moss grass on top rim
    for x in range(0, 64, 4):
        h = (x * 7) % 5 + 3
        draw.rectangle([x, 0, x+3, h], fill=(68, 160, 75, 255))
        draw.point((x+1, h+1), fill=(110, 210, 100, 255))
    img.save(os.path.join(out_dir, "Stone_Platform.png"))

# 2. Wood Plank (64x16) for seesaw beam
def create_wood_plank():
    img = Image.new("RGBA", (64, 16), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rectangle([0, 0, 63, 15], fill=(138, 88, 48, 255))
    # Top highlight
    draw.line([(0, 0), (63, 0)], fill=(185, 125, 75, 255), width=1)
    # Bottom shadow
    draw.line([(0, 15), (63, 15)], fill=(85, 50, 25, 255), width=1)
    # Wood grain streaks
    draw.line([(5, 5), (35, 5)], fill=(115, 70, 38, 255), width=1)
    draw.line([(20, 10), (55, 10)], fill=(115, 70, 38, 255), width=1)
    # Iron bolts on edges
    draw.ellipse([2, 5, 6, 9], fill=(70, 75, 80, 255), outline=(140, 145, 150, 255))
    draw.ellipse([57, 5, 61, 9], fill=(70, 75, 80, 255), outline=(140, 145, 150, 255))
    img.save(os.path.join(out_dir, "Wood_Plank.png"))

# 3. Seesaw Fulcrum Pivot (48x48)
def create_seesaw_fulcrum():
    img = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Triangle stone base
    draw.polygon([(24, 4), (44, 44), (4, 44)], fill=(65, 75, 70, 255), outline=(110, 125, 115, 255))
    # Pivot circle in center
    draw.ellipse([18, 14, 30, 26], fill=(180, 140, 60, 255), outline=(240, 200, 90, 255))
    draw.ellipse([21, 17, 27, 23], fill=(50, 40, 20, 255))
    img.save(os.path.join(out_dir, "Seesaw_Fulcrum.png"))

# 4. Pressure Plate Base and Top (64x24 and 64x16)
def create_pressure_plate():
    # Base
    base = Image.new("RGBA", (64, 20), (0, 0, 0, 0))
    d_b = ImageDraw.Draw(base)
    d_b.rectangle([4, 6, 59, 18], fill=(35, 45, 40, 255), outline=(70, 90, 80, 255))
    d_b.line([(0, 18), (63, 18)], fill=(20, 28, 24, 255), width=2)
    base.save(os.path.join(out_dir, "Plate_Base.png"))

    # Top pad (Green)
    top = Image.new("RGBA", (56, 16), (0, 0, 0, 0))
    d_t = ImageDraw.Draw(top)
    d_t.rectangle([2, 2, 53, 13], fill=(45, 110, 65, 255), outline=(80, 200, 120, 255))
    # Runic glow in center
    d_t.rectangle([16, 6, 39, 9], fill=(120, 255, 160, 255))
    d_t.ellipse([25, 4, 30, 11], fill=(200, 255, 220, 255))
    top.save(os.path.join(out_dir, "Plate_Top.png"))

# 5. Glowing Gems (48x48)
def create_gems():
    # Green Emerald
    g_img = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
    d_g = ImageDraw.Draw(g_img)
    # Diamond shape
    d_g.polygon([(24, 4), (42, 20), (24, 44), (6, 20)], fill=(40, 200, 90, 240), outline=(160, 255, 190, 255))
    d_g.polygon([(24, 10), (36, 20), (24, 38), (12, 20)], fill=(70, 235, 125, 255))
    d_g.polygon([(24, 14), (30, 20), (24, 28), (18, 20)], fill=(220, 255, 235, 255))
    g_img.save(os.path.join(out_dir, "Gem_Green.png"))

    # Purple Amethyst
    p_img = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
    d_p = ImageDraw.Draw(p_img)
    d_p.polygon([(24, 4), (42, 20), (24, 44), (6, 20)], fill=(170, 60, 225, 240), outline=(230, 160, 255, 255))
    d_p.polygon([(24, 10), (36, 20), (24, 38), (12, 20)], fill=(200, 90, 245, 255))
    d_p.polygon([(24, 14), (30, 20), (24, 28), (18, 20)], fill=(250, 225, 255, 255))
    p_img.save(os.path.join(out_dir, "Gem_Purple.png"))

# 6. Runic Exit Door (64x96)
def create_door():
    img = Image.new("RGBA", (64, 96), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Arch stone frame
    draw.rounded_rectangle([4, 4, 59, 93], radius=16, fill=(35, 42, 48, 255), outline=(90, 115, 130, 255), width=3)
    # Portal inner cavity
    draw.rounded_rectangle([10, 12, 53, 91], radius=12, fill=(15, 20, 30, 255))
    # Glowing portal swirl
    draw.ellipse([14, 20, 49, 75], fill=(80, 40, 140, 200), outline=(170, 110, 255, 255))
    draw.ellipse([20, 28, 43, 67], fill=(130, 80, 210, 230), outline=(220, 180, 255, 255))
    draw.ellipse([26, 38, 37, 57], fill=(240, 220, 255, 255))
    img.save(os.path.join(out_dir, "Door_Portal.png"))

create_stone_tile()
create_wood_plank()
create_seesaw_fulcrum()
create_pressure_plate()
create_gems()
create_door()
print("All pixel assets generated successfully!")
