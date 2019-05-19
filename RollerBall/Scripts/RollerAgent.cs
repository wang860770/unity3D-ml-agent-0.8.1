using MLAgents;
using UnityEngine;

public class RollerAgent : Agent
{
    /// <summary>
    /// 目标
    /// </summary>
    public Transform Target;
    /// <summary>
    /// 代理的刚体组件
    /// </summary>
    Rigidbody rBody;
    /// <summary>
    /// 移动的速度，也可以看成给物体施加的力大小
    /// </summary>
    public float speed = 10;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 完成一次任务后重新设置相应属性，调用done后会调用
    /// </summary>
    public override void AgentReset()
    {
        //判断是否掉下平台，是就要重新初始化属性，角速度，速度，本地坐标等
        if (transform.localPosition.y < 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        /*重新设置时目标位置也要重新设置，因为平台大小是5x5的，平台中心在（0，0），
         *目标的宽度是1，所以设置成左右4.5的距离刚好，y轴方向0.5，
         *使得刚好在平台上，不然可能陷下去或者飘起来了
         */
        Target.localPosition = new Vector3(Random.Range(-4.5f,4.5f),
                                      0.5f,
                                      Random.Range(-4.5f, 4.5f));
    }

    /// <summary>
    /// 收集环境信息，即需要传给tensorflow训练的特征向量，
    /// 可以理解为我把环境重要的信息告诉你，你帮我看看我下一步要怎么做。
    /// 这个个数会定义在Brain的参数里
    /// </summary>
    public override void CollectObservations()
    {
        // 我们需要目标和代理的位置，x，y，z 两个即6个值，位置代表我还有里多少距离就要到目标了，以便调节速度
        AddVectorObs(Target.localPosition);
        AddVectorObs(transform.localPosition);

        // 代理的速度，根据距离可以来调节速度，速度也会影响去目标的快慢程度
        AddVectorObs(rBody.velocity.z);
        AddVectorObs(rBody.velocity.x);
        
    }

    /// <summary>
    /// 代理得到决策后采取的动作，会定义在Brain的参数里，即可看做Brain给的决策动作，需要x，z的方向
    /// </summary>
    /// <param name="vectorAction"></param>
    /// <param name="textAction"></param>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        
        // 获得决策的动作数组
        Vector3 controlSignal = Vector3.zero;
        
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        //给代理施加的方向*力，即向量
        rBody.AddForce(controlSignal * speed);

        // 给予相应的奖励
        float distanceToTarget = Vector3.Distance(transform.localPosition,
                                                  Target.localPosition);

        // 达到目标则给予奖励，并且重新开始下一次迭代
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            Done();
        }

        // 如果滚下平台也重新下一次迭代
        if (transform.localPosition.y < 0)
        {
            Done();
        }
    }
}
