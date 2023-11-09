// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "<挂起>")]
[assembly: SuppressMessage("Reliability", "CA2012:正确使用 ValueTask", Justification = "<挂起>")]
[assembly: SuppressMessage("Performance", "CA1859", Justification = "<挂起>")]
[assembly: SuppressMessage("Usage", "CA2225:运算符重载具有命名的备用项", Justification = "<挂起>")]
[assembly: SuppressMessage("Usage", "CA2227:集合属性应为只读", Justification = "<挂起>")]
[assembly: SuppressMessage("Design", "CA1003:使用泛型事件处理程序实例", Justification = "<挂起>", Scope = "member", Target = "~E:zms9110750Library.Complete.SegmentEventAsync`1.EventHandlers")]
[assembly: SuppressMessage("Style", "IDE0028:简化集合初始化", Justification = "<挂起>", Scope = "member", Target = "~F:zms9110750Library.Complete.ObservableAsyncEnumerable`1.sources")]
[assembly: SuppressMessage("Design", "CA1003:使用泛型事件处理程序实例", Justification = "<挂起>")]
[assembly: SuppressMessage("Reliability", "CA2008:不要在未传递 TaskScheduler 的情况下创建任务", Justification = "<挂起>", Scope = "member", Target = "~M:zms9110750Library.StateMachine.StateMachine`1.Dispose")]
