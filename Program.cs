﻿using zms9110750Library.Complete;
using zms9110750Library.StateMachine;
await using StateMachine<PlayerState> StateMachine = new StateMachine<PlayerState>(PlayerState.Idel);
_ = Task.Run(async () =>
{
    await foreach (var item in StateMachine)
    {
        await Console.Out.WriteLineAsync(item + "");
    }
});


StateMachine.SetChildState(PlayerState.Move, PlayerState.Walk, PlayerState.Run);
StateMachine.Register(PlayerState.Idel, PlayerState.Jump, "W", StateTriggerType.Transition);
StateMachine.Register(PlayerState.Idel, (string s) => s switch
{
    "A" or "D" => (PlayerState.Walk, StateTriggerType.Transition),
    _ => (default, StateTriggerType.Ignore)
});
StateMachine.Register(PlayerState.Move, PlayerState.Idel, "S", StateTriggerType.Transition);
StateMachine.Register(PlayerState.Walk, (string s) => s switch
{
    "S" => (PlayerState.Run, StateTriggerType.Transition),
    _ => (default, StateTriggerType.Ignore),
});
StateMachine.Register(PlayerState.Run, PlayerState.Idel, "S", StateTriggerType.Intercept);


StateMachine[PlayerState.Idel].OnEntry += () => { Console.WriteLine("进入空闲"); return Task.CompletedTask; };
StateMachine[PlayerState.Idel].OnExit += () => { Console.WriteLine("退出空闲"); return Task.CompletedTask; };
StateMachine[PlayerState.Move].OnEntry += async () => { await Console.Out.WriteLineAsync("进入移动"); };
StateMachine[PlayerState.Move].OnExit += async () => { await Console.Out.WriteLineAsync("退出移动"); };
StateMachine[PlayerState.Walk].OnEntry += async () => { await Console.Out.WriteLineAsync("进入行走"); };
StateMachine[PlayerState.Walk].OnExit += async () => { await Console.Out.WriteLineAsync("退出行走"); };
StateMachine[PlayerState.Run].OnEntry += async () => { await Console.Out.WriteLineAsync("进入奔跑"); };
StateMachine[PlayerState.Run].OnExit += async () => { await Console.Out.WriteLineAsync("退出奔跑"); };


await StateMachine.Consult("D");
StateMachine.Table<int>(PlayerState.Run).OnExciteFrom += async i => { await Console.Out.WriteLineAsync("起步速度为" + i); };



await StateMachine.Excite(PlayerState.Run, 10);
await StateMachine.Consult("200");
await StateMachine.Consult("S");
await StateMachine.Consult("P");


await Task.Delay(1000);














enum PlayerState
{
    Move, Walk, Run, Idel, Swing, Jump, Fall
}