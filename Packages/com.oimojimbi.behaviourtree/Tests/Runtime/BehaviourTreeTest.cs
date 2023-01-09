using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BlackBoard = BehaviourTree.BlackBoard<BehaviourTreeTest.Keys>;

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
        blackboard.SetFloat(Keys.One, 123.456f);
        Assert.AreEqual(blackboard.GetFloat(Keys.One), 123.456f);
        blackboard.SetBool(Keys.Two, true);
        Assert.AreEqual(blackboard.GetBool(Keys.Two), true);
        blackboard.SetInt(Keys.Three, 654321);
        Assert.AreEqual(blackboard.GetInt(Keys.Three), 654321);
        blackboard.SetVector3(Keys.One, new Vector3(1.0f, 2.0f, 3.0f));
        Assert.AreEqual(blackboard.GetVector3(Keys.One), new Vector3(1.0f, 2.0f, 3.0f));
        var obj = new GameObject("name");
        blackboard.SetGameObject(Keys.Three, obj);
        Assert.AreEqual(blackboard.GetGameObject(Keys.Three), obj);
    }
}
