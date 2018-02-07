# updeps
Uses your csproj file (or multiple) to update nuspec dependencies.

Image you have a project `Foo.csproj` and you created a custom `Foo.nuspec`. You do it by your self because the automated generation of nuspec files in Visual Studio actually sucks. So, now you always forgetting to update your dependencies. With updeps you can update your nuspec file with just one following command. 

```
updeps Foo.csproj Foo.nuspec
```

Or in case you want to combine multiple project in one nuspec file then run following

```
updeps Foo.csproj Bar.csproj Foo.nuspec
```

It will output something like this

```
Reading C:\Projects\Foo.csproj
  Added 2.0.1 Microsoft.AspNetCore.Authorization
  Added 2.0.1 Microsoft.AspNetCore.Hosting.Abstractions
  Added 2.0.1 Microsoft.AspNetCore.WebUtilities
  Added 2.0.0 Microsoft.Extensions.Configuration
  Added 2.0.0 Microsoft.Extensions.Configuration.Binder
  Added 2.0.0 Microsoft.Extensions.Configuration.CommandLine
  Added 2.0.0 Microsoft.Extensions.Configuration.EnvironmentVariables
  Added 2.0.0 Microsoft.Extensions.Configuration.FileExtensions
  Added 2.0.0 Microsoft.Extensions.Configuration.Json
```

And if you hook it up to MSBuild then Visual Studio will do it automatically for you on every build. But for me, I hooked it into my NuGet publishing script. 


