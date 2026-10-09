global using System.Net;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;

global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Diagnostics.HealthChecks;

global using Asp.Versioning;
global using Confluent.Kafka;
global using FluentValidation;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Options;
global using StackExchange.Redis;

global using TraceFlow.Ingestion.Api.Clients;
global using TraceFlow.Ingestion.Api.Configuration;
global using TraceFlow.Ingestion.Api.Contracts;
global using TraceFlow.Ingestion.Api.Contracts.Authentication;
global using TraceFlow.Ingestion.Api.Contracts.BatchLog;
global using TraceFlow.Ingestion.Api.Contracts.Log;
global using TraceFlow.Ingestion.Api.Contracts.RateLimiting;
global using TraceFlow.Ingestion.Api.Endpoints;
global using TraceFlow.Ingestion.Api.Errors;
global using TraceFlow.Ingestion.Api.Infrastructure.Extensions;
global using TraceFlow.Ingestion.Api.Kafka;
global using TraceFlow.Ingestion.Api.Middleware;
global using TraceFlow.Ingestion.Api.Security;
global using TraceFlow.Ingestion.Api.Services.Ingestion;
global using TraceFlow.Ingestion.Api.Services.RateLimiting;
global using TraceFlow.Ingestion.Api.Services.Redis;
global using TraceFlow.Ingestion.Api.Infrastructure.Health;