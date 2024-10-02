Johannes Rebane
jreban
223241IADB


~~~bash
dotnet ef migrations add --project DAL --startup-project ConsoleApp PlayerRemoved

dotnet aspnet-codegenerator razorpage -m Domain.Database.Player -dc AppDbContext -udl -outDir Pages/Players --referenceScriptLibraries

dotnet aspnet-codegenerator razorpage -m Domain.Database.Game -dc AppDbContext -udl -outDir Pages/Games --referenceScriptLibraries
~~~