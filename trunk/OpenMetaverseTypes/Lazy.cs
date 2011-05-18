/*
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OpenMetaverse
{
    public class Lazy<T>
    {
        private T _value = default(T);
        private volatile bool _isValueCreated = false;
        private Func<T> _valueFactory = null;
        private object _lock;

        public bool IsValueCreated { get { return _isValueCreated; } }

        public Lazy()
            : this(() => Activator.CreateInstance<T>())
        {
        }

        public Lazy(bool isThreadSafe)
            : this(() => Activator.CreateInstance<T>(), isThreadSafe)
        {
        }

        public Lazy(Func<T> valueFactory) :
            this(valueFactory, true)
        {
        }

        public Lazy(Func<T> valueFactory, bool isThreadSafe)
        {
            if (isThreadSafe)
            {
                this._lock = new object();
            }

            this._valueFactory = valueFactory;
        }


        public T Value
        {
            get
            {
                if (!this._isValueCreated)
                {
                    if (this._lock != null)
                    {
                        Monitor.Enter(this._lock);
                    }

                    try
                    {
                        T value = this._valueFactory.Invoke();
                        this._valueFactory = null;
                        Thread.MemoryBarrier();
                        this._value = value;
                        this._isValueCreated = true;
                    }
                    finally
                    {
                        if (this._lock != null)
                        {
                            Monitor.Exit(this._lock);
                        }
                    }
                }
                return this._value;
            }
        }
    }
}
