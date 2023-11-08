using zms9110750Library.StateMachine;
StateMachine<PlayerState> StateMachine = new StateMachine<PlayerState>(PlayerState.Idel);
_ = Task.Run(async () =>
{
	await foreach (var item in StateMachine)
	{
		await Console.Out.WriteLineAsync(item + "");
	}
});


StateMachine[PlayerState.Walk].Substate = StateMachine[PlayerState.Move];
StateMachine.SetParentState(PlayerState.Move, PlayerState.Walk, PlayerState.Run);

StateMachine.Table<string>(PlayerState.Idel).Register("W", PlayerState.Jump, StateTriggerType.Transition);
StateMachine.Table<string>(PlayerState.Idel).Register(s => s switch
{
	"A" or "D" => (PlayerState.Walk, StateTriggerType.Transition),
	_ => (default, StateTriggerType.Ignore)
});

StateMachine.Register(PlayerState.Idel, PlayerState.Jump, "W", StateTriggerType.Transition);
StateMachine.Register(PlayerState.Idel, (string s) => s switch
{
	"A" or "D" => (PlayerState.Walk, StateTriggerType.Transition),
	_ => (default, StateTriggerType.Ignore)
});

StateMachine[PlayerState.Idel].OnExit += async () => { Console.WriteLine("退出空闲"); };
StateMachine[PlayerState.Move].OnEntry += async () => { await Console.Out.WriteLineAsync("进入移动"); };
StateMachine[PlayerState.Walk].OnEntry += async () => { await Console.Out.WriteLineAsync("进入行走"); };

await StateMachine.Consult("D");


StateMachine.Table<int>(PlayerState.Run).OnExciteFrom += async i => { await Console.Out.WriteLineAsync("起步速度为" + i); };



await StateMachine.Excite(PlayerState.Run, 10);
await StateMachine.Consult("200");


await Task.Delay(1000);
StateMachine.Dispose();


enum PlayerState
{
	Move, Walk, Run, Idel, Swing, Jump, Fall
}