#!/usr/bin/env bash
set -euo pipefail

# WARSApplication project root (location of this script)
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
DB_DIR="$SCRIPT_DIR/Databases"

mkdir -p "$DB_DIR"

# Fresh start: drop databases and any previous secret.
rm -f "$DB_DIR/Auctions.db" "$DB_DIR/Auth.db" "$DB_DIR/jwt_secret"

# Generate a fresh JwtSecret and persist it (owner-readable only).
umask 077
if command -v openssl >/dev/null 2>&1; then
    openssl rand -base64 48 > "$DB_DIR/jwt_secret"
else
    head -c 64 /dev/urandom | base64 > "$DB_DIR/jwt_secret"
fi
export JwtSecret="$(cat "$DB_DIR/jwt_secret")"

echo "==> Creating Auctions.db"
dotnet ef database update --project "$SCRIPT_DIR" \
    --context AuctionDbContext \
    --connection "Data Source=$DB_DIR/Auctions.db"

echo "==> Creating Auth.db"
dotnet ef database update --project "$SCRIPT_DIR" \
    --context AuthDbContext \
    --connection "Data Source=$DB_DIR/Auth.db"

echo
echo "Done."
echo "  Databases : $DB_DIR/Auctions.db, $DB_DIR/Auth.db"
echo "  JwtSecret : persisted in $DB_DIR/jwt_secret"