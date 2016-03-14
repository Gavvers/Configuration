// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.Configuration.Json
{
    /// <summary>
    /// A JSON file based <see cref="FileConfigurationSource"/>.
    /// </summary>
    public class JsonConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new JsonConfigurationProvider(this);
        }
    }
}