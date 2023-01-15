using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ScriptBehaviourTree;

namespace ScriptBehaviourTree
{
    public class BehaviourTreeTest
    {
        public enum Keys
        {
            One,
            Two,
            Three,
            Four,
            Five,
        }

        [Test]
        public void BehaviourTreeTestBlackBoard()
        {
            var blackboard = new BlackBoard();

            Assert.IsTrue(!blackboard.ContainsKey((int)Keys.One));
            blackboard.SetFloat((int)Keys.One, 123.456f);
            Assert.IsTrue(blackboard.ContainsKey((int)Keys.One));
            Assert.AreEqual(blackboard.GetFloat((int)Keys.One), 123.456f);

            Assert.IsTrue(!blackboard.ContainsKey((int)Keys.Two));
            blackboard.SetBool((int)Keys.Two, true);
            Assert.IsTrue(blackboard.ContainsKey((int)Keys.Two));
            Assert.AreEqual(blackboard.GetBool((int)Keys.Two), true);

            Assert.IsTrue(!blackboard.ContainsKey((int)Keys.Three));
            blackboard.SetInt((int)Keys.Three, 654321);
            Assert.IsTrue(blackboard.ContainsKey((int)Keys.Three));
            Assert.AreEqual(blackboard.GetInt((int)Keys.Three), 654321);

            Assert.IsTrue(!blackboard.ContainsKey((int)Keys.Four));
            blackboard.SetVector3((int)Keys.Four, new Vector3(1.0f, 2.0f, 3.0f));
            Assert.IsTrue(blackboard.ContainsKey((int)Keys.Four));
            Assert.AreEqual(blackboard.GetVector3((int)Keys.Four), new Vector3(1.0f, 2.0f, 3.0f));

            Assert.IsTrue(blackboard.ContainsKey((int)Keys.One));
            blackboard.SetVector3((int)Keys.One, new Vector3(2.0f, 2.0f, 3.0f));
            Assert.IsTrue(blackboard.ContainsKey((int)Keys.One));
            Assert.AreEqual(blackboard.GetVector3((int)Keys.One), new Vector3(2.0f, 2.0f, 3.0f));

            var obj = new GameObject("name");
            Assert.IsTrue(!blackboard.ContainsKey((int)Keys.Five));
            blackboard.SetGameObject((int)Keys.Five, obj);
            Assert.IsTrue(blackboard.ContainsKey((int)Keys.Five));
            Assert.AreEqual(blackboard.GetGameObject((int)Keys.Five), obj);
        }

        private static BTResult TickTaskSuccess(BlackBoard blackboard)
        {
            return BTResult.Success;
        }

        private static BTResult TickTaskFailure(BlackBoard blackboard)
        {
            return BTResult.Failure;
        }

        private static BTResult TickTaskRunning(BlackBoard blackboard)
        {
            return BTResult.Running;
        }

        private BTGraphNode successTask = new BTGraphNodeTask(null, TickTaskSuccess);
        private BTGraphNode failureTask = new BTGraphNodeTask(null, TickTaskFailure);
        private BTGraphNode runningTask = new BTGraphNodeTask(null, TickTaskRunning);

        [Test]
        public void BehaviourTreeTestTraverseLeaf()
        {
            var parent = new BTGraphNodeRepeat();
            var child = successTask;
            var blackboard = new BlackBoard();
            parent.AddChild(child);
            Assert.AreEqual(child.GetNextNode(BTResult.Success, blackboard), parent);
            Assert.AreEqual(child.GetNextNode(BTResult.Failure, blackboard), parent);
            Assert.AreEqual(child.GetNextNode(BTResult.Running, blackboard), child);
            Assert.AreEqual(child.GetNextNode(BTResult.Success, blackboard), parent);
            Assert.AreEqual(child.GetNextNode(BTResult.Failure, blackboard), parent);
            Assert.AreEqual(child.GetNextNode(BTResult.Running, blackboard), child);
        }

        [Test]
        public void BehaviourTreeTestTraverseRepeat()
        {
            var root = new BTGraphNodeRepeat();
            var parent = new BTGraphNodeRepeat(3);
            var child = successTask;
            var blackboard = new BlackBoard();
            root.AddChild(parent);
            parent.AddChild(child);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), root);
        }

        [Test]
        public void BehaviourTreeTestTraverseRepeatUntilFail()
        {
            var root = new BTGraphNodeRepeat();
            var parent = new BTGraphNodeRepeatUntilFail();
            var child = successTask;
            var blackboard = new BlackBoard();
            root.AddChild(parent);
            parent.AddChild(child);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), child);
        }

        [Test]
        public void BehaviourTreeTestTraverseInverter()
        {
            var root = new BTGraphNodeRepeat();
            var parent = new BTGraphNodeInverter();
            var child = successTask;
            var blackboard = new BlackBoard();
            root.AddChild(parent);
            parent.AddChild(child);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), root);
            Assert.AreEqual(parent.Tick(BTResult.Success, blackboard), BTResult.Failure);
            Assert.AreEqual(parent.Tick(BTResult.Failure, blackboard), BTResult.Success);
        }

        [Test]
        public void BehaviourTreeTestTraverseSucceeder()
        {
            var root = new BTGraphNodeRepeat();
            var parent = new BTGraphNodeSucceeder();
            var child = successTask;
            var blackboard = new BlackBoard();
            root.AddChild(parent);
            parent.AddChild(child);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), root);
            Assert.AreEqual(parent.Tick(BTResult.Success, blackboard), BTResult.Success);
            Assert.AreEqual(parent.Tick(BTResult.Failure, blackboard), BTResult.Success);
        }

        [Test]
        public void BehaviourTreeTestTraverseSequence()
        {
            var root = new BTGraphNodeRepeat();
            var parent = new BTGraphNodeSequence();
            var child1 = successTask;
            var child2 = failureTask;
            var child3 = runningTask;
            var blackboard = new BlackBoard();
            root.AddChild(parent);
            parent.AddChild(child1);
            parent.AddChild(child2);
            parent.AddChild(child3);
            Assert.AreEqual(parent.Tick(BTResult.Success, blackboard), BTResult.Success);
            Assert.AreEqual(parent.Tick(BTResult.Failure, blackboard), BTResult.Failure);
            Assert.AreEqual(parent.Tick(BTResult.Success, blackboard), BTResult.Success);
            Assert.AreEqual(parent.Tick(BTResult.Failure, blackboard), BTResult.Failure);
            Assert.AreEqual(parent.Tick(BTResult.Success, blackboard), BTResult.Success);
            Assert.AreEqual(parent.Tick(BTResult.Failure, blackboard), BTResult.Failure);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child1);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child2);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child3);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), child1);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), child1);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child2);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), child1);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child2);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child3);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), root);
        }

        [Test]
        public void BehaviourTreeTestTraverseSelection()
        {
            var root = new BTGraphNodeRepeat();
            var parent = new BTGraphNodeSelection();
            var child1 = successTask;
            var child2 = failureTask;
            var child3 = runningTask;
            var blackboard = new BlackBoard();
            root.AddChild(parent);
            parent.AddChild(child1);
            parent.AddChild(child2);
            parent.AddChild(child3);
            Assert.AreEqual(parent.Tick(BTResult.Success, blackboard), BTResult.Success);
            Assert.AreEqual(parent.Tick(BTResult.Success, blackboard), BTResult.Success);
            Assert.AreEqual(parent.Tick(BTResult.Success, blackboard), BTResult.Success);
            Assert.AreEqual(parent.Tick(BTResult.Failure, blackboard), BTResult.Failure);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child1);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child1);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), child2);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), root);
            parent.PreTick(blackboard);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), child1);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), child2);
            Assert.AreEqual(parent.GetNextNode(BTResult.Failure, blackboard), child3);
            Assert.AreEqual(parent.GetNextNode(BTResult.Success, blackboard), root);
        }

        [Test]
        public void BehaviourTreeTestTraverseSet()
        {
            var root = new BTGraphNodeRepeat();
            var sequence = new BTGraphNodeSequence();
            var selection = new BTGraphNodeSelection();
            var repeat1 = new BTGraphNodeRepeat(2);
            var repeat2 = new BTGraphNodeRepeat(3);
            var repeat3 = new BTGraphNodeRepeatUntilFail();
            var inverter = new BTGraphNodeInverter();
            var successTask1 = new BTGraphNodeTask(null, TickTaskSuccess);
            var successTask2 = new BTGraphNodeTask(null, TickTaskSuccess);
            var successTask3 = new BTGraphNodeTask(null, TickTaskSuccess);
            var runningTask1 = new BTGraphNodeTask(null, TickTaskRunning);
            // root
            root.AddChild(sequence);
            // sequence
            sequence.AddChild(repeat1);
            sequence.AddChild(selection);
            sequence.AddChild(successTask1);
            // sequence[0]
            repeat1.AddChild(successTask2);
            // sequence[1] -> selection
            selection.AddChild(inverter);
            selection.AddChild(runningTask);
            selection.AddChild(repeat3);
            // selection[0]
            inverter.AddChild(repeat2);
            repeat2.AddChild(successTask3);
            // check
            var bt = new BehaviourTreeSet(root);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(bt.Node, root);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, sequence);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, repeat1);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, successTask2);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, repeat1);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, successTask2);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, repeat1);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, sequence);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, selection);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, inverter);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, repeat2);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, successTask3);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, repeat2);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, successTask3);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, repeat2);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, successTask3);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, repeat2);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, inverter);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, selection);
                Assert.IsTrue(bt.TickOnce());
                Assert.AreEqual(bt.Node, runningTask);
                Assert.IsTrue(!bt.TickOnce());
                Assert.AreEqual(bt.Node, runningTask);
                Assert.IsTrue(!bt.TickOnce());
                Assert.AreEqual(bt.Node, runningTask);
                bt.Reset();
            }
        }

        [Test]
        public void BehaviourTreeTestJson()
        {
            string json = "{\"root\":{\"type\":\"sequence\",\"children\":[{\"type\":\"repeat\",\"child\":{\"type\":\"task\",\"task\":\"success\"}},{\"type\":\"selection\",\"children\":[{\"type\":\"inverter\",\"child\":{\"type\":\"repeat\",\"num\":3,\"child\":{\"type\":\"task\",\"task\":\"success\"}}},{\"type\":\"task\",\"task\":\"running\"},{\"type\":\"repeat\",\"num\":3,\"child\":{\"type\":\"task\",\"task\":\"failure\"}}]}]}}";
            var preticks = new Dictionary<string, BTGraphNodeTask.TaskPreTick>();
            preticks.Add("success", null);
            preticks.Add("failure", null);
            preticks.Add("running", null);
            var ticks = new Dictionary<string, BTGraphNodeTask.TaskTick>();
            ticks.Add("success", TickTaskSuccess);
            ticks.Add("failure", TickTaskFailure);
            ticks.Add("running", TickTaskRunning);
            var rootNode = BehaviourTreeImporter.ImportFromJson(json, preticks, ticks);
        }
    }
}
