using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BehaviourTree;

public class BehaviourTreeTest
{
    /*
      var node = root;
      var result = BTResult.Success;
      while (true)
      {
          result = node.Tick();
          if (result == BTResult.Running) {
              return;
          }
          node = node.GetNextNode();
          if (node == null)
          {
              node = root;
          }
          node.PreTick();
      }
    */
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
    public void BehaviourTreeTestTraverseLeaf()
    {
        var parent = new BTGraphNodeRepeat();
        var child = new BTGraphNodeLeaf();
        var blackboard = new BlackBoard();
        parent.AddChild(child);
        child.SetParent(parent);
        Assert.AreEqual(child.GetNextNode(parent, BTResult.Success, blackboard), parent);
        Assert.AreEqual(child.GetNextNode(parent, BTResult.Failure, blackboard), parent);
        Assert.AreEqual(child.GetNextNode(parent, BTResult.Running, blackboard), child);
        Assert.AreEqual(child.GetNextNode(child, BTResult.Success, blackboard), parent);
        Assert.AreEqual(child.GetNextNode(child, BTResult.Failure, blackboard), parent);
        Assert.AreEqual(child.GetNextNode(child, BTResult.Running, blackboard), child);
        Assert.AreEqual(child.Tick(parent, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(child.Tick(parent, BTResult.Failure, blackboard), BTResult.Failure);
        Assert.AreEqual(child.Tick(child, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(child.Tick(child, BTResult.Failure, blackboard), BTResult.Failure);
        Assert.AreEqual(child.Tick(child, BTResult.Running, blackboard), BTResult.Running);
    }

    [Test]
    public void BehaviourTreeTestTraverseRepeat()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeRepeat(3, BTGraphNode.systemIdStart);
        var child = new BTGraphNodeLeaf();
        var blackboard = new BlackBoard();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child);
        child.SetParent(parent);
        parent.PreTick(blackboard);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), root);
        parent.PreTick(blackboard);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), root);
    }

    [Test]
    public void BehaviourTreeTestTraverseRepeatUntilFail()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeRepeatUntilFail();
        var child = new BTGraphNodeLeaf();
        var blackboard = new BlackBoard();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child);
        child.SetParent(parent);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Failure, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Failure, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Failure, blackboard), child);
    }

    [Test]
    public void BehaviourTreeTestTraverseInverter()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeInverter();
        var child = new BTGraphNodeLeaf();
        var blackboard = new BlackBoard();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child);
        child.SetParent(parent);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Failure, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Failure, blackboard), root);
        Assert.AreEqual(parent.Tick(child, BTResult.Success, blackboard), BTResult.Failure);
        Assert.AreEqual(parent.Tick(child, BTResult.Failure, blackboard), BTResult.Success);
    }

    [Test]
    public void BehaviourTreeTestTraverseSucceeder()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeSucceeder();
        var child = new BTGraphNodeLeaf();
        var blackboard = new BlackBoard();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child);
        child.SetParent(parent);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Success, blackboard), child);
        Assert.AreEqual(parent.GetNextNode(child, BTResult.Success, blackboard), root);
        Assert.AreEqual(parent.Tick(child, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(parent.Tick(child, BTResult.Failure, blackboard), BTResult.Success);
    }

    [Test]
    public void BehaviourTreeTestTraverseSequence()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeSequence();
        var child1 = new BTGraphNodeLeaf();
        var child2 = new BTGraphNodeLeaf();
        var child3 = new BTGraphNodeLeaf();
        var blackboard = new BlackBoard();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);
        child1.SetParent(parent);
        child2.SetParent(parent);
        child3.SetParent(parent);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Success, blackboard), child1);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Failure, blackboard), child1);
        Assert.AreEqual(parent.GetNextNode(child1, BTResult.Success, blackboard), child2);
        Assert.AreEqual(parent.GetNextNode(child1, BTResult.Failure, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(child2, BTResult.Success, blackboard), child3);
        Assert.AreEqual(parent.GetNextNode(child2, BTResult.Failure, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(child3, BTResult.Success, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(child3, BTResult.Failure, blackboard), root);
        Assert.AreEqual(parent.Tick(child1, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(parent.Tick(child1, BTResult.Failure, blackboard), BTResult.Failure);
        Assert.AreEqual(parent.Tick(child2, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(parent.Tick(child2, BTResult.Failure, blackboard), BTResult.Failure);
        Assert.AreEqual(parent.Tick(child3, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(parent.Tick(child3, BTResult.Failure, blackboard), BTResult.Failure);
    }

    [Test]
    public void BehaviourTreeTestTraverseSelection()
    {
        var root = new BTGraphNodeRepeat();
        var parent = new BTGraphNodeSelection();
        var child1 = new BTGraphNodeLeaf();
        var child2 = new BTGraphNodeLeaf();
        var child3 = new BTGraphNodeLeaf();
        var blackboard = new BlackBoard();
        root.AddChild(parent);
        parent.SetParent(root);
        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);
        child1.SetParent(parent);
        child2.SetParent(parent);
        child3.SetParent(parent);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Success, blackboard), child1);
        Assert.AreEqual(parent.GetNextNode(root, BTResult.Failure, blackboard), child1);
        Assert.AreEqual(parent.GetNextNode(child1, BTResult.Success, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(child1, BTResult.Failure, blackboard), child2);
        Assert.AreEqual(parent.GetNextNode(child2, BTResult.Success, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(child2, BTResult.Failure, blackboard), child3);
        Assert.AreEqual(parent.GetNextNode(child3, BTResult.Success, blackboard), root);
        Assert.AreEqual(parent.GetNextNode(child3, BTResult.Failure, blackboard), root);
        Assert.AreEqual(parent.Tick(child1, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(parent.Tick(child2, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(parent.Tick(child3, BTResult.Success, blackboard), BTResult.Success);
        Assert.AreEqual(parent.Tick(child3, BTResult.Failure, blackboard), BTResult.Failure);
    }
}
