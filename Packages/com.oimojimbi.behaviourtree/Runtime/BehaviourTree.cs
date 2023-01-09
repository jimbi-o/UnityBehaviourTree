using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

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

    public abstract class BTNode
    {
        public abstract BTResult Execute(BlackBoard blackboard);
    }
}
