FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Practica 2.csproj", "./"]
RUN dotnet restore "Practica 2.csproj"
COPY . .
RUN dotnet publish "Practica 2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Practica 2.dll"]
