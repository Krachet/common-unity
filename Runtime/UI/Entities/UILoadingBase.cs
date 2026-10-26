using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    public abstract class UILoadingBase : UILayerBase
    {
        public float Process { get; private set; }

        private class TaskInfo
        {
            public Func<IProgress<float>, CancellationToken, Task> Task;
            public float Weight;
            public bool IsCompleted;
            public float Progress;
            public string DebugName;

            public TaskInfo(Func<IProgress<float>, CancellationToken, Task> task, float weight = 1f, string debugName = "Unnamed Task")
            {
                Task = task;
                Weight = weight;
                IsCompleted = false;
                Progress = 0f;
                DebugName = debugName;
            }
        }

        // Tasks that affect the loading progress
        private List<TaskInfo> mainTasks = new();

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

        protected override void OnDestroy()
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
        private async Task SmoothUpdateProgress(CancellationToken ct)
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
                await Awaitable.NextFrameAsync(ct);
            }
        }

        /// <summary>
        /// Main loading method that manages all tasks
        /// </summary>
        private async Task Loading(Action loaded, bool autoHideOnEnd, CancellationToken ct)
        {
            isLoading = true;

            if (debugMode) Debug.Log($"[LoadingManager] Loading started. Main tasks: {mainTasks.Count}");

            OnLoadingStart();

            // Start smooth progress updater
            SmoothUpdateProgress(ct).Dispose();

            // Run main tasks sequentially in order
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
                await Awaitable.NextFrameAsync(ct);
            }
            Process = 1.0f;
            UpdateLoadingVisual(Process);

            if (debugMode) Debug.Log("[LoadingManager] Progress reached 100%");

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
        private async Task ExecuteTask(TaskInfo taskInfo, CancellationToken ct)
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

            Loading(loaded, autoHideOnEnd, loadingCts.Token).Dispose();
        }

        /// <summary>
        /// Add a main task that affects the loading progress.
        /// Task should accept IProgress<float> to report progress (0-1) and CancellationToken
        /// </summary>
        public virtual UILoadingBase AddTask(Func<IProgress<float>, CancellationToken, Task> task, float weight = 1f, string debugName = null)
        {
            string taskName = debugName ?? $"Main Task {mainTasks.Count + 1}";
            mainTasks.Add(new TaskInfo(task, weight, taskName));
            if (debugMode) Debug.Log($"[LoadingManager] Added main task: {taskName}");
            return this;
        }

        /// <summary>
        /// Add a main task from a simple Task (no progress reporting)
        /// </summary>
        public virtual UILoadingBase AddTask(Func<Task> task, float weight = 1f, string debugName = null)
        {
            return AddTask((progress, ct) => task(), weight, debugName);
        }

        /// <summary>
        /// Add a main task from a Task instance
        /// </summary>
        public virtual UILoadingBase AddTask(Task task, float weight = 1f, string debugName = null)
        {
            return AddTask((progress, ct) => task, weight, debugName);
        }

        /// <summary>
        /// Clear all pending tasks
        /// </summary>
        public UILoadingBase ClearTasks()
        {
            mainTasks.Clear();
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
}
