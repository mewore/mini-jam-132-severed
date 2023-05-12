#!/bin/bash

old_name=''
new_name_with_spaces=$1
PROJECT_FILE='project.godot'
PROJECT_PROPERTY='project/assembly_name'

if [ -f "${PROJECT_FILE}" ]; then
  old_name=$(grep -oP '^project/assembly_name="\K[^"]+(?="$)' "${PROJECT_FILE}")
else
  echo "The project file does not exist"
  exit 1
fi

if [ -z "${new_name_with_spaces}" ]; then
  read -r -p "Rename project C# assembly '${old_name}' to: " new_name_with_spaces
fi

new_name="${new_name_with_spaces//[^a-zA-Z0-9]}"
if [ -z "${new_name}" ]; then
  echo "Cannot rename to an empty string or only whitespace and special characters!"
  exit 1
fi
echo  "[rename-assembly] Renaming '${old_name}' to '${new_name}'..."
if ! [ -f "${old_name}.sln" ]; then echo "${old_name}.sln does not exist!"; else
  sed -i "s|\"${old_name}|\"${new_name}|g" "${old_name}.sln" || exit 1
  mv "${old_name}.sln" "${new_name}.sln"
fi

if ! [ -f "${old_name}.csproj" ]; then echo "${old_name}.csproj does not exist!"; else
  if grep -q 'RootNamespace>Empty</RootNamespace' "${old_name}.csproj"; then
    sed -i "s|<RootNamespace>Empty</RootNamespace>|<RootNamespace>${new_name}</RootNamespace>|" "${old_name}.csproj" || exit 1
  else
    sed -i "s|<RootNamespace>${old_name}</RootNamespace>|<RootNamespace>${new_name}</RootNamespace>|" "${old_name}.csproj" || exit 1
  fi
  mv "${old_name}.csproj" "${new_name}.csproj"
fi

sed --in-place "s|${PROJECT_PROPERTY}=\"${old_name}\"|${PROJECT_PROPERTY}=\"${new_name}\"|" "${PROJECT_FILE}"
echo "Done."
