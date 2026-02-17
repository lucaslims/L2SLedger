#!/bin/sh
# Generates /app/dist/env-config.js at runtime
# with all VITE_* environment variables from the container.
# Used as ENTRYPOINT — exec's the CMD after generating the file.

ENV_FILE="/app/dist/env-config.js"

echo "window.__ENV__ = {" > "$ENV_FILE"

# Iterate all environment variables starting with VITE_
env | grep '^VITE_' | sort | while IFS='=' read -r key value; do
  # Escape single quotes in the value
  escaped=$(echo "$value" | sed "s/'/\\\\'/g")
  echo "  $key: '$escaped'," >> "$ENV_FILE"
done

echo "};" >> "$ENV_FILE"

echo "[env.sh] env-config.js generated with $(grep -c ':' "$ENV_FILE") variable(s)"

# Execute the CMD (serve)
exec "$@"
