#!/bin/bash

old_name=''
PROJECT_FILE='project.godot'

if [ -f "${PROJECT_FILE}" ]; then
  old_name=$(grep -oP '^config/name="\K[^"]+(?="$)' "${PROJECT_FILE}")
else
  echo "The project file does not exist"
  exit 1
fi

read -r -p "Rename project '${old_name}' to: " new_name
./scripts/rename-assembly.sh "${new_name}"
./scripts/rename-project.sh "${new_name}"
