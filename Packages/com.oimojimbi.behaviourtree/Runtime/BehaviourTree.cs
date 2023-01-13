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
        protected BTGraphNode Parent { get; private set; }
        private static int sequenceNumber = unchecked((int)0xF0000000);

        public BTGraphNode()
        {
            Assert.IsTrue(sequenceNumber < 0);
            NodeId = sequenceNumber;
            sequenceNumber++;
        }

        public void SetParent(BTGraphNode parent)
        {
            Parent = parent;
        }

        public virtual void AddChild(BTGraphNode child)
        {
        }

        public virtual void PreTick(BlackBoard blackboard)
        {
        }

        public virtual BTResult Tick(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return prevResult;
        }

        public abstract BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard);
    }

    public sealed class BTGraphNodeTask : BTGraphNode
    {
        public delegate BTResult TaskTick(BlackBoard blackboard);
        private TaskTick tickTask;
        public BTGraphNodeTask(TaskTick tickTask)
        {
            this.tickTask = tickTask;
        }

        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            if (prevResult == BTResult.Running)
            {
                return this;
            }
            return Parent;
        }

        public override BTResult Tick(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            if (prevNode == Parent)
            {
                Assert.AreNotEqual(prevResult, BTResult.Running);
            }
            return tickTask(blackboard);
        }
    }

    public abstract class BTGraphNodeDecorator : BTGraphNode
    {
        protected BTGraphNode Child { get; private set; }

        public override void AddChild(BTGraphNode child)
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

        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
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
        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            if (prevNode == Parent || prevResult != BTResult.Failure)
            {
                return Child;
            }
            return Parent;
        }
    }

    public abstract class BTGraphNodeResultDecorator : BTGraphNodeDecorator
    {
        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            return (prevNode == Parent) ? Child : Parent;
        }
    }

    public class BTGraphNodeInverter : BTGraphNodeResultDecorator
    {
        public override BTResult Tick(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
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
        public override BTResult Tick(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return BTResult.Success;
        }
    }

    public abstract class BTGraphNodeComposite : BTGraphNode
    {
        private List<BTGraphNode> children = new List<BTGraphNode>();

        public override void AddChild(BTGraphNode child)
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
        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return GetNextNode(blackboard, prevResult == BTResult.Success);
        }
    }

    public class BTGraphNodeSelection : BTGraphNodeComposite
    {
        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return GetNextNode(blackboard, prevResult == BTResult.Failure);
        }
    }

    public class BehaviourTreeSystem
    {
        public BTGraphNode Root { get; private set; } = new BTGraphNodeRepeat();
        public BlackBoard Blackboard { get; private set; } = new BlackBoard();
        public BTGraphNode Node { get; private set; }
        private BTResult result = BTResult.Success;

        public BehaviourTreeSystem()
        {
            Node = Root;
            Node.PreTick(Blackboard);
        }

        public void Tick()
        {
            while (true)
            {
                result = Node.Tick(Node, result, Blackboard);
                if (result == BTResult.Running) {
                    return;
                }
                var nextNode = Node.GetNextNode(Node, result, Blackboard);
                if (Node == null)
                {
                    Node = Root;
                }
                Node.PreTick(Blackboard);
            }
        }
    }
}
