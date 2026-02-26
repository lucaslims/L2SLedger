#!/bin/sh
# Generates /app/dist/env-config.js at runtime
# with WHITELISTED VITE_* environment variables from the container.
# Uses explicit whitelist for security (prevents accidental exposure).
#
# Security: Only variables in ALLOWED_VARS are exported to window.__ENV__
# VITE_ENABLE_DEVTOOLS is explicitly excluded from production builds.

set -e

ENV_FILE="/tmp/env-config.js"
TMP_FILE="$(mktemp)"

# Explicit whitelist of allowed variables
ALLOWED_VARS="
  VITE_API_BASE_URL
  VITE_FIREBASE_API_KEY
  VITE_FIREBASE_AUTH_DOMAIN
  VITE_FIREBASE_PROJECT_ID
  VITE_FIREBASE_STORAGE_BUCKET
  VITE_FIREBASE_MESSAGING_SENDER_ID
  VITE_FIREBASE_APP_ID
  VITE_EMAIL_VERIFICATION_RESEND_COOLDOWN
"

# Build the JS file
printf 'window.__ENV__ = {\n' > "$TMP_FILE"

COUNT=0
for VAR in $ALLOWED_VARS; do
  # Get the value from the environment
  VALUE=$(eval echo "\$$VAR")
  
  # Only export if the variable is set and non-empty
  if [ -n "$VALUE" ]; then
    # Escape backslashes and single quotes for JavaScript
    ESCAPED_VALUE=$(printf '%s' "$VALUE" | sed "s/\\\\/\\\\\\\\/g; s/'/\\\\'/g")
    printf "  %s: '%s',\n" "$VAR" "$ESCAPED_VALUE" >> "$TMP_FILE"
    COUNT=$((COUNT + 1))
  fi
done

printf '};\n' >> "$TMP_FILE"

# Atomically replace the target file
mv "$TMP_FILE" "$ENV_FILE"

echo "[env.sh] env-config.js generated with ${COUNT} whitelisted variable(s)"
echo "[env.sh] Security: VITE_ENABLE_DEVTOOLS and other non-whitelisted vars excluded"

# Execute the CMD (serve)
exec "$@"
