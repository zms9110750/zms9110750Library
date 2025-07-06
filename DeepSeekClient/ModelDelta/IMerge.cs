
namespace zms9110750.DeepSeekClient.ModelDelta;

public interface IMerge<T>
{
	public void Merge(T source);
	/// <summary>
	/// 合并为完整的对象
	/// </summary> 
	T ToFinish();
} 
/*
{
  "index": 0,
  "id": "call_0_452d6d5d-62ad-4cf7-8d85-071beda53d88",
  "type": "function",
  "function": {
    "name": "EvaluateJS",
    "arguments": ""
  }
}
======
{
  "index": 0,
  "function": {
    "arguments": "{\u0022"
  }
}
======
{
  "index": 0,
  "function": {
    "arguments": "code"
  }
}
======
{
  "index": 0,
  "function": {
    "arguments": "\u0022:\u0022"
  }
}
*/

