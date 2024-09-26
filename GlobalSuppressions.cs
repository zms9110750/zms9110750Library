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
[assembly: SuppressMessage("Design", "CA1003:使用泛型事件处理程序实例", Justification = "<挂起>")]
[assembly: SuppressMessage("Design", "CA1034:嵌套类型应不可见", Justification = "<挂起>", Scope = "type", Target = "~T:zms9110750Library.StateMachine.StateTransitionTable`2.StaticTableVoucher")]
[assembly: SuppressMessage("Globalization", "CA1303:请不要将文本作为本地化参数传递", Justification = "<挂起>")]
[assembly: SuppressMessage("Reliability", "CA2000:丢失范围之前释放对象", Justification = "<挂起>")]
[assembly: SuppressMessage("Design", "CA1034:嵌套类型应不可见", Justification = "<挂起>", Scope = "type", Target = "~T:zms9110750Library.Wrapper.AsyncSemaphoreWrapper.Scope")]
[assembly: SuppressMessage("Performance", "CA1815:重写值类型上的 Equals 和相等运算符", Justification = "<挂起>", Scope = "type", Target = "~T:zms9110750Library.Wrapper.AsyncSemaphoreWrapper.Scope")]
 