-- WARNING: Backup your database before running this script.
-- Ensure this connects to the same database used by your app's DefaultConnection.

-- Verify the Categories table exists and columns match the InitialPostgresCreate migration
-- (run these checks first):
-- SELECT table_schema, table_name FROM information_schema.tables WHERE lower(table_name) = lower('Categories');
-- SELECT column_name, data_type, character_maximum_length, is_nullable, ordinal_position
-- FROM information_schema.columns
-- WHERE lower(table_name) = lower('Categories')
-- ORDER BY ordinal_position;

-- Inspect applied migrations:
-- SELECT "MigrationId","ProductVersion" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";

-- If the schema matches the InitialPostgresCreate migration, run the INSERT below to mark it applied.
-- Adjust MigrationId/ProductVersion if your migration files differ.

INSERT INTO "__EFMigrationsHistory" ("MigrationId","ProductVersion")
VALUES ('20260526132835_InitialPostgresCreate','8.0.11');

-- Verify insertion:
-- SELECT "MigrationId","ProductVersion" FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260526132835_InitialPostgresCreate';

-- Example psql command to run this file (Linux/macOS/WSL):
-- psql "postgresql://<user>:<password>@<host>:<port>/<database>" -f scripts/mark_initial_migration_applied.sql

-- Example Windows PowerShell command using psql:
-- psql "postgresql://<user>:<password>@<host>:<port>/<database>" -f .\\scripts\\mark_initial_migration_applied.sql

-- After running, re-start the application or run:
-- dotnet ef database update --project src/FashionEcommerce.Data --startup-project src/FashionEcommerce.API
