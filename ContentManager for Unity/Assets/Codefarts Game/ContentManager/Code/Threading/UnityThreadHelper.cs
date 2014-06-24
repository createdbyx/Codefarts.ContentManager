// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>
// Source: http://forum.unity3d.com/threads/90128-Unity-Threading-Helper
using System.Linq;
using System.Collections.Generic;
using System.Collections;

#if !NO_UNITY
using UnityEngine;

namespace Codefarts.ContentManager.Scripts
{
    [ExecuteInEditMode]
    public class UnityThreadHelper : MonoBehaviour
#else
public class UnityThreadHelper
#endif
    {
        private static UnityThreadHelper instance = null;
        private static object syncRoot = new object();

        public static void EnsureHelper()
        {
            lock (syncRoot)
            {
#if !NO_UNITY
                if (null == (object)instance)
                {
                    instance = FindObjectOfType(typeof(UnityThreadHelper)) as UnityThreadHelper;
                    if (null == (object)instance)
                    {
                        var go = new GameObject("[UnityThreadHelper]");
                        go.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                        instance = go.AddComponent<UnityThreadHelper>();
                        instance.EnsureHelperInstance();
                    }
                }
#else
            if (null == instance)
            {
                instance = new UnityThreadHelper();
                instance.EnsureHelperInstance();
            }
#endif
            }
        }

        private static UnityThreadHelper Instance
        {
            get
            {
                EnsureHelper();
                return instance;
            }
        }

        /// <summary>
        /// Returns the GUI/Main Dispatcher.
        /// </summary>
        public static Codefarts.ContentManager.UnityThreading.Dispatcher Dispatcher
        {
            get
            {
                return Instance.CurrentDispatcher;
            }
        }

        /// <summary>
        /// Returns the TaskDistributor.
        /// </summary>
        public static Codefarts.ContentManager.UnityThreading.TaskDistributor TaskDistributor
        {
            get
            {
                return Instance.CurrentTaskDistributor;
            }
        }

        private Codefarts.ContentManager.UnityThreading.Dispatcher dispatcher;
        public Codefarts.ContentManager.UnityThreading.Dispatcher CurrentDispatcher
        {
            get
            {
                return this.dispatcher;
            }
        }

        private Codefarts.ContentManager.UnityThreading.TaskDistributor taskDistributor;
        public Codefarts.ContentManager.UnityThreading.TaskDistributor CurrentTaskDistributor
        {
            get
            {
                return this.taskDistributor;
            }
        }

        private void EnsureHelperInstance()
        {
            this.dispatcher = Codefarts.ContentManager.UnityThreading.Dispatcher.MainNoThrow ?? new Codefarts.ContentManager.UnityThreading.Dispatcher();
            this.taskDistributor = Codefarts.ContentManager.UnityThreading.TaskDistributor.MainNoThrow ?? new Codefarts.ContentManager.UnityThreading.TaskDistributor("TaskDistributor");
        }

        /// <summary>
        /// Creates new thread which runs the given action. The given action will be wrapped so that any exception will be catched and logged.
        /// </summary>
        /// <param name="action">The action which the new thread should run.</param>
        /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
        /// <returns>The instance of the created thread class.</returns>
        public static Codefarts.ContentManager.UnityThreading.ActionThread CreateThread(System.Action<Codefarts.ContentManager.UnityThreading.ActionThread> action, bool autoStartThread)
        {
            Instance.EnsureHelperInstance();

            System.Action<Codefarts.ContentManager.UnityThreading.ActionThread> actionWrapper = currentThread =>
                {
                    try
                    {
                        action(currentThread);
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex);
                    }
                };
            var thread = new Codefarts.ContentManager.UnityThreading.ActionThread(actionWrapper, autoStartThread);
            Instance.RegisterThread(thread);
            return thread;
        }

        /// <summary>
        /// Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so that any exception will be catched and logged.
        /// </summary>
        /// <param name="action">The action which the new thread should run.</param>
        /// <returns>The instance of the created thread class.</returns>
        public static Codefarts.ContentManager.UnityThreading.ActionThread CreateThread(System.Action<Codefarts.ContentManager.UnityThreading.ActionThread> action)
        {
            return CreateThread(action, true);
        }

        /// <summary>
        /// Creates new thread which runs the given action. The given action will be wrapped so that any exception will be catched and logged.
        /// </summary>
        /// <param name="action">The action which the new thread should run.</param>
        /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
        /// <returns>The instance of the created thread class.</returns>
        public static Codefarts.ContentManager.UnityThreading.ActionThread CreateThread(System.Action action, bool autoStartThread)
        {
            return CreateThread((thread) => action(), autoStartThread);
        }

        /// <summary>
        /// Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so that any exception will be catched and logged.
        /// </summary>
        /// <param name="action">The action which the new thread should run.</param>
        /// <returns>The instance of the created thread class.</returns>
        public static Codefarts.ContentManager.UnityThreading.ActionThread CreateThread(System.Action action)
        {
            return CreateThread((thread) => action(), true);
        }

        #region Enumeratable

        /// <summary>
        /// Creates new thread which runs the given action. The given action will be wrapped so that any exception will be catched and logged.
        /// </summary>
        /// <param name="action">The enumeratable action which the new thread should run.</param>
        /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
        /// <returns>The instance of the created thread class.</returns>
        public static Codefarts.ContentManager.UnityThreading.ThreadBase CreateThread(System.Func<Codefarts.ContentManager.UnityThreading.ThreadBase, IEnumerator> action, bool autoStartThread)
        {
            Instance.EnsureHelperInstance();

            var thread = new Codefarts.ContentManager.UnityThreading.EnumeratableActionThread(action, autoStartThread);
            Instance.RegisterThread(thread);
            return thread;
        }

        /// <summary>
        /// Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so that any exception will be catched and logged.
        /// </summary>
        /// <param name="action">The enumeratable action which the new thread should run.</param>
        /// <returns>The instance of the created thread class.</returns>
        public static Codefarts.ContentManager.UnityThreading.ThreadBase CreateThread(System.Func<Codefarts.ContentManager.UnityThreading.ThreadBase, IEnumerator> action)
        {
            return CreateThread(action, true);
        }

        /// <summary>
        /// Creates new thread which runs the given action. The given action will be wrapped so that any exception will be catched and logged.
        /// </summary>
        /// <param name="action">The enumeratable action which the new thread should run.</param>
        /// <param name="autoStartThread">True when the thread should start immediately after creation.</param>
        /// <returns>The instance of the created thread class.</returns>
        public static Codefarts.ContentManager.UnityThreading.ThreadBase CreateThread(System.Func<IEnumerator> action, bool autoStartThread)
        {
            System.Func<Codefarts.ContentManager.UnityThreading.ThreadBase, IEnumerator> wrappedAction = (thread) => { return action(); };
            return CreateThread(wrappedAction, autoStartThread);
        }

        /// <summary>
        /// Creates new thread which runs the given action and starts it after creation. The given action will be wrapped so that any exception will be catched and logged.
        /// </summary>
        /// <param name="action">The action which the new thread should run.</param>
        /// <returns>The instance of the created thread class.</returns>
        public static Codefarts.ContentManager.UnityThreading.ThreadBase CreateThread(System.Func<IEnumerator> action)
        {
            System.Func<Codefarts.ContentManager.UnityThreading.ThreadBase, IEnumerator> wrappedAction = (thread) => { return action(); };
            return CreateThread(wrappedAction, true);
        }

        #endregion

        List<Codefarts.ContentManager.UnityThreading.ThreadBase> registeredThreads = new List<Codefarts.ContentManager.UnityThreading.ThreadBase>();
        
        private void RegisterThread(Codefarts.ContentManager.UnityThreading.ThreadBase thread)
        {
            if (this.registeredThreads.Contains(thread))
            {
                return;
            }

            this.registeredThreads.Add(thread);
        }

#if !NO_UNITY

        void OnDestroy()
        {
            foreach (var thread in this.registeredThreads)
                thread.Dispose();

            if (this.dispatcher != null)
                this.dispatcher.Dispose();
            this.dispatcher = null;

            if (this.taskDistributor != null)
                this.taskDistributor.Dispose();
            this.taskDistributor = null;

            if (instance == this)
                instance = null;
        }

        void Update()
        {
            if (this.dispatcher != null)
                this.dispatcher.ProcessTasks();

            var finishedThreads = this.registeredThreads.Where(thread => !thread.IsAlive).ToArray();
            foreach (var finishedThread in finishedThreads)
            {
                finishedThread.Dispose();
                this.registeredThreads.Remove(finishedThread);
            }
        }
#endif
    }
}

