# create a new solution with default .gitignore & .editorconfig
dotnet new sln
dotnet new gitignore
dotnet new editorconfig

# create project folders
mkdir HurryUpHaul.Contracts
mkdir HurryUpHaul.Domain
mkdir HurryUpHaul.Api
mkdir HurryUpHaul.IntegrationTests
mkdir HurryUpHaul.UnitTests

# create projects
cd HurryUpHaul.Contracts
dotnet new classlib
cd ../HurryUpHaul.Domain
dotnet new classlib
cd ../HurryUpHaul.Api
dotnet new webapi
cd ../HurryUpHaul.IntegrationTests
dotnet new xunit
cd ../HurryUpHaul.UnitTests
dotnet new xunit

# add projects to solution
cd ..
dotnet sln add HurryUpHaul.Contracts/HurryUpHaul.Contracts.csproj
dotnet sln add HurryUpHaul.Domain/HurryUpHaul.Domain.csproj
dotnet sln add HurryUpHaul.Api/HurryUpHaul.Api.csproj
dotnet sln add HurryUpHaul.IntegrationTests/HurryUpHaul.IntegrationTests.csproj
dotnet sln add HurryUpHaul.UnitTests/HurryUpHaul.UnitTests.csproj

# add references
dotnet add HurryUpHaul.Api/HurryUpHaul.Api.csproj reference HurryUpHaul.Contracts/HurryUpHaul.Contracts.csproj
dotnet add HurryUpHaul.Api/HurryUpHaul.Api.csproj reference HurryUpHaul.Domain/HurryUpHaul.Domain.csproj
dotnet add HurryUpHaul.IntegrationTests/HurryUpHaul.IntegrationTests.csproj reference HurryUpHaul.Api/HurryUpHaul.Api.csproj
dotnet add HurryUpHaul.UnitTests/HurryUpHaul.UnitTests.csproj reference HurryUpHaul.Domain/HurryUpHaul.Domain.csproj