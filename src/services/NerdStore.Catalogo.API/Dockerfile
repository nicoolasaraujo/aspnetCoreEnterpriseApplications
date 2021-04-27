#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["src/services/NerdStore.Catalogo.API/NerdStore.Catalogo.API.csproj", "src/services/NerdStore.Catalogo.API/"]
RUN dotnet restore "src/services/NerdStore.Catalogo.API/NerdStore.Catalogo.API.csproj"
COPY . .
WORKDIR "/src/src/services/NerdStore.Catalogo.API"
RUN dotnet build "NerdStore.Catalogo.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NerdStore.Catalogo.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NerdStore.Catalogo.API.dll"]