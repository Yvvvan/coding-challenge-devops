FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Source/src/CodingChallenge.Web/CodingChallenge.Web.csproj Source/src/CodingChallenge.Web/
RUN dotnet restore Source/src/CodingChallenge.Web/CodingChallenge.Web.csproj

COPY Source/ Source/
WORKDIR /src/Source/src/CodingChallenge.Web
RUN dotnet publish CodingChallenge.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CodingChallenge.Web.dll"]
