// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.Pipeline.Queue.DevTools;
using Projects;

namespace Microsoft.KernelMemory.Aspire.AppHost;

// Run KM in volatile mode, with OpenAI models
internal static class Program
{
    private static readonly SimpleFileStorageConfig s_simpleFileStorageConfig = new();
    private static readonly SimpleQueuesConfig s_simpleQueuesConfig = new();
    private static readonly OpenAIConfig s_openAIConfig = new();

    internal static void Main()
    {
        var builder = DistributedApplication.CreateBuilder();

        var qdrant = builder.AddQdrant("qdrant");

        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.development.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build()
            .BindSection("KernelMemory:Services:SimpleFileStorage", s_simpleFileStorageConfig)
            .BindSection("KernelMemory:Services:SimpleQueues", s_simpleQueuesConfig)
            .BindSection("KernelMemory:Services:OpenAI", s_openAIConfig);

        builder
            .AddProject<Service>("kernel-memory")
            .WithHttpEndpoint(targetPort: 21001)
            .WithEnvironment("ASPNETCORE_URLS", "http://+:21001")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("KernelMemory__Service__OpenApiEnabled", "True")
            .WithKmTextEmbeddingGenerationEnvironment("OpenAI", s_openAIConfig)
            .WithKmTextGenerationEnvironment("OpenAI", s_openAIConfig)
            .WithKmOrchestrationEnvironment("SimpleQueues", s_simpleQueuesConfig)
            .WithKmDocumentStorageEnvironment("SimpleFileStorage", s_simpleFileStorageConfig)
            .WithKmContentSafetyModerationEnvironment(null) // ensure moderation is disabled
            .WithKmOcrEnvironment(null) // ensure OCR is disabled
            .WithReference(qdrant)
            .WithEnvironment(async ctx => await SetVarFromConnectionStringAsync("KernelMemory__Services__Qdrant__Endpoint", ctx, qdrant.Resource, "Endpoint=([^;]+)").ConfigureAwait(false))
            .WithEnvironment(async ctx => await SetVarFromConnectionStringAsync("KernelMemory__Services__Qdrant__APIKey", ctx, qdrant.Resource, "Key=([^;]+)").ConfigureAwait(false))
            .WithEnvironment("KernelMemory__DataIngestion__MemoryDbTypes__0", "Qdrant")
            .WithEnvironment("KernelMemory__Retrieval__MemoryDbType", "Qdrant");

        builder
            .ShowDashboardUrl()
            .LaunchDashboard()
            .Build().Run();
    }

    private static async Task SetVarFromConnectionStringAsync(
        string name,
        EnvironmentCallbackContext context,
        IResourceWithConnectionString resource,
        string pattern)
    {
        string? connectionString = await resource.ConnectionStringExpression.GetValueAsync(default).ConfigureAwait(false);
        string? value = connectionString == null ? string.Empty : Regex.Match(connectionString, pattern).Groups.Cast<Group>().Skip(1).FirstOrDefault()?.Value;
        context.EnvironmentVariables[name] = value ?? string.Empty;
    }
}
