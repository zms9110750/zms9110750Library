// 1. 创建图构建器
using FiniteStateMachine.DirectedAcyclic;

var builder = new GraphBuild<string>();
string[] sArr = ["A", "B", "C"];
int maxLevel = 6;

// 添加节点
for (int i = 0; i < maxLevel; i++)
{
	foreach (var s in sArr)
	{
		builder.AddNode(s + i);
	}
}

// 添加基础边（按顺序连接）
for (int i = 0; i < maxLevel - 1; i++)
{
	foreach (var s in sArr)
	{
		builder.AddDirectedEdge(s + i, s + (i + 1));
	}
}
Random random = new Random(1084);


// 添加随机跳转边
foreach (var s in random.GetItems(sArr, 20).Zip(random.GetItems(sArr, 20)))
{
	int a = random.Next(maxLevel);
	int b;
	do
	{
		b = random.Next(maxLevel);
	} while (b == a);

	if (a > b)
	{
		(a, b) = (b, a);
	}

	builder.AddDirectedEdge(s.First + a, s.Second + b);
}

// 构建图
var graph = builder.Build();

// 核验1：检查每个节点的基本信息
Console.WriteLine("=== 节点基本信息核验 ===");
foreach (var node in graph.Nodes)
{
	Console.WriteLine($"节点 {node.Value} - 层级: {node.Level}");
	Console.WriteLine($"  出边: {string.Join(", ", node.OutEdges.Select(n => $"{n.Value}( )"))}");
	Console.WriteLine($"  入边: {string.Join(", ", node.InEdges.Select(n => n.Value))}");

	// 核验2：检查节点是否在正确的强连通分量中
	Console.WriteLine($"  所属强连通分量: {node.Component.Nodes.Count}个节点");
}

// 核验3：检查图的整体属性
Console.WriteLine("\n=== 图整体属性核验 ===");
Console.WriteLine($"总节点数: {graph.Nodes.Count}");
Console.WriteLine($"强连通分量数: {graph.Components.Count}");
Console.WriteLine($"游离节点数: {graph.IsolatedNodes.Count}");
Console.WriteLine($"源节点数(入度=0): {graph.SourceNodes.Count}");
Console.WriteLine($"汇节点数(出度=0): {graph.SinkNodes.Count}");

// 核验4：检查每个序列的连通性
Console.WriteLine("\n=== 序列连通性核验 ===");
foreach (var s in sArr)
{
	var sequenceNodes = graph.Nodes.Where(n => n.Value.StartsWith(s)).OrderBy(n => n.Value).ToList();
	Console.WriteLine($"序列 {s} 有 {sequenceNodes.Count} 个节点");

	// 检查是否所有节点都在同一个强连通分量中
	var component = sequenceNodes.First().Component;
	if (sequenceNodes.Any(n => n.Component != component))
	{
		Console.WriteLine($"  警告: 序列 {s} 的节点不在同一个强连通分量中");
	}

	// 检查可达性
	for (int i = 0; i < sequenceNodes.Count - 1; i++)
	{
		var currentNode = sequenceNodes[i];
		var nextNode = sequenceNodes[i + 1];

		if (!currentNode.OutReachableNodes.Contains(nextNode))
		{
			Console.WriteLine($"  警告: {currentNode.Value} 无法到达 {nextNode.Value}，全部连通{string.Join(", ", currentNode.OutReachableNodes.Select(n => n.Value))}");
		}
	}
}

// 核验5：检查随机跳转边
Console.WriteLine("\n=== 跳转边核验 ===");
foreach (var s in sArr)
{
	var jumpEdges = graph.Nodes
		.Where(n => n.Value.StartsWith(s))
		.SelectMany(n => n.OutEdges
			.Where(target => target.Value.StartsWith(s) &&
							int.Parse(target.Value[1..]) > int.Parse(n.Value[1..]) + 1)
			.Select(target => $"{n.Value}->{target.Value}"))
		.ToList();

	Console.WriteLine($"序列 {s} 的跳转边({jumpEdges.Count}条): {string.Join(", ", jumpEdges)}");
}

// 核验6：检查层级是否正确
Console.WriteLine("\n=== 层级核验 ===");
foreach (var s in sArr)
{
	var levels = graph.Nodes
		.Where(n => n.Value.StartsWith(s))
		.OrderBy(n => n.Value)
		.Select(n => n.Level)
		.ToList();

	Console.WriteLine($"序列 {s} 的层级: {string.Join(" -> ", levels)}");

	// 检查层级是否单调非减
	for (int i = 0; i < levels.Count - 1; i++)
	{
		if (levels[i] > levels[i + 1])
		{
			Console.WriteLine($"  警告: 层级在 {s}{i} 到 {s}{i + 1} 处减少");
		}
	}
}
Console.WriteLine("\n=== 强连通分量核验 ===");
foreach (var component in graph.Components)
{
	Console.WriteLine($"强连通分量 {component.GetHashCode()} 包含共计 {component.Nodes.Count} 个节点: {string.Join(", ", component.Nodes.Select(n => n.Value))}");
	Console.WriteLine(component);
}
foreach (var node in graph.Nodes)
{
	Console.WriteLine("\n" + node.Level + "," + node + "\n入口可到达" + string.Join(",", node.InReachableNodes.Select(n => n.Value)) + "\n出口可到达" + string.Join(",", node.OutReachableNodes.Select(n => n.Value)));
}