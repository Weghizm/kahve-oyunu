#!/usr/bin/env python3
"""Coffee UI Layout Tool.

This helper turns a simple JSON layout spec into:
- sharp PNG text sprites for static UI labels
- a composed preview image
- a lightweight report that lists exact Unity-style RectTransform slots

Coordinate convention in the JSON:
- x/y are Unity-style centered coordinates relative to the panel center
- width/height are pixel slot sizes in the same reference canvas
- preview conversion: pixel_x = canvas_width / 2 + x
                      pixel_y = canvas_height / 2 - y
"""

from __future__ import annotations

import argparse
import json
import math
import shutil
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Tuple

from PIL import Image, ImageDraw, ImageFont


Color = Tuple[int, int, int, int]


def parse_color(value: str, fallback: Color) -> Color:
    if not value:
        return fallback
    raw = value.strip().lstrip("#")
    try:
        if len(raw) == 6:
            return (int(raw[0:2], 16), int(raw[2:4], 16), int(raw[4:6], 16), 255)
        if len(raw) == 8:
            return (
                int(raw[0:2], 16),
                int(raw[2:4], 16),
                int(raw[4:6], 16),
                int(raw[6:8], 16),
            )
    except ValueError:
        pass
    return fallback


def resolve_path(project_root: Path, path_value: str) -> Path:
    path = Path(path_value)
    if path.is_absolute():
        return path
    return project_root / path


def ensure_dir(path: Path) -> None:
    path.mkdir(parents=True, exist_ok=True)


def load_font(font_path: Path, size: int) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(str(font_path), size)


def text_bbox(draw: ImageDraw.ImageDraw, text: str, font: ImageFont.FreeTypeFont, stroke: int) -> Tuple[int, int, int, int]:
    return draw.textbbox((0, 0), text, font=font, stroke_width=stroke)


def fit_font(
    text: str,
    font_path: Path,
    max_width: int,
    max_height: int,
    start_size: int,
    stroke: int,
) -> Tuple[ImageFont.FreeTypeFont, int]:
    probe = Image.new("RGBA", (8, 8), (0, 0, 0, 0))
    draw = ImageDraw.Draw(probe)
    for size in range(max(8, start_size), 7, -1):
        font = load_font(font_path, size)
        lines = text.split("\n")
        boxes = [text_bbox(draw, line, font, stroke) for line in lines]
        width = max((box[2] - box[0] for box in boxes), default=0)
        height = sum((box[3] - box[1] for box in boxes)) + int(max(0, len(lines) - 1) * size * 0.22)
        if width <= max_width and height <= max_height:
            return font, size
    return load_font(font_path, 8), 8


def draw_straight_text(
    image: Image.Image,
    text: str,
    font_path: Path,
    font_size: int,
    fill: Color,
    stroke_fill: Color,
    stroke_width: int,
    shadow: bool,
) -> None:
    draw = ImageDraw.Draw(image)
    font, fitted_size = fit_font(
        text,
        font_path,
        image.width - 10,
        image.height - 8,
        font_size,
        stroke_width,
    )
    lines = text.split("\n")
    boxes = [text_bbox(draw, line, font, stroke_width) for line in lines]
    heights = [box[3] - box[1] for box in boxes]
    total_height = sum(heights) + int(max(0, len(lines) - 1) * fitted_size * 0.22)
    y = (image.height - total_height) // 2
    for line, box, line_height in zip(lines, boxes, heights):
        width = box[2] - box[0]
        x = (image.width - width) // 2
        if shadow:
            draw.text(
                (x + max(1, fitted_size // 18), y + max(1, fitted_size // 18)),
                line,
                font=font,
                fill=(0, 0, 0, 130),
                stroke_width=stroke_width,
                stroke_fill=(0, 0, 0, 80),
            )
        draw.text((x, y), line, font=font, fill=fill, stroke_width=stroke_width, stroke_fill=stroke_fill)
        draw.text((x, y - 1), line, font=font, fill=(255, 255, 230, 70))
        y += line_height + int(fitted_size * 0.22)


def draw_arc_text(
    image: Image.Image,
    text: str,
    font_path: Path,
    font_size: int,
    fill: Color,
    stroke_fill: Color,
    stroke_width: int,
    shadow: bool,
    arc_height: float,
) -> None:
    """Draw short text on a gentle arc.

    This is intentionally conservative. It is meant for curved button/ribbon labels,
    not paragraphs.
    """
    text = text.replace("\n", " ")
    if not text:
        return

    font, fitted_size = fit_font(text, font_path, image.width - 12, image.height - 8, font_size, stroke_width)
    probe = Image.new("RGBA", (8, 8), (0, 0, 0, 0))
    draw = ImageDraw.Draw(probe)
    widths: List[int] = []
    char_images: List[Image.Image] = []
    for char in text:
        box = text_bbox(draw, char, font, stroke_width)
        cw = max(1, box[2] - box[0] + stroke_width * 4 + 6)
        ch = max(1, box[3] - box[1] + stroke_width * 4 + 8)
        char_img = Image.new("RGBA", (cw, ch), (0, 0, 0, 0))
        char_draw = ImageDraw.Draw(char_img)
        if shadow:
            char_draw.text(
                (stroke_width + 4, stroke_width + 4),
                char,
                font=font,
                fill=(0, 0, 0, 120),
                stroke_width=stroke_width,
                stroke_fill=(0, 0, 0, 70),
            )
        char_draw.text((stroke_width + 2, stroke_width + 2), char, font=font, fill=fill, stroke_width=stroke_width, stroke_fill=stroke_fill)
        widths.append(max(1, box[2] - box[0] + 2))
        char_images.append(char_img)

    total_width = sum(widths)
    start_x = (image.width - total_width) / 2.0
    x = start_x
    center_y = image.height / 2.0
    amplitude = arc_height
    for width, char_img in zip(widths, char_images):
        t = 0.0 if total_width <= 1 else ((x + width * 0.5 - start_x) / total_width)
        local = (t - 0.5) * 2.0
        y_offset = -amplitude * (1.0 - local * local)
        angle = -local * amplitude * 0.35
        rotated = char_img.rotate(angle, resample=Image.Resampling.BICUBIC, expand=True)
        px = int(x + width * 0.5 - rotated.width * 0.5)
        py = int(center_y + y_offset - rotated.height * 0.5)
        image.alpha_composite(rotated, (px, py))
        x += width


def make_text_sprite(spec: Dict[str, Any], output_dir: Path, default_font: Path) -> Path:
    width = int(spec.get("width", 200))
    height = int(spec.get("height", 64))
    text = str(spec.get("text", ""))
    font_path = Path(spec.get("fontPath") or default_font)
    if not font_path.is_absolute():
        font_path = default_font
    font_size = int(spec.get("fontSize", min(height, 48)))
    fill = parse_color(str(spec.get("fill", "#FFE89AFF")), (255, 232, 154, 255))
    stroke_fill = parse_color(str(spec.get("stroke", "#4B260AFF")), (75, 38, 10, 255))
    stroke_width = int(spec.get("strokeWidth", 2))
    shadow = bool(spec.get("shadow", True))
    mode = str(spec.get("mode", "straight")).lower()
    arc_height = float(spec.get("arcHeight", 0.0))

    image = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    if mode == "arc":
        draw_arc_text(image, text, font_path, font_size, fill, stroke_fill, stroke_width, shadow, arc_height)
    else:
        draw_straight_text(image, text, font_path, font_size, fill, stroke_fill, stroke_width, shadow)

    filename = str(spec.get("fileName") or f"{spec.get('id', 'text')}.png")
    out_path = output_dir / filename
    image.save(out_path)
    return out_path


def slot_center_to_pixel(slot: Dict[str, Any], canvas_width: int, canvas_height: int) -> Tuple[float, float]:
    return (canvas_width * 0.5 + float(slot.get("x", 0)), canvas_height * 0.5 - float(slot.get("y", 0)))


def paste_center(canvas: Image.Image, image: Image.Image, center_x: float, center_y: float, width: int, height: int) -> None:
    resized = image.resize((max(1, width), max(1, height)), Image.Resampling.LANCZOS)
    canvas.alpha_composite(resized, (int(center_x - width * 0.5), int(center_y - height * 0.5)))


def draw_slot_overlay(draw: ImageDraw.ImageDraw, slot: Dict[str, Any], canvas_width: int, canvas_height: int) -> None:
    cx, cy = slot_center_to_pixel(slot, canvas_width, canvas_height)
    width = float(slot.get("width", 1))
    height = float(slot.get("height", 1))
    left = cx - width * 0.5
    top = cy - height * 0.5
    right = cx + width * 0.5
    bottom = cy + height * 0.5
    role = str(slot.get("role", "slot")).lower()
    color = (0, 210, 255, 145) if role == "text_png" else (255, 206, 68, 120)
    draw.rounded_rectangle((left, top, right, bottom), radius=8, outline=color, width=2)
    label = str(slot.get("id", "slot"))
    draw.rectangle((left, top - 18, min(right, left + len(label) * 7 + 8), top), fill=(0, 0, 0, 95))
    draw.text((left + 4, top - 16), label, fill=(255, 242, 190, 230))


def build_preview(
    config: Dict[str, Any],
    project_root: Path,
    output_dir: Path,
    generated_texts: Dict[str, Path],
    overlay_override: Optional[bool] = None,
    file_name_override: Optional[str] = None,
) -> Path:
    canvas_width = int(config.get("canvasWidth", 1280))
    canvas_height = int(config.get("canvasHeight", 720))
    reference_path_value = str(config.get("referenceImagePath", ""))
    if reference_path_value:
        reference_path = resolve_path(project_root, reference_path_value)
    else:
        reference_path = Path()

    if reference_path.exists():
        base = Image.open(reference_path).convert("RGBA")
        if base.size != (canvas_width, canvas_height):
            base = base.resize((canvas_width, canvas_height), Image.Resampling.LANCZOS)
    else:
        base = Image.new("RGBA", (canvas_width, canvas_height), (42, 24, 14, 255))

    preview = Image.new("RGBA", (canvas_width, canvas_height), (0, 0, 0, 0))
    preview.alpha_composite(base, (0, 0))

    text_images: Dict[str, Image.Image] = {}
    for text_id, path in generated_texts.items():
        if path.exists():
            text_images[text_id] = Image.open(path).convert("RGBA")

    slots = list(config.get("slots", []))
    for slot in slots:
        bound_text = str(slot.get("textSpriteId", ""))
        if not bound_text or bound_text not in text_images:
            continue
        cx, cy = slot_center_to_pixel(slot, canvas_width, canvas_height)
        paste_center(
            preview,
            text_images[bound_text],
            cx,
            cy,
            int(slot.get("width", text_images[bound_text].width)),
            int(slot.get("height", text_images[bound_text].height)),
        )

    overlay = bool(config.get("drawSlotOverlay", True)) if overlay_override is None else overlay_override
    if overlay:
        draw = ImageDraw.Draw(preview)
        for slot in slots:
            draw_slot_overlay(draw, slot, canvas_width, canvas_height)

    preview_name = file_name_override or str(config.get("previewFileName", "coffee_ui_layout_preview.png"))
    preview_path = output_dir / preview_name
    preview.save(preview_path)
    return preview_path


def write_report(
    config: Dict[str, Any],
    output_dir: Path,
    generated_texts: Dict[str, Path],
    preview_path: Path,
    clean_preview_path: Path,
) -> Path:
    lines = [
        f"Panel: {config.get('panelName', config.get('id', 'Unnamed'))}",
        f"Canvas: {config.get('canvasWidth')}x{config.get('canvasHeight')}",
        f"Technical preview: {preview_path}",
        f"Clean preview: {clean_preview_path}",
        "",
        "Static PNG text assets:",
    ]
    for text_id, path in generated_texts.items():
        lines.append(f"- {text_id}: {path.name}")
    lines.append("")
    lines.append("Unity RectTransform slots:")
    for slot in config.get("slots", []):
        lines.append(
            f"- {slot.get('id')}: pos=({slot.get('x')}, {slot.get('y')}) "
            f"size=({slot.get('width')}, {slot.get('height')}) role={slot.get('role')}"
        )
    report_path = output_dir / "coffee_ui_layout_report.txt"
    report_path.write_text("\n".join(lines), encoding="utf-8")
    return report_path


def run(spec_path: Path, project_root: Path) -> Tuple[Path, Path]:
    config = json.loads(spec_path.read_text(encoding="utf-8"))
    output_dir = resolve_path(project_root, str(config.get("outputFolder", "Assets/Resources/Art/UI/CoffeeUILayoutTool/Output")))
    ensure_dir(output_dir)

    default_font_value = str(config.get("defaultFontPath", "Assets/Resources/Fonts/PaytoneOne-Regular.ttf"))
    default_font = resolve_path(project_root, default_font_value)
    if not default_font.exists():
        default_font = Path(r"C:\Windows\Fonts\arialbd.ttf")

    generated_texts: Dict[str, Path] = {}
    for text_spec in config.get("textSprites", []):
        out_path = make_text_sprite(text_spec, output_dir, default_font)
        generated_texts[str(text_spec.get("id"))] = out_path

    preview_path = build_preview(config, project_root, output_dir, generated_texts)
    clean_preview_path = build_preview(
        config,
        project_root,
        output_dir,
        generated_texts,
        overlay_override=False,
        file_name_override="coffee_ui_layout_clean_preview.png",
    )
    report_path = write_report(config, output_dir, generated_texts, preview_path, clean_preview_path)
    return preview_path, report_path


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate Coffee UI PNG text sprites and preview from a JSON layout spec.")
    parser.add_argument("--spec", required=True, help="Path to the JSON layout spec.")
    parser.add_argument("--project-root", default=".", help="Unity project root. Relative paths in the spec are resolved from here.")
    args = parser.parse_args()

    preview, report = run(Path(args.spec), Path(args.project_root).resolve())
    print(f"Preview: {preview}")
    print(f"Report: {report}")


if __name__ == "__main__":
    main()
