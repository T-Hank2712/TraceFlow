global using System.Text.Json;
global using System.Threading.Channels;

global using Confluent.Kafka;
global using DotNetEnv;
global using Microsoft.Extensions.Options;
global using OpenSearch.Client;

global using TraceFlow.LogProcessor.Configurations;
global using TraceFlow.LogProcessor.Contracts;
global using TraceFlow.LogProcessor.Extentions.Exceptions;
global using TraceFlow.LogProcessor.Processing;
global using TraceFlow.LogProcessor.Services.Batching;
global using TraceFlow.LogProcessor.Services.Kafka;
global using TraceFlow.LogProcessor.Services.Offsets;
global using TraceFlow.LogProcessor.Services.OpenSearch;
global using TraceFlow.LogProcessor.Services.Queue;
global using TraceFlow.LogProcessor.Workers;