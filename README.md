# Disclaimer

This a proof of concept.

This is untested code.


# Microsoft.Extensions.AspNet
`Microsoft.Extensions` extensions for .NET Framework's ASP.NET applications.

## What's included?

* Hosting
  * [Hosting Abstractions for ASP.NET](src/Microsoft.AspNet.Hosting.Abstractions)
  * [Hosting implementations for ASP.NET](src/Microsoft.AspNet.Hosting.SystemWeb)
    * Supports
      * Web Forms
      * Master Pages
      * Web Handlers
    * Provides
      * Dependency Injection
      * Configuration
      * Logging
* MVC
  * [Hosting Abstractions for ASP.NET MVC](src/Microsoft.AspNet.Hosting.SystemWeb.Mvc.Abstractions)
  * [Hosting implementations for ASP.NET MVC](src/Microsoft.AspNet.Hosting.SystemWeb.Mvc)
* Web API
  * [Hosting Abstractions for ASP.NET Web API](src/Microsoft.AspNet.Hosting.SystemWeb.WebApi.Abstractions)
  * [Hosting implementations for ASP.NET Web API](src/Microsoft.AspNet.Hosting.SystemWeb.WebApi)

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