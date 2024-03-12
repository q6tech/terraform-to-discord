FROM mcr.microsoft.com/dotnet/sdk:8.0.101@sha256:1b99a629b4f55a496db835d9c3347737aa4a1d995d43ffe5ab92d04bee69adeb AS backend-builder

WORKDIR /source
COPY TerraformToDiscord.sln /source/
COPY src/TerraformToDiscord/TerraformToDiscord.csproj /source/src/TerraformToDiscord/

RUN set -xe \
    && dotnet restore

COPY / /source/

RUN set -xe \
    && dotnet test \
    && dotnet publish /source/src/TerraformToDiscord/TerraformToDiscord.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0.2@sha256:789045ecae51d62d07877994d567eff4442b7bbd4121867898ee7bf00b7241ea AS final

LABEL org.opencontainers.image.authors "Mark Lopez <hello@q6tech.com>"

RUN set -xe \
    && apt-get update \
    && apt-get install curl -y

HEALTHCHECK --interval=2s --timeout=30s --start-period=1s --retries=10 CMD curl --fail http://localhost:5000/api/v1/health || exit 1

RUN set -xe \
    && addgroup --gid 1000 group \
    && useradd -G group --uid 1000 user

WORKDIR /app

COPY --from=backend-builder /app /app

USER 1000
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HOSTBUILDER__RELOADCONFIGONCHANGE=false

ENTRYPOINT ["dotnet", "TerraformToDiscord.dll"]
