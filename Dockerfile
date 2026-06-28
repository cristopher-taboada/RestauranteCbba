# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el proyecto y restaurar dependencias
COPY ["RestauranteCbba.csproj", "."]
RUN dotnet restore

# Copiar todo el código y compilar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Puerto que usa la aplicación
EXPOSE 8080
EXPOSE 443

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "RestauranteCbba.dll"]