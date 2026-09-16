FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TraceFlow.api/TraceFlow.api.csproj TraceFlow.api/
RUN dotnet restore TraceFlow.api/TraceFlow.api.csproj

COPY TraceFlow.api/ TraceFlow.api/
RUN dotnet publish TraceFlow.api/TraceFlow.api.csproj \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 5075

ENV ASPNETCORE_URLS=http://+:5075

ENTRYPOINT ["dotnet", "TraceFlow.Api.dll"]