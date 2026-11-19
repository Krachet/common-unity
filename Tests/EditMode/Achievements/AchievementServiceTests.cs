using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Com.Krackhet.Runtime.Achievements.Tests
{
    public sealed class AppleEatenTask : CounterAchievementTask { }

    public sealed class AchievementServiceTests
    {
        private MemoryStore _store;
        [SetUp] public void SetUp() => _store = new MemoryStore();

        [Test] public void CounterAllRequiresEveryTask()
        {
            var a = Counter("a"); var b = Counter("b");
            var service = Service(Definition("all", AchievementRequirementPolicy.All, Req(a, 2), Req(b, 1)));
            service.Increment(a, 2); Assert.IsFalse(State(service, "all").Unlocked);
            service.Increment(b); Assert.IsTrue(State(service, "all").Unlocked);
        }

        [Test] public void AnyAndMaximumWork()
        {
            var counter = Counter("counter"); var maximum = Maximum("maximum");
            var service = Service(Definition("any", AchievementRequirementPolicy.Any, Req(counter, 50), Req(maximum, 10)));
            service.RecordValue(maximum, 7); service.RecordValue(maximum, 4);
            Assert.AreEqual(7, service.GetValue(maximum));
            service.RecordValue(maximum, 10); Assert.IsTrue(State(service, "any").Unlocked);
        }

        [Test] public void UnlockOnlyNotifiesOnce()
        {
            var task = Counter("task"); var service = Service(Definition("one", AchievementRequirementPolicy.All, Req(task, 1)));
            var count = 0; service.Unlocked += _ => count++;
            service.Increment(task); service.Increment(task);
            Assert.AreEqual(1, count);
        }

        [Test] public void GenericFactorySupportsTypedSubclassAndStableIdReporting()
        {
            var configured = AchievementTask.Create<AppleEatenTask>("apple-eaten");
            var equivalent = AchievementTask.Create<AppleEatenTask>("apple-eaten");
            var service = Service(Definition("apples", AchievementRequirementPolicy.All, Req(configured, 2)));
            service.Increment(equivalent, 2);
            Assert.AreEqual("apple-eaten", configured.Id);
            Assert.IsTrue(State(service, "apples").Unlocked);
        }

        [Test] public void UnlockNotificationIncludesDefinitionAndState()
        {
            var task = Counter("task");
            var definition = Definition("achievement", AchievementRequirementPolicy.All, Req(task, 1));
            var service = Service(definition);
            AchievementUnlockedNotification notification = null;
            service.Unlocked += value => notification = value;
            service.Increment(task);
            Assert.AreSame(definition, notification.Definition);
            Assert.AreSame(State(service, "achievement"), notification.State);
        }

        [Test] public void SharedTaskProgressUnlocksTiersAndRoundTripsAsOneProgressObject()
        {
            var task = Counter("shared");
            var low = Definition("low", AchievementRequirementPolicy.All, Req(task, 2));
            var high = Definition("high", AchievementRequirementPolicy.All, Req(task, 5));
            var service = Service(low, high);
            service.Increment(task, 5);
            Assert.IsTrue(State(service, "low").Unlocked); Assert.IsTrue(State(service, "high").Unlocked);
            service.Flush();
            var savedProgress = Store().Load().Tasks.Single(x => x.Id == "shared");
            Assert.IsInstanceOf<AchievementTaskProgress>(savedProgress);
            Assert.AreEqual(5, savedProgress.Value);
            var loaded = Service(low, high);
            Assert.AreEqual(5, loaded.GetValue(task));
        }

        [Test] public void PersistenceRoundTrips()
        {
            var task = Counter("task"); var definition = Definition("saved", AchievementRequirementPolicy.All, Req(task, 3));
            var service = Service(definition); service.Increment(task, 3); service.Flush();
            var loaded = Service(definition);
            Assert.AreEqual(3, loaded.GetValue(task)); Assert.IsTrue(State(loaded, "saved").Unlocked);
        }

        [Test] public void ChangedReportWritesProgressAndUnlockOnce()
        {
            var store = Store();
            var task = Counter("task");
            var service = Service(Definition("saved", AchievementRequirementPolicy.All, Req(task, 1)));
            service.Increment(task);
            Assert.AreEqual(1, store.SaveCount);
            Assert.AreEqual(1, store.Load().Tasks.Single(x => x.Id == "task").Value);
            Assert.IsTrue(store.Load().Achievements.Single(x => x.Id == "saved").Unlocked);
        }

        [Test] public void NoOpMaximumReportDoesNotWriteAgain()
        {
            var store = Store();
            var task = Maximum("maximum");
            var service = Service(Definition("saved", AchievementRequirementPolicy.All, Req(task, 10)));
            service.RecordValue(task, 7);
            Assert.AreEqual(1, store.SaveCount);
            service.RecordValue(task, 4);
            Assert.AreEqual(1, store.SaveCount);
        }

        [Test] public void InvalidConfigurationIsIsolated()
        {
            var good = Counter("good"); var blank = Counter("");
            var service = Service(Definition("bad", AchievementRequirementPolicy.All, Req(blank, 0)),
                Definition("good", AchievementRequirementPolicy.All, Req(good, 1)));
            service.Increment(null); service.Increment(blank); service.Increment(good);
            Assert.AreEqual(1, service.States.Count); Assert.IsTrue(State(service, "good").Unlocked);
            Assert.IsNotEmpty(service.ConfigurationErrors);
        }

        [Test] public void LoweredRequirementUnlocksOnInitialize()
        {
            var task = Counter("task");
            var first = Service(Definition("old", AchievementRequirementPolicy.All, Req(task, 10)));
            first.Increment(task, 5); first.Flush();
            var later = Service(Definition("new", AchievementRequirementPolicy.All, Req(task, 5)));
            Assert.IsTrue(State(later, "new").Unlocked);
        }

        [Test] public void UnknownEntriesSurviveFlush()
        {
            Store().Save(new AchievementSaveData {
                Tasks = new[] { new AchievementTaskProgress { Id = "unknown-task", Value = 9 } },
                Achievements = new[] { new AchievementUnlockSaveEntry { Id = "unknown-achievement", Unlocked = true } }
            });
            var service = Service(Definition("known", AchievementRequirementPolicy.All, Req(Counter("known-task"), 2)));
            service.Flush(); var saved = Store().Load();
            Assert.IsTrue(saved.Tasks.Any(x => x.Id == "unknown-task" && x.Value == 9));
            Assert.IsTrue(saved.Achievements.Any(x => x.Id == "unknown-achievement" && x.Unlocked));
        }

        [Test] public void StableIdNotReferenceControlsReporting()
        {
            var configured = Counter("same"); var equivalent = Counter("same");
            var service = Service(Definition("achievement", AchievementRequirementPolicy.All, Req(configured, 1)));
            service.Increment(equivalent);
            Assert.IsTrue(State(service, "achievement").Unlocked);
        }

        private MemoryStore Store() => _store;
        private AchievementService Service(params AchievementDefinition[] definitions)
        {
            var catalog = ScriptableObject.CreateInstance<AchievementCatalog>(); catalog.SetDefinitions(definitions);
            var service = new AchievementService(catalog, Store()); service.Initialize(); return service;
        }
        private static CounterAchievementTask Counter(string id) => AchievementTask.Create<CounterAchievementTask>(id);
        private static MaximumAchievementTask Maximum(string id) => AchievementTask.Create<MaximumAchievementTask>(id);
        private static AchievementRequirement Req(AchievementTask task, long target) => new AchievementRequirement(task, target);
        private static AchievementDefinition Definition(string id, AchievementRequirementPolicy policy, params AchievementRequirement[] requirements) =>
            AchievementDefinition.Create(id, policy, requirements);
        private static AchievementState State(AchievementService service, string id) { Assert.IsTrue(service.TryGetState(id, out var state)); return state; }

        private sealed class MemoryStore : IAchievementStore
        {
            private AchievementSaveData _data = new AchievementSaveData();
            public int SaveCount { get; private set; }
            public AchievementSaveData Load() => _data;
            public void Save(AchievementSaveData data) { _data = data ?? new AchievementSaveData(); SaveCount++; }
        }
    }
}
