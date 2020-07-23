# Exercise #5

## Goal

Our goal in this exercise is to introduce Serilog Distributed Logging

## Expected Results

Upon completion of this lab we will have enhanced our logs with Serilog Functionality.

## Introduction

In this exercise we'll add our Serilog integration to our bootcamp-webapi backend application.

1. Return back to your `bootcamp-webapi` project root and find the project file.  We will add the following nuget packages:

    ```powershell
    dotnet add package Steeltoe.Management.CloudFoundryCore --version 2.4.4
    dotnet add package Steeltoe.Extensions.Logging.SerilogDynamicLogger --version 2.4.4
    ```

2. Navigate to the Program class and make the following edits:

   1. Set the following using statements:

        ```c#
        using Steeltoe.Extensions.Logging.SerilogDynamicLogger;
        using Steeltoe.Management.CloudFoundry;
        ```

   2. In the CreateHostBuilder method make the below `ConfigureLogging` ***before*** the call to `ConfigureWebHostDefaults` to configure Serilog logging.  The AddCloudFoundryActuators and AddConfigServer calls automatically add Dynamic logging.  If you don't put the SerilogDynamicConsole logging before these calls you will see an error.  Also add the call `webBuilder.AddCloudFoundryActuators();` to add the Cloud Foundry Actuators as we did in our Store Front UI.

        ```c#
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((builderContext, loggingBuilder) =>
                {
                    loggingBuilder.AddSerilogDynamicConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.AddCloudFoundryActuators();
                    webBuilder.AddConfigServer(GetLoggerFactory());
                    webBuilder.AddServiceDiscovery();
                    webBuilder.UseStartup<Startup>();
                });
        ```

3. In the root directory navigate to the appsettings.json file and add an entry for Serilog like the below snippet.  These settings tell Serilog how to format the logs.  Also take note of the info section.

    ```json
    "management": {
      "endpoints": {
        "actuator":{
          "exposure": {
            "include": [ "*" ]
          }
        }
      }
    },
    "Serilog": {
      "IncludeScopes": false,
      "MinimumLevel": {
        "Default": "Warning",
        "Override": {
          "Pivotal": "Information",
          "Microsoft": "Warning",
          "Steeltoe": "Information",
          "CloudFoundry.Controllers": "Verbose"
        }
      },
      "WriteTo": [{
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Properties} {NewLine} {EventId} {Message:lj}{NewLine}{Exception}"
        }
      }],
      "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
    },
    "info": {
      "build": "dev.01"
    }
    ```

## Extend application to add Info Contributors

Before we switch to Apps Manager and see all the wonderful benefits of actuators, let's customize things a little. One very useful actuator is the /Info endpoint.

1. Create a new C# class in the app project, named `ProductInStockInfoContributor.cs`. It can be located in the root of the project with the Project.cs and Startup.cs files.  The class should have the following definition:

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

2. Open the `Startup.cs` file, by double clicking.

3. Add a reference to the Info Steeltoe package.

    ```cs
    using Steeltoe.Management.Endpoint.Info;
    ```

4. In the `ConfigureServices` method, register the new custom health check as a singleton.

    ```cs
    services.AddSingleton<IInfoContributor, ProductInStockInfoContributor>();
    ```

5. We will once again publish  our application using the Dotnet Core CLI.

    ```powershell
    dotnet publish -o .\publish
    ```

6. Run the cf push command to build, stage and run your application on PCF.  Ensure you are in the same directory as your manifest file and type `cf push`.

7. Once the command has completed, navigate to the url to see the Swagger definition of our API and then explore some of the Apps Manager integrations with Steeltoe.  Pay special attention to the application logs and the Information section under Settings.
