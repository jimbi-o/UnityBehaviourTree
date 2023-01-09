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

    private class BTNodeSuccess : BTNode {
        private static readonly BTNodeSuccess instance = new BTNodeSuccess();

        public static BTNodeSuccess Instance()
        {
            return instance;
        }

        private BTNodeSuccess()
        {
        }

        public override BTResult Execute(BlackBoard blackboard)
        {
            return BTResult.Success;
        }
    }

    [Test]
    public void BehaviourTreeTestBTNodeSuccess()
    {
        Assert.AreEqual(BTNodeSuccess.Instance().Execute(null), BTResult.Success);
    }

    [Test]
    public void BehaviourTreeTestCreateSequene()
    {
    }
}
