#!/bin/bash

# 1. Define the absolute path to your Python script
# Replace this with the actual path to your script
PYTHON_SCRIPT_PATH="$HOME/Documents/Moonworks/utils/addaftertime.py"

# 2. Capture the Current Working Directory (CWD)
CURRENT_DIR=$(pwd)

# 3. Execute the Python script
# We use "$CURRENT_DIR" as the first argument
python3 "$PYTHON_SCRIPT_PATH" "$CURRENT_DIR"