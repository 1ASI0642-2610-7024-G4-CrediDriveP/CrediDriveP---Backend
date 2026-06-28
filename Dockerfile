FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CrediDriveP.API/CrediDriveP.API.csproj", "CrediDriveP.API/"]
RUN dotnet restore "CrediDriveP.API/CrediDriveP.API.csproj"
COPY . .
WORKDIR "/src/CrediDriveP.API"
RUN dotnet build "CrediDriveP.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrediDriveP.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrediDriveP.API.dll"]