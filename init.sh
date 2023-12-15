# create a new solution with default .gitignore
dotnet new sln
dotnet new gitignore

# create project folders
mkdir HurryUpHaul.Contracts
mkdir HurryUpHaul.Domain
mkdir HurryUpHaul.Endpoint
mkdir HurryUpHaul.ServiceTests
mkdir HurryUpHaul.UnitTests

# create projects
cd HurryUpHaul.Contracts
dotnet new classlib
cd ../HurryUpHaul.Domain
dotnet new classlib
cd ../HurryUpHaul.Endpoint
dotnet new webapi
cd ../HurryUpHaul.ServiceTests
dotnet new xunit
cd ../HurryUpHaul.UnitTests
dotnet new xunit

# add projects to solution
cd ..
dotnet sln add HurryUpHaul.Contracts/HurryUpHaul.Contracts.csproj
dotnet sln add HurryUpHaul.Domain/HurryUpHaul.Domain.csproj
dotnet sln add HurryUpHaul.Endpoint/HurryUpHaul.Endpoint.csproj
dotnet sln add HurryUpHaul.ServiceTests/HurryUpHaul.ServiceTests.csproj
dotnet sln add HurryUpHaul.UnitTests/HurryUpHaul.UnitTests.csproj

# add references
dotnet add HurryUpHaul.Endpoint/HurryUpHaul.Endpoint.csproj reference HurryUpHaul.Contracts/HurryUpHaul.Contracts.csproj
dotnet add HurryUpHaul.Endpoint/HurryUpHaul.Endpoint.csproj reference HurryUpHaul.Domain/HurryUpHaul.Domain.csproj
dotnet add HurryUpHaul.ServiceTests/HurryUpHaul.ServiceTests.csproj reference HurryUpHaul.Endpoint/HurryUpHaul.Endpoint.csproj
dotnet add HurryUpHaul.UnitTests/HurryUpHaul.UnitTests.csproj reference HurryUpHaul.Domain/HurryUpHaul.Domain.csproj