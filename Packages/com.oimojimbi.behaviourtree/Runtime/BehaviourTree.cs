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
        protected BTGraphNode Parent { get; private set; }

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

    public class BTGraphNodeLeaf : BTGraphNode
    {
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
            return prevResult;
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
        private int count = 0; // TODO remove

        public BTGraphNodeRepeat(in int maxCount)
        {
            this.maxCount = maxCount;
        }

        public BTGraphNodeRepeat()
        {
        }

        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            if (maxCount == 0)
            {
                return Child;
            }
            if (prevNode == Parent)
            {
                count = 0;
            }
            if (count >= maxCount)
            {
                return Parent;
            }
            count++;
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

        protected BTGraphNode GetNextNode(BTGraphNode prevNode, in bool seekNextChild)
        {
            if (children.Count == 0)
            {
                return Parent;
            }
            if (prevNode == Parent)
            {
                return children[0];
            }
            if (!seekNextChild)
            {
                return Parent;
            }
            for (int i = 0; i < children.Count - 1; i++)
            {
                if (children[i] == prevNode)
                {
                    return children[i + 1];
                }
            }
            return Parent;
        }
    }

    public class BTGraphNodeSequence : BTGraphNodeComposite
    {
        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return GetNextNode(prevNode, prevResult == BTResult.Success);
        }
    }

    public class BTGraphNodeSelection : BTGraphNodeComposite
    {
        public override BTGraphNode GetNextNode(BTGraphNode prevNode, in BTResult prevResult, BlackBoard blackboard)
        {
            Assert.AreNotEqual(prevResult, BTResult.Running);
            return GetNextNode(prevNode, prevResult == BTResult.Failure);
        }
    }
}
