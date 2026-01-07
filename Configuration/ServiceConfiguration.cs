using RulesEngineEditor.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Amazon.S3;

namespace RulesEngineEditor.Configuration
{
    /// <summary>
    /// Extension methods for configuring Rules Engine Editor services
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Add Rules Engine Editor services to DI container
        /// Call this in Program.cs: services.AddRulesEngineEditor(configuration);
        /// </summary>
        public static IServiceCollection AddRulesEngineEditor(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add storage providers
            services.AddScoped<JsonFileStorageProvider>();
            
            // Add AWS S3 client if configured
            var hasS3Config = !string.IsNullOrEmpty(configuration["AWS:AccessKeyId"]);
            if (hasS3Config)
            {
                services.AddAWSService<IAmazonS3>();
                services.AddScoped<S3StorageProvider>();
            }
            else
            {
                // Provide null implementation for S3 if not configured
                services.AddScoped<S3StorageProvider>(sp => throw new InvalidOperationException(
                    "S3 storage provider not configured. Set AWS credentials in appsettings.json"));
            }
            
            // Add Azure Blob Storage client if configured
            var hasAzureConfig = !string.IsNullOrEmpty(configuration["Storage:Azure:ConnectionString"]);
            if (hasAzureConfig)
            {
                services.AddScoped<AzureBlobStorageProvider>();
            }
            else
            {
                // Provide null implementation for Azure if not configured
                services.AddScoped<AzureBlobStorageProvider>(sp => throw new InvalidOperationException(
                    "Azure Blob Storage not configured. Set Storage:Azure:ConnectionString in appsettings.json"));
            }
            
            // Add storage factory
            services.AddScoped<StorageProviderFactory>();
            
            return services;
        }
    }
}
