#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE DATABASE umami_db;
    GRANT ALL PRIVILEGES ON DATABASE umami_db TO $POSTGRES_USER;
EOSQL