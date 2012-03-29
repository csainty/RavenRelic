###RavenRelic
Add RavenDb profiling data to your New Relic stats.

*NuGet package coming soon*

Once built and referenced in your web application, attaching the logging to your instance is as simple as.

````
var store = new DocumentStore();  
store.Initialize();  
RavenRelic.Monitor.AttachTo(store);
````

To access the logged data in your New Relic console, you will need to add a "Custom View".  
See `SampleView.txt` for an example to get you started.

####Links
* RavenDb - http://ravendb.net
* New Relic - http://newrelic.com
