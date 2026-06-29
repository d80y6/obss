#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
TEMPLATE_DIR="$ROOT_DIR/src/BuildingBlocks/Obss.ModuleTemplate"
MODULES_DIR="$ROOT_DIR/src/Modules"
SOLUTION_FILE="$ROOT_DIR/Obss.sln"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

usage() {
    echo "Usage: $0 <ModuleName>"
    echo ""
    echo "Creates a new module scaffolding from the Obss.ModuleTemplate."
    echo ""
    echo "Arguments:"
    echo "  ModuleName    PascalCase name for the module (e.g., Fulfillment, RatingEngine)"
    echo ""
    echo "Example:"
    echo "  $0 Fulfillment"
    exit 1
}

if [ $# -ne 1 ]; then
    usage
fi

MODULE_NAME="$1"
MODULE_NAME_LOWER="$(echo "$MODULE_NAME" | sed 's/\([A-Z]\)/-\1/g' | sed 's/^-//' | tr '[:upper:]' '[:lower:]')"
TEMPLATE_PREFIX="Obss.ModuleTemplate"
MODULE_PREFIX="Obss.$MODULE_NAME"

if [ -d "$MODULES_DIR/$MODULE_NAME" ]; then
    echo -e "${RED}Error: Module directory already exists: $MODULES_DIR/$MODULE_NAME${NC}"
    exit 1
fi

validate_project_name() {
    if ! echo "$MODULE_NAME" | grep -qE '^[A-Z][a-zA-Z0-9]*$'; then
        echo -e "${RED}Error: ModuleName must be PascalCase (e.g., Fulfillment, RatingEngine)${NC}"
        exit 1
    fi
}
validate_project_name

echo -e "${YELLOW}Scaffolding module: $MODULE_NAME${NC}"
echo ""

# Layer mappings: TemplateDir -> TargetDir
LAYERS=(
    "Domain"
    "Application"
    "Infrastructure"
    "Api"
)

for layer in "${LAYERS[@]}"; do
    template_layer="$TEMPLATE_DIR/Obss.ModuleTemplate.$layer"
    target_layer="$MODULES_DIR/$MODULE_NAME/Obss.$MODULE_NAME.$layer"

    if [ ! -d "$template_layer" ]; then
        echo -e "${RED}Warning: Template layer not found: $template_layer${NC}"
        continue
    fi

    echo -e "${GREEN}Creating $target_layer${NC}"
    mkdir -p "$target_layer"

    # Copy all files recursively, replacing template placeholders in content
    while IFS= read -r -d '' file; do
        rel_path="${file#$template_layer/}"

        # Build target filename, replacing template prefix in filename
        target_rel_path="${rel_path/$TEMPLATE_PREFIX/$MODULE_PREFIX}"
        target_file="$target_layer/$target_rel_path"

        # Create parent directories
        mkdir -p "$(dirname "$target_file")"

        # Replace template namespace/prefix with module namespace/prefix in content
        sed \
            -e "s/Obss\.ModuleTemplate/Obss.$MODULE_NAME/g" \
            -e "s/ModuleTemplate/$MODULE_NAME/g" \
            "$file" > "$target_file"

        echo "  Created: $target_file"
    done < <(find "$template_layer" -type f -print0)

    # Rename any remaining files/folders that reference "ModuleTemplate" in their name
    while IFS= read -r -d '' item; do
        basename_item="$(basename "$item")"
        dir_item="$(dirname "$item")"
        if [[ "$basename_item" == *"ModuleTemplate"* ]]; then
            new_name="${basename_item/ModuleTemplate/$MODULE_NAME}"
            mv "$item" "$dir_item/$new_name"
        fi
    done < <(find "$target_layer" -depth -name "*ModuleTemplate*" -print0 2>/dev/null || true)

done

echo ""
echo -e "${GREEN}Project files created. Adding to solution...${NC}"

# Add projects to solution
for layer in "${LAYERS[@]}"; do
    project_path="src/Modules/$MODULE_NAME/Obss.$MODULE_NAME.$layer/Obss.$MODULE_NAME.$layer.csproj"
    if [ -f "$ROOT_DIR/$project_path" ]; then
        dotnet sln "$SOLUTION_FILE" add "$project_path" --solution-folder "$MODULE_NAME"
        echo "  Added: $project_path"
    fi
done

echo ""
echo -e "${GREEN}Module '$MODULE_NAME' created successfully!${NC}"
echo ""
echo "Next steps:"
echo "  1. Register the module in $ROOT_DIR/src/Host/Obss.Host/Program.cs"
echo "     - Add project reference to Obss.Host.csproj"
echo "     - Call services.Add${MODULE_NAME}Module() and app.Map${MODULE_NAME}Endpoints()"
echo "     - Or implement IModuleRegistration for auto-discovery"
echo "  2. Update the database connection string in appsettings.json"
echo "  3. Run 'dotnet build' to verify compilation"
echo ""
echo "Module location: $MODULES_DIR/$MODULE_NAME/"
