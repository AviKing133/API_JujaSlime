# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# COPIA CORREGIDA: Apuntamos a la subcarpeta donde está el archivo
COPY ["API_JujaSlime/API_JujaSlime.csproj", "API_JujaSlime/"]
RUN dotnet restore "API_JujaSlime/API_JujaSlime.csproj"

# Copia el resto del código
COPY . .
WORKDIR "/src/API_JujaSlime"
RUN dotnet publish "API_JujaSlime.csproj" -c Release -o /app/publish

# Etapa final de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Puerto para Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "API_JujaSlime.dll"]
