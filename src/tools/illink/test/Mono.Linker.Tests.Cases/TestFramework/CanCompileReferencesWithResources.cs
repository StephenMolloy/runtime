﻿using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;
using Mono.Linker.Tests.Cases.TestFramework.Dependencies;

namespace Mono.Linker.Tests.Cases.TestFramework
{
    [SetupCompileBefore("library.dll",
        new[] { "Dependencies/CanCompileReferencesWithResources_Lib1.cs" },
        resources: new object[] { "Dependencies/CanCompileReferencesWithResources_Lib1.txt" },
        // Here to give coverage on the additional args parameter to ensure it is in sync with the more commonly used overload
        additionalArguments: new[] { "/optimize+" })]

    // Compile the same assembly again with another resource to get coverage on SetupCompileAfter
    [SetupCompileAfter("library.dll",
        new[] { "Dependencies/CanCompileReferencesWithResources_Lib1.cs" },
        resources: new object[] { "Dependencies/CanCompileReferencesWithResources_Lib1.txt", "Dependencies/CanCompileReferencesWithResources_Lib1.log" },
        // Here to give coverage on the additional args parameter to ensure it is in sync with the more commonly used overload
        additionalArguments: new[] { "/optimize+" })]

    [KeptResourceInAssembly("library.dll", "CanCompileReferencesWithResources_Lib1.txt")]
    [KeptResourceInAssembly("library.dll", "CanCompileReferencesWithResources_Lib1.log")]
    public class CanCompileReferencesWithResources
    {
        public static void Main()
        {
            // Use something so that reference isn't removed at compile time
            CanCompileReferencesWithResources_Lib1.Used();
        }
    }
}
