"""生成安装器的原生位图；构建直接使用已提交的产物，不依赖 Python。"""

from pathlib import Path
import math

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parent
SIZE = (1136, 688)
OUTLINE = [(324, 0), (818, 0), (840, 18), (1088, 18), (1136, 66),
           (1136, 648), (1096, 688), (682, 688), (658, 672), (24, 672),
           (0, 648), (0, 132), (38, 94), (240, 94), (300, 154), (300, 24)]


def render(theme):
    dark = theme == "dark"
    surface = (22, 16, 37) if dark else (255, 255, 255)
    line = (62, 44, 87) if dark else (223, 211, 249)
    image = Image.new("RGB", SIZE, surface)
    pixels = image.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            if x < 300:
                factor = min(1, max(0, (y - 94) / 594))
                top, bottom = (81, 43, 212), (39, 16, 91)
                glow = max(0, 1 - math.hypot(x - 150, y - 200) / 260) * 22
                pixels[x, y] = tuple(min(255, int(a * (1 - factor) + b * factor + glow))
                                     for a, b in zip(top, bottom))
            elif dark:
                glow = max(0, 1 - math.hypot(x - 300, y - 170) / 570) * .065
                pixels[x, y] = tuple(int(a * (1 - glow) + b * glow)
                                     for a, b in zip(surface, (81, 43, 212)))
    draw = ImageDraw.Draw(image)
    draw.line(OUTLINE + [OUTLINE[0]], fill=(124, 86, 222), width=1)
    draw.line([(300, 154), (300, 672)], fill=(94, 61, 144))
    draw.line([(301, 605), (1135, 605)], fill=line)
    draw.line([(38, 360), (197, 519), (45, 671)], fill=(73, 41, 132))
    draw.line([(301, 0), (301, 605)], fill=line)
    image.save(ROOT / f"frame-{theme}.bmp")
    # 核心动画从同一张背景裁切，避免边缘出现色块。
    reactor = image.crop((40, 104, 260, 284)).resize((660, 540), Image.Resampling.LANCZOS).convert("RGBA")
    halo = Image.new("RGBA", reactor.size)
    ImageDraw.Draw(halo).ellipse((135, 66, 525, 456), fill=(153, 102, 255, 70))
    reactor = Image.alpha_composite(reactor, halo.filter(ImageFilter.GaussianBlur(35)))
    painter = ImageDraw.Draw(reactor)
    painter.ellipse((78, 18, 582, 522), outline=(202, 171, 255, 85), width=2)
    painter.ellipse((48, 90, 612, 450), outline=(202, 171, 255, 70), width=2)
    painter.ellipse((54, 12, 606, 528), outline=(202, 171, 255, 30), width=2)
    reactor.convert("RGB").save(ROOT / f"reactor-{theme}.bmp")
    face_mask = Image.new("L", reactor.size)
    points = [(80, 38), (140, 38), (162, 60), (162, 120), (140, 142),
              (80, 142), (58, 120), (58, 60)]
    points = [(x * 3, y * 3) for x, y in points]
    ImageDraw.Draw(face_mask).polygon(points, fill=255)
    face = Image.new("RGBA", reactor.size)
    face_pixels = face.load()
    for y in range(540):
        for x in range(660):
            light = max(0, 1 - math.hypot(x - 170, y - 100) / 390)
            face_pixels[x, y] = (int(81 + 55 * light), int(43 + 42 * light), min(255, int(212 + 40 * light)), 255)
    reactor.paste(face, (0, 0), face_mask)
    ImageDraw.Draw(reactor).line(points + [points[0]], fill=(189, 150, 255, 220), width=8, joint="curve")
    reactor.crop((144, 84, 516, 456)).convert("RGB").save(ROOT / f"core-{theme}.bmp")


if __name__ == "__main__":
    for variant in ("dark", "light"):
        render(variant)
