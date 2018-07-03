# ConfigApiSharp

Creating a Configuration API proxy client for interfacing with a Milestone Advanced VMS Management Server has a bit of boilerplate involved. In it's present state, this project simply provides a sort of "factory" mechanism for producing a config API client or ServerCommandService client, saving the headache of making sure the client is setup properly.

This is basically all documented and almost entirely copy/pasted from Milestone's MIP SDK documentation with some minor tweaks, so no magic sauce here. Just a conventient wrapper that I intend to use for myself going forward.

## Getting Started

Interfacing with the Configuration API does not specifically require the use of MIP SDK component libraries. This proxy was created by pulling the WSDL from an existing XProtect Corporate 2018 R2 system, and if you're not taking advantage of the built-in configuration item types (eg. Hardware, Camera, RecordingServer etc), then you can do a lot of helpful configuration related operations just using the WCF proxy client. Simply re-use the source here in your own project or add the ConfigApiSharp nuget package. Below is a sample console application where you can supply the login parameters as arguments and simply print the Management Server information to console.

```C#
using ConfigApiSharp;
using System;
using System.Linq;


namespace ConfigApiTest
{
  static class Program
  {
    static void Main(string[] args)
    {
      var host = args[0];
      var port = int.Parse(args[1]);
      var userType = (UserType)Enum.Parse(typeof(UserType), args[2]);
      var username = args.Length > 3 ? args[3] : string.Empty;
      var password = args.Length > 4 ? args[4] : string.Empty;
      
      var configApiResult = ClientFactory.BuildConfigApiClient(host, port, userType, username, password);
      var serverCommandResult = ClientFactory.BuildServerCommandServiceClient(host, port, userType, username, password);
      
      if (configApiResult.Success && serverCommandResult.Success)
      {
        var managementServer = result.Client.GetItem("/");
        Console.WriteLine($"Connected to {managementServer.DisplayName}. Properties:");
        managementServer.Properties.ToList().ForEach(p => Console.WriteLine($"\t{p.DisplayName}: {p.Value}"));
        
        Console.WriteLine($"Server version: {serverCommandResult.Client.GetVersion()}");
      }
      else
      {
        Console.WriteLine(result.Exception.Message);
      }
      
      Console.WriteLine("Press any key to continue. . .");
      Console.ReadKey();
    }
  }
}
```
