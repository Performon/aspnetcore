// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components
{
    /// <summary>
    /// The state for the component and services of a components application.
    /// </summary>
    public class ComponentApplicationState
    {
        private IDictionary<string, byte[]>? _existingState;
        private readonly IDictionary<string, byte[]> _currentState;
        private readonly List<Func<Task>> _registeredCallbacks;

        internal ComponentApplicationState(
            IDictionary<string, byte[]> currentState,
            List<Func<Task>> pauseCallbacks)
        {
            _currentState = currentState;
            _registeredCallbacks = pauseCallbacks;
        }

        internal void InitializeExistingState(IDictionary<string, byte[]> existingState)
        {
            if (_existingState != null)
            {
                throw new InvalidOperationException("ComponentApplicationState already initialized.");
            }
            _existingState = existingState ?? throw new ArgumentNullException(nameof(existingState));
        }

        /// <summary>
        /// Registers a callback that will be invoked when the component application is being paused.
        /// </summary>
        /// <param name="callback">The <see cref="Func{TResult}"/> to invoke.</param>
        public void RegisterOnPersistingCallback(Func<Task> callback)
        {
            _registeredCallbacks.Add(callback);
        }

        /// <summary>
        /// Tries to retrieve the persisted state with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key used to persist the state.</param>
        /// <param name="value">The persisted state.</param>
        /// <returns><c>true</c> if the state was found;<c>false</c> otherwise.</returns>
        public bool TryRetrievePersistedState(string key, [MaybeNullWhen(false)] out byte[] value)
        {
            if (_existingState == null)
            {
                throw new InvalidOperationException("ComponentApplicationState has not been initialized.");
            }

            return _existingState.TryGetValue(key, out value);
        }

        /// <summary>
        /// Persists the serialized state <paramref name="value"/> for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to use to persist the state.</param>
        /// <param name="value">The state to persist.</param>
        public void PersistState(string key, byte[] value)
        {
            _currentState[key] = value;
        }

        /// <summary>
        /// Serializes <paramref name="instance"/> as JSON and persists it under the given <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The <paramref name="instance"/> type.</typeparam>
        /// <param name="key">The key to use to persist the state.</param>
        /// <param name="instance">The instance to persist.</param>
        public void PersistAsJson<T>(string key, T instance)
        {
            PersistState(key, JsonSerializer.SerializeToUtf8Bytes(instance));
        }

        /// <summary>
        /// Tries to retrieve the persisted state as JSON with the given <paramref name="key"/> and deserializes it into an
        /// instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="key">The key used to persist the instance.</param>
        /// <param name="instance">The persisted instance.</param>
        /// <returns><c>true</c> if the state was found;<c>false</c> otherwise.</returns>
        public bool TryRetrieveFromJson<T>(string key, [MaybeNullWhen(false)] out T instance)
        {
            if (TryRetrievePersistedState(key, out var data))
            {
                instance = JsonSerializer.Deserialize<T>(data)!;
                return true;
            }
            else
            {
                instance = default(T);
                return false;
            }
        }
    }
}