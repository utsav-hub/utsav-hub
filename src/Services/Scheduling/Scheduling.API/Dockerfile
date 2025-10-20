FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Scheduling/Scheduling.API/Scheduling.API.csproj", "src/Services/Scheduling/Scheduling.API/"]
COPY ["src/BuildingBlocks/Shared.Contracts/Shared.Contracts.csproj", "src/BuildingBlocks/Shared.Contracts/"]
RUN dotnet restore "src/Services/Scheduling/Scheduling.API/Scheduling.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Scheduling/Scheduling.API"
RUN dotnet build "Scheduling.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scheduling.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scheduling.API.dll"]