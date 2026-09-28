FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Source/src/CodingChallenge.Api/CodingChallenge.Api.csproj Source/src/CodingChallenge.Api/
RUN dotnet restore Source/src/CodingChallenge.Api/CodingChallenge.Api.csproj

COPY Source/ Source/
WORKDIR /src/Source/src/CodingChallenge.Api
RUN dotnet publish CodingChallenge.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
RUN mkdir -p /app/data \
    && chgrp -R 0 /app/data \
    && chmod -R g=u /app/data

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CodingChallenge.Api.dll"]
