
# 1. Derleme aşaması
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 2. Proje dosyasını kopyala ve bağımlılıkları indir
COPY ["OrganizationPlatform.csproj", "./"]
RUN dotnet restore "OrganizationPlatform.csproj"

# 3. Kaynak kodlarını kopyala ve uygulamayı yayımla
COPY . .
RUN dotnet publish "OrganizationPlatform.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# 4. Çalıştırma aşaması
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OrganizationPlatform.dll"]
