"""Create an auditable pixel difference; metrics are not a visual fidelity score."""
import argparse
import json
from pathlib import Path

from PIL import Image, ImageChops, ImageEnhance, ImageStat

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--actual", type=Path, required=True)
parser.add_argument("--prototype", type=Path, required=True)
parser.add_argument("--output", type=Path, required=True)
args = parser.parse_args()
args.output.mkdir(parents=True, exist_ok=True)
measurements = []
for language in ("zh-CN", "en-US"):
    actual = Image.open(args.actual / f"workspace-{language}.png").convert("RGB")
    prototype = Image.open(args.prototype / f"comparison-{language}.png").convert("RGB")
    if actual.size != prototype.size:
        raise ValueError(f"Different image dimensions for {language}: {actual.size} vs {prototype.size}")
    difference = ImageChops.difference(prototype, actual)
    means = ImageStat.Stat(difference).mean
    red, green, blue = difference.split()
    maximum_channel_difference = ImageChops.lighter(ImageChops.lighter(red, green), blue)
    measurements.append({
        "language": language,
        "width": actual.width,
        "height": actual.height,
        "meanAbsoluteChannelDifference": round(sum(means) / 3, 4),
        "differenceScale": "0..255 per RGB channel; lower means fewer pixel differences",
        "exactPixelFraction": round(maximum_channel_difference.histogram()[0] / (actual.width * actual.height), 6),
        "isVisualFidelityScore": False,
    })
    combined = Image.new("RGB", (actual.width * 2, actual.height))
    combined.paste(prototype, (0, 0))
    combined.paste(actual, (actual.width, 0))
    combined.save(args.output / f"{language}-side-by-side.png")
    ImageEnhance.Brightness(difference).enhance(4).save(args.output / f"{language}-difference-x4.png")
(args.output / "measurements.json").write_text(json.dumps(measurements, indent=2), encoding="utf-8")
print(json.dumps(measurements, indent=2))
