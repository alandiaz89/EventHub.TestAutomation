#!/usr/bin/env bash
# Read-only GET probe for the EventHub API. Never prints credentials or the token.
# Usage: probe.sh "<path-and-query>" [--no-auth | --token <value>]
set -euo pipefail

path="${1:?usage: probe.sh \"<path-and-query>\" [--no-auth | --token <value>]}"
mode="${2:-}"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)"
settings="$root/src/EventHub.Core/appsettings.json"
local_settings="$root/src/EventHub.Core/appsettings.local.json"

api="${ApiBaseUrl:-$(jq -r .ApiBaseUrl "$settings")}"

case "$mode" in
  --no-auth) token="" ;;
  --token)   token="${3:?--token needs a value}" ;;
  *)
    email="${UserEmail:-$( [ -f "$local_settings" ] && jq -r '.UserEmail // empty' "$local_settings" || true)}"
    password="${UserPassword:-$( [ -f "$local_settings" ] && jq -r '.UserPassword // empty' "$local_settings" || true)}"
    if [ -z "$email" ] || [ -z "$password" ]; then
      echo "Set UserEmail and UserPassword (env vars or appsettings.local.json)." >&2
      exit 1
    fi
    token="$(curl -s -X POST "$api/auth/login" -H 'Content-Type: application/json' \
      -d "$(jq -n --arg e "$email" --arg p "$password" '{email:$e,password:$p}')" | jq -r '.token // empty')"
    if [ -z "$token" ]; then
      echo "Login failed; no token received." >&2
      exit 1
    fi
    ;;
esac

headers=()
[ -n "$token" ] && headers=(-H "Authorization: Bearer $token")

response="$(curl -s -w '\n%{http_code}' "$api/${path#/}" "${headers[@]+"${headers[@]}"}")"
status="$(printf '%s' "$response" | tail -n 1)"
body="$(printf '%s' "$response" | sed '$d')"

echo "status: $status"
printf '%s' "$body" | jq -c . 2>/dev/null | cut -c1-1500 || printf '%s' "$body" | cut -c1-1500
