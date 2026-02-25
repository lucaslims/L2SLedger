#!/bin/sh
# Generates /app/dist/env-config.js at runtime
# with all VITE_* environment variables from the container.
# Used as ENTRYPOINT — exec's the CMD after generating the file.
#
# Uses awk to avoid pipe-subshell issues on Alpine (busybox ash).
# Handles values with '=' signs correctly (Firebase keys, URLs, etc.).

set -e

ENV_FILE="/tmp/env-config.js"
TMP_FILE="$(mktemp)"

# Build the JS file without pipe-subshell — awk reads env vars directly
printf 'window.__ENV__ = {\n' > "$TMP_FILE"

env | grep '^VITE_' | sort | awk -v sq="'" '
{
  # Split only on the FIRST = to safely handle values containing =
  n = index($0, "=")
  key = substr($0, 1, n - 1)
  val = substr($0, n + 1)
  # Escape backslashes then single quotes
  gsub(/\\/, "\\\\", val)
  gsub(sq, "\\" sq, val)
  printf "  %s: %s%s%s,\n", key, sq, val, sq
}' >> "$TMP_FILE"

printf '};\n' >> "$TMP_FILE"

# Atomically replace the target file
mv "$TMP_FILE" "$ENV_FILE"

COUNT=$(env | grep -c '^VITE_' || echo 0)
echo "[env.sh] env-config.js generated with ${COUNT} VITE_* variable(s)"

# Execute the CMD (serve)
exec "$@"
