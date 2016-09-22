MassActivation
===================
Introduce
--------
This library makes it easy that allows you to split `Startup` or `Shutdown` steps to many difference packages into your application.
It is useful to keep code clean and single-responsibility principle for every package in your solution.
Instead of assembly the startup or shutdown codes into the `Global.asax` or `Startup` class from many packages.

Installing via NuGet!
---------------------
        Install-Package MassActivation
		
Supported platform:
------------------
Microsoft .NET Framework 3.5

Microsoft .NET Framework 4.0+

NetStandard 1.6

Usage
------
#####Step 1. Add startup class to your package.(**Required**)
* **Implicitly**: Create a `Startup` class in your package without namespace or the namespace same as the package name.
  The convension class name can be suffixed with the environment name according to the configuration. For example
  `StartupDevelopment`, `StartupProduction` or `StartupStaging`.
* **Explicitly**: Create a class with specified name and marked the assembly with `AssemblyActivator`. For example

        [assembly: AssemblyActivator(typeof(MyStartup))]
        public class MyStartup
        {
        }

---------------------------------------------------------------------

#####Step 2. Specify the startup codes.(**Optional**)
There are three stages for the application startup process:

* **Static constructor**: It will be executed in the first level stage, before any instance constructors and methods.
* **Instance constructor**: It will be executed in the second level stage, just after all the static constructor executed.
  There must be only one public instance constructor which will be invoked in the startup process.
  It can have multiple neccessary parameters which will be provided by the activation algorithm. For example

        public class MyStartup
        {
            public MyStartup(IActivatingEnvironment environment, IMyService service)
            {
                ...startup logic
            }
        }

* **Startup methods**: The startup methods will be invoked at the last level stage. And it can be defined with suffix of the environment name.
  For example `ConfigurationDevelopment`, `ConfigurationProduction` or `ConfigurationStaging`.
  If the method with environment name suffix has been defined, the non-suffixed method will be ignored.
  It can have multiple neccessary parameters which will be provided by the activation algorithm, and the return value will be ignored. 
  The static method or instance method are all allowed.
  For example

        public void ConfigurationDevelopment(IActivatingEnvironment environment, IMyService service)
        {
            ...configure logic
        }

---------------------------------------------------------------------

#####Step 3. Specify the shutdown codes. (**Optional**)
There are two stages for the application shutdown process:

* **Shutdown methods**: The shutdown methods will be invoked at the first level stage. And the convension rules just same as the startup methods. For example

        public void UnloadDevelopment(IActivatingEnvironment environment, IMyService service)
        {
            ...configure logic
        }

* **Dispose methods**: The dispose method will be invoked after the shutdown methods, if the activation class implements the `IDisposable` interface.

------------------------------------------------------------

#####Step 4. Specify the activation configuration.(**Optional**)
The activation configurations comes as a set of fluent methods starts with `ApplicationActivator` static class.

* The activation environment configuration is used to startup and shutdown with different codes for the different hosting environment. 
  It's used to detect the convension activation class or methods. The default value is "Production" or the system environment variable `ACTIVATION_ENVIRONMENT`. 
  You can specify the configuration value like following

        ApplicationActivator.UseEnvironment(EnvironmentName.Development);

  or

        public void Configuration(IActivatingEnvironment environment)
        {
            environment.UseEnvironment(EnvironmentName.Development);
        }

* The startup method names is an array used to detect the methods that will be invoked in the third stage of the hosting application startup. 
  Its default value has only one name: `Configuration`. 

  Suppose the activation environment is `Development`, then the `ConfigurationDevelopment` method will be detected at first time.
  If it is cannot be found, then the `Configuration` will be detected. You can specify the configuration value like following

    ApplicationActivator.UseStartupSteps("Configuration", "Initialize", "Startup");
  
  Then the methods will be invoked with sequence of `ConfigurationDevelopment`, `Configuration`, `InitializeDevelopment`, `Initialize`, `StartupDevelopment`, `Startup`.

* Then shutdown method names is an array used to detect the methods that will be invoked at the first stage when the hosting application is shuting down.
  Its default value has only one name: `Unload`
 
  The convension rules is the same as the startup methods. You can specifiy it like following
    
        ApplicationActivator.UseShutdownSteps("Unload", "Shutdown");

* The application name configuration indicates the name of the hosting application and is not used in this library. 
  The default value is the name of entry assembly. It can be specified like following
    
        ApplicationActivator.UseApplicationName("MyApplication");

  or

        public void Configuration(IActivatingEnvironment environment)
        {
            environment.UseApplicationName("MyApplication");
        }

* The application version configuration indicates the version of the hosting application and is not used in this library.
  The default value is the version of entry assembly. It can be specified like following

        ApplicationActivator.UseApplicationVersion(new Version("2.0.5"));

  or
  
        public void Configuration(IActivatingEnvironment environment)
        {
            environment.UseApplicationVersion(new Version("2.0.5"));
        }

------------------------------------------------------------

#####Step 5. Startup the application.(**Required**)
You should place the following code in the `Application_Start` method of `Global.asax` for the ASP.NET application or main method for the other applications.
    
        ApplicationActivator.Startup();

or

        ApplicationActivator.UseEnvironment(EnvironmentName.Development).Startup();

------------------------------------------------

#####Step 6. Shutdown the application.(**Optional**)
The library will subscribe the events of the application shutdown(`AppDomain.DomainUnload`, `HttpRuntime.AppDomainShutdown` and `HostingEnvironment.StopListening`).
If you want to shutdown the application manually, just place the following code in your methods.

        ApplicationActivator.Shutdown();

Priority
-----------
#####Default Priority
* For the static constructor or dispose method, their priority are the same in their own stage. They will be invoked with the random sequence in their owned stage.
* For the instance constructors and startup or shutdown methods, their priority are determined by the parameter numbers and types. 
  The less parameters the priority is higher, the parameter type is registered earlier the priority is higher.
  For the constructors or methods, they will be invoked with the random sequence in their owned stage.

------------------------------------------------

#####Specify Priority
If the default priority strategy cannot satisfy your requirement, 
you can specify the priority by using the `ActivationPriorityAttribute` to mark the activation classes, constructors or methods.
There are 5 priorities you can make use: `Highest`, `High`, `Normal`, `Low`, `Lowest`.
* For the classes marked with specified priority, the underlying members(static constructor, instance constructor, static methods, instance methods) will inherit
  the priority level by default. For example

        [ActivationPriority(ActivationPriority.High)]
        public class MyStartup
        {
            public MyStartup(IActivatingEnvironment environment, IMyService service)
            {
                ...startup logic
            }
        }
    
* For the static constructor, instance constructor, static methods and instance methods marked with specified priority, the invocation sequence will be reordered by
  the marked priority at first in their own stage, and then following the default rules for members in the same priority level.

        public class MyStartup
        {
            [ActivationPriority(ActivationPriority.High)]
            public Configuration(IActivatingEnvironment environment, IMyService service)
            {
                ...startup logic
            }
        }

* For the members without marked priority and inherited priority, their priority will be the default level `Normal`.

Component Registration
----------------------
In order to provide parameter values for constructors and methods, you have to register it at first. 
By default the component "IActivatingEnvironment" is out of box, and is used for activation components registration like following.

        public class MyStartup
        {
            public MyStartup(IActivatingEnvironment environment)
            {
                environment.Use<IMyService>(new MyService());
            }
        }

Others
---------
* When you want to search assemblies or types to match specified condition, you can do it like following

        public class MyStartup
        {
            public MyStartup(IActivatingEnvironment environment)
            {
                foreach(var assembly in environment.GetAssemblies())
                {
                    // add you own code...
                }
            }
        }

  It will enumerate all the assemblies includes the following:
1. The dll and exe files in the bin folder for web application or root folder for executable applications
2. The assemblies loaded through `Assembly.Load`, `Assembly.LoadFrom` or `Assembly.LoadFile` method.
3. The assemblies have been dynamically generated through `System.Emit`.
