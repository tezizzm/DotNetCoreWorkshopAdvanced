# Exercise #2

## Goal

Explore Externalized Configuration by working with Spring Cloud Config Server

## Expected Results

Create an instance of Spring Cloud Services Configuration Server and bind our API application to that instance of the Configuration Server.

## Introduction

In this exercise we explore how Configuration Server pulls configuration from a backend repository.  Also observe how we utilize Steeltoe to connect to Configuration Server and manipulate how we retrieve configuration.

1. We will once again be working with our WebAPI based Product microservice.  In the root directory of our microservice add the following Nuget packages.

    ```powershell
    dotnet add package NSwag.AspNetCore --version 13.6.2
    dotnet add package Steeltoe.Extensions.Configuration.ConfigServerCore --version 2.4.4
    ```

2. Edit the Program.cs class.

   1. Add the following using statement:

        ```c#
        using Microsoft.Extensions.DependencyInjection;
        using Steeltoe.Extensions.Configuration.ConfigServer;
        ```

   2. Create a method to configure the LogBuilder and edit the CreateWebHostBuilder method to utilize the extension method to add Config Server:

        ```c#
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.AddConfigServer(GetLoggerFactory());
                    webBuilder.UseStartup<Startup>();
                });

        public static ILoggerFactory GetLoggerFactory()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace)
                    .AddConsole()
                    .AddDebug();
            });
            return serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
        }
        ```

3. Create a file named `ApiSettings.cs`.  This class will be used to store configuration information for our API.  The class should have the following definition:

    ```c#
    namespace bootcamp_webapi
    {
        public class ApiSettings
        {
            public string Version { get; set; }
            public string Title { get; set; }
        }
    }
    ```

4. Navigate to the Startup class and make the following code changes:

   1. Set the following using statements:

        ```c#
        using Steeltoe.Extensions.Configuration.ConfigServer;
        using NJsonSchema;
        using NSwag.AspNetCore;
        ```

   2. In the ConfigureServices method make the following code changes: use an extension method to add swagger to the DI Container with the following lines of code *before* the following line of code `services.AddControllers();`.

        ```c#
        var apiSettings = Configuration
            .GetSection("api")
            .Get<ApiSettings>();

        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = document =>
            {
                document.Info.Version = apiSettings?.Version;
                document.Info.Title = apiSettings?.Title;
                document.Info.Description = "A simple ASP.NET Core web API";
                document.Schemes.Clear();
                document.Schemes.Add(NSwag.OpenApiSchema.Https);
            };
        });
        ```

   3. The Configure method is used to specify how the app responds to specific HTTP requests.  In the Configure method retrieve configuration from Config Server and add swagger to the middleware pipeline by adding the following code snippet just before the `app.UseHttpsRedirection();;` line.  This configures the pipeline to serve the Swagger specification based on our application.  For information on application start up in ASP.NET Core [see](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-3.1)

        ```c#
        app.UseOpenApi();
        app.UseSwaggerUi3(settings => settings.Path = "");
        ```

5. In the root directory navigate to the appsettings.json file and add an entry for spring and spring cloud config like the below snippet.  These values represent settings in Spring Cloud Config Server which indicate such things as the currently executing environment.  Where appropriate replace the `{initials}` placeholder with your initials.

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Warning"
        }
      },
      "AllowedHosts": "*",
      "spring": {
        "application": {
          "name": "bootcamp-api-{initials}"
        }
      },
      "cloud": {
        "config": {
          "name": "bootcamp-api-{initials}",
          "env": "development",
          "validateCertificates" : false
        }
      }
    }
    ```

6. We will once again publish  our application using the Dotnet Core CLI.

    ```powershell
    dotnet publish -o .\publish
    ```

7. In the application root create a file and name it `config.json` and edit it in the following way.  The settings in this file will tell our instance of Spring Cloud Config that we will be connecting to a git repository at the location specified in the uri field.

    ```json
    {
        "git": {
            "uri": "https://github.com/tezizzm/bootcamp-repo"
        }
    }
    ```

8. Run the following command to create an instance of Spring Cloud Config Server with settings from config.json **note: service instance name and plan may be different depending on platform/operator configuration**

    Spring Cloud Configuration Server 2.x

    ```bat
    cf create-service p-config-server standard myConfigServer-{initials} -c .\config.json
    ```

    Spring Cloud Configuration Server 3.x

    ```bat
    cf create-service p.config-server standard myConfigServer-{initials} -c .\config.json
    ```

9. Once your service has been successfully created you are ready to now “push” your application.  To check the status of your service run the command `cf services` find your service by name and ensure it's Last Operation status is listed as Create Succeeded.  Edit the manifest.yml file to add a services section (under the env section) that will automatically bind our application to the config server instance we just created.

    ```yml
    ...
    ...
     env:
       ASPNETCORE_ENVIRONMENT: development
     services:
     - myConfigServer-{initials}
    ```

10. Run the cf push command to build, stage and run your application on PCF.  Ensure you are in the same directory as your manifest file and type `cf push`.

11. Once the `cf push` command has completed navigate to the given url and you should see the Swagger page.  To confirm the configuration have a look at the configured git repository in the config.json file.
