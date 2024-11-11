using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using zms9110750Library.RecipeBalancing;
using zms9110750Library.StateMachine;

Console.WriteLine();

/*
await using StateMachine<PlayerState> StateMachine = new StateMachine<PlayerState>(PlayerState.Idel);
_ = Task.Run(async () =>
{
	await foreach (var item in StateMachine)
	{
		await Console.Out.WriteLineAsync(item + "");
	}
});


StateMachine.SetChildState(PlayerState.Move, PlayerState.Walk, PlayerState.Run);
StateMachine.Register(PlayerState.Idel, PlayerState.Jump, "W", TriggerMode.Transition);
StateMachine.Register(PlayerState.Idel, (string s) => s switch
{
	"A" or "D" => (PlayerState.Walk, TriggerMode.Transition),
	_ => (default, TriggerMode.None)
});
StateMachine.Register(PlayerState.Move, PlayerState.Idel, "S", TriggerMode.Transition);
StateMachine.Register(PlayerState.Walk, (string s) => s switch
{
	"S" => (PlayerState.Run, TriggerMode.None),
	_ => (default, TriggerMode.None),
});
StateMachine.Register(PlayerState.Run, PlayerState.Idel, "S", TriggerMode.Intercept);


StateMachine[PlayerState.Idel].OnEntry += () => { Console.WriteLine("进入空闲"); return Task.CompletedTask; };
StateMachine[PlayerState.Idel].OnExit += () => { Console.WriteLine("退出空闲"); return Task.CompletedTask; };
StateMachine[PlayerState.Move].OnEntry += async () => { await Console.Out.WriteLineAsync("进入移动"); };
StateMachine[PlayerState.Move].OnExit += async () => { await Console.Out.WriteLineAsync("退出移动"); };
StateMachine[PlayerState.Walk].OnEntry += async () => { await Console.Out.WriteLineAsync("进入行走"); };
StateMachine[PlayerState.Walk].OnExit += async () => { await Console.Out.WriteLineAsync("退出行走"); };
StateMachine[PlayerState.Run].OnEntry += async () => { await Console.Out.WriteLineAsync("进入奔跑"); };
StateMachine[PlayerState.Run].OnExit += async () => { await Console.Out.WriteLineAsync("退出奔跑"); };
StateMachine.Table<int>(PlayerState.Run).OnEntryFrom += async i => { await Console.Out.WriteLineAsync("起步速度为" + i); };
 

await StateMachine.Transition("D");//空闲进入行走
await StateMachine.Transition(PlayerState.Run, 10, TriggerMode.Transition);//行走仅触发奔跑 
await StateMachine.Transition("200");//无事发生 
await StateMachine.Transition("S");//移动进入空闲 
await StateMachine.Transition("P");//无事发生 
 

await Task.Delay(100); 

enum PlayerState
{
	Move, Walk, Run, Idel, Swing, Jump, Fall
} */