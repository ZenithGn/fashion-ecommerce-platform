Baseline migration and apply instructions

Precautions
- Backup the database or take a dump before changing migration history.
- Verify the DB schema matches your current model (schema drift may cause issues).

Windows PowerShell instructions (run from repository root)

1) Install or update EF CLI

```powershell
dotnet tool install --global dotnet-ef
# or update if already installed
dotnet tool update --global dotnet-ef
```

2) (Optional) Export DB password so psql/pg_dump can use it

```powershell
$Env:PGPASSWORD = 'npg_XbAEM3kqjV5e'
```

3A) Create a baseline migration (recommended if you didn't keep the generated files)

```powershell
# Create a baseline migration (no schema ops)
dotnet ef migrations add Baseline `
  --context FashionEcommerceDbContext `
  --project src/FashionEcommerce.Data `
  --startup-project src/FashionEcommerce.API -- --ignore-changes

# Record it in the DB (writes to __EFMigrationsHistory)
dotnet ef database update `
  --context FashionEcommerceDbContext `
  --project src/FashionEcommerce.Data `
  --startup-project src/FashionEcommerce.API
```

3B) If you already kept the Baseline files I added, just apply them:

```powershell
dotnet ef database update `
  --context FashionEcommerceDbContext `
  --project src/FashionEcommerce.Data `
  --startup-project src/FashionEcommerce.API
```

4) Verify migration recorded (using psql)

```powershell
psql -h ep-jolly-waterfall-a1xvt7h3-pooler.ap-southeast-1.aws.neon.tech `
  -d neondb -U neondb_owner -c 'SELECT * FROM "__EFMigrationsHistory";'
```

Helpful environment fixes (if EF tools can't read connection)

```powershell
# set connection string as env var read by startup
$Env:ConnectionStrings__DefaultConnection = 'Host=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull;'
```

Safety checklist (do before running)

- Backup DB: pg_dump example

```powershell
pg_dump -h ep-jolly-waterfall-a1xvt7h3-pooler.ap-southeast-1.aws.neon.tech -U neondb_owner -d neondb -F c -f .\neondb-backup.dump
```

- Ensure ASPNETCORE_ENVIRONMENT is set to the environment your app uses, if needed:

```powershell
$Env:ASPNETCORE_ENVIRONMENT = 'Production'
```

Notes
- Commit the new Baseline migration to source control.
- After baseline, prefer applying future migrations from CI/deploy or a single leader instance.
