# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Cambia "API_JujaSlime.csproj" por el nombre exacto de tu archivo .csproj
COPY ["API_JujaSlime.csproj", "./"]
RUN dotnet restore "./API_JujaSlime.csproj"
COPY . .
RUN dotnet publish "API_JujaSlime.csproj" -c Release -o /app/publish

# Etapa 2: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render usa el puerto 10000 por defecto para Docker
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "API_JujaSlime.dll"]