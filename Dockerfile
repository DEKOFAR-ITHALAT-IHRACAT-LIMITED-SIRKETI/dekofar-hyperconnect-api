# BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ENV DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NUGET_PACKAGES=/root/.nuget/packages

# Sadece csproj'leri kopyala
COPY dekofar-hyperconnect-api/dekofar-hyperconnect-api.csproj ./dekofar-hyperconnect-api/
COPY Dekofar.HyperConnect.Application/Dekofar.HyperConnect.Application.csproj ./Dekofar.HyperConnect.Application/
COPY Dekofar.HyperConnect.Domain/Dekofar.HyperConnect.Domain.csproj ./Dekofar.HyperConnect.Domain/
COPY Dekofar.HyperConnect.Infrastructure/Dekofar.HyperConnect.Infrastructure.csproj ./Dekofar.HyperConnect.Infrastructure/
COPY Dekofar.HyperConnect.Integrations/Dekofar.HyperConnect.Integrations.csproj ./Dekofar.HyperConnect.Integrations/
COPY Dekofar.HyperConnect.Shared/Dekofar.HyperConnect.Shared.csproj ./Dekofar.HyperConnect.Shared/

COPY NuGet.Config ./

# İlk restore
RUN dotnet restore dekofar-hyperconnect-api/dekofar-hyperconnect-api.csproj \
  --configfile NuGet.Config \
  -p:RestoreAdditionalFallbackFolders= \
  -p:DisableImplicitNuGetFallbackFolder=true \
  --verbosity minimal

# Kaynakları kopyala
COPY . .

# Bin/obj temizliği
RUN find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +

# Restore tekrar
RUN dotnet restore dekofar-hyperconnect-api/dekofar-hyperconnect-api.csproj \
  --configfile NuGet.Config \
  -p:RestoreAdditionalFallbackFolders= \
  -p:DisableImplicitNuGetFallbackFolder=true \
  --verbosity minimal

# Publish (önemli: yol sadeleştirildi)
WORKDIR /src/dekofar-hyperconnect-api
RUN dotnet publish dekofar-hyperconnect-api.csproj -c Release -o /app/publish --no-restore

# RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT} \
    DOTNET_NOLOGO=1
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "dekofar-hyperconnect-api.dll"]
