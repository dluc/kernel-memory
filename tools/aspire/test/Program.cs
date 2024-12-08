// Copyright (c) Microsoft. All rights reserved.

using Aspire.Hosting;
using Projects;

namespace Microsoft.KernelMemory.Aspire.AppHost;

internal static class Program
{
    private const string OpenAIApiKey = "sk-...";

    internal static void Main()
    {
        var builder = DistributedApplication.CreateBuilder();

        builder.AddProject<Service>("kernel-memory")
            .WithHttpEndpoint(targetPort: 21001)
            .WithEnvironment("ASPNETCORE_URLS", "http://+:21001")
            .WithEnvironment("KernelMemory__Service__OpenApiEnabled", "True")
            .WithEnvironment("KernelMemory__DataIngestion__EmbeddingGeneratorTypes__0", "OpenAI")
            .WithEnvironment("KernelMemory__Retrieval__EmbeddingGeneratorType", "OpenAI")
            .WithEnvironment("KernelMemory__TextGeneratorType", "OpenAI")
            .WithEnvironment("KernelMemory__Services__OpenAI__APIKey", OpenAIApiKey)
            .WithEnvironment("KernelMemory__Services__OpenAI__TextModel", "gpt-4o-mini")
            .WithEnvironment("KernelMemory__Services__OpenAI__EmbeddingModel", "text-embedding-3-small")
            .WithEnvironment("KernelMemory__DataIngestion__MemoryDbTypes__0", "SimpleVectorDb")
            .WithEnvironment("KernelMemory__Retrieval__MemoryDbType", "SimpleVectorDb")
            .WithEnvironment("KernelMemory__DataIngestion__DistributedOrchestration__QueueType", "SimpleQueues")
            .WithEnvironment("KernelMemory__DocumentStorageType", "SimpleFileStorage")
            .WithEnvironment("KernelMemory__ContentModerationType", "")
            .WithEnvironment("KernelMemory__DataIngestion__ImageOcrType", "");

        builder.Build().Run();
    }
}
