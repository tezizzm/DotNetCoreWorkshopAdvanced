# Exercise #7

## Goal

Explore single sign on with Tanzu Application Service

## Expected Results

At the conclusion of this exercise we will have created an instance of the SSO service and bound that instance our Web Store UI application, allowing us to see the SSO experience with Cloud Foundry.

## Introduction

In this exercise we will explore the process of integrating our applications with the SSO tile and enabling a Single Sign On experience.  ***Note: With this exercise we have attempted to create an experience that will work across all environments.  Note, because every environment is configured differently (users, credentials, IDP, etc) these instructions may not align with your specific environment configuration.  Rest assured that the instructor will provide guidance where there is divergence from these instructions and your specific environment.***

1. Before we begin please be sure an SSO service plan has been created by your platform team and is available in your service marketplace.  It is assumed this service is tied to an internal UAA store but not necessarily required.

2. We will make several enhancements to our Web Store UI application to support and show off SSO based features.  

   1. Navigate to the Index.cshtml and edit it to the below definition.  Notice we have created authentication based logic that changes the view based on if the user is currently authenticated.

        ```cs
        @model IEnumerable<Product>

        @{
            ViewData["Title"] = "Home Page";
        }

        <div class="jumbotron">
            <div class="container">
                <h1>Bootcamp Store</h1>
                <h2>Welcome to the Bootcamp Store please see a listing of products below</h2>
            </div>
        </div>
        <div class="container">
            <h2>Products</h2>
            <div class="row">
            @if (User.Identity.IsAuthenticated)
            {
                <p>
                    <a asp-action="Create">Create New</a>
                </p>
                <div class="col-xs-12 table-responsive">
                    <table class="table">
                        <thead>
                        <tr>
                            <th>@Html.DisplayNameFor(model => model.Name)</th>
                            <th>@Html.DisplayNameFor(model => model.Category)</th>
                            <th>@Html.DisplayNameFor(model => model.Inventory)</th>
                            <th></th>
                        </tr>
                        </thead>
                        <tbody>
                        @foreach (var product in Model)
                        {
                            <tr>
                                <td>@Html.DisplayFor(modelItem => product.Name)</td>
                                <td>@Html.DisplayFor(modelItem => product.Category)</td>
                                <td>@Html.DisplayFor(modelItem => product.Inventory)</td>
                                <td>
                                    @Html.ActionLink("Delete", "Delete", new {product.Id})
                                </td>
                            </tr>
                        }
                        </tbody>
                    </table>
                </div>
            }
            else
            {
                <div class="col-xs-12 table-responsive">
                    <table class="table">
                        <thead>
                        <tr>
                            <th>@Html.DisplayNameFor(model => model.Name)</th>
                            <th>@Html.DisplayNameFor(model => model.Category)</th>
                            <th>@Html.DisplayNameFor(model => model.Inventory)</th>
                        </tr>
                        </thead>
                        <tbody>
                        @foreach (var product in Model)
                        {
                            <tr>
                                <td>@Html.DisplayFor(modelItem => product.Name)</td>
                                <td>@Html.DisplayFor(modelItem => product.Category)</td>
                                <td>@Html.DisplayFor(modelItem => product.Inventory)</td>
                            </tr>
                        }
                        </tbody>
                    </table>
                </div>
            }
            </div>
        </div>
        ```

   2. Create a view in the Views > Home folder called `AccessDenied.cshtml` with the following definition.  This view will be displaying when a user is authenticated but not authorized to view a resource.

        ```cs
        @{
            ViewData["Title"] = "Access Denied";
        }
        <h2>Access Denied</h2>
        <h3>The authenticated user @User.Identity.Name does not have access to this resource</h3>
        ```

   3. Navigate to the Views > Shared folder and create a new file named `_LoginPartial.cshtml` with the following definition.  This partial view encapsulates our login markup and allows it to be used in several different places across our application.

        ```cs
        @if (User.Identity.IsAuthenticated)
        {
            <form asp-area="" asp-controller="Home" asp-action="LogOff" method="post" id="logoutForm" class="navbar-right">
                <ul class="navbar-nav">
                    <li class="nav-item">
                        <p class="text-dark" >Hello @User.Identity.Name!</p>
                    </li>
                    <li class="nav-item">
                        <button type="submit" class="btn btn-link navbar-btn navbar-link">Log off</button>
                    </li>
                </ul>
            </form>
        }
        else
        {
            <ul class="navbar-nav">
                <li class="nav-item"><a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Login">Log in</a></li>
            </ul>
        }
        ```

   4. Update the _Layout.cshtml file with the following changes:

      1. On line 19 remove the html class `flex-sm-row-reverse`

            ```cs
            <div class="navbar-collapse collapse d-sm-inline-flex flex-sm-row-reverse">
            ```

            ```cs
            <div class="navbar-collapse collapse d-sm-inline-flex">
            ```

      2. On line 28 add the following line:

        ```cs
        @await Html.PartialAsync("_LoginPartial")
        ```

3. Return back to your `bootcamp-store` project root and find the project file.  We will add the following nuget package:

    ```powershell
    dotnet add package Steeltoe.Security.Authentication.CloudFoundryCore --version 2.4.4
    ```

4. Navigate to the `Startup.cs` class and make the following edits:

   1. Set the following using statements:

        ```c#
        using Microsoft.AspNetCore.Authentication.Cookies;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.HttpOverrides;
        using Steeltoe.Security.Authentication.CloudFoundry;
        ```

   2. In the ConfigureServices method add the following snippet to add the authorization and authentication services to the service container.  Note the scope `admin.write`, any authenticated user will need this scope to perform any action that is governed by the declared policy.

        ```c#
        services.AddAuthentication((options) =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;
        })
        .AddCookie((options) =>
        {
            options.AccessDeniedPath = new PathString("/Home/AccessDenied");
        })
        .AddCloudFoundryOAuth(Configuration);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("admin-policy", policy => policy.RequireClaim("scope", "admin.write"));
        });
        ```

   3. In the Configure method add the following snippet to add  authentication scheme to the middleware pipeline:

        ```cs
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });
        app.UseAuthentication();
        ```

   4. In the HomeController.cs class make the following changes:

      1. Add the following using statements:

            ```cs
            using Microsoft.AspNetCore.Authentication;
            using Microsoft.AspNetCore.Authorization;
            ```

      2. Add the following methods:

            ```cs
            [HttpPost]
            public async Task<IActionResult> LogOff()
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction(nameof(Index), "Home");
            }

            [HttpGet]
            [Authorize]
            public IActionResult Login()
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            public IActionResult AccessDenied()
            {
                ViewData["Message"] = "Insufficient permissions.";
                return View();
            }
            ```

      3. Add authorize attributes `[Authorize(Policy = "admin-policy")]` with the `admin-policy` policy defined in the Startup configure services method to ***both*** of the Delete methods as well as ***both*** of the Create methods.  For example the Create (post based) method should have the following attributes:

        ```cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "admin-policy")]
        public async Task<IActionResult> Create(Product product)
        {
            try
            {
                var client = new HttpClient(_handler, true);
                await client.PostAsJsonAsync(ApiBaseUrl, product);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        ```

5. We will once again publish  our application using the Dotnet Core CLI.

    ```powershell
    dotnet publish -o .\publish
    ```

6. Run the following command to create an instance of Service Discovery **note: service name and type may be different depending on platform/operator configuration.  Please remember to create your service instance with your initials**

    ```powershell
    cf create-service p-identity auth-internal sso-{Initials}
    ```

7. Navigate to the manifest.yml file and in the services section add an entry to bind the application to the newly created instance of the SSO service

    ```yml
     - sso-{initials}
    ```
