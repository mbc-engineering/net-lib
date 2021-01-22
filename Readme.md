# Documentation of this library repository

## About

mbc engineering used to be a company that dealt with software development in an industrial environment. In 2021 the employees decided to break new ground.
The libraries created internally are to be made available to the open source community. In the hope that they can continue to live and help others.

This Repository contains some common behavior code for our projects

## Packages

### Mbc.Hdf5Utils

A more comfortable hdf5 access with a api wrapper for .net. It uses the official raw [HDF.PInvoke package](https://github.com/HDFGroup/HDF.PInvoke) and the not so official [port for .net standard](https://github.com/HDFGroup/HDF.PInvoke.1.10)

### Mbc.TclInterpreter

Contains basic implementation of tcl script for .Net with included native binary.

For background see [Tcl Developer Xchange](http://tcl.tk/)

### Mbc.AsyncUtils

### Mbc.Common & Mbc.Common.Interface

## Build

Use at least Visual Studio 2019 (16.8).

For Deployment there is a Cake Build script. 

```powershell
> cd Source
# Unit Tests
..\Source>  .\build.ps1 -t Test

# Publish nuget
..\Source>  .\build.ps1 --target=NugetPublish --apikey=[xxxxxxxx]
```



## Contribute

Feel free to contribute! After review it will merged into de main branch.

Please write your changes into the [changelog](changelog.md).