//-----------------------------------------------------------------------------
// Copyright (c) 2018 by mbc engineering GmbH, CH-6015 Luzern
// Licensed under the Apache License, Version 2.0
//-----------------------------------------------------------------------------

namespace Mbc.Common.Interface
{
    /// <summary>
    /// When implemented then shuld longruning or hardwork not be executed in the constructor, it should be in
    /// in Start(). Vice versa the Stop is to derminate the start actions. Dispose is for cleanup.
    /// </summary>
    public interface IServiceStartable
    {
        /// <summary>
        /// Perform startup service processing.
        /// </summary>
        void Start();

        /// <summary>
        /// Perform stop service processing.
        /// </summary>
        void Stop();
    }
}
