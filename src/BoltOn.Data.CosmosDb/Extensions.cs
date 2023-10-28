﻿using System.Reflection;
using BoltOn.Bootstrapping;
using Pluralize.NET.Core;

namespace BoltOn.Data.CosmosDb;
public static class Extensions
{
    private static Pluralizer? _pluralizer;

    public static string Pluralize(this string word)
    {
        _pluralizer ??= new Pluralizer();
        return _pluralizer.Pluralize(word);
    }

    public static BootstrapperOptions BoltOnEFModule(this BootstrapperOptions bootstrapperOptions)
    {
        bootstrapperOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
        return bootstrapperOptions;
    }
}

