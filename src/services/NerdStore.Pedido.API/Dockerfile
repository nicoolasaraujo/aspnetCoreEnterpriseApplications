#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["NerdStore.Pedido.API/NerdStore.Pedido.API.csproj", "NerdStore.Pedido.API/"]
RUN dotnet restore "NerdStore.Pedido.API/NerdStore.Pedido.API.csproj"
COPY . .
WORKDIR "/src/NerdStore.Pedido.API"
RUN dotnet build "NerdStore.Pedido.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NerdStore.Pedido.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NerdStore.Pedido.API.dll"]