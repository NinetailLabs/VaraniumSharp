# VaraniumSharp

[![Build status](https://ci.appveyor.com/api/projects/status/vgyue0pbd6fqant0/branch/master?svg=true)](https://ci.appveyor.com/project/DeadlyEmbrace/varaniumsharp/branch/master)
[![NuGet](https://img.shields.io/nuget/v/VaraniumSharp.svg)](https://www.nuget.org/packages/VaraniumSharp/)
[![Coverage Status](https://coveralls.io/repos/github/NinetailLabs/VaraniumSharp/badge.svg?branch=master)](https://coveralls.io/github/NinetailLabs/VaraniumSharp?branch=master)
[![CodeFactor](https://www.codefactor.io/repository/github/ninetaillabs/varaniumsharp/badge)](https://www.codefactor.io/repository/github/ninetaillabs/varaniumsharp)

VaraniumSharp is very simple helper library with the goal of having reusable, reliable components.
It is written to not rely on any other libraries and as such can be easily dropped into any project to get access to the functionality it provides.

## Functionality
- Provides attributes to assist in easy, attribute based dependency injection. Injection is handled by additional packages to make it provider neutral
- Provide different kinds of caches including a reference counting cache
- Provides an interface for PackageManager to leverage
- Provides a collection that prioritize it's contents based on some value
- Provides Concurrency helpers including a disposable SemaphoreSlim for simple use
- Provides a base class for creating reconfigurable setting classes allowing easy rollback of setting values if cancelled
- Provides some extension methods to wrap basic functionality
- Provides various wrappers for static Microsoft classes to allow them to be used in a dependency injection system
- Provides a static logger based on [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging). The logger exists primarily to abstract the user's logging framework from VaraniumSharp libraries so they can log to the user's preffered logger.

## Requirements
To leverage the dependency injection attributes provided by VaraniumSharp a package that wraps the DI framework is required.
Currently two such package are supported as part of the VaraniumSharp framework, one for DryIoC and one for the Microsoft ServiceCollection.
It is also easy to create a custom wrapper for your preffered provider by simply implementing the `VaraniumSharp.DependencyInjection.AutomaticContainerRegistration` class either directly in your project or as an additional library.
To see how to implement this interface please look at one of the existing packages.

## Basic setup
Most of VaraniumSharp's functionality can be leveraged directly, however to make use of the DI attributes in your project there are a few steps.

Add an attribute to the class you want to add to the DI container.
There are two main attributes with various properties, for more details see the [documentation](https://ninetaillabs.github.io/VaraniumSharp/api/VaraniumSharp.Attributes.html)
```csharp
// The attribute tells the DI system to create a registration fo the TabWindowContext for the interface ITabWindowContext
[AutomaticContainerRegistration(typeof(ITabWindowContext))]
public sealed class TabWindowContext : ITabWindowContext
```

Next, during application startup load the DLLs that contains injectable classes into the `AppDomain`, then create the container and request it to be set up.
After that you can simply resolve the main part of the application and execute it from the container.
```csharp
// It is required to pre-load the assemblies that are auto-injected by VaraniumSharp otherwise their injections won't be picked up
AppDomain.CurrentDomain.Load(new AssemblyName("VaraniumSharp.WinUI"));

// Set up your IoC container and request that all classes are registered
var containerSetup = new ContainerSetup();
containerSetup.RetrieveClassesRequiringRegistration(true);
containerSetup.RetrieveConcretionClassesRequiringRegistration(true);
```

## Framework Libraries
## Dependency Injection
- [VaraniumSharp.DryIoC](https://www.nuget.org/packages/VaraniumSharp.DryIoc) Library that wraps the [DryIoc.dll](https://www.nuget.org/packages/DryIoc.dll/) IoC container for use with attribute based DI.
- [VaraniumSharp.ServiceCollection](https://www.nuget.org/packages/VaraniumSharp.ServiceCollection) Library that wraps [Microsoft.Extensions.DependencyInjection.Abstraction](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions/). For easy DI with ASP.net projects.

### Front End
- [VaraniumSharp.WinUI](https://www.nuget.org/packages/VaraniumSharp.WinUI) Library that contains helpers and components for use with WinUI 3.

### Wrappers
- [VaraniumSharp.CompoundDocumentFormatStorage](https://www.nuget.org/packages/VaraniumSharp.CompoundDocumentFormatStorage) Library that wraps [OpenMcdf](https://www.nuget.org/packages/OpenMcdf/). It implements the `IPackageManager` interface.
- [VaraniumSharp.Discord](https://www.nuget.org/packages/VaraniumSharp.Discord) Library that wraps [Discord.Net](https://www.nuget.org/packages/Discord.Net) for easy Discord bot setup.
- [VaraniumSharp.Oidc](https://www.nuget.org/packages/VaraniumSharp.Oidc) Library that wraps the [IdentityModel.OidcClient](https://www.nuget.org/packages/IdentityModel.OidcClient/) for easy OIDC implementation.

#### Deprecated
- [VaraniumSharp.Initiator](https://www.nuget.org/packages/VaraniumSharp.Initiator) Library that wraps DryIoc, OIDC, ApplicationInsights and Serilog. Replaced by individual packages.
- [VaraniumSharp.Monolith](https://github.com/NinetailLabs/VaraniumSharp.Monolith) Library that provides WebService functionality on top of VaraniumSharp.Initiator.
- [VarniumSharp.Shenfield](https://www.nuget.org/packages/VaraniumSharp.Shenfield) Library that wraps WPF helpers.

## Documentation
For a detailed overview of the library see our [documentation](https://ninetaillabs.github.io/VaraniumSharp/index.html)

## Note
With the release of VaraniumSharp 2.0.0 we are moving over to .NetStandard2.0 as this supports all .Net 4.6.1+ framework libraries and grants the ability to make use of DotNet Core2.0+
If an older version is required please file an issue and request support, we're willing to multi-target to older platforms where possible (Note that DotNet Core 1.x cannot be supported due to missing libraries)

## Icon
[Sprout](https://thenounproject.com/term/sprout/607325/) by [parkjisun](https://thenounproject.com/naripuru/) from the Noun Project