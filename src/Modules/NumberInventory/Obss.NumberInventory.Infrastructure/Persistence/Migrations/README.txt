Migrations need to be added for the NumberInventory module.

To create an initial migration, run:
  dotnet ef migrations add InitialNumberInventoryMigration --context NumberDbContext --output-dir Persistence/Migrations --startup-project ../../Host/Obss.Host/Obss.Host.csproj

To apply migrations:
  dotnet ef database update --context NumberDbContext --startup-project ../../Host/Obss.Host/Obss.Host.csproj
