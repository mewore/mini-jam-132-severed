#!/bin/bash

old_name=''
new_name=$1
PROJECT_FILE='project.godot'
PROJECT_PROPERTY='config/name'

if [ -f "${PROJECT_FILE}" ]; then
  old_name=$(grep -oP '^config/name="\K[^"]+(?="$)' "${PROJECT_FILE}")
else
  echo "The project file does not exist"
  exit 1
fi

if [ -z "${new_name}" ]; then
  read -r -p "Rename project '${old_name}' to: " new_name
fi
if [ -z "${new_name// }" ]; then
  echo "Cannot rename to an empty string or only whitespace!"
  exit 1
fi
echo  "[rename-project] Renaming '${old_name}' to '${new_name}'..."

sed --in-place "s|${PROJECT_PROPERTY}=\"${old_name}\"|${PROJECT_PROPERTY}=\"${new_name}\"|" "${PROJECT_FILE}"
sed --in-place "s/# ${old_name}/# ${new_name}/" README.md
