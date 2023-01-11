using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BehaviourTree;

public class BehaviourTreeTest
{
    public enum Keys
    {
        One,
        Two,
        Three,
    }

    [Test]
    public void BehaviourTreeTestBlackBoard()
    {
        var blackboard = new BlackBoard();
        blackboard.SetFloat((int)Keys.One, 123.456f);
        Assert.AreEqual(blackboard.GetFloat((int)Keys.One), 123.456f);
        blackboard.SetBool((int)Keys.Two, true);
        Assert.AreEqual(blackboard.GetBool((int)Keys.Two), true);
        blackboard.SetInt((int)Keys.Three, 654321);
        Assert.AreEqual(blackboard.GetInt((int)Keys.Three), 654321);
        blackboard.SetVector3((int)Keys.One, new Vector3(1.0f, 2.0f, 3.0f));
        Assert.AreEqual(blackboard.GetVector3((int)Keys.One), new Vector3(1.0f, 2.0f, 3.0f));
        var obj = new GameObject("name");
        blackboard.SetGameObject((int)Keys.Three, obj);
        Assert.AreEqual(blackboard.GetGameObject((int)Keys.Three), obj);
    }

    [Test]
    public void BehaviourTreeTestTraverseRepeat()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeRepeat(3);
        var child = new BTGraphNodeLeaf();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child);
        child.SetParent(parent);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), root);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), root);
    }

    [Test]
    public void BehaviourTreeTestTraverseRepeatUntilFail()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeRepeatUntilFail();
        var child = new BTGraphNodeLeaf();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child);
        child.SetParent(parent);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Failure), root);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Failure), root);
    }

    [Test]
    public void BehaviourTreeTestTraverseInverter()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeInverter();
        var child = new BTGraphNodeLeaf();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child);
        child.SetParent(parent);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), root);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), root);
        Assert.AreEqual(parent.GetResult(BTResult.Success), BTResult.Failure);
        Assert.AreEqual(parent.GetResult(BTResult.Failure), BTResult.Success);
    }

    [Test]
    public void BehaviourTreeTestTraverseSucceeder()
    {
        var node = new BTGraphNodeSucceeder();
        Assert.AreEqual(node.GetResult(BTResult.Success), BTResult.Success);
        Assert.AreEqual(node.GetResult(BTResult.Failure), BTResult.Success);
    }

    [Test]
    public void BehaviourTreeTestTraverseSequence()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeSequence();
        var child1 = new BTGraphNodeLeaf();
        var child2 = new BTGraphNodeLeaf();
        var child3 = new BTGraphNodeLeaf();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);
        child1.SetParent(parent);
        child2.SetParent(parent);
        child3.SetParent(parent);
        // root (repeat)
        Assert.AreEqual(root.GetNextNode(BTResult.Success), parent);
        // leaf
        Assert.AreEqual(child1.GetNextNode(BTResult.Success), parent);
        Assert.AreEqual(child1.GetNextNode(BTResult.Failure), parent);
        Assert.AreEqual(child1.GetNextNode(BTResult.Running), child1);
        // sequence
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child1);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child2);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child3);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), root);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child1);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child2);
        Assert.AreEqual(parent.GetNextNode(BTResult.Failure), root);
    }

    [Test]
    public void BehaviourTreeTestTraverseSelection()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeSelection();
        var child1 = new BTGraphNodeLeaf();
        var child2 = new BTGraphNodeLeaf();
        var child3 = new BTGraphNodeLeaf();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);
        child1.SetParent(parent);
        child2.SetParent(parent);
        child3.SetParent(parent);
        // selection
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child1);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), root);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child1);
        Assert.AreEqual(parent.GetNextNode(BTResult.Failure), child2);
        Assert.AreEqual(parent.GetNextNode(BTResult.Failure), child3);
        Assert.AreEqual(parent.GetNextNode(BTResult.Failure), root);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), child1);
        Assert.AreEqual(parent.GetNextNode(BTResult.Failure), child2);
        Assert.AreEqual(parent.GetNextNode(BTResult.Success), root);
    }
}
