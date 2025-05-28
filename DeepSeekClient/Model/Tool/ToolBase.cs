using DeepSeekClient.Clint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DeepSeekClient.Model.Tool;
public abstract class ToolBase : WithJsonObject
{
	public abstract string Type { get; }
	public string Name { get; init => Json["name"] = field = value; }
	public string Key => Type + "." + Name;
	protected ToolBase(string name)
	{
		Name = name;
	}
	public abstract JsonObject Invoke(ToolEntry entry);
}
