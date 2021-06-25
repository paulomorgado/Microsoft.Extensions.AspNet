# Disclaimer

This a proof of concept.

This is untested code.


# Microsoft.Extensions.AspNet
`Microsoft.Extensions` extensions for .NET Framework's ASP.NET applications.

## What's included?

* Hosting
  * [Hosting Abstractions for ASP.NET](src/Microsoft.AspNet.Hosting.HttpRuntime.Abstractions)
  * [Hosting implementations for ASP.NET](src/Microsoft.AspNet.Hosting.HttpRuntime)
    * Supports
      * Web Forms
      * Master Pages
      * Web Handlers
    * Provides
      * Dependency Injection
      * Configuration
      * Logging
* MVC
  * [Hosting Abstractions for ASP.NET MVC](src/Microsoft.AspNet.Hosting.HttpRuntime.Mvc.Abstractions)
  * [Hosting implementations for ASP.NET MVC](src/Microsoft.AspNet.Hosting.HttpRuntime.Mvc)
* Web API
  * [Hosting Abstractions for ASP.NET Web API](src/Microsoft.AspNet.Hosting.HttpRuntime.WebApi.Abstractions)
  * [Hosting implementations for ASP.NET Web API](src/Microsoft.AspNet.Hosting.HttpRuntime.WebApi)

# Demo application

Checkout the [demo application](demo/SampleWebApplication).
This application features:

* Dependency injection for:
  * ASP.NET
  * ASP.NET MVC
  * ASP.NET Web API
  * Logging
  * Configuration
    * Options pattern
  * HttpClient factory
    * Policies

## How can I contribute?

Contributions are Welcome! Help make this project better.

* See [Contributing](CONTRIBUTING.md) for more information.

## Code of Conduct

* See [Code of Conduct](CODE-OF-CONDUCT.md) explains what kinds of changes we welcome

## License

This project is licensed under the [MIT](LICENSE.TXT) license.