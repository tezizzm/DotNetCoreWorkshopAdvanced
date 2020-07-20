# Exercise #6

## Goal

Our goal in this exercise is to introduce Distributed Tracing

## Expected Results

Upon completion of this lab we will have built up a hierarchy of our application dependencies and be able to view how requests flow through the application.

## Introduction

In this exercise we'll add our OpenCensus integration to our bootcamp-store frontend application.

1. Return back to your `bootcamp-store` project root and find the project file.  We will add the following nuget package:

    ```powershell
    dotnet add package Steeltoe.Management.TracingCore --version 2.4.4
    dotnet add package Steeltoe.Management.ExporterCore --version 2.4.4
    ```

2. Navigate to the Startup class and make the following edits:

   1. Set the following using statements:

      ```c#
      using Steeltoe.Management.Exporter.Tracing;
      using Steeltoe.Management.Tracing;
      ```

   2. In the ConfigureServices method add the following lines to add the Distributed Tracing and the Zipkin Exporter to the Service Container.

      ```c#
      services.AddDistributedTracing(Configuration);
      services.AddZipkinExporter(Configuration);
      ```

   3. In the Configure method add the following line to add the Tracing and Exporter to the middleware pipeline.

      ```c#
      app.UseTracingExporter();
      ```

3. In the root directory navigate to the appsettings.json file and add an entry for tracing like the below snippet.  ***Ensure this entry is inside the management object.***  These settings tell the exporter where to forward are tracing data.

    ```json
    "management": {
      "endpoints": {
        "actuator":{
          "exposure": {
            "include": [ "*" ]
          }
        }
      },
      "tracing": {
        "alwaysSample": true,
        "egressIgnorePattern": "/api/v2/spans|/v2/apps/.*/permissions|/eureka/.*|/oauth/.*",
        "useShortTraceIds ": true,
        "exporter": {
          "zipkin": {
            "endpoint": "http://zipkin-{initials}.apps.{cloud-foundry-foundation}/api/v2/spans",
            "validateCertificates": false,
            "useShortTraceIds ": true
          }
        }
      }
    }
    ```

## Extend application to add Info Contributors

Before we switch back to Apps Manager let's customize things further. One very useful actuator is the /Info endpoint.

1. Let's switch back to our API application, bootcamp_webapi.

2. Create a new C# class in the app project, named `ProductInfoContributor.cs`. It can be located in the root of the project with the Project.cs and Startup.cs files.  The class should have the following definition:

    ```cs
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Steeltoe.Management.Endpoint.Info;

    namespace bootcamp_webapi
    {

        public class ProductInStockInfoContributor : IInfoContributor
        {
            private readonly ProductContext _context;

            public ProductInStockInfoContributor([FromServices] ProductContext context)
            {
                _context = context;
            }

            public void Contribute(IInfoBuilder builder)
            {
                var count = _context.Products.Count();

                // pass in the info
                builder.WithInfo("productInStock", new { numberOfProductsInStock = count });
            }
        }
    }
    ```

3. Open the `Startup.cs` file, by double clicking.

4. Add a reference to the HealthChecks Steeltoe package.

    ```cs
    using Steeltoe.Management.Endpoint.Info;
    ```

5. In the `ConfigureServices` method, register the new custom health check as a singleton.

    ```cs
    services.AddSingleton<IInfoContributor, ProductInStockInfoContributor>();
    ```
