using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptBehaviourTree;

public class BTTest : MonoBehaviour
{
    [SerializeField] private TextAsset json;
    [SerializeField] private float moveSpeed = 0.05f;
    private BehaviourTreeSet bt;

    private const int actorId = 0;
    private const int idleDurationTimeId = 1;
    private const int idlePassedTimeId = 2;
    private const int moveDurationTimeId = 3;
    private const int movePassedTimeId = 4;
    private const int moveDirectionId = 5;
    private const int moveSpeedId = 6;

    public static void SetIdleTime(BlackBoard blackboard)
    {
        blackboard.SetFloat(idleDurationTimeId, Random.Range(0.5f, 1.5f));
        blackboard.SetFloat(idlePassedTimeId, 0.0f);
    }

    public static BTResult Idle(BlackBoard blackboard)
    {
        if (IsTimeup(blackboard, idleDurationTimeId, idlePassedTimeId))
        {
            return BTResult.Success;
        }
        return BTResult.Running;
    }

    public static void SetMoveDirection(BlackBoard blackboard)
    {
        blackboard.SetFloat(moveDurationTimeId, Random.Range(0.5f, 1.5f));
        blackboard.SetFloat(movePassedTimeId, 0.0f);
        var direction = new Vector3(
            Random.Range(-1.0f, 1.0f),
            Random.Range(-1.0f, 1.0f),
            Random.Range(-1.0f, 1.0f));
        blackboard.SetVector3(moveDirectionId, direction.normalized);
    }

    public static BTResult Move(BlackBoard blackboard)
    {
        if (IsTimeup(blackboard, moveDurationTimeId, movePassedTimeId))
        {
            return BTResult.Success;
        }
        var transform = blackboard.GetTransform(actorId);
        var moveDirection = blackboard.GetVector3(moveDirectionId);
        var speed = blackboard.GetFloat(moveSpeedId);
        transform.Translate(moveDirection * speed, Space.World);
        return BTResult.Running;
    }

    private static bool IsTimeup(BlackBoard blackboard, in int durationId, in int timeId)
    {
        var timePassed = blackboard.GetFloat(timeId) + Time.deltaTime;
        blackboard.SetFloat(timeId, timePassed);
        return timePassed >= blackboard.GetFloat(durationId);
    }

    void Start()
    {
        var preticks = new Dictionary<string, BTGraphNodeTask.TaskPreTick>();
        var ticks = new Dictionary<string, BTGraphNodeTask.TaskTick>();
        preticks.Add("idle", SetIdleTime);
        preticks.Add("move", SetMoveDirection);
        ticks.Add("idle", Idle);
        ticks.Add("move", Move);
        bt = new BehaviourTreeSet(BehaviourTreeImporter.ImportFromJson(json.text, preticks, ticks));
        bt.Blackboard.SetTransform(actorId, transform);
        bt.Blackboard.SetFloat(moveSpeedId, moveSpeed);
    }

    void Update()
    {
        bt.Tick();
    }
}
