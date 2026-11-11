# API image. Build context: api/
#   docker build -t alpinarc-api api
#
# The image contains the API and an EF Core migration bundle at /app/efbundle.
# On Railway the bundle runs as the pre-deploy command, never on API startup.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# MSBuild reads environment variables as properties, so this reaches every build in
# this stage, including the one `dotnet ef migrations bundle` runs. The committed
# openapi/alpinarc-api.json is what the web app uses, so it isn't regenerated here.
ENV OpenApiGenerateDocuments=false

# Restore first, so the layers stay cached until a project file changes.
COPY global.json Directory.Build.props dotnet-tools.json ./
COPY src/Alpinarc.Api/Alpinarc.Api.csproj src/Alpinarc.Api/
# linux-x64 is restored as well because the migration bundle is self-contained.
RUN dotnet tool restore \
    && dotnet restore src/Alpinarc.Api/Alpinarc.Api.csproj --runtime linux-x64

COPY src/ src/
RUN dotnet publish src/Alpinarc.Api/Alpinarc.Api.csproj \
    --configuration Release --no-restore --output /app
RUN dotnet ef migrations bundle \
    --project src/Alpinarc.Api/Alpinarc.Api.csproj \
    --configuration Release \
    --self-contained --runtime linux-x64 \
    --output /app/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# The non-root user that the .NET images include.
USER $APP_UID

# Railway sets PORT; without it (e.g. `docker run`) the API listens on 8080.
ENV PORT=8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=$PORT exec dotnet Alpinarc.Api.dll"]
