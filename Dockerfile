FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source
COPY HurryUpHaul.sln .
COPY HurryUpHaul.Contracts/HurryUpHaul.Contracts.csproj HurryUpHaul.Contracts/
COPY HurryUpHaul.Domain/HurryUpHaul.Domain.csproj HurryUpHaul.Domain/
COPY HurryUpHaul.Api/HurryUpHaul.Api.csproj HurryUpHaul.Api/
COPY HurryUpHaul.UnitTests/HurryUpHaul.UnitTests.csproj HurryUpHaul.UnitTests/
COPY HurryUpHaul.IntegrationTests/HurryUpHaul.IntegrationTests.csproj HurryUpHaul.IntegrationTests/
RUN dotnet restore HurryUpHaul.sln

COPY . .
WORKDIR /source/HurryUpHaul.Api
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "HurryUpHaul.Api.dll"]
