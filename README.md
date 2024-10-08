Johannes Rebane

Regular UNO card game

Technologies:
  - EF
  - Dotnet
  - Razor Pages

Game can be played both in web and console also started in one, saved and continued in other.
There are many house rules that you can choose to play with, give effects to any card you like.

~~~bash
dotnet ef migrations add --project DAL --startup-project ConsoleApp PlayerRemoved

dotnet aspnet-codegenerator razorpage -m Domain.Database.Player -dc AppDbContext -udl -outDir Pages/Players --referenceScriptLibraries

dotnet aspnet-codegenerator razorpage -m Domain.Database.Game -dc AppDbContext -udl -outDir Pages/Games --referenceScriptLibraries
~~~
