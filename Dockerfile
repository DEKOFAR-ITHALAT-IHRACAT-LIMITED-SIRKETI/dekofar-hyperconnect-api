# BUILD STAGE (.NET 8 SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ENV DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1

# Önce sadece csproj'leri kopyala
COPY dekofar-hyperconnect-api/dekofar-hyperconnect-api.csproj ./dekofar-hyperconnect-api/
COPY Dekofar.HyperConnect.Application/Dekofar.HyperConnect.Application.csproj ./Dekofar.HyperConnect.Application/
COPY Dekofar.HyperConnect.Domain/Dekofar.HyperConnect.Domain.csproj ./Dekofar.HyperConnect.Domain/
COPY Dekofar.HyperConnect.Infrastructure/Dekofar.HyperConnect.Infrastructure.csproj ./Dekofar.HyperConnect.Infrastructure/
COPY Dekofar.HyperConnect.Integrations/Dekofar.HyperConnect.Integrations.csproj ./Dekofar.HyperConnect.Integrations/
COPY Dekofar.HyperConnect.Shared/Dekofar.HyperConnect.Shared.csproj ./Dekofar.HyperConnect.Shared/

# Restore
RUN dotnet restore ./dekofar-hyperconnect-api/dekofar-hyperconnect-api.csproj --verbosity minimal

# Sonra tüm kaynakları kopyala
COPY . .

# Publish
RUN dotnet publish ./dekofar-hyperconnect-api/dekofar-hyperconnect-api.csproj -c Release -o /app/publish --no-restore

# RUNTIME STAGE (.NET 8 ASPNET)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT} \
    DOTNET_NOLOGO=1

COPY --from=build /app/publish .

# DLL adı: publish klasöründe çıkanla aynı olmalı
ENTRYPOINT ["dotnet", "dekofar-hyperconnect-api.dll"]
