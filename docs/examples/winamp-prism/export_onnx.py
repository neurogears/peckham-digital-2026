# /// script
# requires-python = ">=3.10"
# dependencies = ["ultralytics", "onnx", "onnxruntime"]
# ///
"""Export an Ultralytics YOLO model to ONNX in the models/ folder.

    uv run export_onnx.py
    uv run export_onnx.py --model yolo26s
"""

import argparse
import os
from pathlib import Path

from ultralytics import YOLO

parser = argparse.ArgumentParser(description="Export a YOLO model to ONNX.")
parser.add_argument(
    "--model",
    default="yolo26n",
    help="Ultralytics model name or path to .pt weights (default: yolo26n)",
)
args = parser.parse_args()

model = Path(args.model).resolve() if Path(args.model).is_file() else args.model

models_dir = Path(__file__).parent / "models"
models_dir.mkdir(exist_ok=True)

# Downloaded weights and the exported model are written to the working directory.
os.chdir(models_dir)
print(YOLO(model).export(format="onnx"))
