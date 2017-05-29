# New Relic Agent Middleware
Linux-compatible ASP.Net core middleware to publish telemetry using New Relic's beta SDK.

## New Relic SDK installation

The New Relic beta SDK must be downloaded and its shared libraries installed such that
the middleware will be able to consume them. The following instructions were written for the 
`aspnetcore` Docker image. Depending on your flavour of Linux, the following might vary.

```
mkdir -p /opt/newrelic
curl -o /opt/newrelic/nr_agent_sdk.tar.gz http://download.newrelic.com/agent_sdk/nr_agent_sdk-v0.16.2.0-beta.x86_64.tar.gz
tar xvf /opt/newrelic/nr_agent_sdk.tar.gz -C /opt/newrelic --strip-components 1
rm /opt/newrelic/nr_agent_sdk.tar.gz
cp /opt/newrelic/lib/* /lib/x86_64-linux-gnu
```

## Environment preparation

The `libdl` shared library must be available to the middleware. If it can't be found, it
may be necessary to create a symbolic link like this:

```
ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl
```

**Important note:** The beta SDK has a dependency on `libssl.so.1.0.0` This version of
the library is susceptible to the Heartbleed vulnerability. An updated version of the library
can be used if a couple symbolc links are created:

```
ln -s /usr/lib64/libssl.so.1.0.1k /lib64/libssl.so.1.0.0
ln -s /lib64/libcrypto.so.10 /lib64/libcrypto.so.1.0.0
```

## ASP.Net Core Project Configuration

### Nuget Installation

> Install-Package NewRelicAgentMiddleware

### Update appsettings.json

The middleware will expect the following configuration segment in the `appsettings.json` file:

```
"newRelic": {
    "enabled": "true",
    "licenseKey": "YOUR_KEY",
    "appName": "YOUR_APP_NAME",
    "language": "CSharp",
    "languageVersion": "7"
  }
```

| Setting         | Value |
| --------------- |:-------------:                                                                        |
| enabled         | Enables or disables the middleware                                                    |
| licenseKey      | New Relic account license key                                                         |
| appName         | Name of the app, note this will have its hosts hostname appended to ensure uniqueness |
| language        | Programming language used                                                             |
| languageVersion | Version of programming language used                                                  |

### Declare mappings

The middleware has no access to the controller or action context, so we must explicitly map an action that's been overloaded to separate transactions e.g.:
 
```
public class ValuesController : Controller
{
	// GET api/values
	[Route("GetList")]
	public IEnumerable<string> Get()
	{
		...
	}

	// GET api/values/1
	[Route("GetById/{id:int}")]
	public string Get(int id)
	{
		...
	}   
```

For each controller and action combination, we will add a mapping to a mapping document to indicate the
URL pattern to distinguish each endpoint along with the transaction label we wish to see reported 
to New Relic
* Create `/Mappings/mappings.json`
* Add something like the following the following:
```
[
  {
    "actionRoute": "values/get",  <--- Corresponds with the controller name and action name
    "pathMappings": [
      {
        "pattern": "values/getlist$",  <--- Valid regex pattern
        "label": "values/getlist"   <--- The transaction label
      },
      {
        "pattern": "values/getbyid/[\\d]+$",  <-- Pattern to match this dynamic URL
        "label": "values/get/:id"  <--- /value/get/1 and /value/get/2 will both be rolled up with this label
      }
    ]
  },
  ...
```

### Edit Startup.cs

Add the following import statement:

`import NewRelicAgentMiddleware.Extensions;`

Configure the middleware for dependency injection, and add the middleware to the processing pipeline:

```
public void ConfigureServices(IServiceCollection services)
{
	// Setup DI for config options and our middleware
	services.AddOptions();
	services.AddNewRelicServices(Configuration.GetSection("newRelic"));
	// Add framework services.
	services.AddMvc();
}
...
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
{
	bool newRelicSDKEnabled;
	if (Boolean.TryParse(Configuration["newRelic:enabled"], out newRelicSDKEnabled))
	{
		app.UseNewRelicAgent(newRelicSDKEnabled);
	}
	else
	{
		// Maybe just log this instead?
		throw new Exception("Appsetting newrelic:enabled is either missing or malformed");
	}
	app.UseMvc();
}

```

With your API application configured as above, request that are mapped in the `mappings.json` document
should be reported as transactions to New Relic after a few minutes.