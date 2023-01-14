using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace BehaviourTree
{

    [StructLayout(LayoutKind.Explicit)]
    struct ValueUnion
    {
        [FieldOffset(0)]
        public int i;

        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public bool b;
    }

    public class BlackBoard
    {
        private Dictionary<int, ValueUnion> valueMap = new Dictionary<int, ValueUnion>();
        private Dictionary<int, Vector3>    vector3Map = new Dictionary<int, Vector3>();
        private Dictionary<int, GameObject> gameObjectMap = new Dictionary<int, GameObject>();

        public bool ContainsKey(int key)
        {
            if (valueMap.ContainsKey(key))
            {
                return true;
            }
            if (vector3Map.ContainsKey(key))
            {
                return true;
            }
            if (gameObjectMap.ContainsKey(key))
            {
                return true;
            }
            return false;
        }

        public void SetFloat(int key, in float value)
        {
            var data = new ValueUnion();
            data.f = value;
            valueMap[key] = data;
        }

        public float GetFloat(in int key)
        {
            if (!valueMap.ContainsKey(key))
            {
                return 0.0f;
            }
            return valueMap[key].f;
        }

        public void SetBool(int key, in bool value)
        {
            var data = new ValueUnion();
            data.b = value;
            valueMap[key] = data;
        }

        public bool GetBool(in int key)
        {
            if (!valueMap.ContainsKey(key))
            {
                return false;
            }
            return valueMap[key].b;
        }

        public void SetInt(int key, in int value)
        {
            var data = new ValueUnion();
            data.i = value;
            valueMap[key] = data;
        }

        public int GetInt(in int key)
        {
            if (!valueMap.ContainsKey(key))
            {
                return 0;
            }
            return valueMap[key].i;
        }

        public void SetVector3(int key, in Vector3 value)
        {
            vector3Map[key] = value;
        }

        public Vector3 GetVector3(in int key)
        {
            if (!vector3Map.ContainsKey(key))
            {
                return Vector3.zero;
            }
            return vector3Map[key];
        }

        public void SetGameObject(int key, in GameObject value)
        {
            gameObjectMap[key] = value;
        }

        public GameObject GetGameObject(in int key)
        {
            if (!gameObjectMap.ContainsKey(key))
            {
                return null;
            }
            return gameObjectMap[key];
        }
    }

    public enum BTResult
    {
        Success,
        Failure,
        Running,
    }

    public abstract class BTGraphNode
    {
        public int NodeId { get; private set; }
        public BTGraphNode Parent { get; private set; }
        private static int sequenceNumber = unchecked((int)0xF0000000);

        public BTGraphNode()
        {
            Assert.IsTrue(sequenceNumber < 0);
            NodeId = sequenceNumber;
            sequenceNumber++;
        }

        public void AddChild(BTGraphNode child)
        {
            child.Parent = this;
            AddChildImpl(child);
        }

        public virtual void AddChildImpl(BTGraphNode child)
        {
        }

        public virtual void PreTick(BlackBoard blackboard)
        {
        }

        public virtual BTResult Tick(in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return prevResult;
        }

        public abstract BTGraphNode GetNextNode(in BTResult prevResult, BlackBoard blackboard);
    }

    public sealed class BTGraphNodeTask : BTGraphNode
    {
        public delegate BTResult TaskTick(BlackBoard blackboard);
        private TaskTick preTick;
        private TaskTick tickTask;

        public BTGraphNodeTask(TaskTick preTick, TaskTick tickTask)
        {
            this.preTick = preTick;
            this.tickTask = tickTask;
        }

        public override BTGraphNode GetNextNode(in BTResult prevResult, BlackBoard blackboard)
        {
            if (prevResult == BTResult.Running)
            {
                return this;
            }
            return Parent;
        }

        public override void PreTick(BlackBoard blackboard)
        {
            if (preTick != null)
            {
                preTick(blackboard);
            }
        }

        public override BTResult Tick(in BTResult prevResult, BlackBoard blackboard)
        {
            return tickTask(blackboard);
        }
    }

    public abstract class BTGraphNodeDecorator : BTGraphNode
    {
        protected BTGraphNode Child { get; private set; }

        public override void AddChildImpl(BTGraphNode child)
        {
            Child = child;
        }
    }

    public class BTGraphNodeRepeat : BTGraphNodeDecorator
    {
        private int maxCount = 0;

        public BTGraphNodeRepeat(in int maxCount)
        {
            this.maxCount = maxCount;
        }

        public BTGraphNodeRepeat()
        {
        }

        public override void PreTick(BlackBoard blackboard)
        {
            blackboard.SetInt(NodeId, 0);
        }

        public override BTGraphNode GetNextNode(in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            if (maxCount == 0)
            {
                return Child;
            }
            var count = blackboard.GetInt(NodeId);
            if (count >= maxCount)
            {
                return Parent;
            }
            blackboard.SetInt(NodeId, count + 1);
            return Child;
        }
    }

    public class BTGraphNodeRepeatUntilFail : BTGraphNodeDecorator
    {
        public override void PreTick(BlackBoard blackboard)
        {
            blackboard.SetBool(NodeId, true);
        }

        public override BTGraphNode GetNextNode(in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            if (blackboard.GetBool(NodeId) || prevResult != BTResult.Failure)
            {
                blackboard.SetBool(NodeId, false);
                return Child;
            }
            return Parent;
        }
    }

    public abstract class BTGraphNodeResultDecorator : BTGraphNodeDecorator
    {
        public override void PreTick(BlackBoard blackboard)
        {
            blackboard.SetBool(NodeId, true);
        }

        public override BTGraphNode GetNextNode(in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            if (blackboard.GetBool(NodeId))
            {
                blackboard.SetBool(NodeId, false);
                return Child;
            }
            return Parent;
        }
    }

    public class BTGraphNodeInverter : BTGraphNodeResultDecorator
    {
        public override BTResult Tick(in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            if (prevResult == BTResult.Success)
            {
                return BTResult.Failure;
            }
            return BTResult.Success;
        }
    }

    public class BTGraphNodeSucceeder : BTGraphNodeResultDecorator
    {
        public override BTResult Tick(in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return BTResult.Success;
        }
    }

    public abstract class BTGraphNodeComposite : BTGraphNode
    {
        private List<BTGraphNode> children = new List<BTGraphNode>();

        public override void AddChildImpl(BTGraphNode child)
        {
            children.Add(child);
        }

        public override void PreTick(BlackBoard blackboard)
        {
            blackboard.SetInt(NodeId, 0);
        }

        protected BTGraphNode GetNextNode(BlackBoard blackboard, in bool seekNextChild)
        {
            var index = blackboard.GetInt(NodeId);
            if (index >= children.Count || (index > 0 && !seekNextChild))
            {
                return Parent;
            }
            blackboard.SetInt(NodeId, index + 1);
            return children[index];
        }
    }

    public class BTGraphNodeSequence : BTGraphNodeComposite
    {
        public override BTGraphNode GetNextNode(in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return GetNextNode(blackboard, prevResult == BTResult.Success);
        }
    }

    public class BTGraphNodeSelection : BTGraphNodeComposite
    {
        public override BTGraphNode GetNextNode(in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return GetNextNode(blackboard, prevResult == BTResult.Failure);
        }
    }

    public class BehaviourTreeSet
    {
        public BlackBoard Blackboard { get; private set; }
        public BTGraphNode Node { get; private set; }
        private BTGraphNode root;
        private BTResult result = BTResult.Success;

        public BehaviourTreeSet(BTGraphNode root)
        {
            this.root = root;
            Reset();
        }

        public void Reset()
        {
            Node = root;
            Blackboard = new BlackBoard();
            Node.PreTick(Blackboard);
            result = BTResult.Success;
        }

        public void Tick()
        {
            while (true)
            {
                if (!TickOnce())
                {
                    break;
                }
            }
        }

        public bool TickOnce()
        {
            Debug.Log(Node.GetType().Name);
            result = Node.Tick(result, Blackboard);
            if (result == BTResult.Running) {
                return false;
            }
            var nextNode = Node.GetNextNode(result, Blackboard);
            if (Node.Parent != nextNode)
            {
                nextNode.PreTick(Blackboard);
            }
            Node = nextNode;
            return true;
        }

    }
}
