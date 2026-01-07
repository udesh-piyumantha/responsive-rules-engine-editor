using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RulesEngineEditor.Services.Storage
{
    /// <summary>
    /// Factory for creating and resolving storage provider instances
    /// Supports multiple storage backends via configuration
    /// </summary>
    public class StorageProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public StorageProviderFactory(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        /// <summary>
        /// Create a storage provider instance based on configuration
        /// Storage:Type can be: JsonFile, S3, AzureBlob
        /// </summary>
        public IStorageProvider CreateProvider()
        {
            var storageType = _configuration["Storage:Type"] ?? "JsonFile";
            return CreateProvider(storageType);
        }

        /// <summary>
        /// Create a storage provider instance of specified type
        /// </summary>
        public IStorageProvider CreateProvider(string providerType)
        {
            return providerType.ToLowerInvariant() switch
            {
                "jsonfile" => _serviceProvider.GetRequiredService<JsonFileStorageProvider>(),
                "s3" => _serviceProvider.GetRequiredService<S3StorageProvider>(),
                "azureblob" => _serviceProvider.GetRequiredService<AzureBlobStorageProvider>(),
                _ => throw new ArgumentException($"Unknown storage provider: {providerType}")
            };
        }
    }
}
