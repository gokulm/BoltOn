﻿using System.Reflection;
using BoltOn.Bootstrapping;
using Pluralize.NET.Core;

namespace BoltOn.Data.Cosmos
{
    public static class Extensions
    {
        private static Pluralizer _pluralizer;

        public static string Pluralize(this string word)
        {
            _pluralizer = _pluralizer ?? new Pluralizer();
            return _pluralizer.Pluralize(word);
        }

        public static BoltOnOptions BoltOnCosmosModule(this BoltOnOptions boltOnOptions)
        {
            boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            return boltOnOptions;
        }
    }
}