using Confluent.Kafka.Extensions.OpenTelemetry;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using System.Diagnostics;

namespace KafkaFlow.OpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder, string serviceName, string serviceVersion)
    {
        // Add logging with OpenTelemetry
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion);
            options.SetResourceBuilder(resourceBuilder);
        });

        // Add OpenTelemetry tracing and metrics
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => resourceBuilder.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithTracing(tracerProviderBuilder =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracerProviderBuilder.SetSampler<AlwaysOnSampler>();
                }

                tracerProviderBuilder
                    .AddSource(serviceName)
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = async (activity, request) =>
                        {
                            try
                            {
                                if (request.Content != null && request.Content.Headers.ContentLength > 0)
                                {
                                    var body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                                    if (!string.IsNullOrWhiteSpace(body))
                                    {
                                        activity.SetTag("http.request.body", body);
                                        activity.SetStatus(ActivityStatusCode.Error, "An error occurred while processing the request.");
                                    }
                                }
                            }
                            catch { }
                        };
                        options.EnrichWithHttpResponseMessage = async (activity, response) =>
                        {

                            try
                            {
                                if (response.Content != null && response.Content.Headers.ContentLength > 0)
                                {
                                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                    if (!string.IsNullOrWhiteSpace(body))
                                    {
                                        activity.SetTag("http.response.body", body);
                                        activity.SetStatus(ActivityStatusCode.Error, "An error occurred while processing the response.");
                                    }
                                }
                            }
                            catch (Exception ex) 
                            {
                            }
                        };
                    })
                    .AddConfluentKafkaInstrumentation();
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter(serviceName, "Microsoft.Extensions.Hosting", "System.Net.Http", "KafkaFlow");
            });

        // Add OpenTelemetry exporters
        builder.AddOpenTelemetryExports();

        return builder;
    }

    public static IHostApplicationBuilder AddOpenTelemetryExports(this IHostApplicationBuilder builder)
    {
        var uri = builder.Configuration["OTEL_EXPORTER_OTLP_GRPC_ENDPOINT"];
        if (!string.IsNullOrEmpty(uri))
        {
            var otlpExporterOptions = new Action<OtlpExporterOptions>(options =>
            {
                options.Endpoint = new Uri(uri);
                options.Protocol = OtlpExportProtocol.Grpc;
            });

            builder.Services.Configure<OpenTelemetryLoggerOptions>(loggerOptions =>
            {
                loggerOptions.AddOtlpExporter();
            });
            builder.Services.ConfigureOpenTelemetryMeterProvider(meterProviderOptions =>
            {
                meterProviderOptions.AddOtlpExporter();
            });
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracerProviderOptions =>
            {
                tracerProviderOptions.AddOtlpExporter();
            });

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.AddOtlpExporter(otlpExporterOptions);
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(meterProviderBuilder => meterProviderBuilder.AddOtlpExporter(otlpExporterOptions))
                .WithTracing(tracerProviderBuilder => tracerProviderBuilder.AddOtlpExporter(otlpExporterOptions));
        }

        return builder;
    }


    public static IHostApplicationBuilder AddDefaultHealthchecks(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}
