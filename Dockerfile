FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Stalker.csproj", "Stalker/"]
RUN dotnet restore "Stalker/Stalker.csproj"
COPY . .
RUN dotnet build "src/Stalker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Stalker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stalker.dll"]
