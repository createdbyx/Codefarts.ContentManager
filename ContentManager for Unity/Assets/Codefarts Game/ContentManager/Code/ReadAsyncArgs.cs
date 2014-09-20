// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager
{
    using System;

    /// <summary>
    /// Provides arguments for asynchronous read operations.
    /// </summary>
    /// <typeparam name="T">The key type.</typeparam>
    public class ReadAsyncArgs<TKey, TValue>
    {
        /// <summary>
        /// Holds a reference to empty arguments
        /// </summary>
        private static ReadAsyncArgs<TKey, TValue> sharedArgsArguments = new ReadAsyncArgs<TKey, TValue>();

        /// <summary>
        /// Gets the empty arguments.
        /// </summary>  
        public static ReadAsyncArgs<TKey, TValue> SharedArgs
        {
            get
            {
                return sharedArgsArguments;
            }
        }

        /// <summary>
        /// Holds the key that is being read.
        /// </summary>
        private TKey key;

        /// <summary>
        /// Holds the result of the read operation.
        /// </summary>
        private TValue result;

        /// <summary>
        /// Holds the state of the read operation.
        /// </summary>
        private ReadState state;

        /// <summary>
        /// Holds the progres of the read operation.
        /// </summary>
        private float progress;

        /// <summary>
        /// Holds a reference to an error.
        /// </summary>
        private Exception error;

        private bool isCanceled;

        public bool IsCanceled
        {
            get
            {
                return this.isCanceled;
            }
        }

        public void Cancel()
        {
            this.isCanceled = true;
        }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>     
        public Exception Error
        {
            get
            {
                return this.error;
            }

            set
            {
                this.error = value;
            }
        }

        /// <summary>
        /// Gets or sets the key that is being read.
        /// </summary>
        public TKey Key
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
            }
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>   
        public TValue Result
        {
            get
            {
                return this.result;
            }

            set
            {
                this.result = value;
            }
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>  
        public ReadState State
        {
            get
            {
                return this.state;
            }

            set
            {
                this.state = value;
            }
        }

        /// <summary>
        /// Gets or sets the progress.
        /// </summary>   
        /// <remarks>Progress values should be between 0 and 100.</remarks>
        public float Progress
        {
            get
            {
                return this.progress;
            }

            set
            {
                this.progress = value;
            }
        }
    }
}