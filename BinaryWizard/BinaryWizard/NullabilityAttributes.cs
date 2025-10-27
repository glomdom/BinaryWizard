// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// This is a polyfill for nullability attributes that aren't available in .NET Standard 2.0
// Safe to include in your generator or analyzer projects.

#nullable enable

namespace System.Diagnostics.CodeAnalysis {
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class NotNullWhenAttribute : Attribute {
        public NotNullWhenAttribute(bool returnValue) {
            ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }
}