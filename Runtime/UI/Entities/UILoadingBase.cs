using System;
using System.Collections.Generic;
using System.Threading;
using Com.Krackhet.Runtime.UI;

#if CYSHARP_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
#if CYSHARP_UNITASK
    public abstract class UILoadingBase : BaseUILayer 
    {
        public float Process { get; private set; }

        private class TaskInfo
        {
            public Func<IProgress<float>, CancellationToken, UniTask> Task;
            public float Weight;
            public bool IsCompleted;
            public float Progress;
            public string DebugName;

            public TaskInfo(Func<IProgress<float>, CancellationToken, UniTask> task, float weight = 1f, string debugName = "Unnamed Task")
            {
                Task = task;
                Weight = weight;
                IsCompleted = false;
                Progress = 0f;
                DebugName = debugName;
            }
        }

        // Main tasks that affect the loading progress
        private List<TaskInfo> mainTasks = new();

        // Background tasks that don't affect the loading progress but must complete
        private List<TaskInfo> backgroundTasks = new();

        [SerializeField] private float smoothSpeed = 2f;
        [SerializeField] private bool debugMode = false;

        private float targetProgress;
        private bool isLoading = false;
        private CancellationTokenSource loadingCts;

        protected virtual void OnDisable()
        {
            loadingCts?.Cancel();
            loadingCts?.Dispose();
            loadingCts = null;
        }

        private void OnDestroy()
        {
            loadingCts?.Cancel();
            loadingCts?.Dispose();
            loadingCts = null;
        }

        /// <summary>
        /// Override this method to implement custom loading visuals
        /// Called every frame during loading with the current progress (0-1)
        /// </summary>
        protected abstract void UpdateLoadingVisual(float progress);

        /// <summary>
        /// Override this method to initialize your loading visual when loading starts
        /// </summary>
        protected virtual void OnLoadingStart()
        {
            // Optional: Override in derived classes
        }

        /// <summary>
        /// Override this method to cleanup/finalize your loading visual when loading completes
        /// </summary>
        protected virtual void OnLoadingComplete()
        {
            // Optional:  Override in derived classes
        }

        /// <summary>
        /// Smoothly updates the progress value
        /// </summary>
        private async UniTask SmoothUpdateProgress(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (Mathf.Abs(Process - targetProgress) < 0.001f)
                {
                    Process = targetProgress;
                }
                else
                {
                    Process = Mathf.Lerp(Process, targetProgress, Time.deltaTime * smoothSpeed);
                }

                UpdateLoadingVisual(Process);
                await UniTask.Yield(ct);
            }
        }

        /// <summary>
        /// Main loading method that manages all tasks
        /// </summary>
        private async UniTask Loading(Action loaded, bool autoHideOnEnd, CancellationToken ct)
        {
            isLoading = true;

            if (debugMode) Debug.Log($"[LoadingManager] Loading started. Main tasks: {mainTasks.Count}, Background tasks: {backgroundTasks.Count}");

            OnLoadingStart();

            // Start smooth progress updater
            SmoothUpdateProgress(ct).Forget();

            // Start background tasks immediately
            var backgroundTasksRunning = new List<UniTask>();
            if (backgroundTasks.Count > 0)
            {
                if (debugMode) Debug.Log($"[LoadingManager] Starting {backgroundTasks.Count} background tasks");
                foreach (var taskInfo in backgroundTasks)
                {
                    if (debugMode) Debug.Log($"[LoadingManager] Starting background task: {taskInfo.DebugName}");
                    backgroundTasksRunning.Add(ExecuteTask(taskInfo, ct));
                }
            }

            // Phase 1: Run main tasks sequentially in order
            if (mainTasks.Count > 0)
            {
                float totalWeight = 0f;
                foreach (var taskInfo in mainTasks)
                {
                    totalWeight += taskInfo.Weight;
                }

                // Run main tasks one by one in order
                if (debugMode) Debug.Log($"[LoadingManager] Starting {mainTasks.Count} main tasks sequentially");
                float completedWeight = 0f;
                
                foreach (var taskInfo in mainTasks)
                {
                    if (debugMode) Debug.Log($"[LoadingManager] Starting main task: {taskInfo.DebugName}");
                    
                    // Execute task and wait for it to complete
                    await ExecuteTask(taskInfo, ct);
                    
                    // Update progress after each task completes
                    completedWeight += taskInfo.Weight;
                    targetProgress = Mathf.Clamp01(completedWeight / totalWeight);
                    
                    if (debugMode) Debug.Log($"[LoadingManager] Task completed: {taskInfo.DebugName} - Progress: {targetProgress:P0}");
                }

                if (debugMode) Debug.Log("[LoadingManager] All main tasks completed");
                mainTasks.Clear();
            }
            else
            {
                // No main tasks, fill to 100%
                targetProgress = 1.0f;
            }

            // Ensure progress reaches 100%
            targetProgress = 1.0f;
            while (Process < 0.99f)
            {
                await UniTask.Yield(ct);
            }
            Process = 1.0f;
            UpdateLoadingVisual(Process);

            if (debugMode) Debug.Log("[LoadingManager] Progress reached 100%, waiting for background tasks...");

            // Phase 2: Wait for all background tasks to complete
            if (backgroundTasksRunning.Count > 0)
            {
                try
                {
                    await UniTask.WhenAll(backgroundTasksRunning).Timeout(TimeSpan.FromSeconds(5));
                    if (debugMode) Debug.Log("[LoadingManager] All background tasks completed");
                }
                catch (TimeoutException)
                {
                    Debug.LogError("[LoadingManager] Background tasks timed out after 5 seconds!");
                    foreach (var taskInfo in backgroundTasks)
                    {
                        if (!taskInfo.IsCompleted)
                        {
                            Debug.LogError($"[LoadingManager] Task '{taskInfo.DebugName}' did not complete");
                        }
                    }
                }

                backgroundTasks.Clear();
            }

            OnLoadingComplete();

            isLoading = false;

            if (debugMode) Debug.Log("[LoadingManager] Loading complete!");

            if (autoHideOnEnd)
                Hide();

            loaded?.Invoke();
        }

        /// <summary>
        /// Executes a task and marks it as completed
        /// </summary>
        private async UniTask ExecuteTask(TaskInfo taskInfo, CancellationToken ct)
        {
            if (debugMode) Debug.Log($"[LoadingManager] Executing task: {taskInfo.DebugName}");

            if (taskInfo.Task == null)
            {
                Debug.LogError($"[LoadingManager] Task '{taskInfo.DebugName}' is null!");
                taskInfo.IsCompleted = true;
                return;
            }

            // try
            // {
                var progress = new Progress<float>(p => taskInfo.Progress = p);
                await taskInfo.Task(progress, ct);
                taskInfo.IsCompleted = true;
                taskInfo.Progress = 1f;
                if (debugMode) Debug.Log($"[LoadingManager] Task completed: {taskInfo.DebugName}");
            // }
            // catch (Exception ex)
            // {
            //     Debug.LogError($"[LoadingManager] Task '{taskInfo.DebugName}' failed: {ex.Message}");
            //     taskInfo.IsCompleted = true;
            // }
        }

        /// <summary>
        /// Execute an action immediately
        /// </summary>
        public UILoadingBase Do(Action action)
        {
            action?.Invoke();
            return this;
        }

        /// <summary>
        /// Start the loading process
        /// </summary>
        public virtual void Run(Action loaded, bool autoHideOnEnd = true)
        {
            Show();
            Process = 0;
            targetProgress = 0;

            UpdateLoadingVisual(0);

            loadingCts?.Cancel();
            loadingCts?.Dispose();
            loadingCts = new CancellationTokenSource();

            Loading(loaded, autoHideOnEnd, loadingCts.Token).Forget();
        }

        /// <summary>
        /// Add a main task that affects the loading progress.
        /// Task should accept IProgress<float> to report progress (0-1) and CancellationToken
        /// </summary>
        public virtual UILoadingBase AddTask(Func<IProgress<float>, CancellationToken, UniTask> task, float weight = 1f, string debugName = null)
        {
            string taskName = debugName ?? $"Main Task {mainTasks.Count + 1}";
            mainTasks.Add(new TaskInfo(task, weight, taskName));
            if (debugMode) Debug.Log($"[LoadingManager] Added main task: {taskName}");
            return this;
        }

        /// <summary>
        /// Add a main task from a simple UniTask (no progress reporting)
        /// </summary>
        public virtual UILoadingBase AddTask(Func<UniTask> task, float weight = 1f, string debugName = null)
        {
            return AddTask((progress, ct) => task(), weight, debugName);
        }

        /// <summary>
        /// Add a main task from a UniTask instance
        /// </summary>
        public virtual UILoadingBase AddTask(UniTask task, float weight = 1f, string debugName = null)
        {
            return AddTask((progress, ct) => task, weight, debugName);
        }

        /// <summary>
        /// Add a background task that runs independently and doesn't affect the loading progress.
        /// Can be called from anywhere, even after Run() is called.
        /// The final callback will wait for all background tasks to complete.
        /// </summary>
        public virtual void AddBackgroundTask(Func<IProgress<float>, CancellationToken, UniTask> task, string debugName = null, bool autoStart = true)
        {
            string taskName = debugName ?? $"Background Task {backgroundTasks.Count + 1}";
            var taskInfo = new TaskInfo(task, 1f, taskName);
            backgroundTasks.Add(taskInfo);

            if (debugMode) Debug.Log($"[LoadingManager] Added background task: {taskName}");

            // If loading is already in progress OR autoStart is true, start the task immediately
            if (isLoading || autoStart)
            {
                if (debugMode) Debug.Log($"[LoadingManager] Starting background task immediately: {taskName}");
                ExecuteTask(taskInfo, loadingCts?.Token ?? CancellationToken.None).Forget();
            }
        }

        /// <summary>
        /// Add a background task from a simple UniTask (no progress reporting)
        /// </summary>
        public virtual void AddBackgroundTask(Func<UniTask> task, string debugName = null, bool autoStart = true)
        {
            AddBackgroundTask((progress, ct) => task(), debugName, autoStart);
        }

        /// <summary>
        /// Add a background task from a UniTask instance
        /// </summary>
        public virtual void AddBackgroundTask(UniTask task, string debugName = null, bool autoStart = true)
        {
            AddBackgroundTask((progress, ct) => task, debugName, autoStart);
        }

        /// <summary>
        /// Start a background task with the loading UI reference for chaining
        /// </summary>
        public UILoadingBase WithBackgroundTask(Func<IProgress<float>, CancellationToken, UniTask> task, string debugName = null)
        {
            AddBackgroundTask(task, debugName);
            return this;
        }

        /// <summary>
        /// Start a background task with the loading UI reference for chaining (simple UniTask)
        /// </summary>
        public UILoadingBase WithBackgroundTask(Func<UniTask> task, string debugName = null)
        {
            AddBackgroundTask(task, debugName);
            return this;
        }

        /// <summary>
        /// Start a background task with the loading UI reference for chaining (UniTask instance)
        /// </summary>
        public UILoadingBase WithBackgroundTask(UniTask task, string debugName = null)
        {
            AddBackgroundTask(task, debugName);
            return this;
        }

        /// <summary>
        /// Clear all pending tasks
        /// </summary>
        public UILoadingBase ClearTasks()
        {
            mainTasks.Clear();
            backgroundTasks.Clear();
            return this;
        }

        /// <summary>
        /// Check if loading is currently in progress
        /// </summary>
        public bool IsLoading => isLoading;

        /// <summary>
        /// Get the smooth speed value for custom animations
        /// </summary>
        protected float SmoothSpeed => smoothSpeed;

        /// <summary>
        /// Enable or disable debug mode at runtime
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            debugMode = enabled;
        }
    }
#endif
}
