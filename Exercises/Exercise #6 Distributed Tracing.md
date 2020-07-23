# Exercise #6

## Goal

Our goal in this exercise is to introduce Distributed Tracing

## Expected Results

Upon completion of this lab we will have built up a hierarchy of our application dependencies and be able to view how requests flow through the application.

## Introduction

In this exercise we'll add our OpenCensus integration to our bootcamp-store frontend application.  We start by deploying a Zipkin server to our foundation that will serve as a landing spot for our distributed traces.  Note, this landing spot can be one of a number of different tools, both on and off platform.  We then continue on to configure our applications to output their traces to the deployed Zipkin server.

## Deploy Zipkin

1. Navigate to the [zipkin folder](/zipkin) at the root of this repository.
2. Edit the [manifest.yml](/zipkin/manifest.yml) file and exchange the initial placeholder for your initials.
3. You are now ready to deploy your Zipkin server, with the `cf push` command.  *Note: We're showing a little bit of Java love in this .NET workshop :-)*

## Add Tracing and Exporters to Web Store UI

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

3. In the root directory navigate to the appsettings.json file and add an entry for tracing like the below snippet.  ***Ensure this entry is inside the management object.***  These settings tell the exporter where to forward are tracing data.  ***Also take note of the route of the zipkin endpoint and substitute your Zipkin application name Cloud Foundry domain respectively.***

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
            "endpoint": "http://zipkin-{initials}.apps.{cloud-foundry-domain}/api/v2/spans",
            "validateCertificates": false,
            "useShortTraceIds ": true
          }
        }
      }
    }
    ```

4. Run the cf push command to build, stage and run your application on PCF.  Ensure you are in the same directory as your manifest file and type `cf push`.

5. Once the command has completed, navigate to the url to once again see the home page with products listed.  From there go to the Zipkin server and observe traces as you navigate and exercise your web store application.

## Add Tracing and Exporters to Web API

1. Return back to your `bootcamp-webapi` project root and find the project file.  We will add the following nuget package:

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

3. In the root directory navigate to the appsettings.json file and add an entry for tracing like the below snippet.  ***Ensure this entry is inside the management object.***  These settings tell the exporter where to forward are tracing data.  ***Also take note of the route of the zipkin endpoint and substitute your Zipkin application name Cloud Foundry domain respectively.***

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
            "endpoint": "http://zipkin-{initials}.apps.{cloud-foundry-domain}/api/v2/spans",
            "validateCertificates": false,
            "useShortTraceIds ": true
          }
        }
      }
    }
    ```

4. We will once again publish  our application using the Dotnet Core CLI.

    ```powershell
    dotnet publish -o .\publish
    ```

5. Run the cf push command to build, stage and run your application on PCF.  Ensure you are in the same directory as your manifest file and type `cf push`.

6. Once the command has completed, navigate to the url to once again see the home page with products listed.  From there go to the Zipkin server and observe traces as you navigate and exercise your web store application.
