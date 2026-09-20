FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TraceFlow.Ingestion.Api/TraceFlow.Ingestion.Api.csproj TraceFlow.Ingestion.Api/
RUN dotnet restore TraceFlow.Ingestion.Api/TraceFlow.Ingestion.Api.csproj

COPY TraceFlow.Ingestion.Api/ TraceFlow.Ingestion.Api/
RUN dotnet publish TraceFlow.Ingestion.Api/TraceFlow.Ingestion.Api.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5100
EXPOSE 5100

ENTRYPOINT ["dotnet", "TraceFlow.Ingestion.Api.dll"]