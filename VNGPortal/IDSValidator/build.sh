#!/bin/bash
set -e            # Exit on error
set -o pipefail   # Exit if any command in a pipe fails

echo "Compiling IDSValidator..."
export CPLUS_INCLUDE_PATH=./:./include:./parser
g++ ./*.cpp ./parser/*.cpp ./libifcengine.a -o IDSValidator
if [ $? -ne 0 ]; then
    echo "Compilation failed for IDSValidator"
    exit 1
fi

echo "Build completed successfully."
