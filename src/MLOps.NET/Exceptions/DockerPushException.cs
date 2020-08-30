﻿using System;

namespace MLOps.NET.Exceptions
{
    /// <summary>
    /// Custom exception for failure to run docker push
    /// </summary>
    public class DockerPushException : Exception
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="message"></param>
        public DockerPushException(string message) : base(message)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public DockerPushException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
