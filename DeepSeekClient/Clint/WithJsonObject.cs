using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DeepSeekClient.Clint;
public class WithJsonObject
{
	protected JsonObject Json { get; } = new JsonObject(); 
	public override string ToString()
	{
		return Json.ToString();
	}
	public JsonObject CloneJson()
	{
		return Json.DeepClone().AsObject();
	}
}
