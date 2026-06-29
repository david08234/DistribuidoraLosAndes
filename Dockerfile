# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo del proyecto e instalar dependencias
COPY ["DistribuidoraLosAndes.csproj", "./"]
RUN dotnet restore "DistribuidoraLosAndes.csproj"

# Copiar todo el código y publicar la aplicación
COPY . .
RUN dotnet publish "DistribuidoraLosAndes.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto que Render exige por defecto
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DistribuidoraLosAndes.dll"]