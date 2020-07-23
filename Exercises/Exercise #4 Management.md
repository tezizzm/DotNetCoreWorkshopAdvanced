# Exercise #4

## Goal

Our goal in this exercise is to explore Steeltoe Management Endpoints and exercising those endpoints through the Tanzu Application Service Apps Manager integration

## Expected Results

Upon completion of this lab, we will have added Steeltoe Actuators to our Bootcamp Store application.

## Introduction

In this exercise we'll first add actuators to our Bootcamp Store UI.  We'll also define a custom healthcheck to show the extensibility of the actuator endpoints.

1. Return back to your `bootcamp-store` project root and find the project file.  We will add the following nuget package:

    ```powershell
    dotnet add package Steeltoe.Management.CloudFoundryCore --version 2.4.4
    ```

2. Navigate to the Program class and make the following edits:

   1. Set the following using statements:

        ```c#
        using Steeltoe.Management.CloudFoundry;
        ```

   2. In the CreateHostBuilder method add the following line before the `webBuilder.UseStartup<Startup>();` call to configure the service discovery client

        ```c#
        webBuilder.AddCloudFoundryActuators();
        ```

3. In the root directory navigate to the appsettings.json file and add an entry for management like the below snippet.  These settings tell the management components to enable all endpoints.

    ```json
    "management": {
      "endpoints": {
        "actuator":{
          "exposure": {
            "include": [ "*" ]
          }
        }
      }
    }
    ```

## Extend app health check to include a custom check

Before we switch to Apps Manager and see all the wonderful benefits of actuators, let's customize things a little. One very useful actuator is the /health endpoint.  Lets extend the default health check to include a custom check.

1. Create a new C# class in the app project, named `ItunesApiHealthContributor.cs`. It can be located in the root of the project with the Project.cs and Startup.cs files.  The class should have the following definition:

    ```cs
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Steeltoe.Common.HealthChecks;

    namespace bootcamp_store
    {
        public class ItunesApiHealthContributor : IHealthContributor
        {
            public string Id => "itunesApiHealthContributor";
            private readonly HttpClient _httpClient;

            public ItunesApiHealthContributor(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public HealthCheckResult Health()
            {
                var result = new HealthCheckResult
                {
                    Status = HealthStatus.UP,
                    Description = "Current Status of Itunes API Integration!"
                };

                var response = string.Empty;
                try
                {
                    // Ideally this would be called asynchronously
                    response = QueryItunesHubApiAsync().Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    result.Status = HealthStatus.DOWN;
                }

                result.Details.Add("endpoint", "itunes.apple.com/search");
                return result;
            }

            public async Task<string> QueryItunesHubApiAsync()
            {
                return await _httpClient.GetStringAsync("https://itunes.apple.com/search");
            }
        }
    }
    ```

2. Open the `Startup.cs` file, by double clicking.

3. Add a reference to the HealthChecks Steeltoe package.

    ```cs
    using Steeltoe.Common.HealthChecks;
    ```

4. In the `ConfigureServices` method, register the new custom health check as a singleton.

    ```cs
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<IHealthContributor, ItunesApiHealthContributor>(client =>
        {
            client.Timeout = TimeSpan.FromMilliseconds(1000);
        });
        services.AddControllersWithViews();
    }
    ```

5. We will once again publish our application using the Dotnet Core CLI.

    ```powershell
    dotnet publish -o .\publish
    ```

6. Run the cf push command to build, stage and run your application on PCF.  Ensure you are in the same directory as your manifest file and type `cf push`.

7. Once the command has completed, navigate to the url to once again see the home page with products listed and then explore some of the Apps Manager integrations with Steeltoe.
