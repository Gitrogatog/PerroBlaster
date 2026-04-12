#!/bin/bash

# Define the target subfolder
TARGET_DIR="./ContentBuilder/ContentBuilderUI"

# Check if the directory exists before attempting to enter it
if [ -d "$TARGET_DIR" ]; then
    # echo "Moving to $TARGET_DIR and starting the application..."
    # Use ( ) to execute in a subshell so the parent script's 
    # working directory doesn't change permanently
    (cd "$TARGET_DIR" && dotnet run)
else
    echo "Error: Directory $TARGET_DIR not found."
    exit 1
fi